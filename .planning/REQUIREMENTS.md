# Requirements: Stellar Command

**Defined:** 2026-03-21
**Core Value:** Real-life habits directly fuel the spaceship — the game only gets better when the player does.

## v1 Requirements

### System Foundations

- [x] **FOUND-01**: Single save slot — game state auto-saves continuously, no manual save/load UI
- [x] **FOUND-02**: Save file includes a `saveVersion` integer for future migration support
- [x] **FOUND-03**: Save writes are atomic (write-temp-then-move) to prevent corruption on crash
- [ ] **FOUND-04**: Save state syncs to Firebase (Firestore or Realtime Database — TBD) as a corruption-proof backup per user document
- [x] **FOUND-05**: Boot scene auto-loads existing save on start; no "Continue" choice needed
- [x] **FOUND-06**: Boot scene initializes all core singletons before transitioning
- [x] **FOUND-07**: Scene flow supports Boot → Main Menu → Game with async additive loading
- [x] **FOUND-08**: Main menu has New Game (triggers data wipe confirmation), Settings, and Quit — no load option
- [ ] **FOUND-09**: Settings persist across sessions (audio volume, display resolution, fullscreen toggle)
- [ ] **FOUND-10**: Settings include a background FPS cap option (default: 10fps when unfocused)
- [ ] **FOUND-11**: Game launches with Firebase Anonymous auth silently — no login screen
- [ ] **FOUND-12**: Settings include a "Link Account" button to upgrade anonymous auth to Google Sign-In
- [ ] **FOUND-13**: Account linking preserves all existing save data
- [ ] **FOUND-14**: Google-linked accounts restore save data on reinstall or new device
- [x] **FOUND-15**: GameManager exposes a GameState enum with C# events for state transitions

### Habit Quest System

- [ ] **QUEST-01**: Player can view a list of their active daily habits/quests
- [ ] **QUEST-02**: Player can mark a habit as complete with a single interaction
- [ ] **QUEST-03**: Player can create custom habits (name, resource type, reward amount)
- [ ] **QUEST-04**: Player can edit and delete their habits
- [ ] **QUEST-05**: Each habit resets daily at a configurable local midnight
- [ ] **QUEST-06**: Player has a visible streak counter per habit (consecutive days completed)
- [ ] **QUEST-07**: Missing a habit day breaks the streak but does not punish with resource loss
- [ ] **QUEST-08**: Habit definitions are stored as ScriptableObjects; runtime state in save data only
- [ ] **QUEST-09**: Completing a habit fires a C# event consumed by the resource system

### Ship Resource System

- [ ] **RES-01**: Completing a habit generates one or more typed ship resources
- [ ] **RES-02**: Resource types are defined as ScriptableObjects (extensible without code changes)
- [ ] **RES-03**: Resource amounts are stored as `List<ResourceEntry>` structs (not Dictionary — JsonUtility limitation)
- [ ] **RES-04**: Resource totals are visible to the player on the main game HUD
- [ ] **RES-05**: Resource accumulation has daily soft caps (prevents exploit from retroactive logging)

### Automated Combat

- [ ] **COMBAT-01**: Combat engages automatically without player input
- [ ] **COMBAT-02**: Ship combat power is derived from accumulated resources
- [ ] **COMBAT-03**: Combat outcomes (victories, losses, loot) are displayed as a log/ticker
- [ ] **COMBAT-04**: Player can view a summary of recent combat events
- [ ] **COMBAT-05**: Combat system consumes resources at a balanced rate (tunable via SO data)

### Weekly Self-Report

- [ ] **REPORT-01**: A weekly review prompt appears once per 7-day cycle
- [ ] **REPORT-02**: Player rates their week across configurable habit categories
- [ ] **REPORT-03**: Completing the self-report grants a resource bonus
- [ ] **REPORT-04**: Self-report history is saved (player can view past reports)
- [ ] **REPORT-05**: Skipping the self-report does not penalize the player

## Pending Redesign

These systems are confirmed features but their design is not finalized. They are removed from current scope and will be re-added in a future milestone once the design is ready.

### Visual Novel Character System (pending redesign)

- VN-01 through VN-06 — Naninovel-based dialogue, character relationship system, event-triggered dialogue
- *Removed from active scope 2026-03-24 — design to be confirmed before re-adding*

### Desktop Wallpaper Mode (pending redesign)

- WALL-01 through WALL-06 — Win32 WorkerW wallpaper rendering, status overlay, ship visual progression
- *Removed from active scope 2026-03-24 — design to be confirmed before re-adding*
- *Note: FOUND-11 (wallpaper mode toggle in settings) also removed — add back when wallpaper mode is re-scoped*

## v2 Requirements

### Notifications

- **NOTIF-01**: System tray notification when daily habit window opens
- **NOTIF-02**: System tray notification reminder if habits incomplete by evening
- **NOTIF-03**: Notification for weekly self-report prompt

### Ship Customization

- **SHIP-01**: Player can visually customize ship appearance with earned cosmetics
- **SHIP-02**: Cosmetics are unlocked through habit streaks and combat milestones

## Out of Scope

| Feature | Reason |
|---------|--------|
| Auto habit tracking (device sync, app integration) | Manual check-in is intentional — reduces scope, privacy concerns, and external dependencies |
| Multiplayer / leaderboards | Solo self-improvement experience by design |
| Mobile build | Windows desktop target; platform-specific features (Win32) preclude cross-platform v1 |
| Player-controlled combat | Idle/automated combat is a core design pillar |
| Real-time habit timers | Simple mark-complete is the intended interaction; duration logging adds friction |

## Open Design Decisions

| Decision | Impact | Notes |
|----------|--------|-------|
| Firebase Firestore vs Realtime Database | FOUND-04 implementation | Realtime DB is simpler/cheaper for a single JSON save blob; Firestore is fine too. Must be decided before Phase 1. |
| Firebase auth strategy | ✓ Decided | Anonymous auth by default; optional Google Sign-In link in settings; linking preserves data; anonymous data lost on reinstall (acceptable) |
| Daily reset time | QUEST-05 | Midnight local time is the plan — confirm before Phase 2 |
| Resource soft cap amounts | RES-05 | Needs a design pass; can be tuned post-ship via SO data |

## Traceability

| Requirement | Phase | Plan | Status |
|-------------|-------|------|--------|
| FOUND-01 | Phase 1 | 01-01 | Pending |
| FOUND-02 | Phase 1 | 01-01 | Pending |
| FOUND-03 | Phase 1 | 01-01 | Pending |
| FOUND-04 | Phase 1 | 01-04 | Pending |
| FOUND-05 | Phase 1 | 01-01 | Pending |
| FOUND-06 | Phase 1 | 01-02 | Pending |
| FOUND-07 | Phase 1 | 01-02 | Pending |
| FOUND-08 | Phase 1 | 01-02 | Pending |
| FOUND-09 | Phase 1 | 01-03 | Pending |
| FOUND-10 | Phase 1 | 01-03 | Pending |
| FOUND-11 | Phase 1 | 01-04 | Pending |
| FOUND-12 | Phase 1 | 01-04 | Pending |
| FOUND-13 | Phase 1 | 01-04 | Pending |
| FOUND-14 | Phase 1 | 01-04 | Pending |
| FOUND-15 | Phase 1 | 01-02 | Pending |
| QUEST-01 | Phase 2 | 02-02 | Pending |
| QUEST-02 | Phase 2 | 02-02 | Pending |
| QUEST-03 | Phase 2 | 02-04 | Pending |
| QUEST-04 | Phase 2 | 02-04 | Pending |
| QUEST-05 | Phase 2 | 02-02 | Pending |
| QUEST-06 | Phase 2 | 02-02 | Pending |
| QUEST-07 | Phase 2 | 02-02 | Pending |
| QUEST-08 | Phase 2 | 02-01 | Pending |
| QUEST-09 | Phase 2 | 02-03 | Pending |
| RES-01 | Phase 2 | 02-03 | Pending |
| RES-02 | Phase 2 | 02-01 | Pending |
| RES-03 | Phase 2 | 02-01 | Pending |
| RES-04 | Phase 2 | 02-03 | Pending |
| RES-05 | Phase 2 | 02-03 | Pending |
| COMBAT-01 | Phase 3 | 03-01 | Pending |
| COMBAT-02 | Phase 3 | 03-01 | Pending |
| COMBAT-03 | Phase 3 | 03-03 | Pending |
| COMBAT-04 | Phase 3 | 03-03 | Pending |
| COMBAT-05 | Phase 3 | 03-02 | Pending |
| REPORT-01 | Phase 4 | 04-01 | Pending |
| REPORT-02 | Phase 4 | 04-02 | Pending |
| REPORT-03 | Phase 4 | 04-03 | Pending |
| REPORT-04 | Phase 4 | 04-03 | Pending |
| REPORT-05 | Phase 4 | 04-02 | Pending |

**Coverage:**
- v1 requirements: 39 total
- Mapped to phases: 39
- Unmapped: 0 ✓

---
*Requirements defined: 2026-03-21*
*Last updated: 2026-03-24 — removed VN character system and wallpaper mode (pending redesign); clarified Firebase save sync; removed wallpaper toggle from foundations; expanded traceability to per-requirement rows with plan assignments*
