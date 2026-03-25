---
phase: 01-system-foundations
plan: 01
subsystem: save-system, testing
tags: [unity, json, atomic-write, file-io, nunit, test-framework, asmdef]

# Dependency graph
requires: []
provides:
  - "GameSaveData struct with saveVersion and MigrateFrom migration stub"
  - "AtomicFileWriter static utility for safe file writes with .bak fallback"
  - "Hardened SaveManager with unified GameSaveData API and auto-load/auto-save"
  - "StellarCommand.Core assembly definition"
  - "EditMode and PlayMode test assembly definitions"
  - "7 stub test files covering all Phase 1 requirements"
affects: [01-02, 01-03, 01-04]

# Tech tracking
tech-stack:
  added: []
  patterns: [atomic-write-with-backup, unified-save-data-struct, static-utility-for-testability]

key-files:
  created:
    - "StellarCommand/Assets/_Project/Scripts/Data/GameSaveData.cs"
    - "StellarCommand/Assets/_Project/Scripts/Core/AtomicFileWriter.cs"
    - "StellarCommand/Assets/_Project/Scripts/Core/StellarCommand.Core.asmdef"
    - "StellarCommand/Assets/_Project/Tests/EditMode/StellarCommand.Core.EditModeTests.asmdef"
    - "StellarCommand/Assets/_Project/Tests/PlayMode/StellarCommand.Core.PlayModeTests.asmdef"
    - "StellarCommand/Assets/_Project/Tests/EditMode/GameSaveDataTests.cs"
    - "StellarCommand/Assets/_Project/Tests/EditMode/AtomicWriteTests.cs"
    - "StellarCommand/Assets/_Project/Tests/EditMode/SettingsManagerTests.cs"
    - "StellarCommand/Assets/_Project/Tests/EditMode/GameManagerTests.cs"
    - "StellarCommand/Assets/_Project/Tests/PlayMode/SaveManagerTests.cs"
    - "StellarCommand/Assets/_Project/Tests/PlayMode/BootFlowTests.cs"
    - "StellarCommand/Assets/_Project/Tests/PlayMode/SceneControllerTests.cs"
  modified:
    - "StellarCommand/Assets/_Project/Scripts/Core/SaveManager.cs"

key-decisions:
  - "Extracted atomic write logic into static AtomicFileWriter class for testability without MonoBehaviour"
  - "Used File.Replace for atomic swap with automatic .bak creation"
  - "Unified save API around single GameSaveData class, removing generic Save<T>/Load<T>"

patterns-established:
  - "Atomic write pattern: write .tmp, File.Replace to final with .bak backup"
  - "Load fallback chain: main file -> .bak -> CreateNew()"
  - "Static utility classes for file I/O to enable EditMode testing without MonoBehaviour"
  - "Test stubs with Assert.Fail placeholders for future implementation"

requirements-completed: [FOUND-01, FOUND-02, FOUND-03, FOUND-05]

# Metrics
duration: 3min
completed: 2026-03-25
---

# Phase 01 Plan 01: Save System Hardening Summary

**Atomic-write SaveManager with GameSaveData struct, File.Replace .bak fallback, and Wave 0 test scaffolding for full phase**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-25T20:58:44Z
- **Completed:** 2026-03-25T21:01:58Z
- **Tasks:** 2
- **Files modified:** 13

## Accomplishments
- Created GameSaveData struct with saveVersion=1, CreateNew() factory, and MigrateFrom() stub for future migrations
- Hardened SaveManager with atomic writes (write-to-tmp, File.Replace, .bak backup), auto-load on boot, and auto-save on focus loss
- Extracted AtomicFileWriter as static utility class enabling EditMode testing without MonoBehaviour instantiation
- Created 3 assembly definitions (Core, EditMode tests, PlayMode tests) and 7 stub test files covering all Phase 1 requirements
- Implemented real tests for GameSaveData (3 tests) and AtomicWrite (4 tests) replacing Assert.Fail stubs

## Task Commits

Each task was committed atomically:

1. **Task 0: Create assembly definitions and test scaffolding** - `196958c` (chore)
2. **Task 1 RED: Failing tests for GameSaveData and AtomicWrite** - `8ab00fb` (test)
3. **Task 1 GREEN: Implement GameSaveData and harden SaveManager** - `8abe4b5` (feat)

## Files Created/Modified
- `StellarCommand/Assets/_Project/Scripts/Data/GameSaveData.cs` - Unified save data struct with saveVersion and migration support
- `StellarCommand/Assets/_Project/Scripts/Core/AtomicFileWriter.cs` - Static atomic write utility with File.Replace and .bak fallback
- `StellarCommand/Assets/_Project/Scripts/Core/SaveManager.cs` - Rewritten: unified GameSaveData API, atomic writes, auto-load/save
- `StellarCommand/Assets/_Project/Scripts/Core/StellarCommand.Core.asmdef` - Core namespace assembly definition
- `StellarCommand/Assets/_Project/Tests/EditMode/StellarCommand.Core.EditModeTests.asmdef` - EditMode test assembly
- `StellarCommand/Assets/_Project/Tests/PlayMode/StellarCommand.Core.PlayModeTests.asmdef` - PlayMode test assembly
- `StellarCommand/Assets/_Project/Tests/EditMode/GameSaveDataTests.cs` - 3 real tests for GameSaveData
- `StellarCommand/Assets/_Project/Tests/EditMode/AtomicWriteTests.cs` - 4 real tests for atomic write and fallback
- `StellarCommand/Assets/_Project/Tests/EditMode/SettingsManagerTests.cs` - Stub tests for Plan 01-03
- `StellarCommand/Assets/_Project/Tests/EditMode/GameManagerTests.cs` - Stub tests for Plan 01-02
- `StellarCommand/Assets/_Project/Tests/PlayMode/SaveManagerTests.cs` - Stub tests for PlayMode save integration
- `StellarCommand/Assets/_Project/Tests/PlayMode/BootFlowTests.cs` - Stub tests for Plan 01-02
- `StellarCommand/Assets/_Project/Tests/PlayMode/SceneControllerTests.cs` - Stub tests for Plan 01-02

## Decisions Made
- Extracted atomic write logic into a static `AtomicFileWriter` class rather than keeping it as a private method on SaveManager. This enables EditMode unit testing of file I/O without needing to instantiate a MonoBehaviour.
- Used `File.Replace` for the atomic swap operation, which natively creates a .bak copy of the previous file.
- Removed the generic `Save<T>/Load<T>` API entirely in favor of a unified `GameSaveData`-specific workflow, as the plan specifies a single save document design.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added AtomicFileWriter as separate static class**
- **Found during:** Task 1 (SaveManager implementation)
- **Issue:** Plan suggested adding static method to SaveManager or a separate class. A static class is needed for AtomicWriteTests to test file I/O without MonoBehaviour.
- **Fix:** Created `AtomicFileWriter.cs` as a static utility class in StellarCommand.Core namespace with `WriteAtomic` and `LoadWithFallback` methods.
- **Files modified:** StellarCommand/Assets/_Project/Scripts/Core/AtomicFileWriter.cs
- **Verification:** AtomicWriteTests can call AtomicFileWriter directly without MonoBehaviour
- **Committed in:** 8abe4b5 (Task 1 GREEN commit)

---

**Total deviations:** 1 auto-fixed (1 missing critical)
**Impact on plan:** Essential for testability. Plan explicitly suggested this option. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- SaveManager hardened and ready for all downstream systems to call `SaveManager.Instance.Save()`
- GameSaveData struct ready for field additions in Phase 2 (quests, resources)
- Test infrastructure in place: all stub test files ready for Plans 01-02, 01-03, and 01-04
- Assembly definitions enable proper namespace isolation and Test Runner discovery

---
## Self-Check: PASSED

All 13 files verified present. All 3 commits verified in git log.

---
*Phase: 01-system-foundations*
*Completed: 2026-03-25*
