# Technology Stack

**Project:** Stellar Command
**Researched:** 2026-03-21
**Unity version confirmed from repo:** 6000.3.11f1 (Unity 6 LTS)

---

## Confirmed Project Baseline

The following already exists in the repo and is treated as fixed constraints, not decisions:

| Component | Version | Status |
|-----------|---------|--------|
| Unity Editor | 6000.3.11f1 (Unity 6 LTS) | Confirmed — ProjectVersion.txt |
| URP | 17.3.0 | Confirmed — manifest.json |
| Input System | 1.19.0 | Confirmed — manifest.json |
| uGUI | 2.0.0 | Confirmed — manifest.json |
| Timeline | 1.8.11 | Confirmed — manifest.json |
| Visual Scripting | 1.9.10 | Confirmed — manifest.json (unused, can be removed) |
| MonoSingleton base | custom | Confirmed — Core/MonoSingleton.cs |
| SaveManager (JSON) | custom | Confirmed — Core/SaveManager.cs using JsonUtility |
| BootManager | custom | Confirmed — Core/BootManager.cs |

---

## Recommended Stack

### Core Engine

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Unity 6 LTS | 6000.3.x | Engine | Already in project. Unity 6 LTS (6000.x) is the current long-term support line as of 2025. Do not upgrade mid-project — LTS patches are safe; minor version bumps are not. |
| URP | 17.3.0 | Rendering pipeline | Already in project. Correct for a 2D/stylized desktop game. Provides Renderer Features needed for the wallpaper overlay visual layer. |

### Save System

**Recommendation: Keep the existing custom JSON SaveManager. Extend it with versioning and backup. Do not purchase ES3.**

The existing `SaveManager` in `Core/SaveManager.cs` uses `JsonUtility` + `File.WriteAllText` to `Application.persistentDataPath/Saves/`. This is the correct foundation. It needs two additions before shipping any persistent data:

| Addition | Why Needed |
|----------|------------|
| Save versioning field on all save structs | Without a schema version, adding new fields silently breaks old saves — `JsonUtility` leaves unrecognized fields as default, which can corrupt resource counts |
| Backup-before-write pattern | Single file-write is not atomic. A crash mid-write produces a corrupt file. Write to `.json.tmp` then `File.Move` (overwrites atomically on NTFS) |

**Why not ES3 (Easy Save 3):**
ES3 is a paid Asset Store plugin (~$50). It provides encryption, type-safe key-value storage, and reference serialization. None of those justify the dependency for this project. The game's save data is non-sensitive local state (habits, resources, ship progress) that benefits from being human-readable JSON for debugging. ES3's main value is saving complex Unity object graphs (GameObjects with component references); Stellar Command's data model is plain data structs — JsonUtility handles this well. The custom SaveManager already does everything ES3 would do for this use case.

**Why not PlayerPrefs:**
PlayerPrefs writes to the Windows registry. It has a 1MB total size limit, no structured types (only int/float/string), and no versioning. It is appropriate only for settings primitives (audio volume, resolution index). Use it for settings only — see Settings section below.

**Why not Unity's SaveData package (com.unity.savedata):**
This package was experimental/preview as of Unity 6 and targets console platforms. Not appropriate for a Windows desktop project.

### Settings Persistence

**Recommendation: PlayerPrefs for settings, isolated from game save data.**

Settings (audio volume, resolution, fullscreen, wallpaper mode toggle, keybinds) belong in PlayerPrefs, not in the SaveManager's JSON files. Reason: settings need to load before any scene loads, need to survive a "delete save data" reset, and are small key-value data that fits PlayerPrefs' model. The 1MB limit is not a concern for a settings-only usage.

Pattern: a `SettingsManager : MonoSingleton<SettingsManager>` that wraps PlayerPrefs with typed getters/setters and a `Save()` call that flushes with `PlayerPrefs.Save()`. Settings are loaded in `BootManager.Start()` before scene transition.

```csharp
// Settings keys — define as constants, never raw strings at call sites
public static class SettingsKeys
{
    public const string MasterVolume   = "settings.audio.masterVolume";
    public const string MusicVolume    = "settings.audio.musicVolume";
    public const string SfxVolume      = "settings.audio.sfxVolume";
    public const string WallpaperMode  = "settings.display.wallpaperMode";
    public const string Resolution     = "settings.display.resolutionIndex";
    public const string Fullscreen     = "settings.display.fullscreen";
}
```

Namespace your keys (prefix with `settings.`) to avoid collisions with any third-party packages that also use PlayerPrefs.

### Scene Management

**Recommendation: Unity's built-in SceneManager with an async additive loading pattern, managed by a SceneController singleton.**

Do not use a third-party scene management asset. Unity 6's SceneManager API is sufficient, and third-party scene managers (like Opsive or Toolbelt) add abstraction that fights against the existing Boot-first singleton pattern.

Recommended flow:

```
Boot (persistent)
  └─ loads MainMenu (additive or single)
       └─ transition to Game (single)
            ├─ Ship view
            ├─ Quest log
            └─ VN dialogue (additive overlay)
```

Key decisions:
- **Boot scene** stays loaded for the duration of the application (existing pattern — correct).
- **Game scene** is a single persistent scene. Do not split game subsystems into multiple additive scenes at this stage — the complexity is not justified until content requires it.
- **VN dialogue** runs as an additive overlay scene on top of Game (Naninovel's default behavior handles this).
- **Wallpaper mode** does not require a scene change — it changes the window state and rendering target via Win32 API while the Game scene continues running normally.

Use `SceneManager.LoadSceneAsync` for all transitions. Never use `SceneManager.LoadScene` (synchronous) in production — it hitches the main thread. The existing `BootManager` uses synchronous load; this should be updated to async.

Loading screen pattern: show a loading canvas (part of Boot or a dedicated overlay), trigger async load, await `asyncOp.isDone`, hide canvas. For this game's scene count and data size, a simple progress bar is sufficient.

### Win32 API Interop (Wallpaper Mode)

**Recommendation: P/Invoke directly from C#. No third-party plugin needed.**

The desktop wallpaper mode requires:
1. Making the Unity window a child of the Windows desktop worker window (the window that sits behind desktop icons but in front of the wallpaper bitmap).
2. Removing Unity's window decorations (title bar, border) and setting it to fullscreen dimensions.
3. Keeping Unity rendering and input processing running while the window is "behind" the desktop.

This is achieved entirely via P/Invoke to `user32.dll` — no Unity plugin, no asset store package, no native DLL needed.

Core Win32 calls required:

```csharp
[DllImport("user32.dll")]
static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

[DllImport("user32.dll")]
static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

[DllImport("user32.dll")]
static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

[DllImport("user32.dll")]
static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
    int x, int y, int cx, int cy, uint uFlags);
```

The "Progman" / "WorkerW" trick: `FindWindow("Progman", null)` gets the desktop shell window. Sending it message `0x052C` causes Windows to spawn a `WorkerW` window behind the icons. You then call `SetParent` on the Unity `HWND` to reparent it into that `WorkerW`.

**Critical constraint:** This requires Unity's window handle. Use `System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle` to obtain the HWND. In Unity 6 on Windows standalone, `GetCurrentProcess().MainWindowHandle` reliably returns the Unity window's HWND. `Display.main.systemWidth` / `Display.main.systemHeight` give you the target resolution for `SetWindowPos`.

**Input in wallpaper mode:** When the Unity window is reparented to WorkerW, Windows stops sending standard window messages to it for mouse clicks (the desktop shell intercepts them). This is expected behavior — in wallpaper mode, the game is ambient/display-only. The game should disable interactive UI when wallpaper mode is active and rely on a system-tray icon or overlay hotkey to bring the full window back. This is a design constraint to document explicitly.

Confidence: MEDIUM. The P/Invoke pattern is well-established across multiple Unity wallpaper projects on GitHub. The exact HWND acquisition method for Unity 6 standalone should be validated by a test build before implementing the full feature.

### Visual Novel System (Naninovel)

**Recommendation: Naninovel from the Unity Asset Store. Do not build a custom VN system.**

Naninovel is the dominant VN plugin for Unity. It is the correct choice for this project for the following reasons:

**What Naninovel provides:**
- A script-based dialogue scripting language (`.nani` files) — writers/designers can author content without touching C#
- Character sprite management with expression variants
- Background / CG scene management
- Dialogue history / backlog
- Save/load integration (its own save slots that can be coordinated with the game's SaveManager)
- Audio management (voice, BGM, SFX per dialogue scene)
- Integration with Unity 6 and URP — Naninovel has explicit URP support and its own Renderer Feature
- Scene management as an additive overlay — Naninovel runs in its own initialization layer and layers on top of the game's scene, which is exactly the pattern needed for crew dialogue that appears over the ship view

**Pricing:** Naninovel is sold on the Unity Asset Store. As of 2024 the price was approximately $70 USD for a perpetual license per seat. Verify current pricing before purchase. It is not a subscription.

**Unity 6 compatibility:** Naninovel maintained active Unity 6 support throughout 2024-2025. Confidence: MEDIUM (confirmed in training data through mid-2025; verify current version from Asset Store before purchase).

**Integration pattern for Stellar Command:**

Naninovel's initialization should be deferred — do not initialize it at Boot. Initialize it on first crew dialogue trigger. Naninovel provides `RuntimeInitializer` and supports manual initialization via `Engine.InitializeAsync()`. This keeps Boot fast and avoids loading Naninovel's resource bundles until the player actually enters a dialogue sequence.

```csharp
// Lazy init pattern
if (!Engine.Initialized)
    await Engine.InitializeAsync();

var player = Engine.GetService<IScriptPlayer>();
await player.PreloadAndPlayAsync("CrewDialogue_Zara_Meet");
```

**Naninovel save coordination:** Naninovel manages its own save state (script playback position, variable state). Configure it to use the same `Application.persistentDataPath` root as the game's SaveManager. They write to different subdirectories and do not conflict.

**What NOT to use:**
- **Ink / Inkle's ink-unity-integration**: Ink is a narrative scripting language well-suited for branching logic-heavy stories. For Stellar Command the VN system is primarily linear character vignettes with some branch points, not deep interactive fiction. Ink requires building your own presentation layer (character sprites, backgrounds, transitions) on top. Naninovel provides that layer out of the box.
- **Fungus**: Abandoned / unmaintained as of 2023. Do not use.
- **Custom dialogue system**: Building character sprite management, typewriter effects, dialogue history, audio sync, and save state from scratch is a 2-4 week detour. Naninovel exists precisely to avoid this.

### UI Framework

**Recommendation: uGUI (Unity UI) for all game UI. Do not introduce UI Toolkit for runtime UI at this stage.**

uGUI (com.unity.ugui 2.0.0) is already in the project and is the correct choice. It integrates naturally with URP's Screen Space Overlay and Camera modes, has universal community support, and Naninovel's dialogue UI is built on uGUI.

UI Toolkit (the newer Unity UI system) is appropriate for editor tooling and is production-ready for runtime in Unity 6, but it has a steeper learning curve, a different event model, and its URP integration (specifically render-to-world-space) has had issues in prior Unity 6 versions. Since Naninovel uses uGUI, mixing both systems adds complexity with no benefit.

**Do not use TextMeshPro separately** — uGUI 2.0+ in Unity 6 has TMP integration built in. `TextMeshProUGUI` is available without a separate package import.

### Audio

**Recommendation: Unity's built-in Audio Mixer. No third-party audio plugin needed at this stage.**

Stellar Command is a low-intensity idle game with ambient music, SFX for UI interactions, and Naninovel-managed dialogue audio. Unity's Audio Mixer with mixer groups (Master → Music, SFX, Voice) is entirely sufficient. FMOD and Wwise are appropriate for games with complex adaptive audio — do not introduce that complexity here.

The existing `AudioManager.asset` in ProjectSettings confirms the standard Unity audio system is initialized. Wire audio volume to the SettingsManager's PlayerPrefs values via `AudioMixer.SetFloat("MusicVolume", linearToDecibel(volume))`.

Confidence: HIGH — standard Unity pattern, no third-party required.

### Idle Game Patterns (Resource Accumulation)

The game is architecturally an incremental/idle game where resources accumulate via player actions (habit check-ins) rather than a real-time timer. This is a simpler resource model than clicker games like Cookie Clicker.

**Recommended pattern: ScriptableObject-defined resource types + runtime data in plain C# classes saved by SaveManager.**

```
ResourceTypeSO (ScriptableObject)
  - id: string
  - displayName: string
  - icon: Sprite
  - description: string

ShipResourceStore (plain C# class, saved by SaveManager)
  - Dictionary<string, long> resources  // resourceTypeId → amount
```

ScriptableObjects define the resource catalog (no runtime mutation). The runtime store is a plain C# data object serialized by SaveManager. This pattern:
- Survives design changes — add a new resource type by adding a new SO asset, no code change
- Is fully serializable by `JsonUtility` (with a wrapper, since `Dictionary` requires a custom serializer — use a `List<ResourceEntry>` with `string key, long value` instead)
- Avoids the "put game logic in MonoBehaviour" trap

**Time-gap accumulation:** This game does NOT use real-time passive resource accumulation (the player earns resources by logging habits, not by waiting). This eliminates the hardest class of idle game problems (DateTime drift, timezone manipulation, offline calculation). Keep this decision firm — adding real-time accumulation later is possible but changes the save structure significantly.

**Quest system pattern:** Quests should be ScriptableObject-defined templates instantiated into runtime state. A `QuestTemplateSO` defines the quest type; a `QuestInstance` (plain C# class) tracks completion state and is saved by SaveManager. This mirrors the resource pattern.

### Input System

**Recommendation: Use the existing New Input System (com.unity.inputsystem 1.19.0) for all input. Do not add the Legacy Input Manager.**

Input System is already in the project. For a desktop idle game, the primary input surface is UI (mouse clicks on the main window). The game does not need complex action maps. Use a single `PlayerInputActions` asset with one Action Map containing basic UI navigation actions. Naninovel has built-in Input System integration.

**Keybinds for settings:** Naninovel supports rebindable keys for dialogue advance, skip, etc. The game's own hotkeys (open quest log, toggle wallpaper mode) should be registered in the Input Actions asset so they appear in the Settings screen for rebinding.

---

## What Not to Use

| Technology | Category | Why Not |
|------------|----------|---------|
| ES3 / Easy Save 3 | Save system | Paid plugin solving problems this project does not have. Custom JSON SaveManager already exists and is sufficient. |
| PlayerPrefs for game data | Save system | Registry-based, size-limited, no versioning, no structured types. Settings only. |
| Fungus | Visual novel | Abandoned/unmaintained since ~2023. Active maintainership required for a core system. |
| Ink / inkle | Visual novel | Narrative logic only, no presentation layer. Doubles implementation work vs Naninovel. |
| UI Toolkit (runtime) | UI | Mixed system with uGUI adds complexity. Naninovel is uGUI-based. Defer UI Toolkit to a future editor-only tooling need. |
| FMOD / Wwise | Audio | Overkill for ambient idle game audio. Unity Mixer is sufficient. |
| Addressables | Asset loading | Appropriate for large content-heavy games with DLC. Premature for a solo project at this scale. Add if content volume demands it. |
| NavMesh AI | Gameplay | Already in manifest (com.unity.ai.navigation 2.0.11). Combat is fully automated — no pathfinding or NavMesh needed. Can be removed to reduce build size. |
| Visual Scripting | Tooling | In manifest (com.unity.visualscripting 1.9.10). Not used. Should be removed to reduce build size and compilation time. |
| Multiplayer Center | Infrastructure | In manifest (com.unity.multiplayer.center 1.0.1). Project is explicitly single-player. Remove. |
| SceneManager.LoadScene (sync) | Scene management | Blocks main thread. The existing BootManager.cs uses synchronous load and should be updated to async. |

---

## Packages to Remove

The following packages are in `manifest.json` but serve no purpose for this project. Removing them reduces compilation time and build size:

```json
"com.unity.ai.navigation": "2.0.11",        // no pathfinding in an idle game
"com.unity.multiplayer.center": "1.0.1",    // explicitly single-player
"com.unity.visualscripting": "1.9.10",      // not used, adds overhead
```

Remove via Package Manager or by editing `manifest.json` directly.

---

## Packages to Add

| Package | Source | Version | When |
|---------|--------|---------|------|
| Naninovel | Unity Asset Store | Latest stable for Unity 6 | Before visual novel milestone |

No additional UPM packages are required for the current milestone (save/load, scene flow, settings). The existing stack is sufficient.

---

## Installation Reference

Nothing to install for the current System Foundations milestone — all required packages are already in `manifest.json`. When Naninovel is added:

1. Purchase from Unity Asset Store under the StellarCommand project account.
2. Import via Package Manager → My Assets.
3. Follow Naninovel's URP setup guide: add the Naninovel Renderer Feature to the URP Renderer asset.
4. Configure initialization mode to Manual (not Automatic) so it does not load on Boot.

---

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Unity version | HIGH | Confirmed directly from ProjectVersion.txt in repo |
| Existing packages | HIGH | Confirmed directly from manifest.json |
| Custom SaveManager adequacy | HIGH | Code reviewed — JsonUtility + file I/O pattern is standard and correct |
| PlayerPrefs for settings | HIGH | Established Unity pattern, well-documented |
| SceneManager async pattern | HIGH | Standard Unity 6 — no version uncertainty |
| Win32 P/Invoke wallpaper pattern | MEDIUM | Pattern is well-established; exact HWND acquisition for Unity 6 standalone and input-in-wallpaper behavior need a test build to verify |
| Naninovel Unity 6 / URP support | MEDIUM | Confirmed active support in training data through mid-2025; verify current Asset Store version before purchase |
| Naninovel pricing | LOW | ~$70 USD as of 2024; verify current price before budgeting |
| Idle game ScriptableObject pattern | HIGH | Standard Unity data-driven pattern, widely validated |

---

## Sources

- Repo: `StellarCommand/ProjectSettings/ProjectVersion.txt` — Unity 6000.3.11f1 confirmed
- Repo: `StellarCommand/Packages/manifest.json` — all package versions confirmed
- Repo: `Assets/_Project/Scripts/Core/SaveManager.cs` — existing save implementation reviewed
- Repo: `Assets/_Project/Scripts/Core/BootManager.cs` — existing scene flow reviewed
- Unity documentation (training data, verified through Unity 6 release): SceneManager.LoadSceneAsync, PlayerPrefs, Audio Mixer API
- Win32 "WorkerW wallpaper" pattern: known technique across Unity/WPF wallpaper engine implementations (confidence MEDIUM — needs test build validation)
- Naninovel: naninovel.com documentation (training data through mid-2025)
