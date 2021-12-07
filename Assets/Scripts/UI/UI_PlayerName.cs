using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerName : UI_Text
{
    PokerStateMachine psm;

    PlayerObject pObject;

    string playerName = "";

    protected override void Awake()
    {
        base.Awake();
        pObject = GetComponentInParent<PlayerObject>();
        psm = GameObject.FindGameObjectWithTag("GameController").GetComponent<PokerStateMachine>();
    }

    protected void Start()
    {
        playerName = pObject.Username;
        SetText(playerName);
    }

    protected virtual void Update()
    {
        if (!playerName.Equals(pObject.Username))
        {
            playerName = pObject.Username;
            SetText(playerName);
        }
    }
}
