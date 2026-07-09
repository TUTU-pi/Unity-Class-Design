using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight DQN inference engine. Implements Linear + BatchNorm1d + ReLU
/// forward pass in pure C#. Dropout layers (used only in training) are skipped.
/// </summary>
public class NeuralNetwork
{
    public class PhaseNetwork
    {
        // Layer 0: Linear(inDim, 128)
        public float[][] fc0Weight; // [128, inDim]
        public float[] fc0Bias;     // [128]

        // Layer 1: BatchNorm1d(128)
        public float[] bn1Weight;       // gamma  [128]
        public float[] bn1Bias;         // beta   [128]
        public float[] bn1RunningMean;  // [128]
        public float[] bn1RunningVar;   // [128]

        // Layer 4: Linear(128, 128)
        public float[][] fc4Weight; // [128, 128]
        public float[] fc4Bias;     // [128]

        // Layer 5: BatchNorm1d(128)
        public float[] bn5Weight;
        public float[] bn5Bias;
        public float[] bn5RunningMean;
        public float[] bn5RunningVar;

        // Layer 8: Linear(128, outDim)
        public float[][] fc8Weight; // [outDim, 128]
        public float[] fc8Bias;     // [outDim]

        public int outDim;
    }

    private const float EPSILON = 1e-5f;

    /// <summary>
    /// Forward pass through a single phase network.
    /// Returns Q-values array of length outDim.
    /// </summary>
    public static float[] Forward(PhaseNetwork net, float[] input)
    {
        // Layer 0: Linear(inDim → 128)  →  y = x·W^T + b
        float[] h = MatVecMul(input, net.fc0Weight, net.fc0Bias);

        // Layer 1: BatchNorm1d(128)  →  y = gamma * (x - mean) / sqrt(var + eps) + beta
        BatchNorm(h, net.bn1Weight, net.bn1Bias, net.bn1RunningMean, net.bn1RunningVar);

        // Layer 2: ReLU (Dropout skipped)
        ReLU(h);

        // Layer 4: Linear(128 → 128)
        h = MatVecMul(h, net.fc4Weight, net.fc4Bias);

        // Layer 5: BatchNorm1d(128)
        BatchNorm(h, net.bn5Weight, net.bn5Bias, net.bn5RunningMean, net.bn5RunningVar);

        // Layer 6: ReLU (Dropout skipped)
        ReLU(h);

        // Layer 8: Linear(128 → outDim)
        h = MatVecMul(h, net.fc8Weight, net.fc8Bias);

        return h; // raw Q-values
    }

    // y = x @ W^T + b
    // x: [inDim], W: [outDim, inDim], b: [outDim]
    // result: [outDim]
    private static float[] MatVecMul(float[] x, float[][] W, float[] b)
    {
        int outDim = W.Length;
        float[] result = new float[outDim];
        for (int i = 0; i < outDim; i++)
        {
            float sum = b[i];
            float[] row = W[i];
            for (int j = 0; j < x.Length; j++)
                sum += x[j] * row[j];
            result[i] = sum;
        }
        return result;
    }

    private static void BatchNorm(float[] x, float[] gamma, float[] beta,
                                   float[] runningMean, float[] runningVar)
    {
        for (int i = 0; i < x.Length; i++)
        {
            float norm = (x[i] - runningMean[i]) / Mathf.Sqrt(runningVar[i] + EPSILON);
            x[i] = gamma[i] * norm + beta[i];
        }
    }

    private static void ReLU(float[] x)
    {
        for (int i = 0; i < x.Length; i++)
            if (x[i] < 0f) x[i] = 0f;
    }

    // ==================== Weight Loading ====================

    /// <summary>
    /// Load all 4 phase networks from the JSON weight file.
    /// </summary>
    public static Dictionary<int, PhaseNetwork> LoadWeights(TextAsset jsonAsset)
    {
        string json = jsonAsset.text;
        var networks = new Dictionary<int, PhaseNetwork>();

        for (int phase = 1; phase <= 4; phase++)
        {
            string phaseKey = "\"" + phase + "\"";
            int phaseStart = json.IndexOf(phaseKey);
            if (phaseStart < 0) { Debug.LogError("Phase " + phase + " not found in weights"); continue; }

            int dictStart = json.IndexOf('{', phaseStart + phaseKey.Length);
            int dictEnd = FindMatchingBrace(json, dictStart);

            string phaseBlock = json.Substring(dictStart, dictEnd - dictStart + 1);

            PhaseNetwork net = new PhaseNetwork();

            net.fc0Weight = ExtractMatrix(phaseBlock, "net.0.weight");
            net.fc0Bias = ExtractVector(phaseBlock, "net.0.bias");

            net.bn1Weight = ExtractVector(phaseBlock, "net.1.weight");
            net.bn1Bias = ExtractVector(phaseBlock, "net.1.bias");
            net.bn1RunningMean = ExtractVector(phaseBlock, "net.1.running_mean");
            net.bn1RunningVar = ExtractVector(phaseBlock, "net.1.running_var");

            net.fc4Weight = ExtractMatrix(phaseBlock, "net.4.weight");
            net.fc4Bias = ExtractVector(phaseBlock, "net.4.bias");

            net.bn5Weight = ExtractVector(phaseBlock, "net.5.weight");
            net.bn5Bias = ExtractVector(phaseBlock, "net.5.bias");
            net.bn5RunningMean = ExtractVector(phaseBlock, "net.5.running_mean");
            net.bn5RunningVar = ExtractVector(phaseBlock, "net.5.running_var");

            net.fc8Weight = ExtractMatrix(phaseBlock, "net.8.weight");
            net.fc8Bias = ExtractVector(phaseBlock, "net.8.bias");

            net.outDim = net.fc8Weight.Length;

            networks[phase] = net;
        }

        return networks;
    }

    private static float[] ExtractVector(string json, string key)
    {
        int keyIdx = json.IndexOf("\"" + key + "\"");
        if (keyIdx < 0) return new float[0];

        int arrStart = json.IndexOf('[', keyIdx);
        int arrEnd = FindMatchingBracket(json, arrStart);
        string arrStr = json.Substring(arrStart + 1, arrEnd - arrStart - 1);
        return ParseFloatArray(arrStr);
    }

    private static float[][] ExtractMatrix(string json, string key)
    {
        int keyIdx = json.IndexOf("\"" + key + "\"");
        if (keyIdx < 0) return new float[0][];

        int outerStart = json.IndexOf('[', keyIdx);
        int outerEnd = FindMatchingBracket(json, outerStart);

        // Parse each inner row [a,b,c,...], [d,e,f,...], ...
        List<float[]> rows = new List<float[]>();
        int pos = outerStart + 1;
        while (pos < outerEnd)
        {
            // skip whitespace and commas
            while (pos < outerEnd && (json[pos] == ' ' || json[pos] == '\n' || json[pos] == '\r' || json[pos] == '\t' || json[pos] == ','))
                pos++;

            if (pos >= outerEnd || json[pos] == ']') break;

            if (json[pos] == '[')
            {
                int rowEnd = FindMatchingBracket(json, pos);
                string rowStr = json.Substring(pos + 1, rowEnd - pos - 1);
                rows.Add(ParseFloatArray(rowStr));
                pos = rowEnd + 1;
            }
            else
            {
                pos++;
            }
        }
        return rows.ToArray();
    }

    private static float[] ParseFloatArray(string s)
    {
        string[] parts = s.Split(',');
        float[] result = new float[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            string trimmed = parts[i].Trim();
            if (trimmed.Length > 0)
                float.TryParse(trimmed, System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out result[i]);
        }
        return result;
    }

    private static int FindMatchingBrace(string s, int openIdx)
    {
        int depth = 0;
        for (int i = openIdx; i < s.Length; i++)
        {
            if (s[i] == '{') depth++;
            else if (s[i] == '}') { depth--; if (depth == 0) return i; }
        }
        return s.Length - 1;
    }

    private static int FindMatchingBracket(string s, int openIdx)
    {
        int depth = 0;
        for (int i = openIdx; i < s.Length; i++)
        {
            if (s[i] == '[') depth++;
            else if (s[i] == ']') { depth--; if (depth == 0) return i; }
        }
        return s.Length - 1;
    }
}
