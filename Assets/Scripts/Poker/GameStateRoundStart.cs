using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateRoundStart : GameState
{
    public GameStateRoundStart(PokerStateMachine _psm) : base(_psm)
    {

    }

    public override void Run()
    {
        base.Run();
        if (!psm.AuthorityController.TryBeginAuthorityMutation(PokerAuthorityController.MutationReasonGameStateRoundStartRun))
        {
            return;
        }

        psm.MapPhotonPlayersToSeats();
        CreateDeck();
        AddRoundPlayers();
        psm.AuthorityController.PublishSnapshot(PokerAuthorityController.SnapshotPhaseRoundStartReadyToDeal);
        psm.SetState(psm.StateDeal);
    }

    private void CreateDeck()
    {
        psm.deck = new DeckOfCards();
        psm.deck.NewDeck();
    }

    private void AddRoundPlayers()
    {
        psm.queuePlayersInRound.Clear();

        List<PokerPlayer> activePlayers = psm.GetActivePlayers();
        int firstIndex = (activePlayers.IndexOf(psm.BetManager.BigBlindPlayer) + 1) % activePlayers.Count;

        for (int i = 0; i < activePlayers.Count; i++)
        {
            int index = (firstIndex + i) % activePlayers.Count;
            psm.queuePlayersInRound.Enqueue(activePlayers[index]);
            activePlayers[index].SetFolded(false);
        }
    }
}
