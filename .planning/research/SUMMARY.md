# Project Research Summary

**Project:** Stellar Command
**Domain:** Windows desktop idle self-improvement game (habit tracking + idle RPG + desktop companion + visual novel)
**Researched:** 2026-03-21
**Confidence:** MEDIUM-HIGH

---

## Executive Summary

Stellar Command sits at the intersection of four well-understood domains — idle/incremental games, habit-gamification apps, desktop companion apps, and visual novel systems — but the specific combination is genuinely novel. No existing product combines all four, which is both the opportunity and the integration risk. The recommended build approach is conservative on technology (leverage the existing Unity 6 LTS / URP foundation, extend the existing custom save system, use Naninovel for VN, P/Invoke for Win32) and aggressive on design discipline (ScriptableObject-driven data, C# events for inter-system communication, zero hardcoded game values from day one). The architecture should stay simple: MonoSingleton managers for infrastructure, SO definitions for content, plain C# classes for runtime state. Do not introduce DI frameworks, UI Toolkit, FMOD, Addressables, or any heavyweight abstraction at this stage.

The single highest-risk technical system is the desktop wallpaper mode. The Win32 WorkerW embedding technique is well-documented across Unity community projects but relies on undocumented Windows shell internals that have broken on multiple Windows Update releases. This is not a reason to abandon wallpaper mode — it is the primary product differentiator — but it demands being built as an isolated, fallback-safe system rather than tightly coupled to game logic. The second highest-risk area is save system schema evolution: with a habit-tracking game that will iterate design frequently, unversioned save data will cause "save wipe" bugs after every schema change. Both risks have well-known mitigations that must be applied from day one, not retrofitted.

The genre research validates the core concept strongly. Habitica proved that habit-gamification has real user demand and also documented the failure modes (punishment mechanics, overwhelming onboarding) to avoid. Finch proved the "companion that grows with your self-care" emotional mechanic has market validation. Wallpaper Engine proved non-intrusive desktop presence is technically achievable and highly valued. Stellar Command's synthesis of all three plus an idle resource/combat loop is a genuine product gap, not a saturated space. The risk is not concept validity — it is execution depth: one strong character with meaningful dialogue will outperform three shallow ones, and a wallpaper mode that works reliably on Windows Update will retain users that a buggy one loses.

---

## Key Findings

### Recommended Stack

The project baseline is fixed and correct: Unity 6000.3.11f1 (LTS), URP 17.3.0, uGUI 2.0.0, New Input System 1.19.0, custom MonoSingleton and SaveManager. Do not upgrade Unity mid-project; LTS patches are safe, minor version bumps are not. Three packages in manifest.json should be removed as they serve no purpose: `com.unity.ai.navigation`, `com.unity.multiplayer.center`, and `com.unity.visualscripting`. One external package is required before the VN milestone: Naninovel from the Unity Asset Store (approximately $70 USD; verify current pricing and Unity 6 compatibility before purchase).

The save system (JsonUtility + File.WriteAllText) is structurally correct and should not be replaced. It needs two additions before shipping any persistent data: a `saveVersion` field on every save struct (add immediately, even as a stub) and an atomic write pattern (write to `.json.tmp` then `File.Move`). Settings belong in PlayerPrefs (not the JSON save system) because they must survive "delete save data" resets and load before any scene. Use Unity's built-in Audio Mixer (no FMOD). Use P/Invoke directly for Win32 wallpaper embedding (no plugin needed).

**Core technologies:**
- Unity 6 LTS 6000.3.11f1: engine — already in project, do not change
- URP 17.3.0: rendering — correct for stylized desktop game, required for Naninovel Renderer Feature
- Custom SaveManager (JsonUtility): persistence — extend with versioning and atomic write; do not replace
- PlayerPrefs: settings persistence — settings only, not game data
- Naninovel (Asset Store): visual novel system — provides sprites, typewriter, history, save integration, URP support out of the box; do not build custom
- P/Invoke to user32.dll: wallpaper embedding — FindWindow / SetParent / SetWindowLong / SetWindowPos; no third-party plugin needed
- Unity SceneManager (async): scene transitions — use LoadSceneAsync + Awaitable; update BootManager's synchronous LoadScene call
- Unity Audio Mixer: audio — Master → Music / SFX / Voice bus; no FMOD
- uGUI 2.0.0: all runtime UI — Naninovel is uGUI-based; do not introduce UI Toolkit for runtime

### Expected Features

The product has table stakes from four genre contracts simultaneously. All must be met for the core premise to feel coherent.

**Must have (table stakes for v1 coherence):**
- Save/load with offline progress — losing data on restart is fatal for a habit-tracking game
- One-click habit completion with immediate visual/audio feedback — friction kills habit apps
- Ship resource HUD showing the habit → resource translation — loop is invisible without this
- Daily quest reset with clear timing — players need to know when to return
- Streak counter (current streak number at minimum) — highest-ROI retention mechanic per implementation cost
- Wallpaper mode (basic: static ship with resource overlay) — primary differentiator; even basic satisfies the promise
- System tray icon (Show / Hide / Quit) — required contract for a background Windows process
- At least one crew character with relationship meter and event-triggered dialogue — establishes VN system; depth over breadth
- Settings: audio toggle, wallpaper mode toggle, daily reset time
- Weekly self-report form — reinforces the self-improvement loop; already in project scope

**Should have (differentiators worth building in v1):**
- Custom habit creation — pre-set habits undermine the personal nature; this is Habitica's most important lesson
- Resource type to habit category mapping (exercise → engines, sleep → shields, work → weapons) — communicates the thematic link that makes the game concept land
- Graceful miss acknowledgment — "you missed yesterday" with no punishment; reduces guilt-driven churn
- Background FPS throttle (5–10 fps when not active) — wallpaper mode running at 60fps will get the game uninstalled

**Defer (post-v1):**
- Animated combat visuals (automated combat implied by text/simple effects is sufficient for v1)
- Multiple crew characters beyond one or two (writing quality matters more than character count)
- Habit history / calendar view (weekly self-report covers this need in v1)
- Relationship unlock CG scenes (relationship meter can exist without unlockable art)
- Character voice audio (high production cost, low v1 priority)
- Export / backup save

**Anti-features — explicitly do not build:**
- Punishment for missed habits (Habitica's documented churn driver — ships are "less optimal", not damaged)
- Push notifications / OS alerts (the wallpaper IS the notification)
- Real-time automatic habit tracking or device sync
- Social or leaderboard features
- Multiple save slots
- In-app monetization

### Architecture Approach

The recommended architecture is deliberately simple: MonoSingleton managers for infrastructure (GameManager, SaveManager, SceneController, SettingsManager, AudioManager) in a persistent Boot scene, ScriptableObject assets for all static definitions, plain C# classes for all runtime state, and C# events for inter-system communication. This is not a limitation — it is the correct choice for a solo-dev project with high design volatility and eight loosely-coupled systems. The existing MonoSingleton base and SaveManager provide the correct foundation; the key architectural upgrades are (1) making GameManager a proper state machine with a `GameState` enum and `OnStateChanged` event, (2) adding save schema versioning, and (3) migrating BootManager from synchronous to async scene loading using Unity 6's `Awaitable`.

The most important design discipline is the SO definition / C# runtime state split: ScriptableObjects define what things are (QuestDefinitionSO, CharacterDefinitionSO, ResourceTypeSO), plain C# classes track what has happened (QuestSaveData, CharacterSaveData). Never store runtime mutation on SOs — it persists in the Editor, silently resets in builds, and is a consistent source of "this worked in testing but broke in the build" bugs. The second most important discipline is inter-system communication through events, not direct method calls: QuestManager fires `OnQuestCompleted`; ResourceManager, CombatManager, and CharacterManager each subscribe independently. This keeps systems changeable without ripple rewrites.

**Major components and responsibilities:**

| Component | Responsibility | Lives In |
|-----------|---------------|----------|
| GameManager | Global state machine (Boot/Menu/Playing/Paused), OnStateChanged event | Boot scene |
| SaveManager | File I/O only; reads and writes per-system JSON; no knowledge of data meaning | Boot scene |
| SceneController | Async additive scene loading, transition events, fade logic | Boot scene |
| SettingsManager | PlayerPrefs typed wrapper; audio levels, display, wallpaper bool | Boot scene |
| AudioManager | Audio Mixer bus control; volume from SettingsManager | Boot scene |
| QuestManager | Quest lifecycle; fires OnQuestCompleted; does not touch resources directly | Game scene |
| ResourceManager | Resource amounts; fires OnResourceChanged; does not know how resources are spent | Game scene |
| CombatManager | Automated combat loop; reads resources; fires combat events | Game scene |
| CharacterManager | Crew relationship scores; dialogue flags; fires OnRelationshipChanged | Game scene |
| WallpaperManager | Win32 P/Invoke calls only; isolated from all game logic | Game scene |

### Critical Pitfalls

1. **Save schema without versioning from day one** — JsonUtility silently returns default values for missing fields; adding any new field to a save struct after first use will corrupt old saves by zeroing out missing data. Add `int saveVersion = 1` to every save struct before writing a single save file. Write a stub `MigrateFrom()` method even if it does nothing yet. This is the highest-probability "trust-destroying" bug for a habit-tracking game because the player's logged habit data disappears silently.

2. **WorkerW window hierarchy breaking on Windows Update** — The desktop wallpaper embedding technique sends an undocumented message to `Progman` to spawn a `WorkerW`, then reparents the Unity HWND into it. Microsoft has changed the WorkerW tree structure in Windows 10 and 11 updates, causing the window to appear invisible or to float on top of icons rather than behind them. Mitigation: validate the found WorkerW against monitor bounds before reparenting; handle the failure case by falling back to normal windowed mode; test against at least Windows 10 21H2/22H2 and Windows 11 22H2/23H2.

3. **Unity resetting window styles after wallpaper embedding** — Unity's internal player loop reapplies window styles in response to alt-tab, focus changes, or `Screen.SetResolution` calls, popping the game window out of the wallpaper layer. The embedding must be re-applied after every `Application.focusChanged` callback, not just at startup. Never call `Screen.fullScreen = true` in wallpaper mode.

4. **Punishment mechanics causing habit-app churn** — Habitica's well-documented failure: streak penalties, health damage from missed days, and negative consequences from non-engagement cause guilt → avoidance → permanent quit. Design decision: streaks provide bonus resources on maintenance (neutral when broken, not penalized), earned resources are never removed, the ship's power decays over weeks not hours. Weekly self-report must be optional-but-rewarded, never required.

5. **ScriptableObjects used as both definition and runtime state** — Storing quest progress or resource amounts on SOs produces "works in Editor, breaks in builds" bugs where runtime mutations persist in-editor but reset in a build. All runtime state lives in plain C# classes serialized by SaveManager. SOs are read-only at runtime.

---

## Implications for Roadmap

The feature dependency graph from FEATURES.md defines the phase order. Save/load is the root dependency of everything. Settings must exist before wallpaper mode is toggleable. Habit quests must exist before resources can be earned. Resources must exist before combat is meaningful. Naninovel and the VN system require the habit event system to trigger dialogue. Wallpaper mode requires all of the above to have something worth displaying.

### Phase 1: System Foundations

**Rationale:** Everything else depends on the save system, settings, and scene flow. The current milestone in PROJECT.md is already here. No game system can be persisted, no settings can be applied, and no scene can transition cleanly until this phase is complete.

**Delivers:** Save/load with versioning and atomic write; Boot → Main Menu → Game scene flow with async additive loading; Settings system (audio, display, wallpaper toggle) backed by PlayerPrefs; GameManager state machine; SceneController singleton; assembly definitions and folder structure.

**Addresses:** Table stakes #1 (save/load), table stakes #9 (settings), anti-pattern of synchronous SceneManager.LoadScene in BootManager.

**Avoids:** Pitfall 3 (save versioning), Pitfall 4 (SO as runtime state), Pitfall 6 (URP Quality Level misconfiguration), Pitfall 10 (folder structure), Pitfall 17 (persistentDataPath differences between Editor and build).

**Research flag:** Standard Unity patterns — skip phase research. Patterns are well-documented and confirmed against actual project code.

---

### Phase 2: Habit Quest and Resource Systems

**Rationale:** The habit quest loop is the core player interaction and must exist before anything else is meaningful. Resources are the direct output of quest completion; the two are tightly coupled and should be built together. This phase establishes the ScriptableObject registry pattern that all subsequent systems depend on.

**Delivers:** QuestDefinitionSO registry; custom habit creation; one-click quest completion with reward feedback; daily reset with clear timing; streak counter; ShipResourceStore with ResourceTypeSO definitions; resource-to-habit-category mapping (exercise → engines, sleep → shields, work → weapons); resource HUD; miss/skip acknowledgment; balance debug overlay.

**Addresses:** Table stakes #2, #3, #4, #5 (quest completion, HUD, reset, streak); differentiator #5 (habit category mapping); should-have #1 (custom habit creation), #3 (miss acknowledgment).

**Avoids:** Pitfall 5 (habit obligation / punishment mechanics), Pitfall 9 (resource economy imbalance — define unit economics in config before coding), Pitfall 18 (hard-coded habit types — use SOs from the first quest type, never enums).

**Research flag:** Standard idle game and habit app patterns — skip phase research. Resource economy balancing parameters should be authored as SO data or config, not hardcoded.

---

### Phase 3: Automated Combat System

**Rationale:** Combat is the downstream consumer of resources; it has no dependencies that aren't already built by Phase 2. Keeping it as a separate phase lets combat be calibrated against real resource data rather than placeholder values.

**Delivers:** Automated combat tick loop; combat power calculation from resource store; combat outcome feedback (text/simple effects are sufficient for v1); resource consumption and generation from combat; rough unit economics definition (target play-arc: player logging 3 habits/day should see X upgrade within Y days).

**Addresses:** Table stakes — meaningful idle tick; differentiator — habit-to-combat power chain.

**Avoids:** Pitfall 9 (balance calibrated against real resource data before combat goes in, not after).

**Research flag:** Standard idle game economy patterns — skip phase research. The specific balance values require playtesting, not research.

---

### Phase 4: Visual Novel Character System

**Rationale:** Naninovel depends on the habit event system being live (character reactions trigger on `OnQuestCompleted`). Relationship state must be saved, so the save system must be proven before VN state is added. Building one character at full quality is the explicit recommendation; do not add characters until the first one is complete.

**Delivers:** Naninovel integration (lazy-initialized on first dialogue trigger, URP Renderer Feature added, manual init mode); at least one crew character with portrait, name, role; relationship meter; event-triggered dialogue on habit completion, combat events, long absence; relationship state serialized to SaveManager; VN state bridge (`played scene IDs` HashSet in save data to prevent replay); custom Naninovel commands wrapping CharacterManager API; all dialogue in `.nani` scripts (no hardcoded strings in C#).

**Addresses:** Table stakes #8 (character + relationship meter + event-triggered dialogue); VN genre table stakes.

**Avoids:** Pitfall 7 (Naninovel init order — game systems boot first, Naninovel on-demand); Pitfall 8 (localization document drift — treat as release-time step); Pitfall 16 (VN state not in game save — implement the save bridge in this same milestone, not later); Anti-Pattern 5 (game logic inside .nani scripts — use custom commands).

**Research flag:** Needs phase research before implementation. Naninovel's Unity 6 / URP integration specifics and current custom command API should be verified against current Asset Store version before the milestone begins. The `IStateManager` serialization API for VN state coordination also needs current documentation review.

---

### Phase 5: Desktop Wallpaper Mode

**Rationale:** Wallpaper mode is the final integration layer — it requires the ship state (resources, combat) to be live so the HUD overlay has real data to display. It is the most technically risky phase and should be built last so it does not block other systems. It must be isolated behind a clean interface with a working fallback from day one.

**Delivers:** WallpaperManager with P/Invoke to user32.dll (FindWindow / SetParent / SetWindowLong / SetWindowPos); WorkerW validation with monitor-bounds check; re-application of window styles on every `Application.focusChanged` callback; fallback to normal windowed mode if embedding fails; background FPS throttle (Application.targetFrameRate = 10 when unfocused; configurable in Settings); URP Quality Level switching (minimal "wallpaper" quality tier vs. active quality tier); system tray icon with Show / Hide / Quit; glanceable ship HUD on wallpaper layer (resource levels, ship state); previous wallpaper restoration on clean exit; "display only" design (no interactive elements in the embedded window — all interaction via system tray or a spawned overlay window).

**Addresses:** Table stakes #6 (wallpaper mode), #7 (system tray); differentiator #1 (desktop wallpaper mode is the primary differentiator).

**Avoids:** Pitfall 1 (WorkerW breaking on Windows Update — robust HWND validation + fallback); Pitfall 2 (Unity resetting window styles — re-apply after every focusChanged); Pitfall 6 (URP Quality Level without assigned asset); Pitfall 12 (background CPU/GPU hammer — FPS throttle from day one); Pitfall 13 (click-through complexity — design as display-only from the start, not retrofitted).

**Research flag:** Needs phase research before implementation. The WorkerW technique specifics for Windows 11 23H2 and later should be validated against current community sources (lively-wallpaper GitHub issues, Wallpaper Engine community forums) before implementation begins. The specific HWND acquisition method for Unity 6 standalone builds and the URP render texture pipeline for wallpaper output both need a proof-of-concept test build before committing to the full implementation.

---

### Phase 6: Weekly Self-Report System

**Rationale:** The weekly self-report is a lifecycle retention mechanic, not a core loop mechanic. It depends on the habit history data generated by Phase 2 and the resource bonus system from Phase 3. Deferring it to a late phase allows the form's UX to be designed with real data about what players actually track.

**Delivers:** Weekly self-report form (3–5 questions maximum, captain's log framing); habit history pull from QuestManager; resource bonus grant on completion; report state persisted in save data; optional-but-rewarded design (no penalty for skipping; returns following week without judgment); prototype validation of form UX and emotional tone before coding.

**Addresses:** Table stakes #10 (weekly self-report); lifecycle retention.

**Avoids:** Pitfall 14 (self-report that feels like a chore — captain's log framing, 3–5 questions max, optional, tangible reward).

**Research flag:** The form UX and tone should be prototyped (paper or Figma) and emotionally validated before implementation. No technical research needed — implementation patterns are standard Unity UI.

---

### Phase Ordering Rationale

The order is dictated by the feature dependency graph from FEATURES.md:

```
Save/Load [P1] → everything that persists
Settings [P1] → wallpaper toggle, audio before sound events
Habit Quests + Resources [P2] → resource store for combat; events for VN triggers
Automated Combat [P3] → calibrated against real resource data from P2
VN Character System [P4] → triggered by habit events from P2; saves alongside game save from P1
Wallpaper Mode [P5] → displays real ship state from P2+P3; toggle from P1 settings
Weekly Self-Report [P6] → pulls habit history from P2; grants resources from P3 economy
```

No phase can deliver coherent value without its upstream phases complete. Wallpaper mode is last because it integrates all other systems; its wallpaper HUD is only meaningful when ship resources and combat state are real. The VN system comes before wallpaper because character dialogue triggered by habit completion is part of the core gameplay loop, while wallpaper mode is the ambient presentation layer.

### Research Flags Summary

| Phase | Research Needed | Reason |
|-------|----------------|--------|
| Phase 1: System Foundations | No | Standard Unity patterns, confirmed against repo code |
| Phase 2: Habit Quests + Resources | No | Standard idle + habit app patterns; balance via SO data |
| Phase 3: Automated Combat | No | Standard idle economy patterns; balance via playtesting |
| Phase 4: VN Character System | Yes — before milestone starts | Naninovel Unity 6 / URP integration; IStateManager API; custom command API; verify Asset Store version |
| Phase 5: Wallpaper Mode | Yes — before milestone starts | WorkerW behavior on current Windows builds; Unity 6 HWND acquisition; URP render texture pipeline; proof-of-concept test build required |
| Phase 6: Weekly Self-Report | Partial — UX prototype only | Form UX and tone need prototype validation; no technical research needed |

---

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | All core stack confirmed against actual repo files (ProjectVersion.txt, manifest.json, source code). Naninovel pricing and exact Unity 6 compatibility are MEDIUM — verify before purchase. |
| Features | HIGH for table stakes and anti-features | Genre contracts are well-established across Habitica, Cookie Clicker, Wallpaper Engine, Finch. Differentiator novelty assessment is MEDIUM — design judgment, not empirical data. |
| Architecture | HIGH | MonoSingleton and SO patterns confirmed against project source. Awaitable availability confirmed for Unity 6000.x (Unity 2023.1+). Naninovel custom command API is MEDIUM — verify against current version. |
| Pitfalls | HIGH for Unity/design pitfalls; MEDIUM for Win32 | WorkerW behavior on Windows 11 23H2+ is MEDIUM — training data through Aug 2025; Windows Update behavior after that date is not verified. All Unity-specific pitfalls are HIGH confidence. |

**Overall confidence:** MEDIUM-HIGH

### Gaps to Address

- **Naninovel Unity 6 / URP compatibility:** Training data confirms active support through mid-2025. Verify current Asset Store version and URP Renderer Feature setup guide before Phase 4 begins. Purchase only after verification.

- **Win32 WorkerW on current Windows builds:** The WorkerW tree structure change behavior between Windows 11 22H2 and 23H2 cannot be confirmed from training data alone. Before Phase 5 planning, check current GitHub issues in lively-wallpaper (github.com/rocksdanister/lively) and any available Wallpaper Engine community documentation on Windows 11 compatibility.

- **Unity 6 HWND acquisition in standalone builds:** `Process.GetCurrentProcess().MainWindowHandle` is the established method, but should be validated with a minimal test build before committing to the wallpaper architecture. This is a one-day proof-of-concept task, not a research task.

- **Resource economy balance:** Unit economics (how many resources per habit completion, how much power per resource, how many resources combat costs) cannot be defined through research — they require iterative playtesting. Phase 2 should include a debug overlay showing resource rates and projected timelines. Phase 3 should define a target play-arc in a config SO before implementing any combat numbers.

- **Naninovel pricing:** Listed as approximately $70 USD as of 2024; verify current price on the Unity Asset Store before budgeting or purchasing.

---

## Sources

### Primary (HIGH confidence — confirmed against repo code)
- `StellarCommand/ProjectSettings/ProjectVersion.txt` — Unity 6000.3.11f1 confirmed
- `StellarCommand/Packages/manifest.json` — all package versions confirmed
- `Assets/_Project/Scripts/Core/SaveManager.cs` — save implementation reviewed
- `Assets/_Project/Scripts/Core/BootManager.cs` — scene flow reviewed
- `Assets/_Project/Scripts/Core/MonoSingleton.cs` — singleton pattern reviewed
- `Assets/_Project/Scripts/Core/GameManager.cs` — current empty state confirmed

### Secondary (HIGH confidence — stable, well-documented Unity patterns)
- Unity 6 SceneManager.LoadSceneAsync / Awaitable documentation
- Unity JsonUtility serialization constraints (official docs)
- Unity PlayerPrefs API and Windows registry behavior
- Unity Audio Mixer bus pattern
- ScriptableObject definition / runtime state split (Ryan Hipple, Unite Austin 2017)
- C# events vs UnityEvents tradeoff analysis (community consensus)

### Secondary (MEDIUM confidence — community patterns, training data through Aug 2025)
- Win32 WorkerW wallpaper embedding technique (Wallpaper Engine community, open-source wallpaper projects)
- Naninovel URP integration and Unity 6 compatibility (naninovel.com documentation)
- Naninovel IStateManager and custom command API
- Habitica punishment mechanic churn data (Habitica public blog posts, 2015–2020)

### Tertiary (LOW confidence — needs current verification)
- Naninovel pricing (approximately $70 USD as of 2024; verify before purchase)
- WorkerW behavior on Windows 11 23H2 and later (cannot verify against post-Aug-2025 Windows Update changelogs)

---

*Research completed: 2026-03-21*
*Ready for roadmap: yes*
