using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateBetting : GameState
{

    Bet highestBet;
    public Bet HighestBet { get { return highestBet; } }

    PokerPlayer currPlayerBetting;

    public GameStateBetting(PokerStateMachine _psm) : base(_psm)
    {
        highestBet = new Bet(0, -1);
    }

    public override void Run()
    {
        base.Run();
        if (!psm.AuthorityController.TryBeginAuthorityMutation("GameStateBetting.Run"))
        {
            return;
        }

        StartBetRound();
        psm.AuthorityController.PublishSnapshot("Betting.Started");
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
        foreach (PokerPlayer p in psm.queuePlayersInRound)
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
        PokerPlayer nextPlayer = psm.GetNextPlayerInQueue();
        currPlayerBetting = nextPlayer;
        nextPlayer.canInput = psm.AuthorityController.CanControlPlayer(nextPlayer);
        nextPlayer.OnPlayerEvent += ReceiveAction;
        psm.AuthorityController.PublishSnapshot("Betting.WaitingForAction");
    }


    public override void ResetState()
    {
        base.ResetState();
        highestBet = new Bet(0, -1);
        currPlayerBetting = null;
    }

    private void ReceiveAction(Bet newBet)
    {
        if (!psm.AuthorityController.TryBeginAuthorityMutation("GameStateBetting.ReceiveAction")) { return; }
        if (psm.GetNextPlayerInQueue().PlayerID != newBet.playerID) { return; }

        Debug.Log("Receive Action: " + newBet.amount);

        PokerPlayer nextPlayer = psm.queuePlayersInRound.Dequeue();
        nextPlayer.canInput = false;
        nextPlayer.OnPlayerEvent -= ReceiveAction;

        if (newBet.amount >= 0)
        {
            AddBet(newBet);
        }

        psm.AuthorityController.PublishSnapshot("Betting.ActionApplied");

        if (psm.queuePlayersInRound.Count <= 1)
        {
            GoToEndRound();
        }
        else if (highestBet.playerID == psm.GetNextPlayerInQueue().PlayerID)
        {
            GoToDealing();
        }
        else
        {
            RequestAction();
        }
    }

    private void AddBet(Bet newBet)
    {
        PokerPlayer p = psm.GetPlayerWithID(newBet.playerID);

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
