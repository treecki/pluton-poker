using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PokerAuthorityController : MonoBehaviourPunCallbacks
{
    public static PokerAuthorityController Instance { get; private set; }

    public const string MutationReasonStartRound = "StartRound";
    public const string MutationReasonRestartRound = "RestartRound";
    public const string MutationReasonGameStateRoundStartRun = "GameStateRoundStart.Run";
    public const string MutationReasonGameStateDealRun = "GameStateDeal.Run";
    public const string MutationReasonGameStateBettingRun = "GameStateBetting.Run";
    public const string MutationReasonGameStateBettingReceiveAction = "GameStateBetting.ReceiveAction";
    public const string MutationReasonGameStateRoundEndRun = "GameStateRoundEnd.Run";

    public const string SnapshotPhaseAwaitingAuthorityStart = "AwaitingAuthorityStart";
    public const string SnapshotPhaseMasterClientSwitched = "MasterClientSwitched";
    public const string SnapshotPhaseRoundStartReadyToDeal = "RoundStart.ReadyToDeal";
    public const string SnapshotPhaseBettingStarted = "Betting.Started";
    public const string SnapshotPhaseBettingWaitingForAction = "Betting.WaitingForAction";
    public const string SnapshotPhaseBettingActionApplied = "Betting.ActionApplied";
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

    public bool HasAuthority()
    {
        return !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;
    }

    public bool CanControlPlayer(PokerPlayer player)
    {
        if (player == null)
        {
            return false;
        }

        // Offline/single-player sessions do not have Photon ownership, so we allow
        // local control here and only enforce seat ownership once we are in a room.
        if (!PhotonNetwork.InRoom)
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

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        PublishSnapshot(SnapshotPhaseMasterClientSwitched);
    }
}
