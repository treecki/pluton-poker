using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListingsMenu : MonoBehaviourPunCallbacks
{
    private Transform content;

    [SerializeField]
    private PlayerListing playerListingPrefab;
    private List<PlayerListing> listings = new List<PlayerListing>();

    [SerializeField]
    private TMP_Text readyText;
    private bool isReady = false;
    private LobbyCanvases lobbyCanvases;

    private void Awake()
    {
        lobbyCanvases = GetComponentInParent<LobbyCanvases>();
        content = GetComponentInChildren<ScrollRect>().content;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SetReadyUp(false);

        GetCurrentRoomPlayers();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        for (int i = 0; i < listings.Count; i++)
        {
            Destroy(listings[i].gameObject);
        }

        listings.Clear();
    }

    private void SetReadyUp(bool _isReady)
    {
        isReady = _isReady;
        if (isReady)
        {
            readyText.text = "Ready";
        }
        else
        {
            readyText.text = "Not Ready";
        }
    }

    private void GetCurrentRoomPlayers()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null)
            return;

        foreach(KeyValuePair<int, Photon.Realtime.Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            AddPlayerListing(playerInfo.Value);
        }
    }

    private void AddPlayerListing(Photon.Realtime.Player player)
    {
        int index = listings.FindIndex(c => c.Player == player);
        if (index != -1)
        {
            listings[index].SetPlayerInfo(player);
        }
        else
        {
            PlayerListing playerListing = Instantiate(playerListingPrefab, content);

            if (playerListing)
            {
                playerListing.SetPlayerInfo(player);
                listings.Add(playerListing);
            }
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        lobbyCanvases.CurrRoomCanvas.LeaveRoomMenu.OnClickLeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        AddPlayerListing(newPlayer);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        int index = listings.FindIndex(x => x.Player == otherPlayer);
        if (index != -1)
        {
            Destroy(listings[index].gameObject);
            listings.RemoveAt(index);
        }
    }

    public void OnClickStartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < listings.Count; i++)
            {
                if(listings[i].Player != PhotonNetwork.LocalPlayer)
                {
                    if (!listings[i].isReady)
                    {
                        return;
                    }
                }
            }

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel(1);
        }

    }

    public void OnClickReadyUp()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            SetReadyUp(!isReady);
            base.photonView.RPC("RPC_ChangeReadyState", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, isReady);
        }
    }

    [PunRPC]
    private void RPC_ChangeReadyState(Photon.Realtime.Player player, bool ready)
    {
        int index = listings.FindIndex(x => x.Player == player);
        if (index != -1)
        {
            listings[index].isReady = ready;
        }
    }
}
