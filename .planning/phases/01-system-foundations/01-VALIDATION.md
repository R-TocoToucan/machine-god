---
phase: 1
slug: system-foundations
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-24
---

# Phase 1 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Unity Test Framework (com.unity.test-framework — already in project) |
| **Config file** | None — Wave 0 creates assembly definitions and test assemblies |
| **Quick run command** | Unity Editor → Window → General → Test Runner → EditMode → Run All |
| **Full suite command** | Unity Editor → Window → General → Test Runner → Run All (EditMode + PlayMode) |
| **Estimated runtime** | ~30 seconds (EditMode fast; PlayMode includes scene load overhead) |

---

## Sampling Rate

- **After every task commit:** Run EditMode quick suite (Test Runner → EditMode → Run All)
- **After every plan wave:** Run full suite (EditMode + PlayMode)
- **Before `/gsd:verify-work`:** Full suite green + manual Firebase verification
- **Max feedback latency:** ~30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 1-01-01 | 01-01 | 1 | FOUND-02 | EditMode | Test Runner → EditMode → GameSaveDataTests | ❌ W0 | ⬜ pending |
| 1-01-02 | 01-01 | 1 | FOUND-03 | EditMode | Test Runner → EditMode → AtomicWriteTests | ❌ W0 | ⬜ pending |
| 1-01-03 | 01-01 | 1 | FOUND-01, FOUND-05 | PlayMode | Test Runner → PlayMode → SaveManagerTests | ❌ W0 | ⬜ pending |
| 1-02-01 | 01-02 | 2 | FOUND-07 | PlayMode | Test Runner → PlayMode → SceneControllerTests | ❌ W0 | ⬜ pending |
| 1-02-02 | 01-02 | 2 | FOUND-06 | PlayMode | Test Runner → PlayMode → BootFlowTests | ❌ W0 | ⬜ pending |
| 1-02-03 | 01-02 | 2 | FOUND-15 | EditMode | Test Runner → EditMode → GameManagerTests | ❌ W0 | ⬜ pending |
| 1-02-04 | 01-02 | 2 | FOUND-08 | Manual | Manual UI test in PlayMode | Manual-only | ⬜ pending |
| 1-03-01 | 01-03 | 3 | FOUND-09 | EditMode | Test Runner → EditMode → SettingsManagerTests | ❌ W0 | ⬜ pending |
| 1-03-02 | 01-03 | 3 | FOUND-10 | Manual | Manual focus/unfocus test + FPS counter | Manual-only | ⬜ pending |
| 1-04-01 | 01-04 | 4 | FOUND-11 | Manual | Verify anonymous UID in Firebase Console | Manual-only | ⬜ pending |
| 1-04-02 | 01-04 | 4 | FOUND-04 | Manual | Verify JSON blob in Firebase Realtime DB console | Manual-only | ⬜ pending |
| 1-04-03 | 01-04 | 4 | FOUND-12, FOUND-13 | Manual | Firebase Console: verify UID preserved after link | Manual-only | ⬜ pending |
| 1-04-04 | 01-04 | 4 | FOUND-14 | Manual | Reinstall + Google Sign-In: verify save restored | Manual-only | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `StellarCommand/Assets/_Project/Scripts/Core/StellarCommand.Core.asmdef` — assembly definition so tests can reference Core namespace
- [ ] `StellarCommand/Assets/_Project/Tests/EditMode/` directory + EditMode assembly definition
- [ ] `StellarCommand/Assets/_Project/Tests/PlayMode/` directory + PlayMode assembly definition
- [ ] `StellarCommand/Assets/_Project/Tests/EditMode/GameSaveDataTests.cs` — covers FOUND-02 (saveVersion present, MigrateFrom callable)
- [ ] `StellarCommand/Assets/_Project/Tests/EditMode/AtomicWriteTests.cs` — covers FOUND-03 (write-to-tmp, replace, verify file content)
- [ ] `StellarCommand/Assets/_Project/Tests/EditMode/SettingsManagerTests.cs` — covers FOUND-09 (PlayerPrefs round-trip for all setting keys)
- [ ] `StellarCommand/Assets/_Project/Tests/EditMode/GameManagerTests.cs` — covers FOUND-15 (state transitions, OnStateChanged event fires)
- [ ] `StellarCommand/Assets/_Project/Tests/PlayMode/SaveManagerTests.cs` — covers FOUND-01, FOUND-05 (auto-save on trigger, auto-load on boot)
- [ ] `StellarCommand/Assets/_Project/Tests/PlayMode/BootFlowTests.cs` — covers FOUND-06 (all singletons initialized before scene transition)
- [ ] `StellarCommand/Assets/_Project/Tests/PlayMode/SceneControllerTests.cs` — covers FOUND-07 (async additive scene load, no sync LoadScene calls)

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Anonymous auth completes on boot | FOUND-11 | Requires Firebase project + standalone build | Build standalone, launch, check Firebase Console for new anonymous UID |
| Firebase RTDB save sync | FOUND-04 | Requires live Firebase connection | Trigger save, check Firebase Console for JSON document under user UID |
| Link Account to Google Sign-In | FOUND-12 | Requires OAuth flow + Google account | Click "Link Account" in Settings, complete Google OAuth, verify no data loss |
| Data preserved after linking | FOUND-13 | Requires full auth flow test | Confirm anonymous UID's data survives after Google link — check via Firebase Console |
| Save restore on reinstall | FOUND-14 | Requires reinstall scenario | Delete local save, reinstall, sign in with Google — verify save restored from RTDB |
| Background FPS cap | FOUND-10 | Requires window focus change | Run game, alt-tab away, verify FPS drops to ~10 via frame counter |
| Main Menu New Game confirmation | FOUND-08 | UI interaction | Click New Game → confirm modal appears → Confirm wipes data → Cancel preserves data |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
