# Roadmap: Stellar Command

## Overview

Stellar Command is built in four phases that follow a strict dependency chain: persistence and navigation infrastructure first, then the player's core habit-and-resource loop, then the automated combat system that consumes those resources, then the weekly self-report that reinforces the habit lifecycle. Each phase delivers a fully testable capability before the next begins. VN character system and desktop wallpaper mode are deferred pending redesign and are not part of this roadmap.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [ ] **Phase 1: System Foundations** - Save/load with versioning, Boot-to-Menu-to-Game scene flow, settings, and GameManager state machine
- [ ] **Phase 2: Habit Quest and Resource Systems** - Player logs daily habits that generate typed ship resources, with streaks and daily resets
- [ ] **Phase 3: Automated Combat** - Ship engages enemies continuously based on accumulated resources; outcomes are visible to the player
- [ ] **Phase 4: Weekly Self-Report** - Weekly captain's-log review form grants resource bonuses and surfaces habit history

## Phase Details

### Phase 1: System Foundations
**Goal**: The game can persist all state across sessions, authenticate silently with Firebase, navigate cleanly between Boot, Main Menu, and Game scenes, and apply player settings — so every subsequent system has a safe, versioned place to save and a working scene to run in.
**Depends on**: Nothing (first phase)
**Requirements**: FOUND-01, FOUND-02, FOUND-03, FOUND-04, FOUND-05, FOUND-06, FOUND-07, FOUND-08, FOUND-09, FOUND-10, FOUND-11, FOUND-12, FOUND-13, FOUND-14, FOUND-15
**Success Criteria** (what must be TRUE):
  1. Launching the game auto-loads any existing save without a "Continue" prompt; all saved values are intact after a force-quit and relaunch
  2. Scene transitions Boot → Main Menu → Game run asynchronously with no synchronous LoadScene calls; Boot scene managers remain alive throughout
  3. Settings (volume levels, resolution, fullscreen, background FPS cap) persist across sessions and load before any scene transition
  4. Firebase Anonymous auth completes silently on first launch; the Settings screen exposes a "Link Account" button that upgrades to Google Sign-In and preserves all save data
  5. GameManager exposes a `GameState` enum and fires a C# `OnStateChanged` event; no other system polls state directly
**Plans**: 4 plans

Plans:
- [ ] 01-01: SaveManager hardening — add `saveVersion` integer to all save structs, implement atomic write (write-temp-then-move pattern), validate `Application.persistentDataPath` in both Editor and build, write stub `MigrateFrom()` on each save data class
- [ ] 01-02: Scene flow and GameManager — upgrade BootManager to async additive loading using `Awaitable`, create `SceneController` singleton with `LoadSceneAsync(sceneName, unloadScene)`, implement `GameState` enum (`Boot / MainMenu / Playing / Paused`) and `OnStateChanged` C# event on GameManager, wire Boot → Main Menu transition
- [ ] 01-03: Settings system and Audio — create `SettingsManager` singleton wrapping PlayerPrefs with typed constants (`SettingsKeys`), wire Master/Music/SFX Audio Mixer buses to settings values, implement `background FPS cap` via `Application.targetFrameRate` on focus change (default 10 fps), build Settings UI in Main Menu
- [ ] 01-04: Firebase auth and sync — integrate Firebase SDK (Anonymous auth silent sign-in on Boot), implement single-JSON-document-per-user sync layer on SaveManager (Firestore or Realtime DB — resolve open decision before implementing), add "Link Account" Google Sign-In flow in Settings with data-preservation guarantee

### Phase 2: Habit Quest and Resource Systems
**Goal**: The player can create, complete, and manage daily habits that generate typed ship resources, with streaks tracked and daily resets firing at local midnight — establishing the core habit-to-resource loop that all downstream systems depend on.
**Depends on**: Phase 1
**Requirements**: QUEST-01, QUEST-02, QUEST-03, QUEST-04, QUEST-05, QUEST-06, QUEST-07, QUEST-08, QUEST-09, RES-01, RES-02, RES-03, RES-04, RES-05
**Success Criteria** (what must be TRUE):
  1. Player sees a list of active habits and can mark any habit complete in a single interaction; the UI reflects the change immediately with visual/audio feedback
  2. Player can create a custom habit (name, resource type, reward amount) and it persists across sessions; player can edit and delete any habit
  3. All habits reset at local midnight; each habit shows a streak counter that increments on consecutive days and resets to 0 (not below) on a missed day — no resources are ever removed
  4. Completing a habit fires a C# `OnQuestCompleted` event; the resource system subscribes independently and credits the correct resource type and amount, capped by the daily soft cap
  5. Resource totals for all active resource types are visible on the main game HUD and update in real time as habits are completed
**Plans**: 4 plans

Plans:
- [ ] 02-01: ScriptableObject registry foundation — create `QuestDefinitionSO`, `ResourceTypeSO`, `QuestRegistrySO`, and `ResourceTypeRegistrySO` assets; define the SO-definition / plain-C#-runtime-state split (`QuestSaveData`, `ShipResourceStore` with `List<ResourceEntry>`); wire registries into QuestManager and ResourceManager inspector fields
- [ ] 02-02: QuestManager and habit lifecycle — implement `QuestManager` singleton with quest state per definition (isComplete, streakCount, lastCompletedDate), one-click completion firing `OnQuestCompleted(QuestDefinitionSO)`, daily reset coroutine keyed to local midnight (`QUEST-05`), and streak logic (increment on consecutive day, reset to 0 on miss — no penalty)
- [ ] 02-03: ResourceManager and HUD — implement `ResourceManager` with `List<ResourceEntry>` store, subscribe to `OnQuestCompleted` to credit resources, enforce daily soft caps per resource type (value authored on SO), fire `OnResourceChanged` event, build resource HUD panel reading from `OnResourceChanged`
- [ ] 02-04: Custom habit CRUD and balance debug overlay — build create/edit/delete habit UI backed by `QuestDefinitionSO` instance creation at runtime, validate that custom habit definitions persist in save data, add a debug overlay (dev builds only) showing resource rates and streak health to support balance tuning

### Phase 3: Automated Combat
**Goal**: The ship automatically engages enemies on a recurring tick, deriving combat power from accumulated resources and consuming them at a designer-tunable rate, with outcomes displayed as an in-game log so the habit-to-power chain is visible to the player.
**Depends on**: Phase 2
**Requirements**: COMBAT-01, COMBAT-02, COMBAT-03, COMBAT-04, COMBAT-05
**Success Criteria** (what must be TRUE):
  1. Combat ticks fire automatically without any player input; the player cannot manually trigger or halt combat
  2. Ship combat power is visibly derived from resource totals — a player who has logged more habits commands a more powerful ship; this relationship is legible in the UI
  3. Combat outcomes (victories, losses, loot drops) appear in a real-time log or ticker; the player can view a summary of recent combat events
  4. Resources are consumed by the combat system at a rate tunable via ScriptableObject data with no code change required; a target play-arc config SO defines the rough unit economics
**Plans**: 3 plans

Plans:
- [ ] 03-01: CombatManager and tick loop — implement `CombatManager` singleton with a configurable tick interval (authored in config SO), auto-start when `GameState` transitions to `Playing`, read resource totals from `ResourceManager` (no direct field access — via public API), calculate combat power from resource amounts using a formula defined in the config SO
- [ ] 03-02: Combat outcomes and resource consumption — implement combat resolution (victory/loss probability weighted by combat power vs. enemy tier SO data), resource consumption per tick at configurable rates, loot grant on victory via `ResourceManager` API, fire `OnCombatOutcome` C# event with outcome data
- [ ] 03-03: Combat log UI and summary view — build real-time combat ticker consuming `OnCombatOutcome` events, build a combat summary panel showing N most recent events (player can open on demand), surface combat power rating derived from current resources so the habit-to-power link is always legible

### Phase 4: Weekly Self-Report
**Goal**: A weekly captain's-log review form appears once per 7-day cycle, lets the player rate their week across habit categories, and grants a visible resource bonus on completion — reinforcing the self-improvement loop without obligating or penalizing the player.
**Depends on**: Phase 3
**Requirements**: REPORT-01, REPORT-02, REPORT-03, REPORT-04, REPORT-05
**Success Criteria** (what must be TRUE):
  1. A weekly review prompt surfaces automatically once per 7-day cycle; it does not appear more than once per cycle and does not appear if the player just completed one
  2. The form presents rating inputs across configurable habit categories (3-5 questions max, captain's-log framing) and the player can submit in under one minute
  3. Completing the self-report grants a resource bonus that is immediately reflected in the resource HUD; the bonus amount is tunable via SO data
  4. Skipping the weekly report carries no penalty; the prompt reappears the following week without guilt messaging
  5. The player can view a history of past self-reports including ratings and dates; report history is saved and survives session restarts
**Plans**: 3 plans

Plans:
- [ ] 04-01: Report trigger and persistence — implement `SelfReportManager` with 7-day cycle tracking (last-completed timestamp in save data with `schemaVersion`), trigger logic that fires `OnReportAvailable` event when cycle expires, report history stored as `List<ReportEntry>` in save data (not Dictionary — JsonUtility limitation)
- [ ] 04-02: Report form UI — build the weekly self-report form with configurable category questions (sourced from a `SelfReportConfigSO`), captain's-log visual framing, submit action firing `OnReportCompleted` with rating data; ensure the form can be dismissed without penalty (no confirmation required for skip)
- [ ] 04-03: Bonus grant and history view — implement resource bonus calculation on `OnReportCompleted` (bonus amounts on config SO), call `ResourceManager` API to credit resources, build report history panel (scrollable list of past report summaries with date and ratings), wire the "skip this week" path to advance the cycle timer without entry

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. System Foundations | 0/4 | Not started | - |
| 2. Habit Quest and Resource Systems | 0/4 | Not started | - |
| 3. Automated Combat | 0/3 | Not started | - |
| 4. Weekly Self-Report | 0/3 | Not started | - |
