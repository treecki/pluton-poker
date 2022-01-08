using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PlayerListing : MonoBehaviourPunCallbacks
{
    private TMP_Text playerName;

    public Photon.Realtime.Player Player { get; private set; }
    public bool isReady = false;


    private void Awake()
    {
        playerName = GetComponentInChildren<TMP_Text>();
    }

    public void SetPlayerInfo(Photon.Realtime.Player _player)
    {
        Player = _player;
        SetPlayerText(Player);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (targetPlayer != null && targetPlayer == Player)
        {
            if (changedProps.ContainsKey("RandomNumber"))
            {
                SetPlayerText(targetPlayer);
            }

        }
    }

    private void SetPlayerText(Photon.Realtime.Player player)
    {
        int result = -1;
        if (Player.CustomProperties.ContainsKey("RandomNumber"))
        {
            result = (int)Player.CustomProperties["RandomNumber"];
        }
        playerName.text = result.ToString() + ", " + Player.NickName;
    }
}
