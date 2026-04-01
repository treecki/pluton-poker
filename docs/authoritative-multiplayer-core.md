# Milestone 1 — Authoritative Multiplayer Game Core

This pass establishes the authority/snapshot backbone for multiplayer MVP.

## Added
- `PokerAuthorityController`
  - central authority gate
  - blocks non-authority state mutation
  - publishes snapshots after key state changes
- `PokerGameSnapshot`
  - serializable snapshot of phase, blinds, turn actor, pot, highest bet, community cards, and per-player state
  - hides opponent hole cards for non-owning clients
- Photon seat mapping on `PokerPlayer`
  - `ActorNumber`
  - `DisplayName`
  - ownership helper for card visibility

## Updated
- `PokerStateMachine`
  - requires authority controller
  - maps Photon players to seats
  - only authority starts/restarts rounds
  - publishes snapshots around state transitions
- `GameStateRoundStart`, `GameStateDeal`, `GameStateBetting`, `GameStateRoundEnd`
  - authority checks before mutating shared state
  - snapshot publication during core round flow
- `PlayerObject`
  - local client can only control the seat mapped to its Photon actor

## What this milestone does now
- Establishes one source of truth for game-state mutation: the Photon master client
- Tracks actor-number-to-seat mapping in gameplay state
- Produces a consistent serializable snapshot model for later RPC/event replication
- Prevents local clients from controlling seats they do not own

## What is intentionally deferred to Milestone 2+
- actual RPC/event broadcast of commands and snapshots
- remote command validation pipeline (`Fold`, `Check`, `Call`, `Raise`)
- reconnect restoration from received snapshots
- full non-authority client rehydration from networked snapshot data

## Notes
This is the architecture spine, not the full replication layer. It sets up the model so Milestone 2 can wire action replication cleanly instead of bolting it on to purely local state.
