using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class PlayerListing : MonoBehaviour
{
    private TMP_Text playerName;

    public Photon.Realtime.Player Player { get; private set; }

    private void Awake()
    {
        playerName = GetComponentInChildren<TMP_Text>();
    }

    public void SetPlayerInfo(Photon.Realtime.Player _player)
    {
        Player = _player;
        playerName.text = Player.NickName;
    }
}
