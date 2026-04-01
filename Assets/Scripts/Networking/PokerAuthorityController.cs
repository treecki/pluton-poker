using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PokerAuthorityController : MonoBehaviourPunCallbacks
{
    public static PokerAuthorityController Instance { get; private set; }

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
        PublishSnapshot("MasterClientSwitched");
    }
}
