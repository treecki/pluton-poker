using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GenHand
{
    protected List<Card> cardHand;
    public List<Card> CardHand { get { return cardHand; } }

    protected List<Card> sortedValueHand;
    public List<Card> SortedValueHand { get { return sortedValueHand; } }

    protected List<Card> sortedFlushHand;
    public List<Card> SortedFlushHand { get { return sortedFlushHand; } }

    protected Dictionary<SUIT, int> suitDict;
    protected Dictionary<VALUE, int> valueDict;


    public GenHand()
    {
        cardHand = new List<Card>();
        sortedValueHand = new List<Card>();
        sortedFlushHand = new List<Card>();
        suitDict = new Dictionary<SUIT, int>();
        valueDict = new Dictionary<VALUE, int>();
    }

    public virtual void DealCard(Card dealtCard)
    {
        cardHand.Add(dealtCard);
        SortHand();
    }

    protected void SortHand()
    {
        var queryValue = from card in cardHand orderby card.MyValue select card;
        var querySuit  = from card in cardHand orderby card.MySuit select card;

        sortedValueHand.Clear();
        sortedValueHand = queryValue.ToList();

        sortedFlushHand.Clear();
        sortedFlushHand = querySuit.ToList();
    }

    public virtual void ClearHand()
    {
        cardHand.Clear();
        sortedValueHand.Clear();
        sortedFlushHand.Clear();
    }
}
