using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateRoundEnd : GameState
{
    private PokerPlayer winningPlayer;
    public PokerPlayer WinningPlayer { get { return winningPlayer; } }

    private string winningMessage;
    public string WinningMessage { get { return winningMessage; } }

    public GameStateRoundEnd(PokerStateMachine _psm) : base(_psm)
    {

    }

    public override void Run()
    {
        if (!psm.AuthorityController.TryBeginAuthorityMutation("GameStateRoundEnd.Run"))
        {
            return;
        }

        if (psm.queuePlayersInRound.Count <= 1)
        {
            winningPlayer = psm.queuePlayersInRound.Peek();
            winningMessage = "Winner is " + winningPlayer.DisplayName + " by default.";
        }
        else
        {
            winningPlayer = psm.GetPlayerWithID(EvaluateHands());
            foreach (PokerPlayer p in psm.queuePlayersInRound)
            {
                Debug.Log("P" + p.PlayerID + "  has " + p.PlayerHand.FinalHandInfo.HandCombo);
            }
            winningMessage = "Winner is " + winningPlayer.DisplayName + " with a hand of " + winningPlayer.PlayerHand.FinalHandInfo.HandCombo;
        }

        psm.BetManager.GivePotToPlayer(winningPlayer);
        psm.AuthorityController.PublishSnapshot("RoundEnd.Resolved");

        base.Run();
    }

    public override void ResetState()
    {
        base.ResetState();
        winningPlayer = null;
        winningMessage = "";
    }

    protected void EvaluateAllHandsInRound()
    {
        foreach (PokerPlayer p in psm.queuePlayersInRound)
        {
            p.PlayerHand.EvaluateHand(psm.riverHand);
        }
    }

    protected int EvaluateHands()
    {
        EvaluateAllHandsInRound();

        PokerPlayer highestPlayer = null;

        foreach (PokerPlayer p in psm.queuePlayersInRound)
        {

            if (highestPlayer == null)
            {
                highestPlayer = p;
            }
            else
            {
                Debug.Log("Comparing hands of player " + p.PlayerID + " and " + highestPlayer.PlayerID);
                int compareResults = CompareHands(p.PlayerHand, highestPlayer.PlayerHand);
                if (compareResults < 0)
                {
                    highestPlayer = p;
                }
                else if (compareResults == 0)
                {
                }

            }
        }

        return highestPlayer.PlayerID;
    }

    private int CompareHands(PlayerHand negHand, PlayerHand posHand)
    {
        Hand negCardHand = negHand.FinalHandInfo.HandCombo;
        Hand posCardHand = posHand.FinalHandInfo.HandCombo;

        int negTotal = negHand.FinalHandInfo.Total;
        int posTotal = posHand.FinalHandInfo.Total;

        int negHighCard = negHand.FinalHandInfo.HighCard;
        int posHighCard = posHand.FinalHandInfo.HighCard;

        if (negCardHand > posCardHand)
        {
            return -1;
        }
        else if (negCardHand < posCardHand)
        {
            return 1;
        }
        else
        {
            if (negTotal > posTotal)
            {
                return -1;
            }
            else if (negTotal < posTotal)
            {
                return 1;
            }
            else
            {
                if (negHighCard > posHighCard)
                {
                    return -1;
                }
                else if (negHighCard < posHighCard)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

    }
}
