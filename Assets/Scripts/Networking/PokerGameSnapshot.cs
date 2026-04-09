using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[Serializable]
public class PokerGameSnapshot
{
    public const string DealPhasePrefix = "Deal.";

    public string Phase;
    public int DealerActorNumber;
    public int SmallBlindActorNumber;
    public int BigBlindActorNumber;
    public int CurrentTurnActorNumber;
    public float Pot;
    public float HighestBet;
    public int CommunityCardCount;
    public List<CardSnapshot> CommunityCards = new List<CardSnapshot>();
    public List<PlayerSnapshot> Players = new List<PlayerSnapshot>();
    public int WinningPlayerSeatIndex = -1;
    public int WinningPlayerActorNumber = -1;
    public string WinningMessage;
    public string WinningHand;
    public bool ShowdownResolved;
    public string UpdatedBy;
    public string UpdatedAtUtc;

    public static string BuildDealPhaseName(DealingState dealState)
    {
        return DealPhasePrefix + dealState;
    }

    // A snapshot is a read-only picture of the authoritative table state at a moment in time.
    // We capture it on the authority side so later milestones can broadcast or rehydrate from
    // one consistent model instead of rebuilding state from scattered scene objects.
    public static PokerGameSnapshot Capture(PokerStateMachine psm, string phaseOverride = null)
    {
        PokerGameSnapshot snapshot = new PokerGameSnapshot();
        snapshot.Phase = phaseOverride ?? psm.GetCurrentPhaseName();
        snapshot.DealerActorNumber = psm.GetDealerActorNumber();
        snapshot.SmallBlindActorNumber = psm.GetBlindActorNumber(true);
        snapshot.BigBlindActorNumber = psm.GetBlindActorNumber(false);
        snapshot.CurrentTurnActorNumber = psm.GetCurrentTurnActorNumber();
        snapshot.Pot = psm.BetManager != null ? psm.BetManager.PotAmount : 0f;
        snapshot.HighestBet = psm.HighestBet.amount;
        snapshot.CommunityCardCount = psm.riverHand != null ? psm.riverHand.CardHand.Count : 0;
        snapshot.WinningPlayerSeatIndex = psm.StateRoundEnd != null && psm.StateRoundEnd.WinningPlayer != null ? psm.StateRoundEnd.WinningPlayer.PlayerID : -1;
        snapshot.WinningPlayerActorNumber = psm.StateRoundEnd != null && psm.StateRoundEnd.WinningPlayer != null ? psm.StateRoundEnd.WinningPlayer.ActorNumber : -1;
        snapshot.WinningMessage = psm.StateRoundEnd != null ? psm.StateRoundEnd.WinningMessage : string.Empty;
        snapshot.WinningHand = psm.StateRoundEnd != null && psm.StateRoundEnd.WinningPlayer != null && psm.StateRoundEnd.WinningPlayer.PlayerHand != null && psm.StateRoundEnd.WinningPlayer.PlayerHand.FinalHandInfo != null
            ? psm.StateRoundEnd.WinningPlayer.PlayerHand.FinalHandInfo.HandCombo.ToString()
            : string.Empty;
        snapshot.ShowdownResolved = snapshot.WinningPlayerSeatIndex >= 0 && phaseOverride == PokerAuthorityController.SnapshotPhaseRoundEndResolved;
        bool revealAllHoleCards = snapshot.ShowdownResolved && psm.StateRoundEnd != null && psm.StateRoundEnd.ShouldRevealAllRemainingHands();
        snapshot.UpdatedBy = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.NickName : "Offline";
        snapshot.UpdatedAtUtc = DateTime.UtcNow.ToString("o");

        if (psm.riverHand != null)
        {
            snapshot.CommunityCards = psm.riverHand.CardHand.Select(CardSnapshot.FromCard).ToList();
        }

        if (psm.PlayersInGame != null)
        {
            snapshot.Players = psm.PlayersInGame.Select(player => PlayerSnapshot.FromPlayer(psm, player, revealAllHoleCards)).ToList();
        }

        return snapshot;
    }
}

[Serializable]
public class PlayerSnapshot
{
    public int SeatIndex;
    public int ActorNumber;
    public string DisplayName;
    public float Chips;
    public float CurrentBet;
    public bool Folded;
    public bool AllIn;
    public bool IsLocalPlayer;
    public bool HoleCardsVisibleToLocalClient;
    public List<CardSnapshot> HoleCards = new List<CardSnapshot>();

    public static PlayerSnapshot FromPlayer(PokerStateMachine psm, PokerPlayer player, bool revealAllHoleCards = false)
    {
        PlayerSnapshot snapshot = new PlayerSnapshot();
        snapshot.SeatIndex = player.PlayerID;
        snapshot.ActorNumber = player.ActorNumber;
        snapshot.DisplayName = player.DisplayName;
        snapshot.Chips = player.PlayerMoney;
        snapshot.CurrentBet = player.CurrBet.amount;
        snapshot.Folded = player.IsFolded;
        snapshot.AllIn = Mathf.Approximately(player.PlayerMoney, 0f);
        snapshot.IsLocalPlayer = player.ActorNumber == PhotonNetwork.LocalPlayer?.ActorNumber;
        snapshot.HoleCardsVisibleToLocalClient = player.CanRevealHoleCardsTo(PhotonNetwork.LocalPlayer, revealAllHoleCards);

        if (snapshot.HoleCardsVisibleToLocalClient)
        {
            snapshot.HoleCards = player.PlayerHand.DealtHand.Select(CardSnapshot.FromCard).ToList();
        }
        else
        {
            snapshot.HoleCards = player.PlayerHand.DealtHand.Select(_ => CardSnapshot.Hidden()).ToList();
        }

        return snapshot;
    }
}

[Serializable]
public class CardSnapshot
{
    public string Suit;
    public string Value;
    public bool IsHidden;

    public static CardSnapshot FromCard(Card card)
    {
        return new CardSnapshot
        {
            Suit = card.MySuit.ToString(),
            Value = card.MyValue.ToString(),
            IsHidden = false
        };
    }

    public Card ToCard()
    {
        if (IsHidden)
        {
            return null;
        }

        SUIT parsedSuit;
        VALUE parsedValue;
        if (!Enum.TryParse(Suit, out parsedSuit) || !Enum.TryParse(Value, out parsedValue))
        {
            return null;
        }

        return new Card
        {
            MyValue = parsedValue,
            MySuit = parsedSuit
        };
    }

    public static CardSnapshot Hidden()
    {
        return new CardSnapshot
        {
            Suit = "Hidden",
            Value = "Hidden",
            IsHidden = true
        };
    }
}
