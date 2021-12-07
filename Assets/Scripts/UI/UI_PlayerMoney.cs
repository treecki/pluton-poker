using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerMoney : UI_Text
{
    PokerStateMachine psm;

    PlayerObject pObject;

    float playerMoney;

    public PlayerMoneyType moneyType;

    protected override void Awake()
    {
        base.Awake();
        pObject = GetComponentInParent<PlayerObject>();
        psm = PokerStateMachine.PSM;
    }

    protected virtual void Update()
    {

        switch (moneyType)
        {
            case PlayerMoneyType.BANK:
                if (!playerMoney.Equals(pObject.Player.PlayerMoney))
                {
                    playerMoney = pObject.Player.PlayerMoney;
                    SetText("$" + playerMoney);
                }
                break;
            case PlayerMoneyType.BET:
                if (!playerMoney.Equals(pObject.Player.CurrBet.amount))
                {
                    playerMoney = pObject.Player.CurrBet.amount;
                    SetText("$" + playerMoney);
                }
                break;
        }
    }

    public enum PlayerMoneyType
    {
        BANK, BET
    } 
}
