# Architecture Patterns

**Project:** Stellar Command
**Researched:** 2026-03-21
**Confidence note:** WebSearch and WebFetch are disabled in this environment. All findings are from training knowledge (cutoff August 2025) cross-referenced against the actual project code already on disk. Unity architecture patterns in this document are well-established and stable — these are not cutting-edge claims likely to have changed since August 2025.

---

## Recommended Architecture

The project is a single-player idle desktop game with a defined set of loosely-coupled systems: habits/quests, resources, automated combat, character relationships (Naninovel), wallpaper mode, save/load, and settings. The scale is solo dev, Windows-only, with high design volatility early on.

**The recommended architecture is: MonoSingleton managers for infrastructure + ScriptableObject data + C# events for loose coupling, with one upgrade to the save system.**

This is not a complex architecture — it is a deliberately simple one chosen because:

1. The `MonoSingleton<T>` pattern is already established and works correctly for this scale.
2. ScriptableObject folders already exist in the project (`Quests/`, `Characters/`, `Weapons/`).
3. The design volatility constraint rewards data-driven patterns that can change without rewriting logic.
4. A solo dev project does not benefit from heavyweight DI frameworks.

---

## System Map

```
Boot.unity
  └── BootManager            — entry point, async scene load, init sequencing
  └── GameManager            — global state (GameState enum: Boot/Menu/Playing)
  └── SaveManager            — JSON persistence, versioned save data
  └── AudioManager           — global audio (music + SFX bus)
  └── SceneController        — owns scene transition logic
  └── SettingsManager        — player prefs (audio, display, wallpaper toggle)

MainMenu.unity
  └── MainMenuController     — drives menu UI, reads SettingsManager
  └── [UI Toolkit or uGUI]

Game.unity
  └── QuestManager           — owns quest state, fires events on completion
  └── ResourceManager        — owns resource float values, fires events on change
  └── CombatManager          — reads resources, drives automated combat loop
  └── CharacterManager       — owns crew character states, relationship scores
  └── WallpaperManager       — Win32 API interop, Desktop mode toggle

ScriptableObjects/
  └── Quests/                — QuestDefinitionSO assets (one file per habit type)
  └── Characters/            — CharacterDefinitionSO assets (crew members)
  └── Weapons/               — WeaponDefinitionSO assets (ship weapons)
  └── Resources/             — ResourceTypeSO assets (fuel, morale, etc.)
  └── Settings/              — GameSettingsSO (defaults/config, not runtime state)
```

---

## Pattern 1: Service Locator vs DI vs Singleton — Decision

**Decision: Stay with MonoSingleton managers. Do not add a DI framework.**

**Why not a DI framework (Zenject/VContainer):**
- Zenject and VContainer add meaningful learning curve, installer boilerplate, and debugging complexity.
- For a solo dev idle game with ~8 systems, DI frameworks solve problems you don't have yet.
- `MonoSingleton<T>` already handles initialization order (Boot scene Awake), prevents duplicates, and safely handles quit.
- If the team grows or systems multiply into the dozens, migrate then — the singleton pattern is easy to replace.

**Why not a raw Service Locator:**
- A service locator (static `Services.Get<T>()` dictionary) is functionally equivalent to singletons at this scale but adds an indirection layer that makes call sites harder to read without providing testability benefits unless you also add interfaces.
- The existing `MonoSingleton<T>` already IS a type-safe service locator via `ManagerType.Instance`.

**The one upgrade to make: GameManager should become a state machine, not an empty shell.**

The current `GameManager` overrides `OnInitialize()` with an empty body. It should own a `GameState` enum (`Boot`, `MainMenu`, `Playing`, `Paused`) and fire a `C# event` when state changes. Other systems subscribe to `GameManager.OnStateChanged` to activate/deactivate themselves — this prevents systems from polling or referencing each other directly.

```csharp
// Pattern: GameManager as state machine hub
public class GameManager : MonoSingleton<GameManager>
{
    public enum GameState { Boot, MainMenu, Playing, Paused }

    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnStateChanged;

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}
```

---

## Pattern 2: ScriptableObject Data Architecture

**Use ScriptableObjects for all static definitions. Use plain C# classes for runtime state.**

This is the most important data architecture decision for design volatility. When quest types, resource types, character stats, or weapon parameters change, you edit an asset file — no code recompile, no logic rewrite.

### Definition vs Runtime State split

| Type | What It Is | Storage |
|------|-----------|---------|
| `QuestDefinitionSO` | The template: name, icon, resource reward, reward amount | ScriptableObject asset |
| `QuestSaveData` | Runtime instance: isComplete, completionDate, streak | Plain C# class, serialized to JSON |
| `CharacterDefinitionSO` | Static facts: name, portrait, voice key, base stats | ScriptableObject asset |
| `CharacterSaveData` | Runtime state: relationship score, unlocked dialogue flags | Plain C# class, serialized to JSON |
| `ResourceTypeSO` | Resource identity: name, icon, color, max cap | ScriptableObject asset |
| `WeaponDefinitionSO` | Weapon facts: damage, fire rate, unlock cost | ScriptableObject asset |

**Why this split matters for design volatility:** If you decide quests now have a "difficulty tier" field, you add it to `QuestDefinitionSO` and update assets — no save migration needed, no logic change. If you rename a resource, you update the SO asset and its GUID stays stable across all references.

### ScriptableObject limitations to know

1. **SOs do not serialize runtime state across sessions** — if you set a value on a SO at runtime in a build, that change disappears on restart. Always use separate save data classes for anything that persists. (In Editor, SO changes DO persist, which is a common trap for new Unity devs.)
2. **SOs serialize by reference in scenes/prefabs** — safe to reference from MonoBehaviours via `[SerializeField]`.
3. **Collections of SOs need a registry** — use a `QuestRegistrySO` (a SO that holds a `List<QuestDefinitionSO>`) so `QuestManager` can iterate all definitions without hard-coding paths or using `Resources.Load`.

### Registry pattern

```csharp
// QuestRegistrySO.cs
[CreateAssetMenu(menuName = "StellarCommand/Quest Registry")]
public class QuestRegistrySO : ScriptableObject
{
    public List<QuestDefinitionSO> AllQuests;
}

// QuestManager.cs — drag the registry asset into this field in the Inspector
[SerializeField] private QuestRegistrySO _questRegistry;
```

This keeps iteration clean and lets you add/remove quest types by editing the registry asset, no code changes.

---

## Pattern 3: Event System Architecture

**Use C# events (`event Action<T>`) for system-to-system communication. Use UnityEvents only at the UI boundary.**

### Decision breakdown

| Event Type | Use Case | Verdict |
|------------|----------|---------|
| `event Action<T>` (C#) | System-to-system: "quest completed", "resource changed", "state changed" | **Use this** |
| `UnityEvent` | Inspector-wired UI callbacks: button OnClick, slider OnValueChanged | **Use only for UI** |
| ScriptableObject events (Ryan Hipple pattern) | Cross-scene events that need Inspector wiring without direct references | Optional upgrade if needed |
| Message bus / pub-sub | No specific library; Overkill at this scale | Avoid for now |

**Why C# events over UnityEvents for systems:**
- C# events are type-safe, have no reflection overhead, show up properly in IDE navigation (Find All References), and don't require a MonoBehaviour to wire.
- UnityEvents are serialized in the Inspector — good for designers connecting UI buttons, bad for code-level system communication because connections are invisible in code and break silently if the object is null.
- UnityEvent's serialization overhead and null-slot warnings from destroyed subscribers are consistent pain points in production.

**Why C# events over a message bus:**
- A message bus (string keys or type tokens) decouples senders from receivers at the cost of losing compile-time safety. At 8 systems, the payoff does not exist. Use direct C# events where the publisher type is known and stable.

### Core events to define now

```csharp
// In QuestManager
public event Action<QuestDefinitionSO> OnQuestCompleted;

// In ResourceManager
public event Action<ResourceTypeSO, float> OnResourceChanged; // resource, new amount

// In GameManager
public event Action<GameState> OnStateChanged;

// In CharacterManager
public event Action<CharacterDefinitionSO, int> OnRelationshipChanged; // character, new score
```

UI components subscribe to these at `OnEnable` and unsubscribe at `OnDisable` — the standard Unity pattern for avoiding null reference exceptions from destroyed UI objects.

---

## Pattern 4: Save System Architecture — Versioning

**The existing SaveManager is structurally correct. It needs one addition: a schema version field.**

The current `SaveManager` uses `JsonUtility` with a key/file-per-system pattern. This is correct. The gap is that `JsonUtility.FromJson<T>` silently returns default values for missing fields — which means a save file written with an old schema loads into a new schema without error but with wrong data (zero resources, missing quest states). This manifests as "my save got wiped" bugs.

### Version envelope pattern

Every save data class should include a `schemaVersion` field:

```csharp
[Serializable]
public class QuestSaveData
{
    public int schemaVersion = 1;
    public List<QuestInstanceData> quests = new();
    // ... other fields
}
```

`SaveManager` should check version on load and run migration if needed:

```csharp
public T Load<T>(string key) where T : IVersioned
{
    // ... existing load logic ...
    if (data.schemaVersion < CURRENT_VERSION)
        data = MigrationRunner.Migrate(data, CURRENT_VERSION);
    return data;
}
```

In practice for the current milestone, the migration runner can be a stub (`MigrateV1ToV2` methods added only when the schema actually changes). The key discipline is: **never change existing serialized field names or types** — add new optional fields with defaults instead.

### JsonUtility limitations to know

- `JsonUtility` does NOT serialize `Dictionary<K,V>`, `HashSet<T>`, interfaces, or abstract classes.
- Use `List<ResourceEntry>` (a `[Serializable]` struct with key+value) instead of `Dictionary<string, float>` for resource amounts in save data.
- `JsonUtility` only serializes `public` fields or `[SerializeField]` private fields.

**Do not switch to Newtonsoft Json.NET yet.** It handles dictionaries and polymorphism but adds a package dependency and per-object allocation overhead that is not needed for this game's data complexity. Revisit if save data needs dictionary-keyed structures that become awkward as lists.

### Per-system save files (keep this pattern)

The key-per-file pattern in `SaveManager` is correct. It means:
- `quests.json`, `resources.json`, `settings.json`, `characters.json` are independent.
- A corrupted quests file does not wipe resource data.
- Each system manages its own `SaveData` class — `QuestManager` defines `QuestSaveData`, writes `"quests"`, reads `"quests"`. No central god-object save class.

---

## Pattern 5: Scene Management Architecture

**Use async additive loading with a persistent Boot scene. BootManager needs to become async.**

### Scene topology

```
Boot.unity          — persistent, loaded first, never unloaded
                      Contains: GameManager, SaveManager, AudioManager,
                                SceneController, SettingsManager

MainMenu.unity      — loaded additively on top of Boot
Game.unity          — loaded additively on top of Boot
                      (replaces MainMenu — unload Menu when Game loads)
```

The Boot scene being persistent and never unloaded is already the project's intent (via `DontDestroyOnLoad` on all singletons). The upgrade needed is **additive loading** so the Boot scene's managers are always alive even while scenes change.

### Current gap in BootManager

The current `BootManager` calls `SceneManager.LoadScene(_firstSceneName)` synchronously in `Start()`. This replaces the Boot scene itself, which means all the `DontDestroyOnLoad` objects survive (correctly) but if you ever try to load Boot additively, the scene transitions will behave unexpectedly.

The pattern to follow:

```csharp
// SceneController.cs (new singleton)
public class SceneController : MonoSingleton<SceneController>
{
    public async Awaitable LoadSceneAsync(string sceneName, string unloadScene = null)
    {
        // 1. Fire OnSceneTransitionStart event (triggers fade-out UI)
        // 2. LoadSceneMode.Additive the new scene
        // 3. Unload the old scene if provided
        // 4. Fire OnSceneTransitionComplete event (triggers fade-in UI)
    }
}
```

**Use `Awaitable` (Unity 2023.1+) over coroutines.** `Awaitable` is Unity's native async type introduced in 2023.1 — it avoids the C# `Task` allocation overhead and integrates with Unity's frame loop correctly. `AsyncOperation` awaiting via `SceneManager.LoadSceneAsync` returns an `Awaitable` in current Unity versions.

### Boot flow

```
1. Boot.unity loads
2. All MonoSingletons initialize via Awake (alphabetical order matters here — see pitfall)
3. BootManager.Start() → await SceneController.LoadSceneAsync("MainMenu")
4. Player presses Play → await SceneController.LoadSceneAsync("Game", unload: "MainMenu")
5. Player returns to menu → await SceneController.LoadSceneAsync("MainMenu", unload: "Game")
```

---

## Pattern 6: Designing for Change

**The design volatility constraint calls for one specific discipline: separate "what" from "how many" and "how it looks."**

Three rules that prevent the most common rework causes:

### Rule 1: Logic reads from SOs, never from hardcoded values

Bad: `if (questType == "exercise") reward = 10;`
Good: `reward = quest.Definition.ResourceReward;` (where `ResourceReward` is on `QuestDefinitionSO`)

When reward balancing changes, you edit an asset, not code.

### Rule 2: Systems communicate through events, not direct method calls between peers

Bad: `QuestManager.Instance.OnComplete()` calls `ResourceManager.Instance.AddResource()` directly.
Good: `QuestManager` fires `OnQuestCompleted(quest)`. `ResourceManager` subscribes and handles `AddResource` itself.

When the resource grant logic changes (bonus multipliers, caps, weekly limits), you change `ResourceManager` only — `QuestManager` has no knowledge of it.

### Rule 3: UI reads from events and runtime state, never writes to it

Bad: A UI button directly modifies `ResourceManager`'s internal values.
Good: UI button calls `QuestManager.CompleteQuest(id)` — a single intentional API call. The manager fires events. UI reacts.

This means UI panels can be completely replaced (different layout, different widget library) without touching game logic.

### Practical application for current milestone

For the System Foundations milestone (save/load, main menu, settings):

| System | Definition SO | Runtime State | Events |
|--------|--------------|---------------|--------|
| Settings | `GameSettingsSO` (defaults) | `SettingsSaveData` | `OnSettingsChanged` |
| Scene flow | — | `SceneController` state | `OnSceneTransitionStart/Complete` |
| Save | — | Per-system `*SaveData` classes | `OnSaveComplete`, `OnLoadComplete` |
| Audio | `AudioConfigSO` (volume curves, clip refs) | Runtime bus values | `OnVolumeChanged` |

---

## Component Boundaries

| Component | Responsibility | Owns | Does NOT touch |
|-----------|---------------|------|----------------|
| `GameManager` | Global game state machine | `GameState` enum, `OnStateChanged` event | All gameplay systems |
| `SaveManager` | File I/O only | Read/write JSON to disk | Does not know what data means |
| `SceneController` | Scene transition sequencing | Load/unload async, transition events | Does not know what scenes contain |
| `SettingsManager` | Player preferences | Audio levels, display, wallpaper bool | Actual Win32 API calls (that's WallpaperManager) |
| `QuestManager` | Quest lifecycle | Quest state for all quest instances | Combat logic, resource implementation |
| `ResourceManager` | Resource float values | Current amounts, caps, deltas | How resources are spent (that's CombatManager) |
| `CombatManager` | Automated combat loop | Combat ticks, outcome calculations | Resource storage (reads only, fires events) |
| `CharacterManager` | Crew character states | Relationship scores, dialogue flags | Naninovel script content (Naninovel owns that) |
| `WallpaperManager` | Win32 wallpaper interop | `SetWallpaper`, `ClearWallpaper` | Game state logic |

---

## Anti-Patterns to Avoid

### Anti-Pattern 1: Manager Spaghetti

**What:** `QuestManager.Instance` directly calls `ResourceManager.Instance.Add()` and `CombatManager.Instance.RecalculatePower()` in the same method.

**Why bad:** Creates hidden dependency chains. Changing ResourceManager's API breaks QuestManager. Adding a new consumer of quest completion (e.g., CharacterManager needs to react too) requires editing QuestManager.

**Instead:** QuestManager fires `OnQuestCompleted`. ResourceManager, CombatManager, CharacterManager each subscribe independently.

### Anti-Pattern 2: Save Data as ScriptableObjects

**What:** Storing runtime save data in ScriptableObjects and calling `JsonUtility.ToJson(scriptableObject)` to save them.

**Why bad:** SO values modified at runtime in a build reset on next session. In the Editor they persist, so this "works" during development and silently breaks in builds. This is a recurring Unity beginner trap.

**Instead:** Maintain the Definition SO / SaveData class split described in Pattern 2.

### Anti-Pattern 3: Awake Order Dependency Between Singletons

**What:** `QuestManager.Awake()` calls `SaveManager.Instance.Load()` before `SaveManager.Awake()` has run.

**Why bad:** Unity does not guarantee Awake execution order across GameObjects within the same scene except via Script Execution Order settings. If the Boot scene has multiple manager GameObjects, their Awake order is not deterministic.

**Instead:** All managers initialize themselves in `Awake`/`OnInitialize`. Cross-manager initialization (QuestManager loading its save data) happens in `Start()` or in a sequential init chain triggered by `BootManager.Start()` after all Awakes are guaranteed complete.

### Anti-Pattern 4: Blocking Main Thread Saves

**What:** `SaveManager.Save()` called on every game state change without debouncing.

**Why bad:** `File.WriteAllText` is a blocking call. For small save files (a few KB) it's imperceptible. For save data that grows (daily quest history, relationship flags accumulating over months of play), it will stutter.

**Instead:** For the current milestone, the synchronous save is acceptable. When save data grows, wrap in `async Task SaveAsync()` and debounce to prevent multiple rapid saves queuing (a dirty-flag pattern: mark data dirty, save on scene transition or every N seconds).

### Anti-Pattern 5: Naninovel Coupling

**What:** Game logic (relationship score thresholds, quest completion checks) mixed inside Naninovel scripts (.nani files).

**Why bad:** Naninovel scripts are for narrative content. Business logic inside them is untestable, hard to find, and invisible to the C# event/SO architecture.

**Instead:** Naninovel exposes a C# API for custom commands and functions. Write a `@updateRelationship` custom command that calls `CharacterManager.Instance.AddRelationship(characterId, delta)`. The .nani scripts declare intent; C# executes the logic.

---

## Scalability Considerations

| Concern | Now (foundation) | Later (full game) | If content grows large |
|---------|-----------------|-------------------|----------------------|
| Save size | Single JSON per system, few KB | Still fine; daily quest log grows over months | Trim old history (keep 90 days), or migrate to binary |
| Event subscriptions | 8-10 subscribers per event | Fine indefinitely | Not a concern for this game's scale |
| ScriptableObjects | ~20 assets total | ~50-100 assets (many quest/weapon types) | Use `Addressables` only if SO load time becomes measurable |
| Scene memory | 2 scenes max loaded at once | Same | Not a concern for a UI/idle game |
| Win32 wallpaper mode | WallpaperManager in Game scene | Stays in Game scene, toggled via SettingsManager | No scalability concern |

---

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| MonoSingleton pattern | HIGH | Code is on disk; pattern is well-established |
| ScriptableObject data split | HIGH | Unity-native pattern, stable since Unity 2017 |
| C# events over UnityEvents | HIGH | Consistent community consensus, well-documented tradeoffs |
| JsonUtility limitations | HIGH | Official Unity docs, no changes expected |
| Awaitable vs coroutines | MEDIUM | `Awaitable` is Unity 2023.1+ — verify Unity version before use; fallback to coroutines or `UniTask` if on earlier version |
| Save versioning pattern | HIGH | Standard practice across Unity game projects |
| Naninovel custom commands | MEDIUM | Based on Naninovel docs knowledge as of training cutoff; verify current API |

---

## Open Questions for Phase-Specific Research

1. **Unity version:** Confirm the project's Unity version before using `Awaitable`. If pre-2023.1, use `UniTask` (third-party, highly recommended) or Unity coroutines for async scene loading.
2. **Naninovel integration depth:** Naninovel manages its own scene/camera — confirm how it coexists with the Boot singleton pattern. Naninovel has its own initialization engine that may conflict with `DontDestroyOnLoad` managers if not configured correctly.
3. **Win32 wallpaper mode architecture:** The `WallpaperManager` needs to render the Game scene to a texture and present it as a wallpaper. This likely requires a dedicated camera render texture pipeline. This is a non-trivial system deserving its own research phase.

---

## Sources

- Training knowledge of Unity architecture patterns (cutoff August 2025) — HIGH confidence for stable patterns
- Project source code on disk: `MonoSingleton.cs`, `SaveManager.cs`, `BootManager.cs`, `GameManager.cs`
- Unity documentation structural knowledge: ScriptableObjects, JsonUtility constraints, SceneManager async loading, DontDestroyOnLoad behavior
- Ryan Hipple's "Game Architecture with Scriptable Objects" (Unite Austin 2017) — foundational SO event pattern reference
- Naninovel documentation (naninovel.com) — custom commands API
