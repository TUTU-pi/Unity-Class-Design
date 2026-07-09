using System.Collections.Generic;
using UnityEngine;

public class HandDisplay : MonoBehaviour
{
    [SerializeField] private Card[] cardSlots;

    private void Start()
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] == null) continue;
            int index = i;
            cardSlots[i].OnClicked = (card) => OnCardClicked(index, card);
        }
    }

    public void UpdateHand(List<CardType> hand)
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] == null) continue;
            if (i < hand.Count)
            {
                cardSlots[i].SetCardType(hand[i]);
                cardSlots[i].gameObject.SetActive(true);
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetInteractable(bool interactable)
    {
        foreach (var slot in cardSlots)
            if (slot != null) slot.IsInteractable = interactable;
    }

    private void OnCardClicked(int index, Card card)
    {
        if (!card.IsInteractable) return;
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerPlayCard(card.Type);
    }
}
