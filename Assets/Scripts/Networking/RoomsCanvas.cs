using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsCanvas : MonoBehaviour
{
    private LobbyCanvases lobbyCanvases;

    private void Awake()
    {
        lobbyCanvases = GetComponentInParent<LobbyCanvases>();
    }
}


