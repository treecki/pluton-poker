using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentRoomCanvas : MonoBehaviour
{
    private LobbyCanvases lobbyCanvases;

    private PlayerListingsMenu playerListingsMenu;
    private LeaveRoomMenu leaveRoomMenu;
    public LeaveRoomMenu LeaveRoomMenu { get { return leaveRoomMenu; } }
    private void Awake()
    {
        lobbyCanvases = GetComponentInParent<LobbyCanvases>();
        playerListingsMenu = GetComponentInChildren<PlayerListingsMenu>();
        leaveRoomMenu = GetComponentInChildren<LeaveRoomMenu>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
