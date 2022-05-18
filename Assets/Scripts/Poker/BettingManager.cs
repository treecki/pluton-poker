using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BettingManager
{
    private float potAmount;
    public float PotAmount { get { return potAmount; } }

    private float smallBlind = .25f;
    public float SmallBlind { get { return smallBlind; } }
    private PokerPlayer smallBlindPlayer;
    public PokerPlayer SmallBlindPlayer { get { return smallBlindPlayer; } }

    private float bigBlind = .5f;
    public float BigBlind { get { return bigBlind; } }
    private PokerPlayer bigBlindPlayer;
    public PokerPlayer BigBlindPlayer { get { return bigBlindPlayer; } }

    PokerStateMachine psm;

    private int roundNumber;
    public int RoundNumber { get { return roundNumber; } }

    public BettingManager(PokerStateMachine _psm)
    {
        psm = _psm;
        potAmount = 0;
        smallBlindPlayer = psm.PlayersInGame[0];
        bigBlindPlayer = psm.PlayersInGame[1];
        roundNumber = 1;
    }

    public void GivePotToPlayer(PokerPlayer p)
    {
        p.AddMoney(potAmount);
        potAmount = 0;
    }

    public void AddToPot(float _amount)
    {
        potAmount += _amount;
    }

    public void NewRound()
    {
        roundNumber++;
        potAmount = 0;
        ShiftBlinds();
    }

    public void ShiftBlinds()
    {
        List<PokerPlayer> ActivePlayers = psm.GetActivePlayers();
        int indexOfPlayer = psm.PlayersInGame.IndexOf(bigBlindPlayer);
        bool setNewBlinds = false;

        while (!setNewBlinds)
        {
            PokerPlayer newPlayer = psm.PlayersInGame[indexOfPlayer];

            if (ActivePlayers.Contains(newPlayer))
            {
                setNewBlinds = true;
                smallBlindPlayer = ActivePlayers[ActivePlayers.IndexOf(newPlayer)];
                bigBlindPlayer = ActivePlayers[(ActivePlayers.IndexOf(newPlayer) + 1) % ActivePlayers.Count];
            }
            else
            {
                indexOfPlayer = (indexOfPlayer + 1) % psm.PlayersInGame.Count;
            }
        }
    }

    public void SetBlindBets()
    {
        smallBlindPlayer.canInput = true;
        smallBlindPlayer.RaiseCurrentBetTo(smallBlind);
        AddToPot(smallBlind);
        smallBlindPlayer.canInput = false;

        bigBlindPlayer.canInput = true;
        bigBlindPlayer.RaiseCurrentBetTo(bigBlind);
        AddToPot(bigBlind);
        bigBlindPlayer.canInput = false;
    }
}
