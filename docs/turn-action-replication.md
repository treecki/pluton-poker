# Milestone 2 — Turn + Action Replication

This pass builds the command path on top of the Milestone 1 authority backbone.

## Added
- `PokerActionCommand`
  - explicit command model for `Fold`, `Check`, `Call`, `Raise`, and `AutoFold`
  - used as the network payload for betting actions: clients send it to authority, then authority rebroadcasts the resolved action
- Photon event codes for:
  - action request -> authority
  - action resolved -> all clients

## Photon event code note
`ActionRequestEventCode = 41` and `ActionResolvedEventCode = 42` are just project-level custom Photon event IDs.
They are not special built-in Photon values — they simply need to be unique within our own multiplayer event usage.

## Updated
- `PokerAuthorityController`
  - receives command events on master client
  - relays resolved actions to all clients
- `GameStateBetting`
  - translates local bets into action commands
  - validates actor ownership and current-turn ownership on authority
  - applies resolved commands through a single path
  - adds turn timeout with auto-fold
- `PokerPlayer`
  - supports applying an authority-resolved bet delta without re-emitting local events
- `PokerStateMachine`
  - ticks betting timeout logic every frame
  - excludes folded players from active-player calculations

## What this milestone does now
- clients submit action requests to authority instead of directly mutating shared state
- authority validates turn ownership and actor ownership before applying actions
- authority broadcasts resolved actions to all clients
- turn timeout can auto-fold a stalled player
- out-of-turn and wrong-seat actions are rejected at the authority layer

## Current limitation / next step
The current remote-client resolved-action handling is intentionally light: it updates snapshot publication flow, but a full remote rehydration/replay of queue/UI state from the resolved action + snapshot still needs a follow-up pass. This means the networking spine and command validation path are in place, but the last 10-20% of UX sync polish will likely be finished alongside deal/reveal sync in Milestone 3.
