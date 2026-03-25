# Phase 1: System Foundations - Research

**Researched:** 2026-03-25
**Domain:** Unity 6 game infrastructure -- save system, scene management, Firebase auth/sync, settings, audio
**Confidence:** MEDIUM-HIGH (most patterns are well-established Unity patterns; Firebase desktop support has known limitations)

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Use Firebase Realtime Database -- single JSON blob per user document, simpler and cheaper than Firestore for this use case
- **D-02:** Anonymous auth on first launch (silent, no UI); optional Google Sign-In upgrade via "Link Account" button in Settings; linking preserves existing save data; anonymous data lost on reinstall is acceptable documented behavior
- **D-03:** One unified save file -- single root struct (`GameSaveData`) containing all game state; not per-system files
- **D-04:** Auto-save triggers on: any habit completion, any resource update, weekly report submit, and app focus loss (`OnApplicationFocus(false)`)
- **D-05:** Save includes a `saveVersion` integer for future migration; each save data class has a stub `MigrateFrom()` method
- **D-06:** Silent black screen during Boot initialization in v1 -- no splash screen or loading bar; keep it simple
- **D-07:** Modal dialog with message "All progress will be deleted. This cannot be undone." with Confirm and Cancel buttons; Confirm triggers `SaveManager.DeleteAll()` and restarts to Boot scene

### Claude's Discretion
- Exact `GameSaveData` field naming conventions
- Which Unity `Awaitable` API variant to use for async scene loading (Unity 6 LTS)
- Audio Mixer asset naming and bus hierarchy
- `SettingsKeys` constant names

### Deferred Ideas (OUT OF SCOPE)
- Loading bar or splash screen -- explicitly out of scope for v1 (D-06); add when needed
- Desktop wallpaper mode toggle in settings -- removed from Phase 1 scope (see REQUIREMENTS.md Pending Redesign)
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| FOUND-01 | Single save slot, auto-save continuously | Unified GameSaveData struct with atomic write pattern; auto-save trigger list defined in D-04 |
| FOUND-02 | Save file includes saveVersion integer | Version envelope pattern on GameSaveData; stub MigrateFrom() methods |
| FOUND-03 | Atomic writes (write-temp-then-move) | File.Replace or write-to-tmp+File.Move pattern on NTFS; see Architecture Patterns section |
| FOUND-04 | Save state syncs to Firebase Realtime Database | SetRawJsonValueAsync with single JSON blob per user document; see Firebase section |
| FOUND-05 | Boot scene auto-loads existing save on start | BootManager.Start() calls SaveManager.Load; no user choice |
| FOUND-06 | Boot scene initializes all core singletons before transitioning | Existing MonoSingleton pattern + Awake/OnInitialize; BootManager.Start() runs after all Awakes |
| FOUND-07 | Scene flow: Boot -> Main Menu -> Game with async additive loading | SceneController using Awaitable + SceneManager.LoadSceneAsync additive; see Awaitable section |
| FOUND-08 | Main menu: New Game (data wipe confirm), Settings, Quit | New Game triggers D-07 modal; Settings opens SettingsManager UI; Quit calls Application.Quit() |
| FOUND-09 | Settings persist across sessions | SettingsManager wrapping PlayerPrefs with typed SettingsKeys constants |
| FOUND-10 | Background FPS cap (default 10fps unfocused) | Application.targetFrameRate in OnApplicationFocus callback; must disable vSync first |
| FOUND-11 | Firebase Anonymous auth silent on Boot | FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync(); see Firebase Auth section |
| FOUND-12 | Link Account button to upgrade anonymous to Google Sign-In | LinkWithCredentialAsync with GoogleAuthProvider.GetCredential; CRITICAL desktop limitation |
| FOUND-13 | Account linking preserves all save data | Firebase preserves UID on LinkWithCredentialAsync -- same database path, no data migration needed |
| FOUND-14 | Google-linked accounts restore save data on reinstall | Sign in with Google credential, same UID, read from same Realtime Database path |
| FOUND-15 | GameManager exposes GameState enum with C# events | GameState enum + event Action<GameState> OnStateChanged; see Architecture Patterns section |
</phase_requirements>

## Summary

Phase 1 builds the foundational infrastructure that every subsequent system depends on: save persistence, scene flow, settings, Firebase auth, and the GameManager state machine. The existing codebase provides a solid MonoSingleton base class, a functional but bare SaveManager, a synchronous BootManager, and an empty GameManager stub. All four need targeted upgrades rather than rewrites.

The most technically significant finding is the **Firebase desktop limitation**: while Firebase Auth and Realtime Database have functional (not stub) implementations for Windows standalone, Google explicitly labels desktop support as "beta, intended for development workflows only, not for publicly shipping code." This does not mean it will not work -- Auth, Realtime Database, and credential persistence all function on Windows standalone -- but it means Google does not guarantee stability or support for production desktop builds. For a solo Windows game, this is an acceptable risk, but the architecture should include a graceful offline fallback. The second critical finding is that Google Sign-In for Unity has been deprecated as of February 2025. On Windows desktop, there is no first-party Google Sign-In SDK; the recommended approach is a custom OAuth flow (local HTTP listener + browser redirect) that produces a Google ID token, which is then exchanged for a Firebase credential via `GoogleAuthProvider.GetCredential()`.

Unity 6 (6000.3.x) fully supports the `Awaitable` async pattern, and `SceneManager.LoadSceneAsync` returns an awaitable `AsyncOperation`. This is the correct pattern for the SceneController.

**Primary recommendation:** Build all four plans (SaveManager hardening, scene flow, settings, Firebase) using the existing MonoSingleton infrastructure. Use Unity 6 Awaitable for async scene loading. Implement Firebase with a defensive offline-first approach where local save is always the source of truth and Firebase sync is best-effort.

## Standard Stack

### Core (Already in Project)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Unity 6 LTS | 6000.3.11f1 | Engine | Confirmed in ProjectVersion.txt; do not upgrade mid-project |
| URP | 17.3.0 | Rendering | Already in manifest.json |
| uGUI | 2.0.0 | UI framework | Already in manifest.json; includes TMP |
| Input System | 1.19.0 | Input | Already in manifest.json |

### To Add for Phase 1

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Firebase Auth (Unity) | 13.9.0 | Anonymous auth + Google Sign-In linking | Plan 01-04 |
| Firebase Realtime Database (Unity) | 13.9.0 | Cloud save sync | Plan 01-04 |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Firebase Realtime DB | Firestore | Firestore has richer querying; overkill for single-JSON-blob-per-user. Decision locked: use Realtime DB (D-01) |
| JsonUtility | Newtonsoft Json.NET | Json.NET handles Dictionary, polymorphism; adds dependency. Not needed yet -- GameSaveData is flat struct with List-based collections |
| Unity Awaitable | UniTask | UniTask has richer API; Awaitable is built-in to Unity 6 and sufficient for scene loading |
| PlayerPrefs for settings | JSON file | PlayerPrefs survives "Delete Save Data", is simpler for small key-value pairs |

### Installation

Firebase SDK packages must be imported from the Firebase Unity SDK archive (not UPM):
1. Download Firebase Unity SDK 13.9.0 from https://firebase.google.com/download/unity
2. Import `FirebaseAuth.unitypackage` and `FirebaseDatabase.unitypackage`
3. Add `google-services.json` (Android) and `GoogleService-Info.plist` (iOS) -- for Windows standalone, only the Firebase project config in code is needed
4. Call `FirebaseApp.CheckAndFixDependenciesAsync()` during boot initialization

## Architecture Patterns

### Recommended Project Structure (Phase 1 additions)

```
Assets/_Project/
  Scenes/
    Boot.unity               # Existing -- persistent, never unloaded
    MainMenu.unity            # NEW -- loaded additively
    Game.unity                # NEW -- loaded additively (replaces MainMenu)
  Scripts/
    Core/
      MonoSingleton.cs        # Existing -- do not modify
      GameManager.cs          # Existing -- add GameState enum + OnStateChanged event
      SaveManager.cs          # Existing -- add atomic write + versioning + unified GameSaveData
      BootManager.cs          # Existing -- upgrade to async, add save auto-load
      SceneController.cs      # NEW -- async additive scene loading singleton
      SettingsManager.cs      # NEW -- PlayerPrefs wrapper singleton
      SettingsKeys.cs         # NEW -- static string constants for PlayerPrefs keys
    Data/
      GameSaveData.cs         # NEW -- unified serializable save struct
    Firebase/
      FirebaseAuthManager.cs  # NEW -- anonymous auth + Google Sign-In link
      FirebaseSyncManager.cs  # NEW -- Realtime Database read/write
  Audio/
    MainMixer.mixer           # NEW -- Master/Music/SFX groups
  UI/
    Prefabs/
      SettingsPanel.prefab    # NEW -- Settings UI
      ConfirmDialog.prefab    # NEW -- Reusable modal dialog
      MainMenuPanel.prefab    # NEW -- Main Menu buttons
```

### Pattern 1: Unified GameSaveData with Version Envelope

**What:** Single root struct containing all game state, with a version integer for migration.
**When to use:** Always -- this is the decided save architecture (D-03).
**Note:** This reverses the prior architecture research recommendation of per-system files. The user decision (D-03) takes precedence.

```csharp
// Source: Project decision D-03 + D-05
[System.Serializable]
public class GameSaveData
{
    public int saveVersion = 1;
    public long lastSaveTimestamp;

    // Future phases will add fields here:
    // public List<QuestInstanceData> quests = new();
    // public List<ResourceEntry> resources = new();

    public static GameSaveData CreateNew()
    {
        return new GameSaveData
        {
            saveVersion = 1,
            lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public void MigrateFrom(int fromVersion)
    {
        // Stub -- add migration logic when saveVersion increments
        // Example: if (fromVersion < 2) { /* migrate v1 -> v2 */ }
    }
}
```

### Pattern 2: Atomic Write (write-temp-then-replace)

**What:** Write to a `.tmp` file first, then atomically replace the real file.
**When to use:** Every save operation.

```csharp
// Source: .NET File.Replace documentation + NTFS atomicity research
public void SaveAtomically(string key, string json)
{
    string finalPath = GetPath(key);           // e.g., Saves/game.json
    string tempPath = finalPath + ".tmp";
    string backupPath = finalPath + ".bak";

    File.WriteAllText(tempPath, json);

    if (File.Exists(finalPath))
    {
        // File.Replace: atomically replaces destination with source,
        // creating a backup of the original
        File.Replace(tempPath, finalPath, backupPath);
    }
    else
    {
        File.Move(tempPath, finalPath);
    }
}
```

**Key details:**
- `File.Replace(source, destination, backup)` is the safest approach on Windows/NTFS. It atomically swaps the file content and creates a backup.
- `File.Move` with overwrite (`File.Move(src, dst, true)`) is available in .NET Core 3.0+ but Unity's Mono runtime support should be verified. `File.Replace` is the safer bet.
- Both source and destination MUST be on the same volume (they will be -- both under `Application.persistentDataPath`).
- The `.bak` file serves as crash recovery: if the game crashes during a write, the `.bak` file contains the last good save.

### Pattern 3: Unity 6 Awaitable for Async Scene Loading

**What:** Use `async Awaitable` methods with `SceneManager.LoadSceneAsync` for scene transitions.
**When to use:** All scene transitions via SceneController.

```csharp
// Source: Unity 6000.3 Awaitable documentation
// https://docs.unity3d.com/6000.3/Documentation/Manual/async-await-support.html
public class SceneController : MonoSingleton<SceneController>
{
    private string _currentScene;

    public event Action OnSceneTransitionStart;
    public event Action OnSceneTransitionComplete;

    public async Awaitable LoadSceneAsync(string sceneName, string unloadScene = null)
    {
        OnSceneTransitionStart?.Invoke();

        // Load new scene additively
        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Unload old scene if specified
        if (!string.IsNullOrEmpty(unloadScene))
        {
            await SceneManager.UnloadSceneAsync(unloadScene);
        }

        _currentScene = sceneName;
        OnSceneTransitionComplete?.Invoke();
    }
}
```

**Awaitable gotchas (verified from Unity 6 docs):**
1. Never await an Awaitable instance more than once -- they are pooled and reused.
2. `SceneManager.LoadSceneAsync` returns `AsyncOperation`, which is directly awaitable in Unity 6.
3. Awaitable methods run on the Unity main thread by default (player loop integration) -- no thread marshaling needed.
4. Use `destroyCancellationToken` (available on MonoBehaviour in Unity 6) for automatic cancellation when the object is destroyed.

### Pattern 4: GameState Enum + C# Event

**What:** Central state machine on GameManager with typed event.

```csharp
// Source: Architecture research + established Unity pattern
public class GameManager : MonoSingleton<GameManager>
{
    public enum GameState { Boot, MainMenu, Playing, Paused }

    public GameState CurrentState { get; private set; } = GameState.Boot;

    public event Action<GameState, GameState> OnStateChanged; // oldState, newState

    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;
        var oldState = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(oldState, newState);
    }
}
```

**Best practices:**
- Pass both old and new state to the event so subscribers can handle transitions (e.g., "entering Playing from Paused" vs "entering Playing from MainMenu").
- Subscribers attach in `OnEnable`, detach in `OnDisable` to avoid leaks from destroyed objects.
- GameManager does NOT own any gameplay logic -- it is a state hub only.

### Pattern 5: Audio Mixer Runtime Volume Control

**What:** Expose Audio Mixer parameters and control them from SettingsManager.

```csharp
// Source: Unity AudioMixer.SetFloat documentation
// Volume must be set in Start() or later, NOT in Awake/OnInitialize
// Audio Mixer volume range: -80dB (silent) to 0dB (full) -- use logarithmic conversion

public static float LinearToDecibel(float linear)
{
    // Clamp to avoid log(0) = -infinity
    return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
}

// In SettingsManager:
[SerializeField] private AudioMixer _masterMixer;

public void SetMasterVolume(float linearVolume)
{
    _masterMixer.SetFloat("MasterVolume", LinearToDecibel(linearVolume));
    PlayerPrefs.SetFloat(SettingsKeys.MasterVolume, linearVolume);
}
```

**Setup steps for Audio Mixer:**
1. Create Audio Mixer asset: `Assets/_Project/Audio/MainMixer.mixer`
2. Add three groups under Master: `Music`, `SFX` (add `Voice` later for VN phase)
3. Right-click each group's Volume parameter -> "Expose to script" -> name them `MasterVolume`, `MusicVolume`, `SfxVolume`
4. CRITICAL: `SetFloat` must be called in `Start()` or later, NOT during `Awake`/`OnInitialize`. If called too early, it silently fails because the mixer is not yet active.

### Anti-Patterns to Avoid

- **Calling AudioMixer.SetFloat in Awake:** Silently fails. The mixer is not active yet. Apply settings in Start() or via a one-frame delay.
- **Using `File.WriteAllText` directly for saves:** Not atomic. A crash mid-write corrupts the file. Always use the temp-then-replace pattern.
- **Storing linear volume (0-1) directly in the mixer:** The mixer uses decibels (-80 to 0). You must convert with `Mathf.Log10(linear) * 20f`.
- **Forgetting `QualitySettings.vSyncCount = 0` before setting targetFrameRate:** If vSync is enabled, `Application.targetFrameRate` is ignored entirely.
- **Awaiting the same Awaitable twice:** Causes undefined behavior (exception or deadlock). Unity pools Awaitable instances.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Atomic file writes | Custom file-locking system | `File.Replace()` with .tmp + .bak pattern | OS-level atomic swap on NTFS; handles crash during write |
| Decibel volume conversion | Manual dB formula with edge cases | `Mathf.Log10(linear) * 20f` with clamp at 0.0001f | Standard audio engineering formula; clamp prevents -Infinity |
| Async scene loading | Coroutine + callbacks | `async Awaitable` + `await SceneManager.LoadSceneAsync` | Unity 6 native; cleaner than coroutines; no allocation overhead of Task |
| Firebase dependency check | Manual DLL verification | `FirebaseApp.CheckAndFixDependenciesAsync()` | Firebase SDK's own resolver handles platform-specific dependency issues |
| Google OAuth on desktop | Full OAuth2 library | Local HTTP listener + System.Diagnostics.Process.Start(authUrl) + exchange code for token | Minimal; avoids heavy OAuth library dependency |

## Common Pitfalls

### Pitfall 1: Firebase Desktop Support is Beta

**What goes wrong:** The Firebase Unity SDK desktop support is documented as "beta, intended for development workflows, not for publicly shipping code." Auth and Realtime Database have functional (not stub) implementations on Windows standalone, but Google provides no production SLA.
**Why it happens:** Firebase's primary targets are Android and iOS. Desktop is a convenience feature.
**How to avoid:** Design the architecture so local save is always the authoritative source. Firebase sync is best-effort. If Firebase is unreachable, the game works fully offline. Add retry logic with exponential backoff for sync failures. Log Firebase errors but never block gameplay on them.
**Warning signs:** `FirebaseApp.CheckAndFixDependenciesAsync` returns `DependencyStatus.UnavailableOther` on some Windows configurations. Handle this gracefully.

### Pitfall 2: Google Sign-In SDK Deprecated on Desktop

**What goes wrong:** The `google-signin-unity` plugin (by Google) was deprecated in February 2025. It only supported Android and iOS anyway. There is no first-party Google Sign-In SDK for Windows desktop Unity builds.
**Why it happens:** Google removed the legacy Sign-In API in favor of platform-specific flows (Google Play Games for Android, Sign In With Apple for iOS). Desktop was never a supported target.
**How to avoid:** Implement Google Sign-In on Windows using a custom OAuth flow:
  1. Open the user's default browser to Google's OAuth consent URL
  2. Listen on a local HTTP port (e.g., `http://localhost:PORT/callback`) for the redirect
  3. Extract the authorization code from the callback
  4. Exchange the code for an ID token via Google's token endpoint
  5. Call `GoogleAuthProvider.GetCredential(idToken, null)` to create a Firebase credential
  6. Call `auth.CurrentUser.LinkWithCredentialAsync(credential)` to link the account
**Warning signs:** If you try to use the deprecated `google-signin-unity` plugin, it will not compile for Windows standalone.

### Pitfall 3: AudioMixer.SetFloat Silently Fails When Called Too Early

**What goes wrong:** Calling `AudioMixer.SetFloat()` during `Awake()` or `OnInitialize()` does nothing. No error is thrown. The volume stays at whatever the mixer's default is.
**Why it happens:** The Audio Mixer is not fully initialized until after the first frame. Unity's audio system completes initialization between Awake and Start.
**How to avoid:** Apply saved volume settings in `Start()` or later. If using SettingsManager as a MonoSingleton, wire the audio application in `Start()`, not `OnInitialize()`.
**Warning signs:** Volume settings appear to not persist across sessions even though PlayerPrefs values are correct.

### Pitfall 4: vSync Overrides targetFrameRate

**What goes wrong:** Setting `Application.targetFrameRate = 10` has no effect because `QualitySettings.vSyncCount` is nonzero (default is 1 for most quality levels).
**Why it happens:** When vSync is enabled, Unity locks to the monitor's refresh rate (or a divisor). `targetFrameRate` is completely ignored.
**How to avoid:** Before setting `targetFrameRate`, set `QualitySettings.vSyncCount = 0`. Store the previous vSync setting and restore it when the app regains focus if you want vSync during active play.
**Warning signs:** Background FPS cap appears to have no effect; game still runs at 60fps when unfocused.

### Pitfall 5: Unified Save File Makes Partial Corruption Total

**What goes wrong:** With a single `GameSaveData` JSON file (per D-03), a corruption event affects ALL game state -- habits, resources, combat data, settings -- instead of just one system.
**Why it happens:** All data is in one file. One bad write corrupts everything.
**How to avoid:** The atomic write pattern (Pitfall avoidance for FOUND-03) is the primary defense. Additionally:
  - Keep the `.bak` file from `File.Replace` as a recovery option
  - On load failure, attempt to load from `.bak` before creating a new save
  - Log corruption events for debugging
  - Firebase sync acts as a secondary backup (but is not guaranteed to be current if the last sync failed)

### Pitfall 6: JsonUtility Cannot Serialize Dictionary or Null

**What goes wrong:** `JsonUtility.ToJson` silently drops `Dictionary<K,V>` fields and does not serialize null values (it writes default values instead).
**Why it happens:** JsonUtility only supports Unity's serialization rules: public fields or `[SerializeField]` fields of supported types. Dictionary is not a supported type.
**How to avoid:** Use `List<T>` with a custom struct (e.g., `List<ResourceEntry>` with `string id` + `float amount`) instead of Dictionary. For optional/nullable data, use sentinel values (e.g., `-1` for "not set") rather than null.
**Warning signs:** Save data loads successfully but collections are empty or values are zero.

## Code Examples

### Background FPS Cap via OnApplicationFocus

```csharp
// Source: Unity Application.targetFrameRate docs + OnApplicationFocus pattern
// Place on a persistent MonoBehaviour (e.g., GameManager or SettingsManager)

private int _activeFps = -1; // -1 = unlimited (or vsync)
private int _backgroundFps = 10;

private void OnApplicationFocus(bool hasFocus)
{
    if (hasFocus)
    {
        // Restore active settings
        QualitySettings.vSyncCount = 1; // or user's preference
        Application.targetFrameRate = _activeFps;
    }
    else
    {
        // Throttle in background
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _backgroundFps;

        // Also trigger auto-save (D-04)
        SaveManager.Instance?.Save();
    }
}
```

### Firebase Anonymous Auth + Account Linking

```csharp
// Source: Firebase Auth Unity docs
// https://firebase.google.com/docs/auth/unity/anonymous-auth

using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class FirebaseAuthManager : MonoSingleton<FirebaseAuthManager>
{
    private FirebaseAuth _auth;
    private FirebaseUser _user;
    public string UserId => _user?.UserId;
    public bool IsAuthenticated => _user != null;
    public bool IsAnonymous => _user?.IsAnonymous ?? true;

    public async Awaitable InitializeAsync()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
        {
            Debug.LogError($"[FirebaseAuth] Dependencies not available: {status}");
            return;
        }

        _auth = FirebaseAuth.DefaultInstance;
        _auth.StateChanged += OnAuthStateChanged;

        // Auto sign-in: use existing session or create anonymous
        if (_auth.CurrentUser != null)
        {
            _user = _auth.CurrentUser;
            Debug.Log($"[FirebaseAuth] Existing session: {_user.UserId}");
        }
        else
        {
            var result = await _auth.SignInAnonymouslyAsync();
            _user = result.User;
            Debug.Log($"[FirebaseAuth] Anonymous sign-in: {_user.UserId}");
        }
    }

    // Link anonymous account to Google credential
    public async Awaitable<bool> LinkWithGoogleAsync(string idToken)
    {
        var credential = GoogleAuthProvider.GetCredential(idToken, null);
        try
        {
            var result = await _user.LinkWithCredentialAsync(credential);
            _user = result.User;
            Debug.Log($"[FirebaseAuth] Linked to Google: {_user.UserId}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FirebaseAuth] Link failed: {e.Message}");
            return false;
        }
    }

    private void OnAuthStateChanged(object sender, System.EventArgs e)
    {
        _user = _auth.CurrentUser;
    }
}
```

### Firebase Realtime Database Sync

```csharp
// Source: Firebase Realtime Database Unity docs
// Single JSON blob per user at path: users/{userId}/save

public class FirebaseSyncManager : MonoSingleton<FirebaseSyncManager>
{
    private DatabaseReference _dbRef;

    public void Initialize(string userId)
    {
        _dbRef = FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .Child(userId)
            .Child("save");
    }

    public async Awaitable PushSaveAsync(GameSaveData data)
    {
        if (_dbRef == null) return;
        try
        {
            string json = JsonUtility.ToJson(data);
            await _dbRef.SetRawJsonValueAsync(json);
            Debug.Log("[FirebaseSync] Save pushed to cloud.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[FirebaseSync] Push failed (will retry): {e.Message}");
        }
    }

    public async Awaitable<GameSaveData> PullSaveAsync()
    {
        if (_dbRef == null) return null;
        try
        {
            var snapshot = await _dbRef.GetValueAsync();
            if (!snapshot.Exists) return null;
            string json = snapshot.GetRawJsonValue();
            return JsonUtility.FromJson<GameSaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[FirebaseSync] Pull failed: {e.Message}");
            return null;
        }
    }
}
```

### SettingsManager with PlayerPrefs

```csharp
// Source: Unity PlayerPrefs docs + STACK.md recommendations

public static class SettingsKeys
{
    public const string MasterVolume = "settings.audio.masterVolume";
    public const string MusicVolume  = "settings.audio.musicVolume";
    public const string SfxVolume    = "settings.audio.sfxVolume";
    public const string Fullscreen   = "settings.display.fullscreen";
    public const string Resolution   = "settings.display.resolutionIndex";
    public const string BackgroundFps = "settings.performance.backgroundFps";
}

public class SettingsManager : MonoSingleton<SettingsManager>
{
    [SerializeField] private AudioMixer _masterMixer;

    // Defaults
    private const float DefaultVolume = 0.75f;
    private const int DefaultBackgroundFps = 10;

    protected override void OnInitialize()
    {
        // Load values from PlayerPrefs (defaults used if key missing)
        // NOTE: Do NOT apply audio here -- mixer is not ready in Awake
    }

    private void Start()
    {
        // Audio mixer is ready in Start -- apply saved settings now
        ApplyAudioSettings();
        ApplyDisplaySettings();
    }

    public float GetFloat(string key, float defaultValue)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    private void ApplyAudioSettings()
    {
        float master = PlayerPrefs.GetFloat(SettingsKeys.MasterVolume, DefaultVolume);
        float music = PlayerPrefs.GetFloat(SettingsKeys.MusicVolume, DefaultVolume);
        float sfx = PlayerPrefs.GetFloat(SettingsKeys.SfxVolume, DefaultVolume);

        _masterMixer.SetFloat("MasterVolume", LinearToDecibel(master));
        _masterMixer.SetFloat("MusicVolume", LinearToDecibel(music));
        _masterMixer.SetFloat("SfxVolume", LinearToDecibel(sfx));
    }

    private static float LinearToDecibel(float linear)
    {
        return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
    }
}
```

### Google OAuth on Windows Desktop (Custom Flow)

```csharp
// Source: Google OAuth 2.0 for Desktop Apps documentation
// This is the recommended approach since google-signin-unity is deprecated

using System;
using System.Net;
using System.Threading.Tasks;

public static class GoogleOAuthDesktop
{
    private const string ClientId = "YOUR_CLIENT_ID.apps.googleusercontent.com";
    private const string RedirectUri = "http://localhost:8856/callback";
    private const string AuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

    /// <summary>
    /// Opens browser for Google OAuth, listens for callback, returns ID token.
    /// </summary>
    public static async Task<string> GetIdTokenAsync()
    {
        string codeVerifier = GeneratePKCEVerifier();
        string codeChallenge = GeneratePKCEChallenge(codeVerifier);

        // 1. Open browser to Google consent screen
        string authUrl = $"{AuthEndpoint}?client_id={ClientId}" +
            $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
            $"&response_type=code" +
            $"&scope=openid%20email%20profile" +
            $"&code_challenge={codeChallenge}" +
            $"&code_challenge_method=S256";

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = authUrl,
            UseShellExecute = true
        });

        // 2. Listen for redirect callback
        using var listener = new HttpListener();
        listener.Prefixes.Add(RedirectUri + "/");
        listener.Start();
        var context = await listener.GetContextAsync();
        string code = context.Request.QueryString["code"];

        // Send response to browser
        var response = context.Response;
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(
            "<html><body>Sign-in complete. You can close this tab.</body></html>");
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();

        // 3. Exchange code for tokens (using PKCE -- no client secret needed)
        // ... HTTP POST to TokenEndpoint with code + code_verifier ...
        // Parse response JSON for id_token field
        // return idToken;

        return null; // Placeholder -- implement token exchange
    }
}
```

**Note:** This pattern requires careful implementation. The PKCE flow avoids needing a client secret (which should not be embedded in a desktop app). The `HttpListener` approach works on Windows. The full token exchange implementation is straightforward HTTP POST but should be implemented with proper error handling.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Coroutines for async scene loading | `async Awaitable` with `await` | Unity 2023.1 (stable in Unity 6) | Cleaner code; no IEnumerator boilerplate; proper exception handling |
| google-signin-unity plugin | Custom OAuth flow (desktop) | Deprecated Feb 2025 | Must implement browser-based OAuth for Windows |
| Firebase desktop = stubs only | Firebase desktop = functional (beta) for Auth + RTDB | Firebase Unity SDK ~12.x | Auth and RTDB work on Windows but labeled beta |
| Separate per-system save files | Unified GameSaveData (project decision) | D-03 | Single file simplifies Firebase sync; increases corruption blast radius |

**Deprecated/outdated:**
- `google-signin-unity` GitHub plugin: deprecated Feb 2025; Android/iOS only anyway
- `SceneManager.LoadScene` (synchronous): still works but blocks main thread; always use async
- Unity coroutines for async operations: still work but `Awaitable` is preferred in Unity 6

## Open Questions

1. **Firebase Auth credential persistence on Windows standalone**
   - What we know: Firebase docs state "user credentials now persist between sessions on desktop platforms." The auth state should survive app restarts.
   - What's unclear: Whether credential persistence works reliably across all Windows configurations (antivirus, AppData permissions, etc.) in production.
   - Recommendation: Test in a standalone build early (Plan 01-04). If persistence fails, fall back to re-authenticating anonymously on each launch (data is still preserved because the UID is stored in the local save file and can be used to re-associate).

2. **Google OAuth redirect on Windows firewall**
   - What we know: `HttpListener` on localhost requires no admin permissions for ports above 1024. Windows Firewall should not block localhost traffic.
   - What's unclear: Whether some antivirus or corporate firewall configurations block localhost HTTP listeners.
   - Recommendation: Use a high-numbered port (e.g., 8856). Add a timeout (30 seconds) to the listener so the game does not hang if the user closes the browser without completing OAuth. Provide a fallback "enter token manually" option for edge cases.

3. **File.Replace atomicity guarantee**
   - What we know: `File.Replace` on NTFS performs a metadata-level swap. It is the closest to atomic that Windows provides.
   - What's unclear: Whether Unity's Mono runtime on Unity 6 fully supports `File.Replace` (it should, as it maps to the Win32 `ReplaceFile` API).
   - Recommendation: Test `File.Replace` in a standalone build during Plan 01-01. If it throws `PlatformNotSupportedException`, fall back to `File.Delete(dst) + File.Move(tmp, dst)` which is near-atomic on the same NTFS volume.

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | Unity Test Framework (com.unity.test-framework, included in project) |
| Config file | None -- needs assembly definitions and test assemblies created in Wave 0 |
| Quick run command | Unity Editor > Window > General > Test Runner > EditMode > Run All |
| Full suite command | Unity Editor > Window > General > Test Runner > Run All (EditMode + PlayMode) |

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| FOUND-01 | Single save auto-saves on triggers | PlayMode | Test Runner > PlayMode > SaveManagerTests | Wave 0 |
| FOUND-02 | Save includes saveVersion | EditMode | Test Runner > EditMode > GameSaveDataTests | Wave 0 |
| FOUND-03 | Atomic write produces valid file | EditMode | Test Runner > EditMode > AtomicWriteTests | Wave 0 |
| FOUND-04 | Firebase sync pushes/pulls JSON | Manual | Verify in Firebase Console after standalone build test | Manual-only (requires Firebase project) |
| FOUND-05 | Boot auto-loads save | PlayMode | Test Runner > PlayMode > BootFlowTests | Wave 0 |
| FOUND-06 | Singletons initialized before transition | PlayMode | Test Runner > PlayMode > BootFlowTests | Wave 0 |
| FOUND-07 | Async additive scene loading | PlayMode | Test Runner > PlayMode > SceneControllerTests | Wave 0 |
| FOUND-08 | Main Menu buttons work | Manual | Manual UI test in PlayMode | Manual-only (UI interaction) |
| FOUND-09 | Settings persist via PlayerPrefs | EditMode | Test Runner > EditMode > SettingsManagerTests | Wave 0 |
| FOUND-10 | Background FPS cap applies | Manual | Manual focus/unfocus test + FPS counter | Manual-only (requires window focus change) |
| FOUND-11 | Anonymous auth on boot | Manual | Verify in Firebase Console | Manual-only (requires Firebase project) |
| FOUND-12 | Link Account to Google | Manual | Verify in Firebase Console | Manual-only (requires OAuth flow) |
| FOUND-13 | Linking preserves save data | Manual | Verify UID unchanged after link | Manual-only |
| FOUND-14 | Linked account restores data | Manual | Reinstall + sign in test | Manual-only |
| FOUND-15 | GameState enum + OnStateChanged event | EditMode | Test Runner > EditMode > GameManagerTests | Wave 0 |

### Sampling Rate
- **Per task commit:** Quick EditMode tests via Test Runner
- **Per wave merge:** Full EditMode + PlayMode suite
- **Phase gate:** Full suite green + manual Firebase verification before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `Assets/_Project/Tests/EditMode/` directory + EditMode test assembly definition
- [ ] `Assets/_Project/Tests/PlayMode/` directory + PlayMode test assembly definition
- [ ] `Assets/_Project/Scripts/Core/StellarCommand.Core.asmdef` -- needed so tests can reference Core
- [ ] `GameSaveDataTests.cs` -- covers FOUND-02 (saveVersion present, MigrateFrom callable)
- [ ] `AtomicWriteTests.cs` -- covers FOUND-03 (write-to-tmp, replace, verify file content)
- [ ] `SettingsManagerTests.cs` -- covers FOUND-09 (PlayerPrefs round-trip)
- [ ] `GameManagerTests.cs` -- covers FOUND-15 (state transitions, event firing)

## Sources

### Primary (HIGH confidence)
- [Unity 6000.3 Awaitable documentation](https://docs.unity3d.com/6000.3/Documentation/Manual/async-await-support.html) -- Awaitable class, async scene loading pattern
- [Unity Awaitable code examples](https://docs.unity3d.com/6000.2/Documentation/Manual/async-awaitable-examples.html) -- SceneManager.LoadSceneAsync await, cancellation tokens, composition
- [Unity AudioMixer.SetFloat API](https://docs.unity3d.com/ScriptReference/Audio.AudioMixer.SetFloat.html) -- exposed parameter runtime control
- [Unity Application.targetFrameRate](https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html) -- vSync override behavior, default values
- [.NET File.Replace](https://learn.microsoft.com/en-us/dotnet/api/system.io.file.replace) -- atomic file replacement on Windows
- [.NET File.Move](https://learn.microsoft.com/en-us/dotnet/api/system.io.file.move) -- overwrite parameter in .NET Core 3.0+
- Project source: `MonoSingleton.cs`, `SaveManager.cs`, `BootManager.cs`, `GameManager.cs` -- reviewed in full

### Secondary (MEDIUM confidence)
- [Firebase Unity setup docs](https://firebase.google.com/docs/unity/setup) -- desktop support labeled beta; Auth + RTDB functional on Windows
- [Firebase anonymous auth Unity](https://firebase.google.com/docs/auth/unity/anonymous-auth) -- SignInAnonymouslyAsync pattern
- [Firebase account linking Unity](https://firebase.google.com/docs/auth/unity/account-linking) -- LinkWithCredentialAsync flow
- [Firebase Realtime Database Unity](https://firebase.google.com/docs/database/unity/save-data) -- SetRawJsonValueAsync for JSON blobs
- [Firebase Unity SDK GitHub releases](https://github.com/firebase/firebase-unity-sdk/releases) -- v13.9.0 latest
- [NTFS atomic file operations](https://antonymale.co.uk/windows-atomic-file-writes.html) -- File.Replace atomicity analysis

### Tertiary (LOW confidence)
- Google Sign-In deprecation (Feb 2025) -- confirmed via multiple WebSearch sources but no official deprecation page URL found
- Google OAuth desktop PKCE flow -- based on [Google OAuth 2.0 for Desktop Apps](https://developers.google.com/identity/protocols/oauth2/native-app) general documentation, not Unity-specific
- Firebase credential persistence on desktop -- mentioned in release notes but exact behavior on Windows standalone needs validation

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all packages confirmed from manifest.json and project files
- Architecture (save, scene, settings): HIGH -- well-established Unity patterns with official documentation
- Architecture (Firebase auth + sync): MEDIUM -- functional on desktop but beta; Google Sign-In requires custom OAuth
- Pitfalls: HIGH -- all critical pitfalls verified against official docs or multiple sources

**Research date:** 2026-03-25
**Valid until:** 2026-04-25 (Firebase SDK releases frequently; verify version before Plan 01-04)
