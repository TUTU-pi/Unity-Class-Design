using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum SkillType
{
    None = 0,
    StonePower = 1,
    ClothGuard = 2,
    ScissorEdge = 3,
    LastMinuteChange = 5,
    PolarityReversal = 7
}

public static class SkillData
{
    public static string GetName(SkillType type) => type switch
    {
        SkillType.StonePower => "石之力量",
        SkillType.ClothGuard => "布之守护",
        SkillType.ScissorEdge => "剪之锋芒",
        SkillType.LastMinuteChange => "临阵换策",
        SkillType.PolarityReversal => "两极反转",
        _ => ""
    };

    public static string GetDescription(SkillType type) => type switch
    {
        SkillType.StonePower => "出石头获胜得2分",
        SkillType.ClothGuard => "出布获胜得2分",
        SkillType.ScissorEdge => "出剪刀获胜得2分",
        SkillType.LastMinuteChange => "收回已出牌，重新出牌",
        SkillType.PolarityReversal => "互换双方本轮得分",
        _ => ""
    };

    public static bool IsPhaseA(SkillType type) =>
        type == SkillType.StonePower || type == SkillType.ClothGuard || type == SkillType.ScissorEdge;

    public static bool IsPhaseB(SkillType type) =>
        type == SkillType.LastMinuteChange || type == SkillType.PolarityReversal;

    public static CardType GetBonusCardType(SkillType type) => type switch
    {
        SkillType.StonePower => CardType.Rock,
        SkillType.ClothGuard => CardType.Paper,
        SkillType.ScissorEdge => CardType.Scissors,
        _ => CardType.None
    };
}

public class SkillCardDisplay : MonoBehaviour, IPointerClickHandler
{
    public SkillType Type { get; private set; } = SkillType.None;
    public bool IsInteractable { get; set; } = true;

    [SerializeField] private GameObject highlightObj;
    [SerializeField] private Image bgImage;
    [SerializeField] private Text nameText;

    public System.Action<SkillCardDisplay> OnClicked;

    private void Awake()
    {
        if (bgImage == null)
            bgImage = GetComponent<Image>();
        if (nameText == null)
            nameText = GetComponentInChildren<Text>(true);
    }

    public void SetSkillType(SkillType type)
    {
        Type = type;
        if (nameText != null)
            nameText.text = SkillData.GetName(type);
    }

    public void SetHighlight(bool highlight)
    {
        if (highlightObj != null)
            highlightObj.SetActive(highlight);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsInteractable) return;
        OnClicked?.Invoke(this);
    }
}
