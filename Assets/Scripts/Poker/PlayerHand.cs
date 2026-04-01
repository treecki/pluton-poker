using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerHand : GenHand
{
    protected List<Card> dealtHand;
    public List<Card> DealtHand { get { return dealtHand; } }

    protected HandInfo finalHandInfo;
    public HandInfo FinalHandInfo { get { return finalHandInfo; } }

    protected List<HandInfo> potentialHandList;

    public PlayerHand() : base()
    {
        potentialHandList = new List<HandInfo>();
        finalHandInfo = new HandInfo();
        dealtHand = new List<Card>();
        cardHand = new List<Card>();
    }

    public override void DealCard(Card dealtCard)
    {
        dealtHand.Add(dealtCard);      
    }

    public override void ClearHand()
    {
        base.ClearHand();
        dealtHand.Clear();
    }

    public Hand EvaluateHand(GenHand RiverHand)
    {
        Hand currHand = Hand.NULL;

        cardHand.Clear();
        sortedValueHand.Clear();
        sortedFlushHand.Clear();
        suitDict.Clear();
        valueDict.Clear();
        potentialHandList.Clear();

        foreach (Card c in RiverHand.CardHand)
        {
            cardHand.Add(c);
        }

        foreach (Card c in dealtHand)
        {
            cardHand.Add(c);
        }

        SortHand();

        if (sortedValueHand.Count != 7)
        {
            Debug.LogError("count not 7 for hand it is" + sortedValueHand.Count);
            return currHand;
        }

        SetHighCard(ref currHand);

        GetNumberOfAllSuits();
        GetNumberOfAllValues();

        SetPairs(ref currHand);
        SetFlush(ref currHand);
        SetStraight(ref currHand);
        SetStraightFlush(ref currHand);
        SetRoyalFlush(ref currHand);

        finalHandInfo = GetHighestPotentialHand(currHand);

        Debug.Log("Hand is " + currHand);
        return currHand;
    }

    protected HandInfo GetHighestPotentialHand(Hand highestHand)
    {
        Debug.Log("Requesting highest hand of: " + highestHand);
        List<HandInfo> listHands = GetHandInPotentialHands(highestHand);
        Debug.Log("List Hands length: " + listHands.Count);
        HandInfo highestHandInfo = listHands[0];

        if (listHands.Count == 1) { return listHands[0]; }

        for (int i = 0; i < listHands.Count; i++)
        {
            if (highestHandInfo.Total < listHands[i].Total)
            {
                highestHandInfo = listHands[i];
            }
            else if (highestHandInfo.Total == listHands[i].Total)
            {
                if (highestHandInfo.HighCard < listHands[i].HighCard)
                {
                    highestHandInfo = listHands[i];
                }
                else if (highestHandInfo.HighCard == listHands[i].HighCard)
                {
                    highestHandInfo = listHands[i];
                }
            }            
        }

        return highestHandInfo;
    }

    protected int[] Get5CardIndexesInOrder(int firstIndex)
    {
        int[] cardIndexes = new int[5];
        int index = 0;
        for (int i = firstIndex; i < firstIndex + 5; i++)
        {
            cardIndexes[index] = i;
            index++;
        }
        return cardIndexes;
    }

    protected void SetHighCard(ref Hand currHand)
    {
        int lastIndex = sortedValueHand.Count - 1;
        int firstIndex = lastIndex - 4;

        int highCard = (int)sortedValueHand[lastIndex].MyValue;
        int total = (int)sortedValueHand[lastIndex].MyValue;

        int[] cardIndexes = Get5CardIndexesInOrder(firstIndex);

        potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.HighCard, total, highCard));
        currHand = Hand.HighCard;
    }

    protected void SetFlush(ref Hand currHand)
    {
        foreach (SUIT s in suitDict.Keys)
        {
            if (suitDict[s] >= 5)
            {
                if (currHand < Hand.Flush)
                {
                    currHand = Hand.Flush;
                }
                Flush(s);
            }
        }
    }

    protected void SetStraight(ref Hand currHand)
    {
        List<int> distinctValues = sortedValueHand
            .Select(card => (int)card.MyValue)
            .Distinct()
            .OrderBy(value => value)
            .ToList();

        if (distinctValues.Contains((int)VALUE.VA))
        {
            distinctValues.Insert(0, 1);
        }

        for (int i = 0; i <= distinctValues.Count - 5; i++)
        {
            bool isStraight = true;
            for (int j = 1; j < 5; j++)
            {
                if (distinctValues[i + j] != distinctValues[i] + j)
                {
                    isStraight = false;
                    break;
                }
            }

            if (!isStraight)
            {
                continue;
            }

            List<int> straightValues = distinctValues.Skip(i).Take(5).ToList();
            int[] cardIndexes = GetCardIndexesForValues(straightValues);
            int highCard = straightValues.Last() == 1 ? 5 : straightValues.Last();
            int total = highCard;

            potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.Straight, total, highCard));

            if (currHand < Hand.Straight)
            {
                currHand = Hand.Straight;
            }
        }
    }

    protected void SetPairs(ref Hand currHand)
    {
        int twoCard = 0;
        int threeCard = 0;
        int fourCard = 0;

        foreach (VALUE v in valueDict.Keys)
        {
            switch (valueDict[v])
            {
                case 2:
                    twoCard++;
                    break;
                case 3:
                    threeCard++;
                    break;
                case 4:
                    fourCard++;
                    break;
            }
        }

        if (twoCard > 0)
        {
            if (twoCard > 1)
            {
                currHand = Hand.TwoPair;
                TwoPairs();
            }
            else
            {
                currHand = Hand.OnePair;
                OnePair();
            }

            if (threeCard > 1)
            {
                currHand = Hand.FullHouse;
                FullHouse();
            }
        }
        if (threeCard > 0 && currHand < Hand.ThreeKind)
        {
            currHand = Hand.ThreeKind;
            ThreeOfKind();
        }
        if (fourCard > 0)
        {
            currHand = Hand.FourKind;
            FourOfKind();
        }
    }

    protected void GetNumberOfAllValues()
    {
        foreach (Card c in sortedValueHand)
        {
            if (valueDict.ContainsKey(c.MyValue))
            {
                valueDict[c.MyValue] += 1;
            }
            else
            {
                valueDict.Add(c.MyValue, 1);
            }
        }
    }

    protected void GetNumberOfAllSuits()
    {
        foreach (Card c in sortedValueHand)
        {
            if (suitDict.ContainsKey(c.MySuit))
            {
                suitDict[c.MySuit] += 1;
            }
            else
            {
                suitDict.Add(c.MySuit, 1);
            }
        }

    }

    protected void SetRoyalFlush(ref Hand currHand)
    {

        List<HandInfo> FlushHandList = GetHandInPotentialHands(Hand.Flush);
        List<HandInfo> StraightHandList = GetHandInPotentialHands(Hand.Straight);

        if (FlushHandList.Count > 0 && StraightHandList.Count > 0)
        {
            foreach (HandInfo hInfo in StraightHandList)
            {
                if (hInfo.HighCard != (int)VALUE.VA) { continue; }

                List<Card> tempHand = new List<Card>();

                foreach (int i in hInfo.cardIndexes)
                {
                    tempHand.Add(sortedValueHand[i]);
                }

                var querySuit = from card in tempHand orderby card.MySuit select card;

                tempHand = querySuit.ToList();

                if (tempHand[0].MySuit == tempHand[tempHand.Count - 1].MySuit)
                {
                    HandInfo RoyalFlush = CreateHandInfo(hInfo.cardIndexes, Hand.RoyalFlush, hInfo.Total, hInfo.HighCard);
                    potentialHandList.Add(RoyalFlush);
                    currHand = Hand.RoyalFlush;
                }
            }
        }
    }

    protected void SetStraightFlush(ref Hand currHand)
    {
        List<HandInfo> FlushHandList = GetHandInPotentialHands(Hand.Flush);
        List<HandInfo> StraightHandList = GetHandInPotentialHands(Hand.Straight);

        if (FlushHandList.Count > 0 && StraightHandList.Count > 0)
        {
            foreach (HandInfo hInfo in StraightHandList)
            {
                List<Card> tempHand = new List<Card>();

                foreach (int i in hInfo.cardIndexes)
                {
                    tempHand.Add(sortedValueHand[i]);
                }

                var querySuit = from card in tempHand orderby card.MySuit select card;

                tempHand = querySuit.ToList();

                if (tempHand[0].MySuit == tempHand[tempHand.Count - 1].MySuit)
                {
                    HandInfo StraightFlush = CreateHandInfo(hInfo.cardIndexes, Hand.StraightFlush, hInfo.Total, hInfo.HighCard);
                    potentialHandList.Add(StraightFlush);
                    currHand = Hand.StraightFlush;
                }
            }
        }
    }

    protected List<HandInfo> GetHandInPotentialHands(Hand hand)
    {
        List<HandInfo> HandInfoList = new List<HandInfo>();

        foreach (HandInfo h in potentialHandList)
        {
            if (h.HandCombo == hand)
            {
                HandInfoList.Add(h);
            }
        }


        return HandInfoList;
    }

    protected void FourOfKind()
    {
        int[] cardIndexes = new int[5];
        int highCard = 0;
        int index = 0;
        for (int i = 0; i < sortedValueHand.Count - 3; i++)
        {
            if (sortedValueHand[i].MyValue == sortedValueHand[i + 3].MyValue)
            {
                cardIndexes[index++] = i;
                cardIndexes[index++] = i + 1;
                cardIndexes[index++] = i + 2;
                cardIndexes[index++] = i + 3;

                int kickerIndex = -1;
                for (int j = sortedValueHand.Count - 1; j >= 0; j--)
                {
                    if (j < i || j > i + 3)
                    {
                        kickerIndex = j;
                        break;
                    }
                }

                if (kickerIndex >= 0)
                {
                    cardIndexes[index] = kickerIndex;
                    highCard = (int)sortedValueHand[kickerIndex].MyValue;
                }

                break;
            }
        }

        int total = (int)sortedValueHand[cardIndexes[0]].MyValue * 4;

        potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.FourKind, total, highCard));
    }

    protected void FullHouse()
    {
        int[] cardIndexes = new int[5];
        int highCard = 0;
        int index = 0;
        int total = 0;

        for (int i = sortedValueHand.Count - 1; i >= 0 ; i--)
        {
            if (index > 4)
            {
                break;
            }

            if (i - 2 >= 0)
            {
                if (sortedValueHand[i] == sortedValueHand[i - 2])
                {
                    highCard = (int)sortedValueHand[i].MyValue;
                    total += (int)sortedValueHand[i].MyValue * 3;

                    cardIndexes[index] = i;
                    index++;
                    cardIndexes[index] = i - 1;
                    index++;
                    cardIndexes[index] = i - 2;
                    index++;

                    i -= 2;
                }
            }
            else if (i - 1 >= 0)
            {
                if (sortedValueHand[i] == sortedValueHand[i - 1] && index != 2)
                {
                    total += (int)sortedValueHand[i].MyValue * 2;

                    cardIndexes[index] = i;
                    index++;
                    cardIndexes[index] = i - 1;
                    index++;

                    i--;
                }
            }
        }

        potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.FullHouse, total, highCard));
    }

    protected void Flush(SUIT suit)
    {
        List<int> suitedIndexes = new List<int>();

        for (int i = 0; i < sortedValueHand.Count; i++)
        {
            if (sortedValueHand[i].MySuit == suit)
            {
                suitedIndexes.Add(i);
            }
        }

        suitedIndexes = suitedIndexes
            .OrderBy(index => sortedValueHand[index].MyValue)
            .ToList();

        if (suitedIndexes.Count < 5)
        {
            return;
        }

        int[] cardIndexes = suitedIndexes.Skip(suitedIndexes.Count - 5).ToArray();
        int highCard = (int)sortedValueHand[cardIndexes[cardIndexes.Length - 1]].MyValue;
        int total = highCard;

        potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.Flush, total, highCard));
    }

    protected void ThreeOfKind()
    {
        int[] cardIndexes = new int[5];
        int highCard = 0;
        int index = 0;
        for (int i = 0; i < sortedValueHand.Count - 1; i++)
        {
            if (index > 2 && index < 5)
            {
                if (i + 2 >= sortedValueHand.Count)
                {
                    cardIndexes[index] = i;
                    index++;
                }
            }
            if (i + 2 < sortedValueHand.Count)
            {
                if (sortedValueHand[i].MyValue == sortedValueHand[i + 2].MyValue)
                {
                    cardIndexes[index] = i;
                    index++;
                    cardIndexes[index] = i + 1;
                    index++;
                    cardIndexes[index] = i + 2;
                    index++;

                    if (i + 2 == sortedValueHand.Count - 1)
                    {
                        highCard = (int)sortedValueHand[i - 1].MyValue;
                    }
                    else
                    {
                        highCard = (int)sortedValueHand[sortedValueHand.Count - 1].MyValue;
                    }

                    i += 2;
                }
            }            
        }

        int j = 1;
        while (index < 5)
        {
            cardIndexes[index] = cardIndexes[0] - j;
            j++;
            index++;
        }


        int total = (int)sortedValueHand[cardIndexes[0]].MyValue * 3;

        potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.ThreeKind, total, highCard));
    }

    protected void TwoPairs()
    {
        int[] cardIndexes = new int[5];
        int index = 0;
        int highCardIndex = -1;
        for (int i = sortedValueHand.Count - 1; i > -1; i--)
        {
            if (i-1 >= 0)
            {
                if (sortedValueHand[i] == sortedValueHand[i-1])
                {
                    cardIndexes[index] = i;
                    index++;
                    cardIndexes[index] = i - 1;
                    index++;
                    i--;
                }
                else if(highCardIndex < 0 || sortedValueHand[highCardIndex].MyValue < sortedValueHand[i].MyValue)
                {
                    highCardIndex = i;
                }
            }
        }

        cardIndexes[4] = highCardIndex;
        int total = (int)sortedValueHand[cardIndexes[0]].MyValue * 2;

        int highCard = (int)sortedValueHand[highCardIndex].MyValue;

        potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.TwoPair, total, highCard));
    }

    protected void OnePair()
    {

        int[] cardIndexes = new int[5];
        int highCard = 0;
        int index = 0;
        for (int i = 0; i < sortedValueHand.Count - 1; i++)
        {
            if (index > 1 && index < 5)
            {
                if (i + 3 >= sortedValueHand.Count)
                {
                    cardIndexes[index] = i;
                    index++;
                }
            }

            if (i+1 < sortedValueHand.Count)
            {
                if (sortedValueHand[i].MyValue == sortedValueHand[i + 1].MyValue)
                {
                    cardIndexes[index] = i;
                    index++;
                    cardIndexes[index] = i + 1;
                    index++;
                    i++;

                    if (i + 1 == sortedValueHand.Count - 1)
                    {
                        highCard = (int)sortedValueHand[i - 1].MyValue;
                    }
                    else
                    {
                        highCard = (int)sortedValueHand[sortedValueHand.Count - 1].MyValue;
                    }
                }
            }

        }

        int j = 1;
        while (index < 5)
        {
            cardIndexes[index] = cardIndexes[0] - j;
            j++;
            index++;
        }


        int total = (int)sortedValueHand[cardIndexes[0]].MyValue * 2;

        potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.OnePair, total, highCard));

    }

    protected int[] GetCardIndexesForValues(List<int> values)
    {
        int[] cardIndexes = new int[values.Count];
        List<int> remainingIndexes = Enumerable.Range(0, sortedValueHand.Count).ToList();

        for (int i = values.Count - 1; i >= 0; i--)
        {
            int targetValue = values[i] == 1 ? (int)VALUE.VA : values[i];
            int cardIndex = remainingIndexes.Last(index => (int)sortedValueHand[index].MyValue == targetValue);
            cardIndexes[i] = cardIndex;
            remainingIndexes.Remove(cardIndex);
        }

        return cardIndexes;
    }

    public HandInfo CreateHandInfo(int[] cardIndexes, Hand handCombo, int total, int highCard)
    {
        HandInfo newHandInfo = new HandInfo();

        newHandInfo.cardIndexes = cardIndexes;
        newHandInfo.HandCombo = handCombo;
        newHandInfo.Total = total;
        newHandInfo.HighCard = highCard;
        return newHandInfo;
    }
}

public struct HandInfo
{
    public int[] cardIndexes { get; set; }
    public Hand HandCombo { get; set; }
    public int Total { get; set; }
    public int HighCard { get; set; }
}

public enum Hand { NULL, HighCard, OnePair, TwoPair, ThreeKind, Straight, Flush, FullHouse, FourKind, StraightFlush, RoyalFlush }
