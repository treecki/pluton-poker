using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentRoomCanvas : MonoBehaviour
{
    private LobbyCanvases lobbyCanvases;

    private void Awake()
    {
        lobbyCanvases = GetComponentInParent<LobbyCanvases>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
