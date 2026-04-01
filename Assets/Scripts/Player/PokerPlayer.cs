using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Realtime;

public class PokerPlayer
{
    protected int playerID;
    public int PlayerID { get { return playerID; } }

    public int ActorNumber { get; private set; }
    public string DisplayName { get; private set; }

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
        ActorNumber = -1;
        DisplayName = "Seat " + _id;
    }

    public void BindToPhotonPlayer(Player photonPlayer)
    {
        if (photonPlayer == null)
        {
            ActorNumber = -1;
            DisplayName = "Seat " + playerID;
            return;
        }

        ActorNumber = photonPlayer.ActorNumber;
        DisplayName = string.IsNullOrEmpty(photonPlayer.NickName) ? "Player " + photonPlayer.ActorNumber : photonPlayer.NickName;
    }

    public bool CanRevealHoleCardsTo(Player viewer)
    {
        if (viewer == null)
        {
            return true;
        }

        return viewer.ActorNumber == ActorNumber;
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

    public bool CommitResolvedBet(float amountDiff)
    {
        if (amountDiff < 0f)
        {
            return false;
        }

        if (playerMoney - amountDiff < 0f)
        {
            return false;
        }

        currBet.amount += amountDiff;
        playerMoney -= amountDiff;
        return true;
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
