using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillHandDisplay : MonoBehaviour
{
    [SerializeField] private SkillCardDisplay[] skillSlots;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Text phaseText;
    [SerializeField] private GameObject noSkillHint;

    public System.Action<SkillType> OnSkillUsed;
    public System.Action OnSkipped;

    private void Start()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == null) continue;
            int index = i;
            skillSlots[i].OnClicked = (card) => OnSkillClicked(index, card);
        }

        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);

        if (skillPanel != null)
            skillPanel.SetActive(false);
    }

    public void ShowPhaseA(List<SkillType> skills, bool hasUsable)
    {
        if (phaseText != null)
            phaseText.text = "阶段A：出牌前使用技能";
        ShowSkills(skills, true, hasUsable);
    }

    public void ShowPhaseB(List<SkillType> skills, bool hasUsable)
    {
        if (phaseText != null)
            phaseText.text = "阶段B：出牌后使用技能";
        ShowSkills(skills, false, hasUsable);
    }

    private void ShowSkills(List<SkillType> skills, bool phaseA, bool hasUsable)
    {
        if (skillPanel != null)
            skillPanel.SetActive(true);

        List<SkillType> usable = skills.FindAll(s =>
            phaseA ? SkillData.IsPhaseA(s) : SkillData.IsPhaseB(s));

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == null) continue;
            if (i < usable.Count)
            {
                skillSlots[i].SetSkillType(usable[i]);
                skillSlots[i].IsInteractable = hasUsable;
                skillSlots[i].gameObject.SetActive(true);
            }
            else
            {
                skillSlots[i].gameObject.SetActive(false);
            }
        }

        if (noSkillHint != null)
            noSkillHint.SetActive(!hasUsable);

        if (skipButton != null)
            skipButton.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (skillPanel != null)
            skillPanel.SetActive(false);
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
    }

    public void SetInteractable(bool interactable)
    {
        foreach (var slot in skillSlots)
        {
            if (slot != null && slot.gameObject.activeSelf)
                slot.IsInteractable = interactable;
        }
        if (skipButton != null)
            skipButton.interactable = interactable;
    }

    private void OnSkillClicked(int index, SkillCardDisplay card)
    {
        if (!card.IsInteractable) return;
        SetInteractable(false);
        OnSkillUsed?.Invoke(card.Type);
    }

    private void OnSkipClicked()
    {
        SetInteractable(false);
        OnSkipped?.Invoke();
    }
}
