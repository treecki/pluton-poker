# Pluton Poker — Current Architecture Overview

## High-level snapshot

This project already has two important pillars in place:
1. **Core local Texas Hold'em game loop** (state machine + betting + hand evaluation)
2. **Photon lobby/room scaffolding** (connect, room list, player list, ready/start flow)

What it does **not** yet have is a full authoritative multiplayer gameplay layer that synchronizes actual hand state and actions across clients.

---

## Runtime architecture

## 1) Core game domain (local simulation)

### Entry point: `PokerStateMachine`
- File: `Assets/Scripts/Poker/PokerStateMachine.cs`
- Responsibilities:
  - Initializes players, deck, betting manager, and game states
  - Owns player list and in-round action queue
  - Creates/tears down hand/river visuals
  - Transitions state machine stages for each round

### State machine flow
- `GameStateRoundStart` -> `GameStateDeal` -> `GameStateBetting` -> `GameStateRoundEnd`
- File group: `Assets/Scripts/Poker/GameState*.cs`

Round lifecycle today:
1. Build fresh deck + queue active players
2. Deal private cards / flop / turn / river in phases
3. Run betting cycle between deal phases
4. Evaluate winner and award pot
5. Restart round manually via UI

### Betting + turns
- `BettingManager` owns:
  - pot
  - blind amounts
  - blind rotation logic
- `GameStateBetting` owns:
  - whose turn it is
  - highest bet tracking
  - transition rules to next state

### Player model
- `PokerPlayer` is currently a pure C# data object:
  - money
  - hand
  - current bet
  - folded state
  - event callback for action submission

### Hand evaluation
- `PlayerHand` + `GenHand` evaluate best combination from 7 cards.
- This appears mostly implemented, but there are likely correctness edge cases (see risks section).

---

## 2) Visual/UI layer

- `PlayerObject`, `RiverObject`, `CardObject` create and render card visuals.
- UI scripts (`UI_*`) subscribe to state events and render simple status:
  - round end winner text
  - blind indicators
  - pot text
  - per-player money/name indicators

This is a straightforward adapter layer from gameplay state to scene objects.

---

## 3) Networking layer (Photon)

### Current capabilities
- Connect to Photon (`TestConnect`)
- Create/join room (`CreateRoomMenu`, `RoomListing`, `RoomListingsMenu`)
- Show players in room (`PlayerListingsMenu`)
- Ready/start gate in room
- Master client can load gameplay scene

### Current limitation
Gameplay itself is not yet truly network-authoritative:
- No synchronized deck seed/deal stream
- No authoritative turn executor
- No robust action replication/validation per player
- No deterministic reconciliation for disconnect/rejoin

So multiplayer plumbing exists in lobby UX, but the **actual poker round simulation is still effectively local**.

---

## Scene / system split (inferred)

- **Lobby scene**: Photon connection + room list + room UI
- **Game scene**: local poker simulation and table UI

`PhotonNetwork.AutomaticallySyncScene = true` is enabled, so master-driven scene loading is already wired.

---

## Key technical risks observed

1. **Potential hand-evaluation correctness bugs**
   - Example: flush detection checks `== 5` suit count (not `>= 5`), which can miss 6/7-card flush scenarios.
   - Example: straight detection window loop likely misses one valid starting window.
   - Example: `FourOfKind()` appears to add hand as `ThreeKind` (likely typo bug).

2. **No authoritative multiplayer game state owner**
   - Any client-side mismatch can desync bets/pot/winner.

3. **Local data model not mapped to Photon actor IDs**
   - `PokerPlayer` currently index-based (`PlayerID`) and local-first.

4. **No recovery model for disconnect/rejoin during a hand**

5. **No side-pot/all-in logic yet**
   - Current bet model seems geared to simple call/raise/fold loop.

---

## What is in good shape already

- Clean conceptual state machine layout (great foundation)
- Clear betting phase boundaries
- Working room/lobby shell in Photon
- Reusable table/card prefab architecture

This is a very good MVP base — it mainly needs correctness hardening and a server-authoritative multiplayer pass.
