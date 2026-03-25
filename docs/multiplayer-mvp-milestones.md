# Pluton Poker — Multiplayer MVP Milestone Plan

Goal: ship a **playable, stable online Texas Hold'em MVP** (private rooms, 2–6 players, one table, no economy backend).

## MVP scope (explicit)

### In scope
- Create/join private room
- Sit players in fixed seats
- Start hand when room is ready
- Authoritative dealing/turns/bets/winner
- Fold/call/check/raise actions
- Pot payout to winner
- Basic reconnect handling (same room, mid-hand snapshot restore)

### Out of scope (post-MVP)
- Matchmaking/ranked
- Tournaments
- Cosmetics/store
- Full side-pot complexity (can be added in v2)
- Anti-cheat hardening beyond basic server authority

---

## Milestone 0 — Code Health + Rule Correctness (must do first)

Purpose: avoid building networking on top of broken hand logic.

Tasks:
- Add deterministic test coverage for hand evaluator (`PlayerHand`)
  - royal flush, straight flush, quads, full house, flush(6/7 cards), wheel straight, tie breakers
- Fix known likely bugs:
  - flush `== 5` -> `>= 5`
  - straight window iteration bounds
  - `FourOfKind` hand-tag typo
  - any pair/full-house edge-case index errors found by tests
- Add a tiny debug test harness to run scripted hands quickly in-editor

Done when:
- Hand-ranking tests pass with stable expected outcomes.

---

## Milestone 1 — Authoritative Multiplayer Game Core

Purpose: make one source of truth for game state.

Tasks:
- Define a serializable `PokerGameSnapshot`:
  - phase, dealer/blinds, current turn actor, pot, highest bet
  - player chips/current bet/folded/all-in
  - community cards + masked hole card ownership
- Introduce network authority model:
  - Master client acts as temporary authority for MVP (or Photon room owner logic)
  - only authority mutates round state
- Convert player identity from local index to Photon ActorNumber mapping
- Broadcast state deltas via RPC/Event code

Done when:
- Non-authority clients are pure viewers/inputs; authority resolves state.

---

## Milestone 2 — Turn + Action Replication

Purpose: make betting rounds reliable online.

Tasks:
- Action command pipeline (`Fold`, `Check`, `Call`, `Raise(amount)`)
  - client sends request
  - authority validates + applies
  - authority broadcasts accepted action + new snapshot
- Enforce turn timeout behavior (basic auto-fold for MVP)
- Prevent out-of-turn input on all clients
- Keep UI in sync with authority snapshot after every action

Done when:
- 2+ clients can complete full preflop/flop/turn/river action loops without desync.

---

## Milestone 3 — Deal/Reveal Sync + Winner Resolution

Purpose: ensure every client sees consistent cards and outcomes.

Tasks:
- Authority-generated shuffled deck seed/order
- Deterministic dealing events for hole cards/community cards
- Hide opponent hole cards until showdown unless folded reveal is desired
- Authority computes winner and payout, then publishes final hand result payload

Done when:
- All clients agree on same board, same winner, same chip totals.

---

## Milestone 4 — Room-to-Table UX Glue

Purpose: get an actual playable loop from lobby to repeated hands.

Tasks:
- Seat assignment and display names on table UI
- Ready/start checks in room tied to game start conditions
- End-of-hand -> next hand transition
- Add minimal in-game status text:
  - whose turn
  - call amount
  - pot
  - last action

Done when:
- Friends can create room, start game, and play multiple hands smoothly.

---

## Milestone 5 — Resilience Pass (MVP hardening)

Purpose: avoid rage-quit bugs and stuck matches.

Tasks:
- Rejoin same room restore:
  - on rejoin, authority sends latest snapshot
- Master client migration fallback:
  - if authority leaves, pause hand + elect new authority + recover snapshot
- Guard rails:
  - no stuck turn states
  - no negative chip bugs
  - no phantom duplicated actions

Done when:
- Basic disconnect/reconnect paths recover without killing session.

---

## Immediate next 10 tasks (small-bite execution)

1. Create `docs/rules-test-cases.md` with 20 canonical hand scenarios.
2. Add unit-test scaffold for hand eval (EditMode tests).
3. Fix + verify flush detection for 6/7 suited cards.
4. Fix + verify straight window scanning bounds.
5. Fix quads hand labeling bug.
6. Define `PokerGameSnapshot` C# model.
7. Add Photon actor-to-seat mapping model.
8. Add authority-only state mutation gate in game manager.
9. Add one action RPC (`Fold`) end-to-end as spike.
10. Add snapshot broadcast + UI refresh after action.

---

## Recommended order for you right now

If you want fastest path to confidence:
1. **Milestone 0 first** (rule correctness)
2. Then **Milestone 1 + 2** (authoritative loop)
3. Then **Milestone 3** (deal/reveal/winner sync)
4. Then UX/resilience polish

That sequence gives you a playable multiplayer poker MVP without pretending the hard parts are solved.
