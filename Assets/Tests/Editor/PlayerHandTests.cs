using NUnit.Framework;
using System.Linq;
using UnityEngine;

public class PlayerHandTests
{
    [Test]
    public void EvaluateHand_DetectsRoyalFlush()
    {
        var result = Evaluate(
            Hole(Card(SUIT.H, VALUE.VA), Card(SUIT.H, VALUE.VK)),
            Board(Card(SUIT.H, VALUE.VQ), Card(SUIT.H, VALUE.VJ), Card(SUIT.H, VALUE.V10), Card(SUIT.C, VALUE.V2), Card(SUIT.D, VALUE.V3)));

        Assert.AreEqual(Hand.RoyalFlush, result.Hand);
        Assert.AreEqual(Hand.RoyalFlush, result.Info.HandCombo);
        Assert.AreEqual((int)VALUE.VA, result.Info.HighCard);
    }

    [Test]
    public void EvaluateHand_DetectsStraightFlush()
    {
        var result = Evaluate(
            Hole(Card(SUIT.S, VALUE.V8), Card(SUIT.S, VALUE.V7)),
            Board(Card(SUIT.S, VALUE.V6), Card(SUIT.S, VALUE.V5), Card(SUIT.S, VALUE.V4), Card(SUIT.H, VALUE.VK), Card(SUIT.D, VALUE.V2)));

        Assert.AreEqual(Hand.StraightFlush, result.Hand);
        Assert.AreEqual(Hand.StraightFlush, result.Info.HandCombo);
        Assert.AreEqual((int)VALUE.V8, result.Info.HighCard);
    }

    [Test]
    public void EvaluateHand_DetectsFourOfAKind_AndUsesCorrectHandTag()
    {
        var result = Evaluate(
            Hole(Card(SUIT.H, VALUE.V9), Card(SUIT.S, VALUE.V9)),
            Board(Card(SUIT.D, VALUE.V9), Card(SUIT.C, VALUE.V9), Card(SUIT.H, VALUE.VA), Card(SUIT.S, VALUE.V3), Card(SUIT.D, VALUE.V2)));

        Assert.AreEqual(Hand.FourKind, result.Hand);
        Assert.AreEqual(Hand.FourKind, result.Info.HandCombo);
        Assert.AreEqual((int)VALUE.VA, result.Info.HighCard);
    }

    [Test]
    public void EvaluateHand_DetectsFullHouse()
    {
        var result = Evaluate(
            Hole(Card(SUIT.H, VALUE.VK), Card(SUIT.S, VALUE.VK)),
            Board(Card(SUIT.D, VALUE.VK), Card(SUIT.C, VALUE.V10), Card(SUIT.H, VALUE.V10), Card(SUIT.S, VALUE.V4), Card(SUIT.D, VALUE.V2)));

        Assert.AreEqual(Hand.FullHouse, result.Hand);
        Assert.AreEqual(Hand.FullHouse, result.Info.HandCombo);
    }

    [Test]
    public void EvaluateHand_DetectsFlush_WithSixSuitedCards()
    {
        var result = Evaluate(
            Hole(Card(SUIT.H, VALUE.VA), Card(SUIT.H, VALUE.V8)),
            Board(Card(SUIT.H, VALUE.VK), Card(SUIT.H, VALUE.V10), Card(SUIT.H, VALUE.V5), Card(SUIT.H, VALUE.V2), Card(SUIT.C, VALUE.V3)));

        Assert.AreEqual(Hand.Flush, result.Hand);
        Assert.AreEqual(Hand.Flush, result.Info.HandCombo);
        Assert.AreEqual((int)VALUE.VA, result.Info.HighCard);
    }

    [Test]
    public void EvaluateHand_DetectsFlush_WithSevenSuitedCards()
    {
        var result = Evaluate(
            Hole(Card(SUIT.S, VALUE.VA), Card(SUIT.S, VALUE.VJ)),
            Board(Card(SUIT.S, VALUE.V9), Card(SUIT.S, VALUE.V7), Card(SUIT.S, VALUE.V5), Card(SUIT.S, VALUE.V3), Card(SUIT.S, VALUE.V2)));

        Assert.AreEqual(Hand.Flush, result.Hand);
        Assert.AreEqual(Hand.Flush, result.Info.HandCombo);
        Assert.AreEqual((int)VALUE.VA, result.Info.HighCard);
    }

    [Test]
    public void EvaluateHand_DetectsWheelStraight()
    {
        var result = Evaluate(
            Hole(Card(SUIT.H, VALUE.VA), Card(SUIT.S, VALUE.V2)),
            Board(Card(SUIT.D, VALUE.V3), Card(SUIT.C, VALUE.V4), Card(SUIT.H, VALUE.V5), Card(SUIT.S, VALUE.VK), Card(SUIT.D, VALUE.V9)));

        Assert.AreEqual(Hand.Straight, result.Hand);
        Assert.AreEqual(Hand.Straight, result.Info.HandCombo);
        Assert.AreEqual((int)VALUE.V5, result.Info.HighCard);
    }

    [Test]
    public void EvaluateHand_PrefersHigherStraightWindow()
    {
        var result = Evaluate(
            Hole(Card(SUIT.H, VALUE.V9), Card(SUIT.S, VALUE.V8)),
            Board(Card(SUIT.D, VALUE.V7), Card(SUIT.C, VALUE.V6), Card(SUIT.H, VALUE.V5), Card(SUIT.S, VALUE.V4), Card(SUIT.D, VALUE.V2)));

        Assert.AreEqual(Hand.Straight, result.Hand);
        Assert.AreEqual((int)VALUE.V9, result.Info.HighCard);
    }

    [Test]
    public void EvaluateHand_UsesHighCardToBreakPairTie()
    {
        var stronger = Evaluate(
            Hole(Card(SUIT.H, VALUE.VA), Card(SUIT.S, VALUE.VA)),
            Board(Card(SUIT.D, VALUE.VK), Card(SUIT.C, VALUE.VQ), Card(SUIT.H, VALUE.V9), Card(SUIT.S, VALUE.V4), Card(SUIT.D, VALUE.V2)));

        var weaker = Evaluate(
            Hole(Card(SUIT.H, VALUE.VA), Card(SUIT.S, VALUE.VA)),
            Board(Card(SUIT.D, VALUE.VJ), Card(SUIT.C, VALUE.V10), Card(SUIT.H, VALUE.V8), Card(SUIT.S, VALUE.V4), Card(SUIT.D, VALUE.V2)));

        Assert.AreEqual(Hand.OnePair, stronger.Hand);
        Assert.Greater(stronger.Info.HighCard, weaker.Info.HighCard);
    }

    [Test]
    public void EvaluateHand_ResetsStateBetweenEvaluations()
    {
        var playerHand = Hole(Card(SUIT.H, VALUE.VA), Card(SUIT.S, VALUE.VK));

        var first = Evaluate(playerHand,
            Board(Card(SUIT.D, VALUE.VQ), Card(SUIT.C, VALUE.VJ), Card(SUIT.H, VALUE.V10), Card(SUIT.S, VALUE.V2), Card(SUIT.D, VALUE.V3)));
        var second = Evaluate(playerHand,
            Board(Card(SUIT.H, VALUE.V9), Card(SUIT.S, VALUE.V9), Card(SUIT.D, VALUE.V4), Card(SUIT.C, VALUE.V3), Card(SUIT.H, VALUE.V2)));

        Assert.AreEqual(Hand.Straight, first.Hand);
        Assert.AreEqual(Hand.OnePair, second.Hand);
    }

    private static (Hand Hand, HandInfo Info) Evaluate(PlayerHand playerHand, GenHand board)
    {
        var hand = playerHand.EvaluateHand(board);
        return (hand, playerHand.FinalHandInfo);
    }

    private static PlayerHand Hole(params Card[] cards)
    {
        var hand = new PlayerHand();
        foreach (var card in cards)
        {
            hand.DealCard(card);
        }

        return hand;
    }

    private static GenHand Board(params Card[] cards)
    {
        var board = new GenHand();
        foreach (var card in cards)
        {
            board.DealCard(card);
        }

        return board;
    }

    private static Card Card(SUIT suit, VALUE value)
    {
        return new Card { MySuit = suit, MyValue = value };
    }
}
