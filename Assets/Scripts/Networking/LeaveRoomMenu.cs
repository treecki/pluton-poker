using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaveRoomMenu : MonoBehaviour
{
    private LobbyCanvases lobbyCanvases;

    private void Awake()
    {
        lobbyCanvases = GetComponentInParent<LobbyCanvases>();
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom(true);
        lobbyCanvases.CurrRoomCanvas.Hide();
    }
}
