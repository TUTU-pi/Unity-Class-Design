using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Neural network AI driver. Loads trained DQN weights and makes decisions
/// for all 4 game phases. State encoding matches the Python training code.
/// </summary>
public class NeuralAIDriver : MonoBehaviour
{
    [SerializeField] private TextAsset weightFile;
    [SerializeField] private bool debugLog;

    private Dictionary<int, NeuralNetwork.PhaseNetwork> networks;
    private const int AI_PLAYER_ID = 1; // AI always plays as player 1 (opponent)

    public bool IsLoaded { get; private set; }

    private void Awake()
    {
        if (weightFile != null)
        {
            networks = NeuralNetwork.LoadWeights(weightFile);
            IsLoaded = true;
            Debug.Log("Neural AI loaded: " + networks.Count + " phases");
        }
        else
        {
            Debug.LogWarning("No weight file assigned to NeuralAIDriver!");
        }
    }

    // ==================== Decision Interface ====================

    /// <summary>
    /// Phase 1: decide whether to use a pre-play skill (1/2/3).
    /// Returns SkillType or null for skip.
    /// </summary>
    public SkillType? DecideSkillPhaseA(
        List<CardType> myHand, List<CardType> oppHand,
        int myScore, int oppScore, int myLoseStreak,
        List<SkillType> mySkills, bool myUsedSkillBefore)
    {
        if (!IsLoaded) return null;

        List<SkillType> available = mySkills.FindAll(s => SkillData.IsPhaseA(s));
        if (available.Count == 0) return null;

        float[] input = EncodePhase1(myHand, oppHand, myScore, oppScore,
                                      myLoseStreak, mySkills, myUsedSkillBefore);
        float[] qValues = NeuralNetwork.Forward(networks[1], input);

        if (debugLog) Debug.Log($"[AI-Phase1] input=[{string.Join(",", input)}] " +
            $"Q(skip/Stone/Cloth/Scissor)=[{qValues[0]:F3},{qValues[1]:F3},{qValues[2]:F3},{qValues[3]:F3}]");

        // Action: 0=skip, 1=StonePower, 2=ClothGuard, 3=ScissorEdge
        int[] actionIds = GetAvailablePhase1Actions(available);
        int best = SelectBestAction(qValues, actionIds);

        if (best == 0) return null;
        return best switch
        {
            1 => SkillType.StonePower,
            2 => SkillType.ClothGuard,
            3 => SkillType.ScissorEdge,
            _ => null
        };
    }

    /// <summary>
    /// Phase 2: decide which card to play.
    /// </summary>
    public CardType DecideCard(
        List<CardType> myHand, List<CardType> oppHand,
        int myScore, int oppScore, int myLoseStreak,
        bool myUsedSkillBefore, SkillType? myUsedSkill, SkillType? oppUsedSkill,
        bool oppUsedSkillBefore,
        int[] cardsAvailable)
    {
        if (!IsLoaded) return (CardType)cardsAvailable[Random.Range(0, cardsAvailable.Length)];

        float[] input = EncodePhase2(myHand, oppHand, myScore, oppScore,
                                      myLoseStreak, myUsedSkillBefore, myUsedSkill,
                                      oppUsedSkill, oppUsedSkillBefore);
        float[] qValues = NeuralNetwork.Forward(networks[2], input);

        if (debugLog) Debug.Log($"[AI-Phase2] input=[{string.Join(",", input)}] " +
            $"Q(Rock/Scissor/Paper)=[{qValues[0]:F3},{qValues[1]:F3},{qValues[2]:F3}]");

        int[] actionIds = GetAvailablePhase2Actions(cardsAvailable);
        int best = SelectBestAction(qValues, actionIds);
        return (CardType)best;
    }

    /// <summary>
    /// Phase 3: decide whether to use a post-play skill (5/7).
    /// Returns SkillType or null for skip.
    /// </summary>
    public SkillType? DecideSkillPhaseB(
        List<CardType> myHand, List<CardType> oppHand,
        int myScore, int oppScore, int myLoseStreak,
        CardType myCard, CardType oppCard,
        bool myUsedSkillBefore, SkillType? myUsedSkill, SkillType? oppUsedSkill,
        bool oppUsedSkillBefore,
        List<SkillType> mySkills)
    {
        if (!IsLoaded) return null;

        List<SkillType> available = mySkills.FindAll(s => SkillData.IsPhaseB(s));
        if (available.Count == 0) return null;

        float[] input = EncodePhase3(myHand, oppHand, myScore, oppScore,
                                      myLoseStreak, (int)myCard, (int)oppCard,
                                      myUsedSkillBefore, myUsedSkill, oppUsedSkill,
                                      oppUsedSkillBefore, mySkills);
        float[] qValues = NeuralNetwork.Forward(networks[3], input);

        if (debugLog) Debug.Log($"[AI-Phase3] input=[{string.Join(",", input)}] " +
            $"Q(skip/Change/Reverse)=[{qValues[0]:F3},{qValues[1]:F3},{qValues[2]:F3}]");

        // Action: 0=skip, 1=skill5, 2=skill7
        int[] actionIds = GetAvailablePhase3Actions(available);
        int best = SelectBestAction(qValues, actionIds);

        if (best == 0) return null;
        return best switch
        {
            1 => SkillType.LastMinuteChange,
            2 => SkillType.PolarityReversal,
            _ => null
        };
    }

    /// <summary>
    /// Phase 4: decide which pool card to pick (by index 0-2).
    /// </summary>
    public int DecidePoolDraw(
        List<CardType> myHand, List<CardType> oppHand,
        CardType[] publicPool, List<SkillType> mySkills,
        int oppDrawnCardType)
    {
        if (!IsLoaded)
        {
            // Fallback: pick first available
            for (int i = 0; i < publicPool.Length; i++)
                if (publicPool[i] != CardType.None) return i;
            return 0;
        }

        float[] input = EncodePhase4(myHand, oppHand, publicPool, mySkills, oppDrawnCardType);
        float[] qValues = NeuralNetwork.Forward(networks[4], input);

        if (debugLog) Debug.Log($"[AI-Phase4] input=[{string.Join(",", input)}] " +
            $"Q(pos0/pos1/pos2)=[{qValues[0]:F3},{qValues[1]:F3},{qValues[2]:F3}]");

        int[] actionIds = GetAvailablePhase4Actions(publicPool);
        return SelectBestAction(qValues, actionIds);
    }

    // ==================== State Encoding ====================

    private static float[] EncodePhase1(
        List<CardType> myHand, List<CardType> oppHand,
        int myScore, int oppScore, int myLoseStreak,
        List<SkillType> mySkills, bool myUsedSkillBefore)
    {
        int[] slots = EncodeSkillSlotsPhase1(mySkills);
        return new float[] {
            HandTernary3(myHand),
            HandTernary3(oppHand),
            ClampScore(myScore),
            ClampScore(oppScore),
            Mathf.Min(myLoseStreak, 2),
            slots[0], slots[1], slots[2],
            myUsedSkillBefore ? 1f : 0f,
        };
    }

    private static float[] EncodePhase2(
        List<CardType> myHand, List<CardType> oppHand,
        int myScore, int oppScore, int myLoseStreak,
        bool myUsedSkillBefore, SkillType? myUsedSkill, SkillType? oppUsedSkill,
        bool oppUsedSkillBefore)
    {
        return new float[] {
            HandTernary3(myHand),
            HandTernary3(oppHand),
            ClampScore(myScore),
            ClampScore(oppScore),
            Mathf.Min(myLoseStreak, 2),
            SkillBeforeCode(myUsedSkillBefore, myUsedSkill),
            SkillBeforeCode(oppUsedSkillBefore, oppUsedSkill),
        };
    }

    private static float[] EncodePhase3(
        List<CardType> myHand, List<CardType> oppHand,
        int myScore, int oppScore, int myLoseStreak,
        int myCard, int oppCard,
        bool myUsedSkillBefore, SkillType? myUsedSkill, SkillType? oppUsedSkill,
        bool oppUsedSkillBefore,
        List<SkillType> mySkills)
    {
        int has5 = mySkills.Contains(SkillType.LastMinuteChange) ? 1 : 0;
        int has7 = mySkills.Contains(SkillType.PolarityReversal) ? 1 : 0;

        return new float[] {
            HandTernary2(myHand),
            HandTernary2(oppHand),
            ClampScore(myScore),
            ClampScore(oppScore),
            Mathf.Min(myLoseStreak, 2),
            myCard,
            oppCard,
            SkillBeforeCode(myUsedSkillBefore, myUsedSkill),
            SkillBeforeCode(oppUsedSkillBefore, oppUsedSkill),
            has5, has7,
        };
    }

    private static float[] EncodePhase4(
        List<CardType> myHand, List<CardType> oppHand,
        CardType[] pool, List<SkillType> mySkills, int oppDrawn)
    {
        int[] slots = EncodeSkillSlotsFull(mySkills);
        return new float[] {
            HandTernary2(myHand),
            HandTernary2(oppHand),
            PoolTernary(pool),
            slots[0], slots[1], slots[2],
            oppDrawn + 1f,
        };
    }

    // ==================== Ternary Encodings ====================

    private static int HandTernary3(List<CardType> hand)
    {
        List<int> pos = ExpandHand(hand);
        while (pos.Count < 3)
            pos.Add(pos.Count > 0 ? pos[pos.Count - 1] : 0);
        return pos[0] + pos[1] * 3 + pos[2] * 9;
    }

    private static int HandTernary2(List<CardType> hand)
    {
        List<int> pos = ExpandHand(hand);
        while (pos.Count < 2)
            pos.Add(pos.Count > 0 ? pos[pos.Count - 1] : 0);
        return pos[0] + pos[1] * 3;
    }

    private static List<int> ExpandHand(List<CardType> hand)
    {
        // Must iterate in fixed ROCK→SCISSORS→PAPER order, matching Python
        int countRock = 0, countScissors = 0, countPaper = 0;
        foreach (CardType c in hand)
        {
            if (c == CardType.Rock) countRock++;
            else if (c == CardType.Scissors) countScissors++;
            else if (c == CardType.Paper) countPaper++;
        }
        List<int> result = new List<int>();
        for (int i = 0; i < countRock; i++) result.Add(0);
        for (int i = 0; i < countScissors; i++) result.Add(1);
        for (int i = 0; i < countPaper; i++) result.Add(2);
        return result;
    }

    private static int PoolTernary(CardType[] pool)
    {
        int n = pool.Length;
        if (n >= 3) return (int)pool[0] + (int)pool[1] * 3 + (int)pool[2] * 9;
        if (n == 2) return (int)pool[0] + (int)pool[1] * 3 + (int)pool[0] * 9;
        if (n == 1) return (int)(pool[0]) * (1 + 3 + 9);
        return 0;
    }

    // ==================== Skill Encoding ====================

    private static int[] EncodeSkillSlotsPhase1(List<SkillType> skills)
    {
        int[] slots = { 0, 0, 0 };
        int idx = 0;
        foreach (SkillType s in skills)
        {
            if (SkillData.IsPhaseA(s) && idx < 3)
            {
                slots[idx] = (int)s;
                idx++;
            }
        }
        return slots;
    }

    private static int[] EncodeSkillSlotsFull(List<SkillType> skills)
    {
        int[] slots = { 0, 0, 0 };
        int idx = 0;
        foreach (SkillType s in skills)
        {
            if (idx >= 3) break;
            slots[idx] = s switch
            {
                SkillType.StonePower => 1,
                SkillType.ClothGuard => 2,
                SkillType.ScissorEdge => 3,
                SkillType.LastMinuteChange => 4,
                SkillType.PolarityReversal => 5,
                _ => 0
            };
            idx++;
        }
        return slots;
    }

    private static int SkillBeforeCode(bool used, SkillType? skill)
    {
        if (!used || !skill.HasValue) return 0;
        if (SkillData.IsPhaseA(skill.Value))
            return (int)skill.Value; // 1, 2, or 3
        return 0;
    }

    private static int ClampScore(int s) => Mathf.Clamp(s, 0, 5);

    // ==================== Action Selection ====================

    private int[] GetAvailablePhase1Actions(List<SkillType> available)
    {
        List<int> actions = new List<int> { 0 }; // skip always available
        foreach (SkillType s in available)
        {
            if (s == SkillType.StonePower) actions.Add(1);
            else if (s == SkillType.ClothGuard) actions.Add(2);
            else if (s == SkillType.ScissorEdge) actions.Add(3);
        }
        return actions.ToArray();
    }

    private int[] GetAvailablePhase2Actions(int[] cardsAvailable)
    {
        List<int> actions = new List<int>();
        foreach (int c in cardsAvailable)
            if (c == 0 || c == 1 || c == 2) // Rock, Scissors, Paper
                actions.Add(c);
        return actions.ToArray();
    }

    private int[] GetAvailablePhase3Actions(List<SkillType> available)
    {
        List<int> actions = new List<int> { 0 };
        if (available.Contains(SkillType.LastMinuteChange)) actions.Add(1);
        if (available.Contains(SkillType.PolarityReversal)) actions.Add(2);
        return actions.ToArray();
    }

    private int[] GetAvailablePhase4Actions(CardType[] pool)
    {
        List<int> actions = new List<int>();
        for (int i = 0; i < pool.Length; i++)
            if (pool[i] != CardType.None)
                actions.Add(i);
        return actions.ToArray();
    }

    private int SelectBestAction(float[] qValues, int[] availableActions)
    {
        if (availableActions.Length == 0) return 0;
        int best = availableActions[0];
        float bestQ = qValues.Length > best ? qValues[best] : float.MinValue;
        foreach (int a in availableActions)
        {
            if (a < qValues.Length && qValues[a] > bestQ)
            {
                bestQ = qValues[a];
                best = a;
            }
        }
        return best;
    }
}
