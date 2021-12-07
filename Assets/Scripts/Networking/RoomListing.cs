using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomListing : MonoBehaviour
{
    private TMP_Text roomName;
    private RoomInfo roomInfo;
    public RoomInfo RoomInfo { get { return roomInfo; } }

    private void Awake()
    {
        roomName = GetComponentInChildren<TMP_Text>();
    }

    public void SetRoomInfo(RoomInfo _roomInfo)
    {
        roomInfo = _roomInfo;
        roomName.text = _roomInfo.Name + "("+ _roomInfo.PlayerCount +"/" + _roomInfo.MaxPlayers +")";
    }
}
