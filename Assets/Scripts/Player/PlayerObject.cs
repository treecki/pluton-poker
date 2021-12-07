using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerObject : MonoBehaviour
{

    private Player player;
    public Player Player { get { return player; } }

    private int playerNum;

    private PokerStateMachine psm;

    public Transform cardHandTransform;

    private string username;
    public string Username { get { return username; } }

    private void Awake()
    {
        psm = PokerStateMachine.PSM;
    }

    public void SetHand(Player _player)
    {
        player = _player;
        playerNum = player.PlayerID;
        username = "Player " + playerNum;
    }

    public void CreateCards()
    {
        List<Card> cardsInHand = psm.GetPlayerWithID(playerNum).PlayerHand.DealtHand;

        foreach (Card c in cardsInHand)
        {
            GameObject spawnedCard = Instantiate(psm.cardPrefab);
            spawnedCard.transform.parent = cardHandTransform;
            spawnedCard.GetComponent<CardObject>().SetCard(c);
        }
    }

    public void ClearCards()
    {
        CardObject[] cards = cardHandTransform.GetComponentsInChildren<CardObject>();
        foreach (CardObject c in cards)
        {
            Destroy(c.gameObject);
        }
    }

    public void Call()
    {
        Debug.Log("Highest bet is "+ psm.HighestBet.amount);
        bool callBet = player.RaiseCurrentBetTo(psm.HighestBet.amount);

        if (!callBet)
        {
            Debug.Log("Cannot call bet!");
        }
    }

    public void Raise(float amount)
    {
        bool raiseBet = player.RaiseCurrentBetTo(psm.HighestBet.amount + amount);

        if (!raiseBet)
        {
            Debug.Log("Cannot raise bet!");
        }
    }

    public void Fold()
    {
        bool foldPlayer = player.SetFolded(true);

        if (!foldPlayer)
        {
            Debug.Log("Cannot fold right now!");
        }
    }

    public void Check()
    {
        if (psm.HighestBet.amount == 0)
        {
            bool check = player.RaiseCurrentBetTo(psm.HighestBet.amount);

            if (!check)
            {
                Debug.Log("Cannot check!");
            }
        }
        else
        {
            Debug.Log("Cannot check!");
        }

    }

}
