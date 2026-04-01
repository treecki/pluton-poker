using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System;

public class PokerAuthorityController : MonoBehaviourPunCallbacks
{
    public static PokerAuthorityController Instance { get; private set; }

    public const byte ActionRequestEventCode = 41;
    public const byte ActionResolvedEventCode = 42;
    public const float DefaultTurnTimeoutSeconds = 15f;

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

    public void SubmitActionRequest(PokerActionCommand command)
    {
        if (command == null)
        {
            return;
        }

        if (!PhotonNetwork.InRoom)
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

        if (!PhotonNetwork.InRoom)
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
        PublishSnapshot("MasterClientSwitched");
    }

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
