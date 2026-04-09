# Milestone 4 Breakdown — Room-to-Table UX Glue

Milestone 4 is the bridge between a technically synced poker prototype and a multiplayer loop that actually feels playable.

## Goal

Get the game from **room/lobby setup → table start → hand end → next hand** with enough clarity that friends can play multiple hands without getting confused or stuck.

---

## The 3 Task Buckets

### 1. Room → Table Start Flow

**Goal:** make the transition from lobby room to active table deterministic and understandable.

**Includes:**
- Ready/start gate remains authoritative
- Room only starts when the right players are ready
- Table scene starts with seats mapped to real players
- Display names carry over cleanly from Photon room to table

**Athena can do:**
- Document the expected flow and ownership rules
- Audit existing ready/start code for gaps
- Tighten seat/display-name behavior in code where data already exists
- Add defensive logic for missing names / empty seats

**Drew likely needs to do:**
- Unity scene wiring / inspector hookups if UI objects are missing
- Any desired layout decisions for how seat/name presentation should look visually
- Manual multiplayer playtesting to confirm the start flow feels right

**Success looks like:**
- Players ready up in the room
- Master starts the game
- Everyone lands at the table with correct seat ownership and names

---

### 2. In-Game Table Status + Clarity

**Goal:** make it obvious what is happening during play.

**Includes:**
- Whose turn it is
- Current pot
- Call amount / highest bet
- Last action / latest meaningful status text
- Basic player/table context that reduces confusion

**Athena can do:**
- Add a lightweight status model / helper methods in gameplay code
- Add event hooks so UI can render authoritative state changes
- Update existing UI manager scripts to display status text when the references already exist
- Document what UI fields need to exist if they are not wired yet

**Drew likely needs to do:**
- Hook TMP/UI references in Unity if the scene does not already expose them
- Decide where status text should live visually on the table
- Adjust polish/layout/fonts/anchoring so it actually looks good

**Success looks like:**
- A remote player can glance at the table and understand the current hand state without guessing

---

### 3. End-of-Hand → Next-Hand Loop

**Goal:** make the table continue into the next hand smoothly instead of feeling like a dead-end prototype.

**Includes:**
- End-of-hand state is shown clearly
- Round resolution leads back into the next hand cleanly
- No confusing stall after showdown / payout
- Minimal repeated-hand loop works for private-room MVP play

**Athena can do:**
- Define the state transition plan
- Add code support for a basic replay/continue loop where the logic already exists
- Add guard rails around round-end UI visibility and restart flow
- Document what still needs manual Unity hookup or playtest verification

**Drew likely needs to do:**
- Decide whether next-hand progression is automatic or button-driven
- Wire any restart/continue button in the scene if needed
- Playtest pacing and UX feel in-editor / with another client

**Success looks like:**
- Hand ends
- Players understand the result
- Game can move into the next hand without weird dead air or manual dev intervention

---

## What Athena can implement right now

These are the parts that are mostly code-side and do not obviously require new art/layout work:

1. **Document Milestone 4 clearly**
2. **Add a table-status snapshot/helper layer** for UI consumption
3. **Improve seat/display-name fallback behavior**
4. **Add clearer round/turn status plumbing in code**
5. **Add a documented checklist of Unity-side hookups Drew still needs to verify**

## What probably still needs Drew

1. **Unity inspector hookups** for any new text fields/buttons
2. **Visual/layout decisions** for where status text and seat labels should live
3. **Multiplayer feel testing** with real clients
4. **Final product decisions** like automatic next-hand start vs button-driven continue

---

## Recommended implementation order

1. **Seat + display-name clarity**
2. **Status text / table clarity**
3. **Next-hand loop glue**
4. **Manual Unity hookup + multiplayer playtest**

That order keeps the work honest: first make the table understandable, then make it loop.
