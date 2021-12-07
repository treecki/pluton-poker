using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_RoundManager : MonoBehaviour
{
    PokerStateMachine psm;

    public GameObject RoundOver;
    private UI_Text roundEndText;

    private void Awake()
    {
        roundEndText = RoundOver.GetComponentInChildren<UI_Text>();
        RoundOver.SetActive(false);
    }

    private void Start()
    {
        psm = PokerStateMachine.PSM;
        psm.StateRoundEnd.OnStateStart += ShowRoundEnd;
        psm.StateRoundEnd.OnStateEnd += HideRoundEnd;
    }

    private void OnDisable()
    {
        psm.StateRoundEnd.OnStateStart -= ShowRoundEnd;
        psm.StateRoundEnd.OnStateEnd -= HideRoundEnd;
    }

    private void ShowRoundEnd()
    {
        RoundOver.SetActive(true);
        roundEndText.SetText("Round " + psm.BetManager.RoundNumber + " over!\n " + psm.StateRoundEnd.WinningMessage);
    }

    private void HideRoundEnd()
    {
        RoundOver.SetActive(false);
    }

    public void RestartRound()
    {
        psm.RestartRound();
    }



}
