using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PotText : UI_Text
{
    PokerStateMachine psm;

    float potAmount;

    protected override void Awake()
    {
        base.Awake();
        psm = GameObject.FindGameObjectWithTag("GameController").GetComponent<PokerStateMachine>();
    }

    protected virtual void Update()
    {
        if (potAmount != psm.BetManager.PotAmount)
        {
            potAmount = psm.BetManager.PotAmount;
            SetText("Current Pot: $" + potAmount);
        }
    }
}
