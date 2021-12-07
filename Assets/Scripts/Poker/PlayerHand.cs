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
        //cardHand = new List<Card>();

        cardHand.Clear();
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

        //get the number of each suit on hand
        GetNumberOfAllSuits();

        //get the number of each value on hand
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
                    //umm tie but fuck it?
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
            if (suitDict[s] == 5)
            {
                if (currHand < Hand.Flush)
                {
                    currHand = Hand.Flush;
                }
                Flush();
            }
        }
    }

    protected void SetStraight(ref Hand currHand)
    {
        //So if we are checking a straight with 7 cards
        //We essentially just need to go through the first 3
        for (int i = 0; i+5 < sortedValueHand.Count; i++)
        {
            bool isStraight = true;
            int initCheck = 0;
            //Lets go through 5 cards with the starting index
            for (int j = i; j < i+5; j++ )
            {
                int currValue = (int)sortedValueHand[j].MyValue;
                if (j == 0)
                {
                    initCheck = currValue;
                }
                else if (currValue != initCheck + 1)
                {
                    isStraight = false;
                }
                else
                {
                    initCheck = currValue;
                }

                //If we are at the very last index of this straight check
                //And if isStraight was never interrupted
                if (j == i+4 && isStraight)
                {
                    int firstIndex = i;
                    int lastIndex = j;
                    int total = (int)sortedValueHand[j].MyValue;
                    int highCard = (int)sortedValueHand[j].MyValue;

                    int[] cardIndexes = Get5CardIndexesInOrder(firstIndex);

                    potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.Straight, total, highCard));

                    if (currHand < Hand.Straight)
                    {
                        currHand = Hand.Straight;
                    }
                }
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
        for (int i = 0; i < sortedValueHand.Count - 1; i++)
        {

            if (i + 3 < sortedValueHand.Count)
            {
                //Do we have a 3 of a kind
                if (sortedValueHand[i].MyValue == sortedValueHand[i + 3].MyValue)
                {
                    //Put the pair in the card indexes
                    cardIndexes[index] = i;
                    index++;
                    cardIndexes[index] = i + 1;
                    index++;
                    cardIndexes[index] = i + 2;
                    index++;
                    cardIndexes[index] = i + 3;
                    index++;

                    //Lets get the highest card
                    if (i + 2 == sortedValueHand.Count - 1)
                    {
                        highCard = (int)sortedValueHand[i - 1].MyValue;
                        cardIndexes[index] = i - 1;
                    }
                    else
                    {
                        highCard = (int)sortedValueHand[sortedValueHand.Count - 1].MyValue;
                        cardIndexes[sortedValueHand.Count - 1] = i - 1;
                    }

                    break;
                }
            }
        }


        int total = (int)sortedValueHand[cardIndexes[0]].MyValue * 4;

        potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.ThreeKind, total, highCard));
    }

    protected void FullHouse()
    {
        int[] cardIndexes = new int[5];
        int highCard = 0;
        int index = 0;
        int total = 0;

        //Lets go through all the cards from the top
        //This way if we have 2 pairs and a 3 of a kind
        //It'll automatically grab the highest pair
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

                    //Increment past the 3
                    i -= 2;
                }
            }
            else if (i - 1 >= 0)
            {
                //We need to check if we have already found the first pair
                //If we have then we have to ignore second by checking the index doesn't equal 2
                if (sortedValueHand[i] == sortedValueHand[i - 1] && index != 2)
                {
                    total += (int)sortedValueHand[i].MyValue * 2;

                    cardIndexes[index] = i;
                    index++;
                    cardIndexes[index] = i - 1;
                    index++;

                    //Increment past the pair
                    i--;
                }
            }
        }

        potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.FullHouse, total, highCard));
    }

    protected void Flush()
    {
        int firstIndex = 0;

        for (int i = 0; i+5 < sortedFlushHand.Count; i++)
        {
            if (sortedFlushHand[i].MySuit == sortedFlushHand[i+5].MySuit)
            {
                if (firstIndex < i) firstIndex = 0;
            }
        }

        int lastIndex = firstIndex + 4;
        int total = 0;
        int highCard = 0;

        for (int i = sortedValueHand.Count - 1; i > -1; i--)
        {
            if(SortedValueHand[i].MySuit == sortedFlushHand[firstIndex].MySuit)
            {
                total = (int)SortedValueHand[i].MyValue;
                highCard = (int)SortedValueHand[i].MyValue;
                break;
            }
        }

        int[] cardIndexes = Get5CardIndexesInOrder(firstIndex);

        potentialHandList.Add(CreateHandInfo(cardIndexes, Hand.Flush, total, highCard));
    }

    protected void ThreeOfKind()
    {
        int[] cardIndexes = new int[5];
        int highCard = 0;
        int index = 0;
        for (int i = 0; i < sortedValueHand.Count - 1; i++)
        {
            //If we have already found the 3 of a kind
            if (index > 2 && index < 5)
            {
                //lets add the higher cards
                //if we add 2 and we are not at the end
                //then we are at the lower end of the cards and we shoudln't add em
                if (i + 2 >= sortedValueHand.Count)
                {
                    cardIndexes[index] = i;
                    index++;
                }
            }
            if (i + 2 < sortedValueHand.Count)
            {
                //Do we have a 3 of a kind
                if (sortedValueHand[i].MyValue == sortedValueHand[i + 2].MyValue)
                {
                    //Put the pair in the card indexes
                    cardIndexes[index] = i;
                    index++;
                    cardIndexes[index] = i + 1;
                    index++;
                    cardIndexes[index] = i + 2;
                    index++;


                    //Lets get the highest card
                    if (i + 2 == sortedValueHand.Count - 1)
                    {
                        highCard = (int)sortedValueHand[i - 1].MyValue;
                    }
                    else
                    {
                        highCard = (int)sortedValueHand[sortedValueHand.Count - 1].MyValue;
                    }

                    //Increment past the three with i
                    i += 2;
                }
            }            
        }

        //If we still haven't filled up the card indexes
        //Lets go down from the start of the three of a kind
        //And add cards until it's filled up
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
            //Check if we aren't out of bounds
            if (i-1 >= 0)
            {
                //If the two are a pair
                if (sortedValueHand[i] == sortedValueHand[i-1])
                {
                    //set index of one
                    cardIndexes[index] = i;
                    index++;
                    //set index of other
                    cardIndexes[index] = i - 1;
                    //decrement i so we skip over the next check
                    i--;
                }
                //If it's not a pair then check if we can set it to high card
                else if(highCardIndex < 0 || sortedValueHand[highCardIndex].MyValue < sortedValueHand[i].MyValue)
                {
                    highCardIndex = i;
                }
            }
        }

        //Set the last index to the high card
        cardIndexes[4] = highCardIndex;
        //Take the highest pair, which should be the lowest index in card indexes
        //Calculate the value by doubling it
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
            //If we have already found the pair
            if (index > 1 && index < 5)
            {
                //lets add the highest cards
                //if we add 3 and we are not at the end
                //then we are at the lower end of the cards and we shoudln't add em
                if (i + 3 >= sortedValueHand.Count)
                {
                    cardIndexes[index] = i;
                    i++;
                }
            }

            if (i+1 < sortedValueHand.Count)
            {
                //Do we have a pair?
                if (sortedValueHand[i].MyValue == sortedValueHand[i + 1].MyValue)
                {
                    //Put the pair in the card indexes
                    cardIndexes[index] = i;
                    index++;
                    cardIndexes[index] = i + 1;
                    index++;
                    //Increment past the pair with i
                    i++;

                    //Lets get the highest card
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


        //If we still haven't filled up the card indexes
        //Lets go down from the start of the pair
        //And add cards until it's filled up
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
