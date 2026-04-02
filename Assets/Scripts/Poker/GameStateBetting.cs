using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameStateBetting : GameState
{
    Bet highestBet;
    public Bet HighestBet { get { return highestBet; } }

    PokerPlayer currPlayerBetting;
    private float turnStartedAt = -1f;
    private bool actionResolvedForTurn = false;

    public GameStateBetting(PokerStateMachine _psm) : base(_psm)
    {
        highestBet = new Bet(0, -1);
    }

    public override void Run()
    {
        base.Run();
        if (!psm.AuthorityController.TryBeginAuthorityMutation(PokerAuthorityController.MutationReasonGameStateBettingRun))
        {
            return;
        }

        StartBetRound();
        psm.AuthorityController.PublishSnapshot(PokerAuthorityController.SnapshotPhaseBettingStarted);
    }

    // Only the authority advances timeout-driven turn resolution so clients do not
    // race each other trying to auto-resolve the same stalled turn.
    public void Tick()
    {
        if (!psm.AuthorityController.HasAuthority())
        {
            return;
        }

        if (currPlayerBetting == null || actionResolvedForTurn || turnStartedAt < 0f)
        {
            return;
        }

        if (Time.time - turnStartedAt < PokerAuthorityController.DefaultTurnTimeoutSeconds)
        {
            return;
        }

        PokerActionCommand autoFold = PokerActionCommand.Create(
            PokerActionType.AutoFold,
            currPlayerBetting.ActorNumber,
            currPlayerBetting.PlayerID);

        ReceiveNetworkAction(autoFold);
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
        actionResolvedForTurn = false;
        turnStartedAt = Time.time;
        // Only the seat owned by this client should be interactable in multiplayer.
        // Offline we still allow local control so the table remains playable without Photon.
        nextPlayer.canInput = psm.AuthorityController.CanControlPlayer(nextPlayer);
        nextPlayer.OnPlayerEvent += ReceiveLocalAction;
        psm.AuthorityController.PublishSnapshot(PokerAuthorityController.SnapshotPhaseBettingWaitingForAction);
    }

    public override void ResetState()
    {
        base.ResetState();
        highestBet = new Bet(0, -1);
        currPlayerBetting = null;
        turnStartedAt = -1f;
        actionResolvedForTurn = false;
    }

    // Local UI/input still produces a Bet-like signal; this translates that into the
    // network command model and routes it through the authority path.
    private void ReceiveLocalAction(Bet newBet)
    {
        PokerActionCommand command = TranslateBetToCommand(newBet);
        if (command == null)
        {
            return;
        }

        if (psm.AuthorityController.HasAuthority())
        {
            ReceiveNetworkAction(command);
        }
        else
        {
            psm.AuthorityController.SubmitActionRequest(command);
        }
    }

    // The authority is the only place where a betting command becomes real shared state.
    // This method validates turn ownership / actor ownership, applies the result, then
    // advances the round and broadcasts the resolved action.
    public void ReceiveNetworkAction(PokerActionCommand command)
    {
        if (!psm.AuthorityController.TryBeginAuthorityMutation(PokerAuthorityController.MutationReasonGameStateBettingReceiveNetworkAction)) { return; }
        if (command == null) { return; }
        if (currPlayerBetting == null) { return; }
        if (currPlayerBetting.PlayerID != command.PlayerId) { return; }
        if (currPlayerBetting.ActorNumber != command.ActorNumber && PhotonNetwork.InRoom) { return; }

        PokerPlayer actingPlayer = psm.GetPlayerWithID(command.PlayerId);
        if (actingPlayer == null) { return; }

        if (!ApplyCommand(actingPlayer, command))
        {
            return;
        }

        actionResolvedForTurn = true;
        turnStartedAt = -1f;

        PokerPlayer nextPlayer = psm.queuePlayersInRound.Dequeue();
        nextPlayer.canInput = false;
        nextPlayer.OnPlayerEvent -= ReceiveLocalAction;

        if (command.ActionType != PokerActionType.Fold && command.ActionType != PokerActionType.AutoFold)
        {
            AddBetFromResolvedAction(actingPlayer, command);
        }

        psm.AuthorityController.PublishSnapshot(PokerAuthorityController.SnapshotPhaseBettingActionApplied);
        psm.AuthorityController.BroadcastResolvedAction(command);

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

    // Non-authority clients receive the already-resolved command after authority approval.
    // For this milestone we only hook that into snapshot publication; fuller remote state
    // rehydration/UI replay is still a later pass.
    public void OnRemoteActionResolved(PokerActionCommand command)
    {
        if (psm.AuthorityController.HasAuthority())
        {
            return;
        }

        if (command == null)
        {
            return;
        }

        psm.AuthorityController.PublishSnapshot(PokerAuthorityController.SnapshotPhaseBettingRemoteActionResolved);
    }

    private bool ApplyCommand(PokerPlayer player, PokerActionCommand command)
    {
        switch (command.ActionType)
        {
            case PokerActionType.Fold:
            case PokerActionType.AutoFold:
                player.SetFolded(true);
                return true;
            case PokerActionType.Check:
                return Mathf.Approximately(player.CurrBet.amount, highestBet.amount);
            case PokerActionType.Call:
                if (command.Amount < 0f)
                {
                    return false;
                }
                player.CommitResolvedBet(command.Amount);
                return true;
            case PokerActionType.Raise:
                if (command.Amount <= 0f)
                {
                    return false;
                }
                player.CommitResolvedBet(command.Amount);
                return true;
            default:
                return false;
        }
    }

    private void AddBetFromResolvedAction(PokerPlayer player, PokerActionCommand command)
    {
        if (highestBet.playerID == -1 || highestBet.amount < player.CurrBet.amount)
        {
            highestBet = player.CurrBet;
            Debug.Log("New highest bet of" + highestBet.amount);
        }

        psm.queuePlayersInRound.Enqueue(player);
        psm.BetManager.AddToPot(command.Amount);
    }

    // Action commands are the network payload for betting input: clients send them to the
    // authority, then the authority validates/applies them and rebroadcasts the resolved result.
    private PokerActionCommand TranslateBetToCommand(Bet newBet)
    {
        PokerPlayer player = psm.GetPlayerWithID(newBet.playerID);
        if (player == null)
        {
            return null;
        }

        if (newBet.amount < 0)
        {
            return PokerActionCommand.Create(PokerActionType.Fold, player.ActorNumber, player.PlayerID);
        }

        if (Mathf.Approximately(newBet.amount, 0f) && Mathf.Approximately(player.CurrBet.amount, highestBet.amount))
        {
            return PokerActionCommand.Create(PokerActionType.Check, player.ActorNumber, player.PlayerID, 0f);
        }

        float targetBet = player.CurrBet.amount;
        bool isRaise = targetBet > highestBet.amount;
        PokerActionType actionType = isRaise ? PokerActionType.Raise : PokerActionType.Call;
        return PokerActionCommand.Create(actionType, player.ActorNumber, player.PlayerID, newBet.amount);
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
