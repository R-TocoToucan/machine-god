# Stellar Command

## What This Is

Stellar Command is a Windows desktop idle self-improvement game built with Unity URP. The player captains the SSV Ardent — a massive spaceship — where real-life daily habits (exercise, sleep, cooking, work) are logged as manual quests that generate ship resources. Combat is fully automated; the player's real-world discipline drives the ship's power. The game includes a visual novel character relationship system, a desktop wallpaper mode via Win32 API, and a weekly self-report system for lifecycle motivation.

## Core Value

Real-life habits directly fuel the spaceship — the game only gets better when the player does.

## Requirements

### Validated

- ✓ Unity URP project initialized with Boot scene — existing
- ✓ Core singletons scaffolded (GameManager) — existing

### Active

<!-- System Foundations — current milestone scope -->

- [ ] Save/load system — persistent game state across sessions
- [ ] Main menu + scene flow — Boot → Main Menu → Game scene transitions
- [ ] Settings system — audio, display, keybinds, wallpaper mode toggle

<!-- Core game systems — future milestones -->

- [ ] Habit quest system — player manually logs daily habits as completable quests
- [ ] Ship resource system — completed habits generate typed resources for the ship
- [ ] Automated combat system — ship engages enemies based on accumulated resources/power
- [ ] Visual novel character relationship system — crew characters with dialogue and relationship progression
- [ ] Desktop wallpaper mode — live ship scene + status overlay rendered behind desktop icons via Win32 API
- [ ] Weekly self-report system — player review form that grants resource bonuses

### Out of Scope

- Real-time habit tracking / device sync — manual check-in is the intended interaction; external integrations add complexity without improving the habit loop
- Multiplayer / leaderboards — solo experience by design
- Mobile build — Windows desktop only; Win32 wallpaper mode is platform-specific

## Context

- **Engine:** Unity URP (Windows desktop target)
- **Current state:** Project bootstrapped with Boot scene, URP pipeline, and GameManager singleton. No gameplay systems implemented yet.
- **Design fluidity:** Visual designs and system designs are not finalized. The project will go through iterative editing. Foundations must be built to support change, not lock in specifics.
- **Brownfield note:** ARCHITECTURE.md in codebase map reflects the repo at initialization (pre-Unity code). Actual architecture follows Unity patterns: MonoBehaviour singletons, ScriptableObjects for data, scene-based flow.

## Constraints

- **Platform:** Windows only — Win32 API usage for wallpaper mode requires Windows; no cross-platform concern for v1
- **Engine:** Unity URP — all rendering, save, and settings solutions must be Unity-native or Unity-compatible
- **Design volatility:** UI and system designs will change; foundations should use data-driven patterns (ScriptableObjects, events) to minimize rework when designs shift

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Manual habit check-in (not auto-tracked) | Simpler, more reliable; external tracking adds scope and privacy complexity | — Pending |
| Windows-only for v1 | Win32 API wallpaper mode is the differentiating feature; cross-platform deferred | — Pending |
| Automated combat (not player-controlled) | Keeps the game idle; combat power comes from real-life behavior, not reaction time | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd:transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd:complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-03-21 after initialization*
