using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverObject : MonoBehaviour
{
    private GenHand riverHand;
    private PokerStateMachine pokerManager;

    private void Awake()
    {
        pokerManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<PokerStateMachine>();
    }

    public void SetHand(GenHand _River)
    {
        riverHand = _River;
    }

    public void CreateCards()
    {
        ClearCards();
        List<Card> cardsInHand = riverHand.CardHand;

        foreach (Card c in cardsInHand)
        {
            GameObject spawnedCard = Instantiate(pokerManager.cardPrefab);
            spawnedCard.transform.parent = this.transform;
            spawnedCard.GetComponent<CardObject>().SetCard(c);
        }
    }

    public void ClearCards()
    {
        CardObject[] cards = GetComponentsInChildren<CardObject>();
        foreach (CardObject c in cards)
        {
            Destroy(c.gameObject);
        }
    }
}
