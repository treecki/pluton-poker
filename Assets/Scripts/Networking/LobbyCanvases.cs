using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCanvases : MonoBehaviour
{
    [SerializeField]
    private RoomsCanvas roomsCanvas;
    public RoomsCanvas RoomsCanvas { get { return roomsCanvas; } }

    [SerializeField]
    private CurrentRoomCanvas currRoomCanvas;
    public CurrentRoomCanvas CurrRoomCanvas { get { return currRoomCanvas; } }

    private void Awake()
    {
        currRoomCanvas.Hide();
    }
}
