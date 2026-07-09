using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum CardType
{
    Rock,
    Scissors,
    Paper,
    None
}

public class Card : MonoBehaviour, IPointerClickHandler
{
    public CardType Type { get; private set; } = CardType.None;
    public bool IsInteractable { get; set; } = true;

    [SerializeField] private GameObject highlightObj;

    private Image bgImage;
    private Text typeText;

    public System.Action<Card> OnClicked;

    private void Awake()
    {
        bgImage = GetComponent<Image>();

        Transform typeChild = transform.Find("Type");
        if (typeChild != null)
            typeText = typeChild.GetComponent<Text>();

        if (typeText == null)
            typeText = GetComponentInChildren<Text>(true);
    }

    public void SetCardType(CardType type)
    {
        Type = type;

        if (typeText != null)
        {
            typeText.text = type switch
            {
                CardType.Rock => "石头",
                CardType.Scissors => "剪刀",
                CardType.Paper => "布",
                _ => ""
            };
        }
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
