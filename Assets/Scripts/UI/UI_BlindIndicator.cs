using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BlindIndicator : MonoBehaviour
{
    private Image indicatorImage;
    private PlayerObject playerObject;
    private BettingManager bm; 

    public Color smallColor;
    public Color bigColor;
    public Color nonColor;

    private void Awake()
    {
        indicatorImage = GetComponent<Image>();
        playerObject = GetComponentInParent<PlayerObject>();
        bm = PokerStateMachine.PSM.BetManager;
    }

    private void Update()
    {
        if (bm.BigBlindPlayer.PlayerID == playerObject.Player.PlayerID)
        {
            indicatorImage.color = bigColor;
        }
        else if (bm.SmallBlindPlayer.PlayerID == playerObject.Player.PlayerID)
        {
            indicatorImage.color = smallColor;
        }
        else
        {
            indicatorImage.color = nonColor;
        }
    }
}
