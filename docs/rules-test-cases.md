# Poker Rules Test Cases

Canonical hand-evaluation scenarios for Milestone 0.

## Royal Flush
- Hole: Ah Kh
- Board: Qh Jh Th 2c 3d
- Expected: Royal Flush

## Straight Flush
- Hole: 8s 7s
- Board: 6s 5s 4s Kh 2d
- Expected: Straight Flush, high card 8

## Four of a Kind
- Hole: 9h 9s
- Board: 9d 9c Ah 3s 2d
- Expected: Four of a Kind, kicker A

## Full House
- Hole: Kh Ks
- Board: Kd Tc Th 4s 2d
- Expected: Full House, Kings over Tens

## Flush with Six Suited Cards
- Hole: Ah 8h
- Board: Kh Th 5h 2h 3c
- Expected: Flush, high card A

## Flush with Seven Suited Cards
- Hole: As Js
- Board: 9s 7s 5s 3s 2s
- Expected: Flush, high card A

## Wheel Straight
- Hole: Ah 2s
- Board: 3d 4c 5h Ks 9d
- Expected: Straight, high card 5

## Higher Straight Window
- Hole: 9h 8s
- Board: 7d 6c 5h 4s 2d
- Expected: Straight, high card 9

## Pair Tie Breaker
- Hand A Hole: Ah As
- Hand A Board: Kd Qc 9h 4s 2d
- Expected: Pair of Aces, kicker K
- Hand B Hole: Ah As
- Hand B Board: Jd Tc 8h 4s 2d
- Expected: Pair of Aces, kicker J
- Winner: Hand A

## State Reset Between Evaluations
- Evaluate one straight, then one pair using the same PlayerHand instance.
- Expected: second result reflects only current cards.
