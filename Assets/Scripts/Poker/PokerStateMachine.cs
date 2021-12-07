using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PokerStateMachine : StateMachine
{
    private static PokerStateMachine psm;

    public static PokerStateMachine PSM { get { return psm; } }

    public Transform PokerHandsTransform;
    public Transform RiverHandTransform;

    public DeckOfCards deck;

    public const int NUM_OF_PLAYERS = 4;

    public CardFaces DeckCardVisuals;

    protected List<Player> playersInGame;
    public List<Player> PlayersInGame { get { return playersInGame; } }

    public Queue<Player> queuePlayersInRound;

    public GameObject cardPrefab;
    public GameObject handPrefab;
    public GameObject riverPrefab;

    public GenHand riverHand;

    GameObject riverHandGameObject;

    private BettingManager betManager;
    public BettingManager BetManager { get { return betManager; } }

    private float buyInAmount = 10f;

    public DealingState DealState { get { return stateDeal.dealState; } }
    public Bet HighestBet { get { return stateBetting.HighestBet; } }

    public Action<Bet> OnBetEvent = delegate { };
    public Action<int> OnFoldEvent = delegate { };

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

        SetUpGame();
    }

    public void Start()
    {
        StartRound();
    }

    public void SetUpGame()
    {
        playersInGame = new List<Player>();
        riverHand = new GenHand();

        for (int i = 0; i < NUM_OF_PLAYERS; i++)
        {
            Player p = new Player(i);
            p.AddMoney(buyInAmount); 
            playersInGame.Add(p);
        }

        queuePlayersInRound = new Queue<Player>();

        betManager = new BettingManager(this);

        stateRoundStart = new GameStateRoundStart(this);
        stateBetting = new GameStateBetting(this);
        stateDeal = new GameStateDeal(this);
        stateRoundEnd = new GameStateRoundEnd(this);
    }

    public void StartRound()
    {
        SetState(stateRoundStart);
    }

    public void RestartRound()
    {
        betManager.NewRound();
        stateDeal.NewRound();
        DestroyAllVisuals();
        StartRound();
    }

    public override void SetState(GameState inputState)
    {
        base.SetState(inputState);
        inputState.Run();
    }

    public Player GetPlayerWithID(int _id)
    {
        Player currPlayer = playersInGame.Find(x => x.PlayerID == _id);
        return currPlayer;
    }

    public void CreateAllHandsVisuals()
    {
        foreach(Player p in queuePlayersInRound)
        {
            CreatePlayerHandVisual(p.PlayerID);
        }
    }

    protected void CreatePlayerHandVisual(int playerID)
    {
        GameObject playerHandGameObject = Instantiate(handPrefab);
        playerHandGameObject.transform.parent = PokerHandsTransform;
        PlayerObject handObject = playerHandGameObject.GetComponent<PlayerObject>();
        Player p = GetPlayerWithID(playerID);
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

    public List<Player> GetActivePlayers()
    {
        List<Player> activePlayers = new List<Player>();
        foreach (Player p in playersInGame)
        {
            if (!p.IsPlayerBroke())
            {
                activePlayers.Add(p);
            }
        }

        return activePlayers;
    }

    public Player GetNextPlayerInQueue()
    {
        return queuePlayersInRound.Peek();
    }

    public void SendNextToBackOfQueue()
    {
        Player p = queuePlayersInRound.Dequeue();
        queuePlayersInRound.Enqueue(p);
    }

    public void SetStartPlayerInQueue()
    {
        Player startPlayer = GetStartPlayer();

        while (GetNextPlayerInQueue().PlayerID != startPlayer.PlayerID)
        {
            SendNextToBackOfQueue();
        }
    }

    public Player GetStartPlayer()
    {
        int indexBigBlind = Array.IndexOf(playersInGame.ToArray(), BetManager.BigBlindPlayer);

        int counter = 0;
        Player startPlayer;
        //search for a start player that's not folded and return that
        do
        {
            counter++;
            startPlayer = playersInGame[(indexBigBlind + counter) % playersInGame.Count];
        }
        while (startPlayer.IsFolded && counter < playersInGame.Count);

        return startPlayer;
    }
}
