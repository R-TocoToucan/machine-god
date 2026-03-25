---
phase: 01-system-foundations
plan: 02
subsystem: core
tags: [gamemanager, state-machine, scene-flow, async-awaitable, unity6, singleton, ui]

# Dependency graph
requires:
  - phase: 01-system-foundations/01-01
    provides: "SaveManager singleton with unified save API, MonoSingleton base class, AtomicFileWriter"
provides:
  - "GameManager with GameState enum (Boot/MainMenu/Playing/Paused) and OnStateChanged event"
  - "SceneController singleton for async additive scene loading via Awaitable"
  - "BootManager async boot flow initializing singletons then transitioning to MainMenu"
  - "MainMenuController with New Game (confirmation modal), Settings (stub), Quit"
  - "Reusable ConfirmDialog component with parameterized message and callbacks"
affects: [01-03-settings, 01-04-firebase, 02-01-quest-registry]

# Tech tracking
tech-stack:
  added: [TextMeshPro, InputSystem UI module]
  patterns: [async Awaitable scene loading, GameState event-driven state machine, MonoSingleton for core managers]

key-files:
  created:
    - StellarCommand/Assets/_Project/Scripts/Core/SceneController.cs
    - StellarCommand/Assets/_Project/Scripts/UI/MainMenuController.cs
    - StellarCommand/Assets/_Project/Scripts/UI/ConfirmDialog.cs
    - StellarCommand/Assets/_Project/Editor/SceneSetupTool.cs
    - StellarCommand/Assets/_Project/Scenes/MainMenu.unity
  modified:
    - StellarCommand/Assets/_Project/Scripts/Core/GameManager.cs
    - StellarCommand/Assets/_Project/Scripts/Core/BootManager.cs
    - StellarCommand/Assets/_Project/Tests/EditMode/GameManagerTests.cs
    - StellarCommand/Assets/_Project/Tests/PlayMode/BootFlowTests.cs
    - StellarCommand/Assets/_Project/Tests/PlayMode/SceneControllerTests.cs

key-decisions:
  - "Used async Awaitable (Unity 6 native) for all scene transitions instead of coroutines"
  - "BootManager is not a singleton -- it is a one-time boot script on MonoBehaviour"
  - "Boot scene stays loaded (singletons persist) while MainMenu loads additively"
  - "Created SceneSetupTool editor script to programmatically generate MainMenu.unity scene"
  - "Moved StellarCommand.Core.asmdef to Scripts/ root to cover all subdirectories including UI/"

patterns-established:
  - "Event-driven state transitions: GameManager.OnStateChanged(oldState, newState) -- no polling"
  - "Async additive scene loading: SceneController.LoadSceneAsync(scene, unloadScene)"
  - "Modal confirmation pattern: ConfirmDialog.Show(message, onConfirm, onCancel)"

requirements-completed: [FOUND-06, FOUND-07, FOUND-08, FOUND-15]

# Metrics
duration: multi-session
completed: 2026-03-25
---

# Phase 01 Plan 02: Scene Flow and GameManager Summary

**Async Boot-to-MainMenu scene flow with GameState event-driven state machine, SceneController additive loading, and Main Menu UI with New Game confirmation modal**

## Performance

- **Duration:** Multi-session (TDD + implementation + fixes + human verification)
- **Started:** 2026-03-25
- **Completed:** 2026-03-25
- **Tasks:** 3 (2 auto + 1 human-verify checkpoint)
- **Files modified:** 22

## Accomplishments
- GameManager state machine with GameState enum (Boot/MainMenu/Playing/Paused) and OnStateChanged(old, new) event with same-state no-op guard
- SceneController singleton providing async additive scene loading via Unity 6 Awaitable, with transition events
- BootManager upgraded from synchronous to async flow -- verifies singletons, loads MainMenu additively, sets GameState
- Main Menu UI with New Game (D-07 confirmation modal), Settings (stub), and Quit buttons
- Reusable ConfirmDialog component with parameterized message and confirm/cancel callbacks
- 4 EditMode GameManagerTests passing green

## Task Commits

Each task was committed atomically:

1. **Task 1 RED: GameManager state machine and SceneController - failing tests** - `a981218` (test)
2. **Task 1 GREEN: GameManager state machine and SceneController - implementation** - `bd6597d` (feat)
3. **Task 2: Async BootManager, MainMenuController, ConfirmDialog, BootFlowTests** - `44d531d` (feat)
4. **Fix: Move GameSaveData.cs into Core/ for assembly resolution** - `706169c` (fix)
5. **Fix: SceneSetupTool editor script for MainMenu.unity** - `2edc5e1` (feat)
6. **Fix: Move asmdef to Scripts/ root** - `2a077e1` (fix)
7. **Fix: Add TextMeshPro reference to asmdef** - `3d154ba` (fix)
8. **Fix: InputSystemUIInputModule and AudioListener** - `c0d449f` (fix)
9. **Fix: Enforce single AudioListener** - `212bb01` (fix)
10. **Fix: ConfirmDialog button sizing** - `28118b3` (fix)
11. **Task 3: Human verification** - approved (all 8 checks passed)

## Files Created/Modified
- `Scripts/Core/GameManager.cs` - GameState enum, OnStateChanged event, SetState with no-op guard
- `Scripts/Core/SceneController.cs` - Async additive scene loading singleton via Awaitable
- `Scripts/Core/BootManager.cs` - Async boot flow with singleton verification and MainMenu transition
- `Scripts/UI/MainMenuController.cs` - Main Menu with New Game/Settings/Quit button handlers
- `Scripts/UI/ConfirmDialog.cs` - Reusable modal dialog with parameterized message and callbacks
- `Editor/SceneSetupTool.cs` - Editor tool to programmatically create MainMenu.unity scene
- `Scenes/MainMenu.unity` - Main Menu scene with Canvas, buttons, and ConfirmDialog
- `Tests/EditMode/GameManagerTests.cs` - 4 tests for state machine behavior
- `Tests/PlayMode/BootFlowTests.cs` - Boot flow singleton initialization tests
- `Tests/PlayMode/SceneControllerTests.cs` - Scene controller tests

## Decisions Made
- Used `async Awaitable` (Unity 6 native) for all scene transitions -- no coroutines, per research guidance
- BootManager is a plain MonoBehaviour (not singleton) since it runs once at boot
- Boot scene persists (singletons stay alive) while MainMenu loads additively on top
- Created SceneSetupTool editor script to programmatically build MainMenu.unity since Unity scenes cannot be authored as text
- Moved StellarCommand.Core.asmdef from Scripts/Core/ to Scripts/ root so UI/ subdirectory scripts are included in the assembly
- Added TextMeshPro and InputSystem assembly references for UI components

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Moved GameSaveData.cs from Data/ to Core/ directory**
- **Found during:** Task 2
- **Issue:** CS0246 assembly reference error -- GameSaveData was in a separate Data/ folder outside the Core asmdef
- **Fix:** Moved file to Core/ directory where the asmdef covers it
- **Committed in:** 706169c

**2. [Rule 3 - Blocking] Created SceneSetupTool editor script**
- **Found during:** Task 2
- **Issue:** MainMenu.unity scene needed to be created with proper Canvas, buttons, and wiring -- cannot be done as text
- **Fix:** Created editor script to programmatically generate the scene
- **Committed in:** 2edc5e1

**3. [Rule 3 - Blocking] Moved asmdef to Scripts/ root**
- **Found during:** Task 2
- **Issue:** StellarCommand.Core.asmdef in Scripts/Core/ did not cover Scripts/UI/ subdirectory
- **Fix:** Moved asmdef to Scripts/ root so all subdirectories are included
- **Committed in:** 2a077e1

**4. [Rule 3 - Blocking] Added TextMeshPro assembly reference**
- **Found during:** Task 2
- **Issue:** ConfirmDialog.cs uses TMPro but the asmdef lacked the reference
- **Fix:** Added Unity.TextMeshPro to asmdef references
- **Committed in:** 3d154ba

**5. [Rule 1 - Bug] Fixed InputSystem and AudioListener conflicts**
- **Found during:** Task 2
- **Issue:** Duplicate AudioListener warnings and missing InputSystemUIInputModule for UI interaction
- **Fix:** Added InputSystemUIInputModule, disabled AudioListener on MainMenu camera
- **Committed in:** c0d449f, 212bb01

**6. [Rule 1 - Bug] Fixed ConfirmDialog button sizing and overlap**
- **Found during:** Task 2
- **Issue:** Confirm and Cancel buttons overlapped in the dialog panel
- **Fix:** Adjusted button layout and sizing in SceneSetupTool
- **Committed in:** 28118b3

---

**Total deviations:** 6 auto-fixed (2 bugs, 4 blocking)
**Impact on plan:** All fixes were necessary for the scene flow to compile and run correctly. No scope creep.

## Issues Encountered
- Unity assembly definition (asmdef) file placement required iteration to cover all script subdirectories
- Scene creation required an editor script approach since Unity scenes are binary assets

## User Setup Required
None - no external service configuration required.

## Known Stubs
- `MainMenuController.OnSettingsClicked()` logs "not yet implemented" -- will be wired in Plan 01-03 (Settings system)

## Next Phase Readiness
- GameManager state machine ready for all downstream subscribers (combat, quests, settings)
- SceneController ready for Game scene loading in Phase 2
- ConfirmDialog reusable for any future confirmation modals
- Settings button stub ready to wire in Plan 01-03

## Self-Check: PASSED

All 7 key files verified present. All 10 commit hashes verified in git log.

---
*Phase: 01-system-foundations*
*Completed: 2026-03-25*
