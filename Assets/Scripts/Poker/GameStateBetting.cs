using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateBetting : GameState
{

    Bet highestBet;
    public Bet HighestBet { get { return highestBet; } }

    Player currPlayerBetting;

    public GameStateBetting(PokerStateMachine _psm) : base(_psm)
    {
        highestBet = new Bet(0, -1);
    }

    public override void Run()
    {
        base.Run();
        StartBetRound();
    }

    private void StartBetRound()
    {
        ResetCurrentBets();

        if (psm.DealState == DealingState.HANDS)
        {
            SetBlinds();
        }

        psm.SetStartPlayerInQueue();

        RequestAction();
    }

    private void ResetCurrentBets()
    {
        foreach (Player p in psm.queuePlayersInRound)
        {
            p.ResetCurrentBet();
        }
        highestBet = new Bet(0, -1);
    }

    private void SetBlinds()
    {
        psm.BetManager.SetBlindBets();
        highestBet = new Bet(psm.BetManager.BigBlind, psm.BetManager.BigBlindPlayer.PlayerID);
    }

    private void RequestAction()
    {
        Player nextPlayer = psm.GetNextPlayerInQueue();
        nextPlayer.canInput = true;
        nextPlayer.OnPlayerEvent += ReceiveAction;
    }


    public override void ResetState()
    {
        base.ResetState();
        highestBet = new Bet(0, -1);
    }

    private void ReceiveAction(Bet newBet)
    {
        if (psm.GetNextPlayerInQueue().PlayerID != newBet.playerID) { return; }

        Debug.Log("Receive Action: " + newBet.amount);

        Player nextPlayer = psm.queuePlayersInRound.Dequeue();
        nextPlayer.canInput = false;
        nextPlayer.OnPlayerEvent -= ReceiveAction;

        if (newBet.amount >= 0)
        {
            AddBet(newBet);
        }

        if (psm.queuePlayersInRound.Count <= 1)
        {
            GoToEndRound();
        }
        else if (highestBet.playerID == psm.GetNextPlayerInQueue().PlayerID)
        {
            //We need to skip over the next player and go to round
            GoToDealing();
        }
        else
        {
            RequestAction();
        }
    }

    private void AddBet(Bet newBet)
    {
        Player p = psm.GetPlayerWithID(newBet.playerID);

        //To check for highest bet, we check how the current bet on the player has changed
        //New bet could be a call or a raise, but it won't include the amount previously bet
        if (highestBet.playerID == -1 || highestBet.amount < p.CurrBet.amount)
        {
            highestBet = p.CurrBet;
            Debug.Log("New highest bet of" + highestBet.amount);
        }

        psm.queuePlayersInRound.Enqueue(p);
        psm.BetManager.AddToPot(newBet.amount);        
    }

    protected void GoToDealing()
    {
        psm.SetState(psm.StateDeal);
    }

    protected void GoToEndRound()
    {
        psm.SetState(psm.StateRoundEnd);
    }
}
