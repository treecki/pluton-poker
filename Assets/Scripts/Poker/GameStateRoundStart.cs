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
        CreateDeck();
        AddRoundPlayers();
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

        //Lets get the index of the small blind because we'll have to force them to start and play bet in the beginning
        List<Player> activePlayers = psm.GetActivePlayers();
        int firstIndex = (activePlayers.IndexOf(psm.BetManager.BigBlindPlayer) + 1) % activePlayers.Count;

        for (int i = 0; i < activePlayers.Count; i++)
        {
            int index = (firstIndex + i) % activePlayers.Count;
            psm.queuePlayersInRound.Enqueue(activePlayers[index]);
            activePlayers[index].SetFolded(false);
        }
    }
}
