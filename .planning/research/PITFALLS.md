# Domain Pitfalls

**Domain:** Windows desktop idle self-improvement game (Unity URP, Win32 wallpaper mode, Naninovel)
**Researched:** 2026-03-21
**Confidence note:** WebSearch and WebFetch were unavailable during this research session. All findings draw from training knowledge (cutoff August 2025). Confidence levels reflect that constraint. Claims marked HIGH are well-established, reproducible technical facts; MEDIUM are widely-reported community patterns; LOW are plausible but unverified specifics.

---

## Critical Pitfalls

Mistakes that cause rewrites, data loss, or architectural dead ends.

---

### Pitfall 1: WorkerW Window Hierarchy Breaks on Windows Update

**Confidence:** MEDIUM

**What goes wrong:** The desktop wallpaper embedding trick relies on sending `0x052C` (the undocumented `SendMessageTimeout` to the `Progman` window) to spawn a `WorkerW` window, then reparenting your Unity HWND as a child of that `WorkerW` instance. This is not a documented or supported API. Microsoft has changed the `WorkerW` tree structure in Windows 10 version updates and in Windows 11 22H2. After certain cumulative updates the shell creates two `WorkerW` windows instead of one; code that picks the first one gets the wrong one and renders behind the wrong layer, appearing invisible or on top of icons instead of behind them.

**Why it happens:** The technique depends on internal shell (`explorer.exe`) implementation details, not a public contract.

**Consequences:** Wallpaper mode stops working silently after a Windows Update. The Unity window either disappears behind the desktop background entirely or sits on top of icons as a floating window instead of being embedded. The bug only appears on the user's machine after the update; it does not reproduce on the developer's machine until they update.

**Prevention:**
- Write a robust HWND-walking function that validates the found `WorkerW` by checking its parent, position, and size against the monitor bounds before reparenting.
- Keep the wallpaper attachment code isolated in a single `WallpaperEmbedder` class so it can be patched without touching game logic.
- Always handle the failure case: if reparenting fails or the window is no longer correctly embedded, fall back gracefully to a normal windowed mode and notify the user via a settings flag.
- Test against at least Windows 10 21H2, Windows 10 22H2, and Windows 11 22H2 and 23H2 before release.

**Detection:** Add a post-attach verification step that queries the actual parent HWND and logs it. If parent is not the expected `WorkerW`, log an error and bail to fallback.

---

### Pitfall 2: Unity Window Style Mutations Conflict With Wallpaper Embedding

**Confidence:** HIGH

**What goes wrong:** To embed Unity behind the desktop, you must strip the Unity window's title bar and borders (clear `WS_CAPTION`, `WS_THICKFRAME`, `WS_SYSMENU` via `SetWindowLong`) and set `WS_CHILD`. Unity's own window management code (the player loop, fullscreen toggle, display settings changes) can re-apply window styles, un-child the window, or force it back to a normal top-level window. This happens when:
- The user alt-tabs.
- Unity's `Screen.fullScreen` or `Screen.SetResolution` is called.
- The OS sends a `WM_ACTIVATE` or `WM_SIZE` message that Unity's internal handler processes.

**Consequences:** The window pops out of the wallpaper layer and appears as a visible floating window over everything, or the game window disappears entirely.

**Prevention:**
- Intercept `WM_WINDOWPOSCHANGING` and `WM_STYLECHANGING` messages in a native plugin to prevent Unity from resetting styles.
- Alternatively, use a `WS_EX_LAYERED` + `WS_EX_TRANSPARENT` approach for the input pass-through layer separately from the rendering window.
- Never call `Screen.fullScreen = true` in wallpaper mode; keep the game in windowed mode internally and manage size via Win32.
- Apply the wallpaper embedding after every `Application.focusChanged` callback, not just at startup.

**Detection:** Log `GetWindowLong(hwnd, GWL_STYLE)` and `GetWindowLong(hwnd, GWL_EXSTYLE)` every frame in debug builds. Alert when styles change unexpectedly.

---

### Pitfall 3: Save System Without Versioning Causes Silent Data Loss on Schema Changes

**Confidence:** HIGH

**What goes wrong:** You serialize game state to JSON (or binary) without a version field. When you add a new field (e.g., a new resource type, a new quest property), old save files silently use `default(T)` for the missing field — often `0`, `null`, or `false`. When you rename or remove a field, old data is silently dropped. When you change a field's type (e.g., `int` to `enum`), deserialization throws an exception that corrupts or wipes the save.

This is the most common cause of "my save broke after the update" bugs. With a solo game that will go through iterative design changes (as explicitly called out in PROJECT.md), save schema will change frequently.

**Why it happens:** Developers add the save system early in a "good enough for now" form and never retrofit versioning because it "works currently."

**Consequences:** Players lose progress after any update that touches save data. In a habit-tracking game, losing logged habit data is a high-trust violation — the player may quit permanently.

**Prevention:**
- Add `int saveVersion` as the first field in every save file from Day 1.
- Write a `MigrateFrom(int fromVersion, SaveData raw)` function that is called on load whenever `fromVersion < currentVersion`.
- Prefer additive schema changes (new optional fields with defaults) over destructive changes (renames, type changes, removals).
- Use `[JsonProperty("snake_case_stable_name")]` or `[DataMember(Name = "...")]` attributes so C# field renames do not break existing saves.
- Store save files in `Application.persistentDataPath`, not `Application.dataPath`.
- Back up the save file before attempting migration; if migration throws, keep the backup and surface an error.

**Detection:** Write unit tests for each migration path. Commit a "golden" save file at each version transition and assert it loads correctly.

---

### Pitfall 4: Treating ScriptableObjects as Both Data Definition and Runtime State

**Confidence:** HIGH

**What goes wrong:** A `QuestDefinition` ScriptableObject holds both static definition data (name, description, reward amounts) and runtime state (isCompleted, currentProgress). Because ScriptableObjects are assets, mutations made in Play Mode in the Editor persist between sessions if the asset is dirty. In a build, ScriptableObject instances are loaded from disk but are not automatically reset between game sessions. If the player's quest progress is stored directly on the SO, quitting and restarting does not reset it unless you explicitly do so at startup.

More subtly: if you pass a SO reference to a save system and serialize "the SO's state," you are serializing runtime mutations of a design asset, making it impossible to distinguish "what the designer defined" from "what the player has done."

**Prevention:**
- ScriptableObjects hold definition data only (immutable at runtime): names, base reward values, icon references, dialogue triggers.
- Runtime state lives in plain serializable C# classes (`QuestState`, `ShipResourceState`) that reference SOs by a stable string ID.
- Save data serializes only the plain C# runtime state, never SO references.
- In Editor, use `[NonSerialized]` or runtime-only fields with `OnEnable` reset logic for any SO that needs to carry transient state during testing.

---

### Pitfall 5: Habit Loop Design That Turns Into Obligation

**Confidence:** HIGH (based on well-documented Habitica community postmortems and habit app research)

**What goes wrong:** The game imposes daily streaks, daily quests, or daily caps that punish the player for missing a day. The player misses one day due to illness or travel. They return to the game to find broken streaks, lost resources, or negative consequences. The emotional response is not motivation to return — it is guilt and avoidance. The player quits.

Habitica's forums and user-churn data consistently show that streak penalties are the single largest churn driver in habit-gamification apps. "Negative reinforcement" (taking away progress) works for short-term compliance but causes long-term abandonment.

**Why it happens:** Designers copy mechanics from games (streaks, daily quests) that work in games because the player chose to engage that day. In a habit-gamification app, the player is being asked to engage every day whether or not they want to. The asymmetry destroys the mechanic.

**Prevention:**
- Use "streak bonus" not "streak penalty": consecutive days add bonus resources; missing a day sets the bonus multiplier to 1 (neutral), not below 1 (punishing).
- Never remove earned resources or progress. Negative consequences feel arbitrary and unfair in a game tied to real-life effort.
- Make the ship's power decay slowly and gracefully (over weeks, not hours) if the player stops logging — the game should wait for the player, not punish them.
- Weekly self-report as a "catch-up" mechanic is good design: it gives players a scheduled low-friction re-entry point.

**Detection:** Play-test with a break of 3–7 days and observe the emotional state when returning.

---

## Moderate Pitfalls

---

### Pitfall 6: URP Renderer Asset Per-Quality-Level Misconfiguration

**Confidence:** HIGH

**What goes wrong:** URP requires a `UniversalRenderPipelineAsset` assigned both at the top-level Graphics settings and at each Quality Level. If a quality level is missing its own URP asset (or points to the wrong one), switching quality levels at runtime crashes with a null-reference in the renderer or silently falls back to no renderer, producing a black screen. For a desktop idle game that should run at low GPU cost in the background, you will want at least two quality tiers: a minimal "background/wallpaper" tier and a normal "active window" tier.

**Prevention:**
- Create explicit URP assets for each quality level (at minimum: Low, Medium/High).
- Assign them in Edit > Project Settings > Quality — every row needs its own asset, not just the default.
- Validate at startup with `QualitySettings.renderPipeline != null` before allowing quality switching.

---

### Pitfall 7: Naninovel Initialization Order and Engine Conflicts

**Confidence:** MEDIUM

**What goes wrong:** Naninovel manages its own initialization sequence via its `EngineConfiguration` asset. If your game's own initialization (GameManager singleton, save system load) runs before or after Naninovel's engine init in a way that conflicts with scene loading, dialogue commands that touch game state (awarding resources, unlocking quests) can fire before the systems they depend on are ready. The reverse also causes problems: if Naninovel initializes first and immediately begins executing a script that calls a custom command, the command handler may not be registered yet.

**Prevention:**
- Use Naninovel's `RuntimeInitialized` event as the safe point to register custom commands and state handlers.
- Keep Naninovel's initialization mode as "On Demand" (not "On Engine Start") until all your own singletons are bootstrapped.
- Do not mix Naninovel's managed scene loading with Unity's `SceneManager.LoadScene` directly — Naninovel can get confused about which scene owns which resources.

**Detection:** Put `Debug.Log("[Naninovel] Initialized")` and `Debug.Log("[GameManager] Ready")` in the respective init methods and verify order in the console during boot.

---

### Pitfall 8: Naninovel Script Localization Baking Breaks Iterative Editing

**Confidence:** MEDIUM

**What goes wrong:** Naninovel's localization system generates localization documents from `.nani` scripts. If you run "Generate Localization" while scripts are still in rapid flux, the generated documents become out of sync with the scripts and produce missing-key warnings or show raw keys to the player. For a single-language project this is low risk, but if the plan is ever to add localization, the documents must be regenerated after every script edit.

**Prevention:**
- Treat localization generation as a release-time step, not a per-commit step, until scripts are stable.
- Keep all dialogue in `.nani` scripts (never hardcoded in C#) from the start so future localization requires only tooling, not refactoring.

---

### Pitfall 9: Resource Generation Balance That Makes the Idle Loop Feel Empty

**Confidence:** HIGH

**What goes wrong:** The habit → resource → combat loop breaks if the economy is not calibrated from the start. Two failure modes:
1. **Oversupply:** Logging any habit floods the ship with enough resources to win everything. There is no meaningful scarcity, so logging habits feels pointless — you would win anyway.
2. **Undersupply:** Logging habits produces so few resources that ship progress is imperceptible. The player cannot see the connection between their real-life behavior and in-game outcomes. They stop logging.

The "unit economics" of idle games (how much resource per action, how much resource per combat tick) must be rough-balanced even in early implementation, or early play-testers get the wrong mental model and disengage.

**Prevention:**
- Define a target play-arc before implementing the resource system: "A player who logs 3 habits/day should see X ship upgrade within Y days."
- Implement resource amounts and costs as data (ScriptableObjects or config files), not hardcoded constants, so balance can be tuned without code changes.
- Add a debug overlay showing resource rates and projected upgrade timelines during development.

---

### Pitfall 10: Unity Project Folder Structure That Doesn't Scale

**Confidence:** HIGH

**What goes wrong:** The default Unity project structure puts everything in `Assets/` with no subdirectories. As features accumulate, you get `Assets/Scripts/` with 60 `.cs` files and no way to find anything. More critically: assets without an `asmdef` (Assembly Definition) file all compile into one `Assembly-CSharp.dll`, so a change to any script triggers a full recompile of all scripts. On a large project this means 15–30 second compile times after every edit.

**Prevention:**
- Establish a folder structure on Day 1 and enforce it:
  ```
  Assets/
    _Game/
      Core/          (GameManager, singletons, events) + Core.asmdef
      SaveSystem/    + SaveSystem.asmdef
      Quests/        + Quests.asmdef
      Ship/          + Ship.asmdef
      Combat/        + Combat.asmdef
      UI/            + UI.asmdef
      Wallpaper/     + Wallpaper.asmdef
      VN/            (Naninovel integration) + VN.asmdef
    _Naninovel/      (Naninovel package-generated files)
    _ThirdParty/     (other packages)
    Art/
    Audio/
    ScriptableObjects/
  ```
- Each `asmdef` depends only on `asmdef`s it actually needs; keep dependency graph acyclic.
- Prefix top-level folders with `_` to sort them above Unity's auto-generated folders in the Project window.

---

### Pitfall 11: Singleton Architecture That Becomes a God-Object Tangle

**Confidence:** HIGH

**What goes wrong:** GameManager starts as the convenient place for "everything." Over time it accumulates references to SaveSystem, ResourceSystem, QuestSystem, CombatSystem, UIManager, and SettingsManager. Each of those systems also holds references back to GameManager. You end up with a circular dependency web where nothing can be tested in isolation and initialization order causes NullReferenceExceptions during scenes with different boot paths.

**Prevention:**
- Keep GameManager as a pure bootstrapper and dependency registry — it creates and holds references to other managers but does not contain their logic.
- Use a service-locator pattern or ScriptableObject-based event channels so systems communicate through events, not direct method calls across manager boundaries.
- Each system should be independently initializable (able to run without requiring GameManager to be present) for test scenes.

---

### Pitfall 12: Desktop Idle Game That Hammers the CPU/GPU in the Background

**Confidence:** HIGH

**What goes wrong:** A standard Unity game runs at uncapped or vsync-capped frame rates. In wallpaper mode (always-on behind the desktop), the game runs 24/7. At 60fps with full URP rendering, it uses 5–15% GPU and 2–5% CPU continuously, causing heat, fan noise, battery drain (on laptops), and user complaints. The user opens Task Manager, sees Unity eating resources, and closes the game permanently.

**Prevention:**
- Implement an explicit "background throttle" mode: when the player is not actively viewing the game window, drop to 5–10 fps using `Application.targetFrameRate = 10` and disable expensive rendering features.
- Use `Application.isFocused` and a custom `OnApplicationFocus` hook to detect when the game is the active window vs. running in background.
- In background mode: disable particle systems, reduce shadow distance to 0, disable bloom and post-processing via the URP Volume system.
- Expose a "Background FPS" setting in the settings UI and default it to something conservative (e.g., 10 fps).
- For the wallpaper animation specifically: consider capping at 15–24 fps as a fixed ceiling, since the player is not actively watching it most of the time.

---

### Pitfall 13: Win32 Input Pass-Through Complexity

**Confidence:** MEDIUM

**What goes wrong:** When Unity is embedded behind the desktop, the user still needs to click on desktop icons. By default, a child window embedded in `WorkerW` captures all mouse input over its area, making desktop icons unclickable. The common fix is to set `WS_EX_TRANSPARENT` and `WS_EX_LAYERED` extended styles so mouse events pass through to the desktop. But `WS_EX_TRANSPARENT` makes the entire Unity window click-through — including any settings overlay, ship status panel, or interactive element you want the player to be able to click.

**Why it happens:** Click-through is binary at the window level unless you implement per-pixel hit testing, which requires intercepting `WM_NCHITTEST` and returning `HTTRANSPARENT` based on whether the mouse is over a Unity UI element.

**Prevention:**
- Design the wallpaper mode as "display only with no interactive elements" — all interaction happens in a separate settings/overlay window spawned as a normal top-level window.
- If interactive wallpaper is a requirement, implement `WM_NCHITTEST` per-pixel hit testing early; retrofitting it is painful.
- Document this decision in the architecture from the start so the UX design does not promise interactive wallpaper elements that cannot be delivered cleanly.

---

### Pitfall 14: Weekly Self-Report That Feels Like a Chore

**Confidence:** MEDIUM

**What goes wrong:** The weekly self-report is intended as a lifecycle motivation mechanic. If it requires the player to fill in a long form or justify behavior, it feels like a work performance review, not a game. Players skip it, then feel guilty about skipping it, then quit the game to escape that guilt. This is the same mechanism as the streak punishment problem (Pitfall 5) but triggered by omission rather than failure.

**Prevention:**
- Keep the self-report to 3–5 questions maximum.
- Make it feel like a captain's log, not a self-assessment form — use ship/space metaphor throughout ("Mission debrief," "Fleet status update").
- Give a meaningful, visible resource reward for completing it, not a motivational quote.
- Make it optional-but-rewarded, never required. If the player skips a week, the option should reappear the following week without penalty.

---

## Minor Pitfalls

---

### Pitfall 15: URP Post-Processing in Background Mode Stacks Silently

**Confidence:** HIGH

**What goes wrong:** URP Volume profiles for post-processing (bloom, tonemapping, ambient occlusion) can stack if you have both a global volume and a local scene volume active simultaneously. In desktop/idle games where the camera never moves much, duplicate volumes produce over-brightened bloom or incorrect color grading that does not match the reference.

**Prevention:** Name all Volume profiles explicitly. Keep exactly one global volume per scene. Validate in the URP Frame Debugger before each milestone.

---

### Pitfall 16: Naninovel State Not Included in Game Save

**Confidence:** MEDIUM

**What goes wrong:** By default, Naninovel maintains its own save slots separate from your game save. If the player reloads a game save, the VN state (which character relationship scenes have played, what the current dialogue state is) is not synchronized with the game state. A character relationship at level 3 in the game system might trigger a scene that has already been played, replaying it unexpectedly.

**Prevention:**
- Use Naninovel's `IStateManager` service to serialize VN state into your own save file as a JSON blob.
- Define clear "VN checkpoints" (not continuous state sync) tied to game events: "when relationship level reaches X, trigger scene Y, mark Y as played, save that flag."
- Store "played scene IDs" as a `HashSet<string>` in your save data and check it before triggering any VN sequence.

---

### Pitfall 17: Application.persistentDataPath Varying Between Editor and Build

**Confidence:** HIGH

**What goes wrong:** In the Unity Editor, `Application.persistentDataPath` resolves to a different path than in a build. Developers test save/load in-editor, confirm it works, then ship a build where saved data goes to a different location and "save files are missing." This also affects Naninovel's default save path.

**Prevention:**
- Always log `Application.persistentDataPath` at startup.
- Test save/load in an actual build (not just Editor) before considering the save system done.
- Never hardcode any path — always construct paths from `Application.persistentDataPath`.

---

### Pitfall 18: Hard-Coded Quest and Resource Definitions

**Confidence:** HIGH

**What goes wrong:** Quest types, habit categories, and resource types are defined as `enum` or `const string` in code. When the design changes (a new habit category is added, a resource is renamed), changing the enum breaks existing save data (the integer values shift) and requires recompilation.

**Prevention:**
- Define all quest types, habit categories, and resource types as ScriptableObject assets with a stable GUID-based or explicit string ID.
- Reference them by that stable ID in save data, not by enum value or index.
- Store the SO lookup in a registry (`ResourceTypeRegistry`, `QuestTypeRegistry`) that is itself a ScriptableObject.

---

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|---------------|------------|
| Save/load system (current milestone) | No versioning added at start | Add `saveVersion: 1` to save schema on Day 1, write stub migration function even if it does nothing yet |
| Save/load system (current milestone) | SO state mixed with runtime state | Define `SaveData` as plain C# class before writing any SO |
| Main menu / scene transitions | Naninovel init order conflict | Decide boot order: game systems first, then Naninovel; document it |
| Settings system | Quality level missing URP asset | Verify each Quality Level has an assigned URP asset after settings scaffolding |
| Settings system | No background FPS cap | Add `targetFrameRate` control to settings system from the start, not as a later optimization |
| Habit quest system | Hard-coded habit types | Use ScriptableObject definitions from the first quest type, never define habits as enums |
| Resource system | No balance scaffolding | Add a debug overlay showing resource rates before tuning any values |
| Combat system | Balanced after core loop, not before | Define rough unit economics in a config file before implementing combat |
| Wallpaper mode | Applied as an afterthought | Isolate all Win32 calls in a single class with a clean interface; design fallback mode from day one |
| Wallpaper mode | Interactive elements in click-through window | Decide "display only vs. interactive" architecture before implementing any wallpaper UI |
| VN character system | Naninovel state not in game save | Implement VN state serialization bridge in the same milestone as the VN system, not later |
| VN character system | Localization document drift | Treat localization generation as a release step; never commit stale generated files |
| Weekly self-report | Feels like obligation | Prototype the form UX and emotional tone with a paper/Figma mock before coding it |

---

## Sources

- Training knowledge — Unity URP documentation, Unity Save System patterns (HIGH confidence for Unity-specific items)
- Training knowledge — WorkerW wallpaper embedding technique as documented in wallpaper engine community and open-source implementations (MEDIUM confidence; cannot verify against 2025 Windows Update changelog without web access)
- Training knowledge — Habitica community postmortems and habit-gamification research on streak mechanics (HIGH confidence for behavioral design conclusions; MEDIUM for specific Habitica data points)
- Training knowledge — Naninovel documentation patterns as of mid-2025 (MEDIUM confidence; Naninovel releases frequently and specifics may have changed)
- WebFetch/WebSearch: not available in this research session — all findings are training-derived only

**Verification gaps:** The specific WorkerW behavior differences between Windows 11 22H2 and 23H2 should be validated against current GitHub issues in open-source wallpaper apps (e.g., Wallpaper Engine community forums, lively-wallpaper GitHub issues) before the wallpaper milestone begins. This is flagged as a "needs deeper research" item in SUMMARY.md.
