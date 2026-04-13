using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_RoundManager : MonoBehaviour
{
    PokerStateMachine psm;

    public GameObject RoundOver;
    public UI_Text TableStatusText;
    private UI_Text roundEndText;
    private string lastStatusMessage;

    private void Awake()
    {
        roundEndText = RoundOver.GetComponentInChildren<UI_Text>();
        RoundOver.SetActive(false);
        SetTableStatus("Waiting for table state...");
    }

    private void Start()
    {
        psm = PokerStateMachine.PSM;
        psm.StateRoundEnd.OnStateStart += ShowRoundEnd;
        psm.StateRoundEnd.OnStateEnd += HideRoundEnd;
        psm.OnRoundResultSnapshotApplied += ShowRoundResultFromSnapshot;
        psm.OnSnapshotApplied += RefreshTableStatusFromSnapshot;
        RefreshLocalTableStatus();
    }

    private void OnDisable()
    {
        if (psm == null)
        {
            return;
        }

        psm.StateRoundEnd.OnStateStart -= ShowRoundEnd;
        psm.StateRoundEnd.OnStateEnd -= HideRoundEnd;
        psm.OnRoundResultSnapshotApplied -= ShowRoundResultFromSnapshot;
        psm.OnSnapshotApplied -= RefreshTableStatusFromSnapshot;
    }


    private void RefreshLocalTableStatus()
    {
        if (psm == null)
        {
            return;
        }

        string currentTurn = GetCurrentTurnDisplayName();
        float highestBet = psm.HighestBet.amount;
        float callAmount = 0f;
        PokerPlayer localPlayer = psm.GetPlayerForLocalClient();
        if (localPlayer != null)
        {
            callAmount = Mathf.Max(0f, highestBet - localPlayer.CurrBet.amount);
        }

        string status = "Turn: " + currentTurn
            + "\nPot: " + psm.BetManager.PotAmount
            + "\nCall: " + callAmount
            + "\nHighest Bet: " + highestBet;
        SetTableStatus(status);
    }

    private void RefreshTableStatusFromSnapshot(PokerGameSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        string turnName = GetSnapshotTurnDisplayName(snapshot);
        float localCurrentBet = 0f;
        PlayerSnapshot localSnapshot = snapshot.Players != null ? snapshot.Players.Find(player => player.IsLocalPlayer) : null;
        if (localSnapshot != null)
        {
            localCurrentBet = localSnapshot.CurrentBet;
        }

        float callAmount = Mathf.Max(0f, snapshot.HighestBet - localCurrentBet);
        string status = "Turn: " + turnName
            + "\nPot: " + snapshot.Pot
            + "\nCall: " + callAmount
            + "\nHighest Bet: " + snapshot.HighestBet;

        if (snapshot.ShowdownResolved && !string.IsNullOrEmpty(snapshot.WinningMessage))
        {
            status += "\nLast Action: " + snapshot.WinningMessage;
        }
        else if (!string.IsNullOrEmpty(snapshot.Phase))
        {
            status += "\nPhase: " + snapshot.Phase;
        }

        SetTableStatus(status);
    }

    private string GetCurrentTurnDisplayName()
    {
        if (psm == null)
        {
            return "Waiting";
        }

        int actorNumber = psm.GetCurrentTurnActorNumber();
        PokerPlayer currentPlayer = psm.PlayersInGame.Find(player => player.ActorNumber == actorNumber);
        return currentPlayer != null ? currentPlayer.DisplayName : "Waiting";
    }

    private string GetSnapshotTurnDisplayName(PokerGameSnapshot snapshot)
    {
        if (snapshot == null || snapshot.Players == null)
        {
            return "Waiting";
        }

        PlayerSnapshot currentPlayer = snapshot.Players.Find(player => player.ActorNumber == snapshot.CurrentTurnActorNumber);
        if (currentPlayer == null)
        {
            return snapshot.ShowdownResolved ? "Round complete" : "Waiting";
        }

        return string.IsNullOrEmpty(currentPlayer.DisplayName) ? "Player " + currentPlayer.ActorNumber : currentPlayer.DisplayName;
    }

    private void SetTableStatus(string status)
    {
        lastStatusMessage = status;
        if (TableStatusText != null)
        {
            TableStatusText.SetText(status);
        }
        else
        {
            Debug.Log("TableStatus: " + status.Replace("\n", " | "));
        }
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

    private void ShowRoundResultFromSnapshot(PokerGameSnapshot snapshot)
    {
        if (snapshot == null || !snapshot.ShowdownResolved)
        {
            return;
        }

        RoundOver.SetActive(true);
        roundEndText.SetText("Round " + psm.BetManager.RoundNumber + " over!\n " + snapshot.WinningMessage);
    }

    public void RestartRound()
    {
        psm.RestartRound();
    }



}
