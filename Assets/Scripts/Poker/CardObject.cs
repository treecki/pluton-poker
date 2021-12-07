using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardObject : MonoBehaviour
{
    private Card playingCard;

    private PokerStateMachine pokerManager;

    public SUIT CardSuit { get { return playingCard.MySuit; } }
    public VALUE CardValue { get { return playingCard.MyValue; } }

    private bool isFaceUp;

    private Image image;

    private void Awake()
    {
        pokerManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<PokerStateMachine>();
        image = GetComponent<Image>();
    }

    public void SetCard(Card _card)
    {
        playingCard = _card;
        image.sprite = pokerManager.DeckCardVisuals.GetCardSprite(CardSuit, CardValue);
    }

    public void SetFaceUp(bool _isFaceUp)
    {
        isFaceUp = _isFaceUp;
        if (isFaceUp)
        {
            image.sprite = pokerManager.DeckCardVisuals.GetCardSprite(CardSuit, CardValue);
        }
        else
        {
            image.sprite = pokerManager.DeckCardVisuals.GetCardBack();
        }
    }


}
