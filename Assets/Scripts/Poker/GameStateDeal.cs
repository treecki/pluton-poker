using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateDeal : GameState
{
    public DealingState dealState;

    public GameStateDeal(PokerStateMachine _psm) : base(_psm)
    {

    }

    public override void Run()
    {
        base.Run();
        Deal();
    }

    protected void Deal()
    {
        switch (dealState)
        {
            case DealingState.NONE:
                DealHands();
                dealState++;
                GoToBetting();
                break;
            case DealingState.HANDS:
                DealFlop();
                dealState++;
                GoToBetting();
                break;
            case DealingState.FLOP:
            case DealingState.TURN:
                DealToRiver();
                dealState++;
                GoToBetting();
                break;
            case DealingState.RIVER:
                EndDealing();
                break;
        }

    }

    protected void DealHands()
    {
        int activePlayerSize = psm.GetActivePlayers().Count;
        int cardsBeingDealt = activePlayerSize * 2;

        for (int i = 0; i < cardsBeingDealt; i++)
        {
           psm.GetActivePlayers()[i % activePlayerSize].PlayerHand.DealCard(psm.deck.TakeTopCard());
        }

        psm.CreateAllHandsVisuals();
    }

    protected void DealFlop()
    {
        psm.deck.DiscardTopCard();
        psm.riverHand.DealCard(psm.deck.TakeTopCard());
        psm.riverHand.DealCard(psm.deck.TakeTopCard());
        psm.riverHand.DealCard(psm.deck.TakeTopCard());
        psm.CreateRiverHandVisual();
    }

    protected void DealToRiver()
    {
        psm.deck.DiscardTopCard();
        psm.riverHand.DealCard(psm.deck.TakeTopCard());
        psm.CreateRiverHandVisual();
    }

    protected void GoToBetting()
    {
        psm.SetState(psm.StateBetting);
    }

    protected void EndDealing()
    {
        psm.SetState(psm.StateRoundEnd);
    }

    public void NewRound()
    {
        foreach (PokerPlayer p in psm.PlayersInGame)
        {
            p.PlayerHand.ClearHand();
        }

        dealState = 0;
    }
}

public enum DealingState { NONE, HANDS, FLOP, TURN, RIVER}
