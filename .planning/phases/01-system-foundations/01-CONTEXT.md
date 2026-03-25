# Phase 1: System Foundations - Context

**Gathered:** 2026-03-24
**Status:** Ready for planning

<domain>
## Phase Boundary

Establish persistence infrastructure (save/load with versioning and atomic writes), Firebase Anonymous auth with optional Google Sign-In upgrade, async Boot → Main Menu → Game scene flow with singleton persistence, player settings (audio, display, FPS cap), and GameManager state machine. All downstream systems depend on this foundation.

</domain>

<decisions>
## Implementation Decisions

### Firebase backend
- **D-01:** Use Firebase Realtime Database — single JSON blob per user document, simpler and cheaper than Firestore for this use case
- **D-02:** Anonymous auth on first launch (silent, no UI); optional Google Sign-In upgrade via "Link Account" button in Settings; linking preserves existing save data; anonymous data lost on reinstall is acceptable documented behavior

### Save data structure
- **D-03:** One unified save file — single root struct (`GameSaveData`) containing all game state; not per-system files
- **D-04:** Auto-save triggers on: any habit completion, any resource update, weekly report submit, and app focus loss (`OnApplicationFocus(false)`)
- **D-05:** Save includes a `saveVersion` integer for future migration; each save data class has a stub `MigrateFrom()` method

### Boot loading experience
- **D-06:** Silent black screen during Boot initialization in v1 — no splash screen or loading bar; keep it simple

### New Game confirmation
- **D-07:** Modal dialog with message "All progress will be deleted. This cannot be undone." with Confirm and Cancel buttons; Confirm triggers `SaveManager.DeleteAll()` and restarts to Boot scene

### Claude's Discretion
- Exact `GameSaveData` field naming conventions
- Which Unity `Awaitable` API variant to use for async scene loading (Unity 6 LTS)
- Audio Mixer asset naming and bus hierarchy
- `SettingsKeys` constant names

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements and roadmap
- `.planning/REQUIREMENTS.md` — FOUND-01 through FOUND-15, the full Phase 1 requirement set
- `.planning/ROADMAP.md` — Phase 1 plan breakdown (01-01 through 01-04) with success criteria

### Architecture research
- `.planning/research/ARCHITECTURE.md` — Architectural decisions and patterns for the project
- `.planning/research/STACK.md` — Technology choices and Unity package versions
- `.planning/research/PITFALLS.md` — Known pitfalls to avoid during implementation

### Codebase conventions
- `.planning/codebase/CONVENTIONS.md` — Coding conventions in use
- `.planning/codebase/ARCHITECTURE.md` — Existing architecture patterns

### Existing core scripts (MUST read before modifying)
- `StellarCommand/Assets/_Project/Scripts/Core/MonoSingleton.cs` — Base singleton; all managers extend this
- `StellarCommand/Assets/_Project/Scripts/Core/SaveManager.cs` — Existing save/load (needs atomic write + versioning added)
- `StellarCommand/Assets/_Project/Scripts/Core/BootManager.cs` — Existing boot entry point (uses sync LoadScene — needs async upgrade)
- `StellarCommand/Assets/_Project/Scripts/Core/GameManager.cs` — Empty stub (needs GameState enum + OnStateChanged event)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `MonoSingleton<T>`: All new managers (SceneController, SettingsManager) must extend this. Provides DontDestroyOnLoad, lazy init, duplicate detection, and `OnInitialize()` callback.
- `SaveManager`: Core Save/Load/Exists/Delete/DeleteAll API exists. Needs atomic write (write-temp-then-move) and `saveVersion` added — do not rewrite from scratch.
- `BootManager`: Initialization wiring exists. Needs async upgrade and save-auto-load added.
- `GameManager`: Empty but correctly inherits MonoSingleton. Expand in-place.

### Established Patterns
- All manager singletons live in `StellarCommand/Assets/_Project/Scripts/Core/`
- Namespace: `StellarCommand.Core`
- All managers attach to GameObjects in `Boot.unity` and survive scene loads via DontDestroyOnLoad
- `OnInitialize()` is the correct override point for manager setup (not `Awake()`)

### Integration Points
- `SaveManager` → receives atomic write upgrade; other managers call `SaveManager.Instance.Save/Load`
- `BootManager` → calls `SceneController.Instance.LoadSceneAsync` after init; auto-loads save via `SaveManager`
- `GameManager` → fires `OnStateChanged` event consumed by `SceneController` for transitions
- `SettingsManager` → reads/writes PlayerPrefs; wires Audio Mixer exposed parameters on `OnInitialize()`

</code_context>

<deferred>
## Deferred Ideas

- Loading bar or splash screen — explicitly out of scope for v1 (D-06); add when needed
- Desktop wallpaper mode toggle in settings — removed from Phase 1 scope (see REQUIREMENTS.md Pending Redesign)

</deferred>

---

*Phase: 01-system-foundations*
*Context gathered: 2026-03-24*
