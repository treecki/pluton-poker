using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DeckOfCards
{
    const int NUM_OF_CARDS = 52; //num of all cards in deck
    private List<Card> deck;
    public List<Card> Deck { get { return deck; } }

    public DeckOfCards()
    {
        NewDeck();
    }

    public Card TakeTopCard()
    {
        Card topCard = deck[0];
        deck.RemoveAt(0);
        return topCard;
    }

    public void NewDeck()
    {
        deck = new List<Card>();
        SetUpDeck();
    }

    public void DiscardTopCard()
    {
        deck.RemoveAt(0);
    }

    //create deck of 52 cards: 13 values, with 4 suits
    public void SetUpDeck()
    {
        int i = 0;
        foreach (SUIT s in Enum.GetValues(typeof(SUIT)))
        {
            foreach (VALUE v in Enum.GetValues(typeof(VALUE)))
            {
                deck.Add(new Card { MySuit = s, MyValue = v });
                i++;
            }
        }

        ShuffleCards();
    }

    //shuffles the deck randomly
    public void ShuffleCards()
    {
        System.Random rand = new System.Random();

        //run the shuffle 1000 times
        for (int shuffles = 0; shuffles < 72; shuffles++)
        {
            Shuffle(deck);
        }
    }

    void Shuffle<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }
}
