using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class CreateRoomMenu : MonoBehaviourPunCallbacks
{
    private LobbyCanvases lobbyCanvases;

    private TMP_InputField roomInputName;

    private void Awake()
    {
        roomInputName = GetComponentInChildren<TMP_InputField>();
        lobbyCanvases = GetComponentInParent<LobbyCanvases>();
    }

    public void OnClickCreateRoom()
    {
        if (!PhotonNetwork.IsConnected) { return; }
        RoomOptions options = new RoomOptions();
        options.BroadcastPropsChangeToAll = true;
        options.MaxPlayers = 6;
        PhotonNetwork.JoinOrCreateRoom(roomInputName.text, options, TypedLobby.Default);

    }

    public override void OnCreatedRoom()
    {
        print("Created room" + roomInputName.text + "successfully");
        lobbyCanvases.CurrRoomCanvas.Show();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Room creation failed: " + message);
    }
}
