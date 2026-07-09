using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillBar : MonoBehaviour
{
    [SerializeField] private Text[] skillNameTexts;

    private void Awake()
    {
        if (skillNameTexts == null || skillNameTexts.Length == 0)
            skillNameTexts = GetComponentsInChildren<Text>();
    }

    public void UpdateSkills(List<SkillType> skills)
    {
        if (skillNameTexts == null) return;

        for (int i = 0; i < skillNameTexts.Length; i++)
        {
            if (skillNameTexts[i] == null) continue;
            if (i < skills.Count)
            {
                skillNameTexts[i].text = SkillData.GetName(skills[i]);
                skillNameTexts[i].gameObject.SetActive(true);
            }
            else
            {
                skillNameTexts[i].text = "";
                skillNameTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
