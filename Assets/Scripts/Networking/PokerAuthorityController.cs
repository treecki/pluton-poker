using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System;

public class PokerAuthorityController : MonoBehaviourPunCallbacks
{
    public static PokerAuthorityController Instance { get; private set; }

    // Photon custom event codes only need to be unique within this project's event usage.
    // 41/42 are arbitrary picks for Milestone 2's action request/resolution flow.
    public const byte ActionRequestEventCode = 41;
    public const byte ActionResolvedEventCode = 42;
    public const float DefaultTurnTimeoutSeconds = 15f;

    public const string MutationReasonStartRound = "StartRound";
    public const string MutationReasonRestartRound = "RestartRound";
    public const string MutationReasonGameStateRoundStartRun = "GameStateRoundStart.Run";
    public const string MutationReasonGameStateDealRun = "GameStateDeal.Run";
    public const string MutationReasonGameStateBettingRun = "GameStateBetting.Run";
    public const string MutationReasonGameStateBettingReceiveAction = "GameStateBetting.ReceiveAction";
    public const string MutationReasonGameStateBettingReceiveNetworkAction = "GameStateBetting.ReceiveNetworkAction";
    public const string MutationReasonGameStateRoundEndRun = "GameStateRoundEnd.Run";

    public const string SnapshotPhaseAwaitingAuthorityStart = "AwaitingAuthorityStart";
    public const string SnapshotPhaseMasterClientSwitched = "MasterClientSwitched";
    public const string SnapshotPhaseRoundStartReadyToDeal = "RoundStart.ReadyToDeal";
    public const string SnapshotPhaseBettingStarted = "Betting.Started";
    public const string SnapshotPhaseBettingWaitingForAction = "Betting.WaitingForAction";
    public const string SnapshotPhaseBettingActionApplied = "Betting.ActionApplied";
    public const string SnapshotPhaseBettingRemoteActionResolved = "Betting.RemoteActionResolved";
    public const string SnapshotPhaseRoundEndResolved = "RoundEnd.Resolved";

    private PokerGameSnapshot latestSnapshot;
    public PokerGameSnapshot LatestSnapshot { get { return latestSnapshot; } }

    private PokerStateMachine psm;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        psm = GetComponent<PokerStateMachine>();
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    public bool IsOffline()
    {
        return !PhotonNetwork.InRoom;
    }

    public bool HasAuthority()
    {
        return IsOffline() || PhotonNetwork.IsMasterClient;
    }

    public bool CanControlPlayer(PokerPlayer player)
    {
        if (player == null)
        {
            return false;
        }

        // Offline/single-player sessions do not have Photon ownership, so we allow
        // local control here and only enforce seat ownership once we are in a room.
        if (IsOffline())
        {
            return true;
        }

        Player local = PhotonNetwork.LocalPlayer;
        if (local == null)
        {
            return false;
        }

        return player.ActorNumber == local.ActorNumber;
    }

    public bool TryBeginAuthorityMutation(string reason)
    {
        if (HasAuthority())
        {
            return true;
        }

        Debug.LogWarning("Blocked non-authority mutation: " + reason);
        return false;
    }

    public void PublishSnapshot(string phaseOverride = null)
    {
        if (psm == null)
        {
            psm = GetComponent<PokerStateMachine>();
        }

        latestSnapshot = PokerGameSnapshot.Capture(psm, phaseOverride);
        Debug.Log("Published snapshot phase=" + latestSnapshot.Phase + " turnActor=" + latestSnapshot.CurrentTurnActorNumber + " pot=" + latestSnapshot.Pot);
    }

    public void SubmitActionRequest(PokerActionCommand command)
    {
        if (command == null)
        {
            return;
        }

        if (IsOffline())
        {
            psm.StateBetting.ReceiveNetworkAction(command);
            return;
        }

        object[] payload = SerializeAction(command);
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(ActionRequestEventCode, payload, options, SendOptions.SendReliable);
    }

    public void BroadcastResolvedAction(PokerActionCommand command)
    {
        if (command == null)
        {
            return;
        }

        if (IsOffline())
        {
            return;
        }

        object[] payload = SerializeAction(command);
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(ActionResolvedEventCode, payload, options, SendOptions.SendReliable);
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {
        if (obj.Code == ActionRequestEventCode)
        {
            if (!HasAuthority())
            {
                return;
            }

            PokerActionCommand command = DeserializeAction(obj.CustomData as object[]);
            psm.StateBetting.ReceiveNetworkAction(command);
        }
        else if (obj.Code == ActionResolvedEventCode)
        {
            PokerActionCommand command = DeserializeAction(obj.CustomData as object[]);
            psm.StateBetting.OnRemoteActionResolved(command);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        PublishSnapshot(SnapshotPhaseMasterClientSwitched);
    }

    // For simple command payloads we serialize to an object[] of Photon-friendly values.
    // If we later need richer data (lists, dictionaries, nested models), we should either
    // flatten them into primitives, serialize them into a transport-safe format, or register
    // custom Photon types if the shape becomes stable and worth formalizing.
    private object[] SerializeAction(PokerActionCommand command)
    {
        return new object[]
        {
            (int)command.ActionType,
            command.ActorNumber,
            command.PlayerId,
            command.Amount,
            command.RequestedAtUtc
        };
    }

    private PokerActionCommand DeserializeAction(object[] payload)
    {
        if (payload == null || payload.Length < 5)
        {
            return null;
        }

        return new PokerActionCommand
        {
            ActionType = (PokerActionType)(int)payload[0],
            ActorNumber = (int)payload[1],
            PlayerId = (int)payload[2],
            Amount = Convert.ToSingle(payload[3]),
            RequestedAtUtc = (string)payload[4]
        };
    }
}
