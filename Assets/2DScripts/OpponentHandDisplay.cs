using System.Collections.Generic;
using UnityEngine;

public class OpponentHandDisplay : MonoBehaviour
{
    [SerializeField] private Card[] cardSlots;

    private void Start()
    {
        foreach (var slot in cardSlots)
            if (slot != null) slot.IsInteractable = false;
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
                cardSlots[i].IsInteractable = false;
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }
    }
}
