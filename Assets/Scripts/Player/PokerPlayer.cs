using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PokerPlayer
{
    protected int playerID;
    public int PlayerID { get { return playerID; } }

    protected float playerMoney;
    public float PlayerMoney { get { return playerMoney; } }

    protected PlayerHand playerHand;
    public PlayerHand PlayerHand { get { return playerHand; } }

    protected Bet currBet;
    public Bet CurrBet { get { return currBet; } }

    protected bool isFolded;
    public bool IsFolded { get { return isFolded; } }

    public bool canInput = false;

    public Action<Bet> OnPlayerEvent = delegate { };

    public PokerPlayer(int _id)
    {
        playerID = _id;
        playerHand = new PlayerHand();
        currBet = new Bet(0, playerID);
    }

    public bool SetFolded(bool _isFolded)
    {
        if (!canInput && _isFolded)
        {
            return false;
        }
        else if(_isFolded)
        {
            currBet.amount = -1;
            OnPlayerEvent(currBet);
        }

        isFolded = _isFolded;
        return true;
    }

    public bool RaiseCurrentBetTo(float _amount)
    {
        if (!canInput || currBet.amount > _amount)
        {
            return false;
        }

        float amountDiff = _amount - currBet.amount;

        if (playerMoney - amountDiff >= 0)
        {
            currBet.amount = _amount;
            playerMoney -= amountDiff;
            OnPlayerEvent(new Bet(amountDiff, playerID));
            return true;
        }
        return false;
    }

    public void ResetCurrentBet()
    {
        currBet.amount = 0;
    }

    public void AddMoney(float _amount)
    {
        playerMoney += _amount;
    }

    public bool IsPlayerBroke() { return playerMoney <= 0; }
}

public struct Bet
{
    public float amount;
    public int playerID;

    public Bet(float _amount, int _playerID)
    {
        amount = _amount;
        playerID = _playerID;
    }
}
