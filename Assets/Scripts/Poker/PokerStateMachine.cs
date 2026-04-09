using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

[RequireComponent(typeof(PokerAuthorityController))]
public class PokerStateMachine : StateMachine
{
    private static PokerStateMachine psm;

    public static PokerStateMachine PSM { get { return psm; } }

    public Transform PokerHandsTransform;
    public Transform RiverHandTransform;

    public DeckOfCards deck;

    public const int NUM_OF_PLAYERS = 4;

    public CardFaces DeckCardVisuals;

    protected List<PokerPlayer> playersInGame;
    public List<PokerPlayer> PlayersInGame { get { return playersInGame; } }

    public Queue<PokerPlayer> queuePlayersInRound;

    public GameObject cardPrefab;
    public GameObject handPrefab;
    public GameObject riverPrefab;

    public GenHand riverHand;

    GameObject riverHandGameObject;
    private string lastAppliedSnapshotAtUtc;

    private BettingManager betManager;
    public BettingManager BetManager { get { return betManager; } }

    private float buyInAmount = 10f;

    private PokerAuthorityController authorityController;
    public PokerAuthorityController AuthorityController { get { return authorityController; } }

    public DealingState DealState { get { return stateDeal.dealState; } }
    public Bet HighestBet { get { return stateBetting.HighestBet; } }

    public Action<Bet> OnBetEvent = delegate { };
    public Action<int> OnFoldEvent = delegate { };
    public Action<PokerGameSnapshot> OnSnapshotApplied = delegate { };
    public Action<PokerGameSnapshot> OnRoundResultSnapshotApplied = delegate { };

    protected GameStateRoundStart stateRoundStart;
    public GameStateRoundStart StateRoundStart { get{ return stateRoundStart; } }
    protected GameStateDeal stateDeal;
    public GameStateDeal StateDeal { get { return stateDeal; } }
    protected GameStateBetting stateBetting;
    public GameStateBetting StateBetting { get { return stateBetting; } }
    protected GameStateRoundEnd stateRoundEnd;
    public GameStateRoundEnd StateRoundEnd { get { return stateRoundEnd; } }

    public void Awake()
    {
        if (psm != null && psm != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            psm = this;
        }

        authorityController = GetComponent<PokerAuthorityController>();
        SetUpGame();
    }

    public void Start()
    {
        if (authorityController.HasAuthority())
        {
            StartRound();
        }
        else
        {
            authorityController.PublishSnapshot(PokerAuthorityController.SnapshotPhaseAwaitingAuthorityStart);
        }
    }

    public void Update()
    {
        if (stateBetting != null)
        {
            stateBetting.Tick();
        }

        if (!authorityController.HasAuthority())
        {
            ApplyLatestAuthoritativeSnapshot();
        }
    }

    public void SetUpGame()
    {
        playersInGame = new List<PokerPlayer>();
        riverHand = new GenHand();

        for (int i = 0; i < NUM_OF_PLAYERS; i++)
        {
            PokerPlayer p = new PokerPlayer(i);
            p.AddMoney(buyInAmount);
            playersInGame.Add(p);
        }

        MapPhotonPlayersToSeats();

        queuePlayersInRound = new Queue<PokerPlayer>();
        lastAppliedSnapshotAtUtc = null;

        betManager = new BettingManager(this);

        stateRoundStart = new GameStateRoundStart(this);
        stateBetting = new GameStateBetting(this);
        stateDeal = new GameStateDeal(this);
        stateRoundEnd = new GameStateRoundEnd(this);
    }

    public void StartRound()
    {
        if (!authorityController.TryBeginAuthorityMutation(PokerAuthorityController.MutationReasonStartRound))
        {
            return;
        }

        SetState(stateRoundStart);
    }

    public void RestartRound()
    {
        if (!authorityController.TryBeginAuthorityMutation(PokerAuthorityController.MutationReasonRestartRound))
        {
            return;
        }

        betManager.NewRound();
        stateDeal.NewRound();
        lastAppliedSnapshotAtUtc = null;
        DestroyAllVisuals();
        StartRound();
    }

    public override void SetState(GameState inputState)
    {
        base.SetState(inputState);
        authorityController.PublishSnapshot(inputState.GetType().Name);
        inputState.Run();
        authorityController.PublishSnapshot(inputState.GetType().Name + ".RunComplete");
    }

    public PokerPlayer GetPlayerWithID(int _id)
    {
        PokerPlayer currPlayer = playersInGame.Find(x => x.PlayerID == _id);
        return currPlayer;
    }

    public PokerPlayer GetPlayerForLocalClient()
    {
        Player local = PhotonNetwork.LocalPlayer;
        if (local == null)
        {
            return GetPlayerWithID(0);
        }

        return playersInGame.FirstOrDefault(player => player.ActorNumber == local.ActorNumber);
    }

    public void CreateAllHandsVisuals()
    {
        foreach(PokerPlayer p in queuePlayersInRound)
        {
            CreatePlayerHandVisual(p.PlayerID);
        }
    }

    protected void CreatePlayerHandVisual(int playerID)
    {
        GameObject playerHandGameObject = Instantiate(handPrefab);
        playerHandGameObject.transform.parent = PokerHandsTransform;
        PlayerObject handObject = playerHandGameObject.GetComponent<PlayerObject>();
        PokerPlayer p = GetPlayerWithID(playerID);
        handObject.SetHand(p);
        handObject.CreateCards();
    }

    public void CreateRiverHandVisual()
    {
        if (riverHandGameObject == null)
        {
            riverHandGameObject = Instantiate(riverPrefab);
            riverHandGameObject.transform.parent = RiverHandTransform;
            riverHandGameObject.transform.position = Vector3.zero;
        }

        RiverObject handObject = riverHandGameObject.GetComponent<RiverObject>();
        handObject.SetHand(riverHand);
        handObject.CreateCards();
    }

    private void DestroyAllVisuals()
    {
        foreach (Transform child in PokerHandsTransform)
        {
            Destroy(child.gameObject);
        }
        if (riverHandGameObject)
        {
            Destroy(riverHandGameObject);
        }
    }

    public List<PokerPlayer> GetActivePlayers()
    {
        List<PokerPlayer> activePlayers = new List<PokerPlayer>();
        foreach (PokerPlayer p in playersInGame)
        {
            if (!p.IsPlayerBroke() && !p.IsFolded)
            {
                activePlayers.Add(p);
            }
        }

        return activePlayers;
    }

    public PokerPlayer GetNextPlayerInQueue()
    {
        return queuePlayersInRound.Peek();
    }

    public void SendNextToBackOfQueue()
    {
        PokerPlayer p = queuePlayersInRound.Dequeue();
        queuePlayersInRound.Enqueue(p);
    }

    public void SetStartPlayerInQueue()
    {
        PokerPlayer startPlayer = GetStartPlayer();

        while (GetNextPlayerInQueue().PlayerID != startPlayer.PlayerID)
        {
            SendNextToBackOfQueue();
        }
    }

    public PokerPlayer GetStartPlayer()
    {
        int indexBigBlind = Array.IndexOf(playersInGame.ToArray(), BetManager.BigBlindPlayer);

        int counter = 0;
        PokerPlayer startPlayer;
        do
        {
            counter++;
            startPlayer = playersInGame[(indexBigBlind + counter) % playersInGame.Count];
        }
        while (startPlayer.IsFolded && counter < playersInGame.Count);

        return startPlayer;
    }

    public void MapPhotonPlayersToSeats()
    {
        Player[] photonPlayers = PhotonNetwork.PlayerList;

        for (int i = 0; i < playersInGame.Count; i++)
        {
            if (i < photonPlayers.Length)
            {
                playersInGame[i].BindToPhotonPlayer(photonPlayers[i]);
            }
            else
            {
                playersInGame[i].BindToPhotonPlayer(null);
            }
        }
    }

    public string GetCurrentPhaseName()
    {
        if (currentState == null)
        {
            return "Uninitialized";
        }

        return currentState.GetType().Name;
    }

    public int GetCurrentTurnActorNumber()
    {
        if (queuePlayersInRound == null || queuePlayersInRound.Count == 0)
        {
            return -1;
        }

        PokerPlayer player = queuePlayersInRound.Peek();
        return player != null ? player.ActorNumber : -1;
    }

    public int GetBlindActorNumber(bool smallBlind)
    {
        PokerPlayer player = smallBlind ? BetManager.SmallBlindPlayer : BetManager.BigBlindPlayer;
        return player != null ? player.ActorNumber : -1;
    }

    public int GetDealerActorNumber()
    {
        PokerPlayer dealer = BetManager.SmallBlindPlayer;
        return dealer != null ? dealer.ActorNumber : -1;
    }

    // Milestone 2.5 starts here: non-authority clients consume the latest authoritative
    // snapshot as real state input so remote tables stop depending on local inference.
    public void ApplyLatestAuthoritativeSnapshot()
    {
        PokerGameSnapshot snapshot = authorityController != null ? authorityController.LatestSnapshot : null;
        if (snapshot == null)
        {
            return;
        }

        if (lastAppliedSnapshotAtUtc == snapshot.UpdatedAtUtc)
        {
            return;
        }

        ApplySnapshot(snapshot);
        lastAppliedSnapshotAtUtc = snapshot.UpdatedAtUtc;
    }

    public void ApplySnapshot(PokerGameSnapshot snapshot)
    {
        if (snapshot == null || snapshot.Players == null)
        {
            return;
        }

        Debug.Log("Applying authoritative snapshot phase=" + snapshot.Phase + " updatedAt=" + snapshot.UpdatedAtUtc + " turnActor=" + snapshot.CurrentTurnActorNumber);

        ApplyPlayerSnapshots(snapshot.Players, snapshot.CurrentTurnActorNumber);
        ApplyCommunityCardSnapshot(snapshot);
        SyncQueueToSnapshot(snapshot);
        RefreshPlayerHandVisuals(snapshot.Players);
        ApplyRoundResultSnapshot(snapshot);
        OnSnapshotApplied(snapshot);
    }

    private void ApplyPlayerSnapshots(List<PlayerSnapshot> playerSnapshots, int currentTurnActorNumber)
    {
        foreach (PlayerSnapshot snapshot in playerSnapshots)
        {
            PokerPlayer player = GetPlayerWithID(snapshot.SeatIndex);
            if (player == null)
            {
                continue;
            }

            player.ApplySnapshotState(snapshot);
            bool canInput = !snapshot.Folded && snapshot.ActorNumber == GetCurrentLocalActorNumber() && snapshot.ActorNumber == currentTurnActorNumber;
            player.SetCanInput(canInput);
        }
    }

    private void ApplyCommunityCardSnapshot(PokerGameSnapshot snapshot)
    {
        if (riverHand == null)
        {
            riverHand = new GenHand();
        }

        riverHand.ClearHand();
        if (snapshot.CommunityCards != null)
        {
            foreach (CardSnapshot card in snapshot.CommunityCards)
            {
                Card rebuiltCard = card.ToCard();
                if (rebuiltCard != null)
                {
                    riverHand.DealCard(rebuiltCard);
                }
            }
        }

        if (snapshot.CommunityCardCount > 0)
        {
            CreateRiverHandVisual();
        }
        else if (riverHandGameObject != null)
        {
            RiverObject riverObject = riverHandGameObject.GetComponent<RiverObject>();
            if (riverObject != null)
            {
                riverObject.ClearCards();
            }
        }
    }

    private void SyncQueueToSnapshot(PokerGameSnapshot snapshot)
    {
        if (queuePlayersInRound == null)
        {
            queuePlayersInRound = new Queue<PokerPlayer>();
        }

        queuePlayersInRound.Clear();
        foreach (PokerPlayer player in playersInGame)
        {
            if (!player.IsFolded && !player.IsPlayerBroke())
            {
                queuePlayersInRound.Enqueue(player);
            }
        }

        if (snapshot.CurrentTurnActorNumber == -1 || queuePlayersInRound.Count == 0)
        {
            return;
        }

        int safety = queuePlayersInRound.Count;
        while (queuePlayersInRound.Count > 0 && queuePlayersInRound.Peek().ActorNumber != snapshot.CurrentTurnActorNumber && safety-- > 0)
        {
            SendNextToBackOfQueue();
        }
    }

    private void RefreshPlayerHandVisuals(List<PlayerSnapshot> playerSnapshots)
    {
        if (PokerHandsTransform == null)
        {
            return;
        }

        Dictionary<int, PlayerSnapshot> snapshotBySeat = playerSnapshots.ToDictionary(snapshot => snapshot.SeatIndex, snapshot => snapshot);
        foreach (Transform child in PokerHandsTransform)
        {
            PlayerObject playerObject = child.GetComponent<PlayerObject>();
            if (playerObject == null || playerObject.Player == null)
            {
                continue;
            }

            PlayerSnapshot snapshot;
            if (!snapshotBySeat.TryGetValue(playerObject.Player.PlayerID, out snapshot))
            {
                continue;
            }

            playerObject.SetHand(playerObject.Player);
            playerObject.ApplySnapshotCards(snapshot.HoleCards);
        }
    }

    private void ApplyRoundResultSnapshot(PokerGameSnapshot snapshot)
    {
        if (!snapshot.ShowdownResolved)
        {
            return;
        }

        Debug.Log("Round result snapshot winnerSeat=" + snapshot.WinningPlayerSeatIndex + " winnerActor=" + snapshot.WinningPlayerActorNumber + " hand=" + snapshot.WinningHand + " message=" + snapshot.WinningMessage + " revealAll=" + snapshot.Players.Any(player => player.HoleCardsVisibleToLocalClient && player.ActorNumber != GetCurrentLocalActorNumber()));
        OnRoundResultSnapshotApplied(snapshot);
    }

    private int GetCurrentLocalActorNumber()
    {
        return PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : -1;
    }
}
