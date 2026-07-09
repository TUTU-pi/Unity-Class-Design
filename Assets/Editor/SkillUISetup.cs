using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SkillUISetup
{
    [MenuItem("Tools/Setup Skill Card UI")]
    public static void SetupSkillUI()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Scene has no Canvas! Make sure a Canvas exists.");
            return;
        }

        Transform existingPanel = canvas.transform.Find("SkillPanel");

        if (existingPanel == null)
        {
            CreateSkillPanel(canvas);
        }
        else
        {
            Debug.Log("SkillPanel already exists, rewiring references...");
            RewireSkillPanel(existingPanel.gameObject);
        }

        CreateSkillCountText(canvas);

        Debug.Log("Skill Card UI setup complete!");
    }

    private static void CreateSkillPanel(Canvas canvas)
    {
        GameObject skillPanel = new GameObject("SkillPanel", typeof(RectTransform));
        skillPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRt = skillPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0f);
        panelRt.anchorMax = new Vector2(0.5f, 0f);
        panelRt.pivot = new Vector2(0.5f, 0f);
        panelRt.anchoredPosition = new Vector2(0, 180);
        panelRt.sizeDelta = new Vector2(540, 180);

        Image panelBg = skillPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

        // Phase Text
        GameObject phaseTextObj = new GameObject("PhaseText", typeof(RectTransform));
        phaseTextObj.transform.SetParent(skillPanel.transform, false);
        RectTransform phaseRt = phaseTextObj.GetComponent<RectTransform>();
        phaseRt.anchorMin = new Vector2(0.5f, 1f);
        phaseRt.anchorMax = new Vector2(0.5f, 1f);
        phaseRt.pivot = new Vector2(0.5f, 1f);
        phaseRt.anchoredPosition = new Vector2(0, -5);
        phaseRt.sizeDelta = new Vector2(300, 30);
        Text phaseText = phaseTextObj.AddComponent<Text>();
        phaseText.fontSize = 18;
        phaseText.alignment = TextAnchor.MiddleCenter;
        phaseText.color = Color.white;
        phaseText.text = "";

        // No-skill hint (shown when no usable skills)
        GameObject noSkillHintObj = new GameObject("NoSkillHint", typeof(RectTransform));
        noSkillHintObj.transform.SetParent(skillPanel.transform, false);
        RectTransform hintRt = noSkillHintObj.GetComponent<RectTransform>();
        hintRt.anchorMin = new Vector2(0.5f, 0.5f);
        hintRt.anchorMax = new Vector2(0.5f, 0.5f);
        hintRt.pivot = new Vector2(0.5f, 0.5f);
        hintRt.anchoredPosition = new Vector2(0, 10);
        hintRt.sizeDelta = new Vector2(300, 30);
        Text hintText = noSkillHintObj.AddComponent<Text>();
        hintText.fontSize = 16;
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.color = new Color(0.6f, 0.6f, 0.6f);
        hintText.text = "当前阶段无可用的技能牌";
        noSkillHintObj.SetActive(false);

        // Skill slots container
        GameObject slotsContainer = new GameObject("SkillSlots", typeof(RectTransform));
        slotsContainer.transform.SetParent(skillPanel.transform, false);
        RectTransform slotsRt = slotsContainer.GetComponent<RectTransform>();
        slotsRt.anchorMin = new Vector2(0.5f, 0.5f);
        slotsRt.anchorMax = new Vector2(0.5f, 0.5f);
        slotsRt.pivot = new Vector2(0.5f, 0.5f);
        slotsRt.anchoredPosition = new Vector2(0, 10);
        slotsRt.sizeDelta = new Vector2(480, 110);

        HorizontalLayoutGroup layout = slotsContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        SkillCardDisplay[] skillSlots = new SkillCardDisplay[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject slot = CreateSkillCardSlot("SkillSlot_" + i, slotsContainer.transform);
            skillSlots[i] = slot.GetComponent<SkillCardDisplay>();
        }

        // Skip Button
        GameObject skipBtnObj = new GameObject("SkipButton", typeof(RectTransform));
        skipBtnObj.transform.SetParent(skillPanel.transform, false);
        RectTransform skipRt = skipBtnObj.GetComponent<RectTransform>();
        skipRt.anchorMin = new Vector2(0.5f, 0f);
        skipRt.anchorMax = new Vector2(0.5f, 0f);
        skipRt.pivot = new Vector2(0.5f, 0f);
        skipRt.anchoredPosition = new Vector2(0, 8);
        skipRt.sizeDelta = new Vector2(100, 32);

        Image skipImg = skipBtnObj.AddComponent<Image>();
        skipImg.color = new Color(0.4f, 0.4f, 0.5f);
        Button skipBtn = skipBtnObj.AddComponent<Button>();

        GameObject skipTextObj = new GameObject("Text", typeof(RectTransform));
        skipTextObj.transform.SetParent(skipBtnObj.transform, false);
        RectTransform skipTextRt = skipTextObj.GetComponent<RectTransform>();
        skipTextRt.anchorMin = Vector2.zero;
        skipTextRt.anchorMax = Vector2.one;
        skipTextRt.sizeDelta = Vector2.zero;
        Text skipText = skipTextObj.AddComponent<Text>();
        skipText.fontSize = 16;
        skipText.alignment = TextAnchor.MiddleCenter;
        skipText.color = Color.white;
        skipText.text = "跳过";

        // SkillHandDisplay component
        SkillHandDisplay shd = skillPanel.AddComponent<SkillHandDisplay>();
        SetSerializedField(shd, "skillSlots", skillSlots);
        SetSerializedField(shd, "skipButton", skipBtn);
        SetSerializedField(shd, "skillPanel", skillPanel);
        SetSerializedField(shd, "phaseText", phaseText);
        SetSerializedField(shd, "noSkillHint", noSkillHintObj);

        WireGameManagerSkillDisplay(shd);

        skillPanel.SetActive(false);
        Selection.activeGameObject = skillPanel;
    }

    private static void RewireSkillPanel(GameObject skillPanel)
    {
        SkillHandDisplay shd = skillPanel.GetComponent<SkillHandDisplay>();
        if (shd == null)
        {
            shd = skillPanel.AddComponent<SkillHandDisplay>();
        }

        // Gather existing skill slots
        Transform slotsContainer = skillPanel.transform.Find("SkillSlots");
        SkillCardDisplay[] slots = new SkillCardDisplay[3];
        if (slotsContainer != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Transform slot = slotsContainer.Find("SkillSlot_" + i);
                if (slot != null)
                    slots[i] = slot.GetComponent<SkillCardDisplay>();
            }
        }

        Button skipBtn = null;
        Transform skipT = skillPanel.transform.Find("SkipButton");
        if (skipT != null) skipBtn = skipT.GetComponent<Button>();

        Text phaseText = null;
        Transform phaseT = skillPanel.transform.Find("PhaseText");
        if (phaseT != null) phaseText = phaseT.GetComponent<Text>();

        SetSerializedField(shd, "skillSlots", slots);
        SetSerializedField(shd, "skipButton", skipBtn);
        SetSerializedField(shd, "skillPanel", skillPanel);
        SetSerializedField(shd, "phaseText", phaseText);

        // Create NoSkillHint if missing
        Transform hintT = skillPanel.transform.Find("NoSkillHint");
        if (hintT == null)
        {
            GameObject noSkillHint = new GameObject("NoSkillHint", typeof(RectTransform));
            noSkillHint.transform.SetParent(skillPanel.transform, false);
            RectTransform hintRt = noSkillHint.GetComponent<RectTransform>();
            hintRt.anchorMin = new Vector2(0.5f, 0.5f);
            hintRt.anchorMax = new Vector2(0.5f, 0.5f);
            hintRt.pivot = new Vector2(0.5f, 0.5f);
            hintRt.anchoredPosition = new Vector2(0, 10);
            hintRt.sizeDelta = new Vector2(300, 30);
            Text hintText = noSkillHint.AddComponent<Text>();
            hintText.fontSize = 16;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.color = new Color(0.6f, 0.6f, 0.6f);
            hintText.text = "当前阶段无可用的技能牌";
            SetSerializedField(shd, "noSkillHint", noSkillHint);
        }
        else
        {
            SetSerializedField(shd, "noSkillHint", hintT.gameObject);
        }

        WireGameManagerSkillDisplay(shd);
    }

    private static void WireGameManagerSkillDisplay(SkillHandDisplay shd)
    {
        GameManager gm = Object.FindObjectOfType<GameManager>();
        if (gm != null)
        {
            SerializedObject gmSo = new SerializedObject(gm);
            gmSo.FindProperty("skillHandDisplay").objectReferenceValue = shd;
            gmSo.ApplyModifiedProperties();
            Debug.Log("Wired SkillHandDisplay to GameManager!");
        }
        else
        {
            Debug.LogWarning("No GameManager found. Drag SkillPanel (SkillHandDisplay) into GameManager manually.");
        }
    }

    private static void CreateSkillCountText(Canvas canvas)
    {
        Transform existing = canvas.transform.Find("SkillCountText");
        if (existing != null) return;

        GameObject skillCountObj = new GameObject("SkillCountText", typeof(RectTransform));
        skillCountObj.transform.SetParent(canvas.transform, false);
        RectTransform scRt = skillCountObj.GetComponent<RectTransform>();
        scRt.anchorMin = new Vector2(1f, 1f);
        scRt.anchorMax = new Vector2(1f, 1f);
        scRt.pivot = new Vector2(1f, 1f);
        scRt.anchoredPosition = new Vector2(-20, -120);
        scRt.sizeDelta = new Vector2(350, 30);
        Text skillCountText = skillCountObj.AddComponent<Text>();
        skillCountText.fontSize = 18;
        skillCountText.alignment = TextAnchor.MiddleRight;
        skillCountText.color = new Color(1f, 0.85f, 0.3f);
        skillCountText.text = "技能牌: 0张";

        UIManager ui = Object.FindObjectOfType<UIManager>();
        if (ui != null)
        {
            SerializedObject uiSo = new SerializedObject(ui);
            uiSo.FindProperty("skillCountText").objectReferenceValue = skillCountText;
            uiSo.ApplyModifiedProperties();
            Debug.Log("SkillCountText created and wired to UIManager!");
        }
    }

    private static void SetSerializedField(Object target, string fieldName, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            if (prop.isArray)
            {
                // Array case - assign each element
            }
            else
            {
                prop.objectReferenceValue = value;
            }
        }
        so.ApplyModifiedProperties();
    }

    private static void SetSerializedField(Object target, string fieldName, Object[] values)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop != null && prop.isArray)
        {
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
        so.ApplyModifiedProperties();
    }

    private static GameObject CreateSkillCardSlot(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(140, 100);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.35f, 0.55f);

        GameObject textObj = new GameObject("Name", typeof(RectTransform));
        textObj.transform.SetParent(go.transform, false);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(4, 4);
        textRt.offsetMax = new Vector2(-4, -4);
        Text text = textObj.AddComponent<Text>();
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = "";

        SkillCardDisplay scd = go.AddComponent<SkillCardDisplay>();
        SetSerializedField(scd, "bgImage", img);
        SetSerializedField(scd, "nameText", text);

        return go;
    }
}
