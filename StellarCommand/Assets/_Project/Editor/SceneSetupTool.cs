using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using StellarCommand.Core;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace StellarCommand.Editor
{
    /// <summary>
    /// Programmatically creates and wires up scenes required for Phase 1.
    /// Menu: StellarCommand > Setup Scenes
    /// </summary>
    public static class SceneSetupTool
    {
        private const string BootScenePath     = "Assets/_Project/Scenes/Boot.unity";
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";

        [MenuItem("StellarCommand/Setup Scenes")]
        public static void SetupScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[SceneSetupTool] Cancelled by user.");
                return;
            }

            UpdateBootScene();
            CreateMainMenuScene();
            UpdateBuildSettings();

            EditorUtility.DisplayDialog(
                "Scene Setup Complete",
                "Boot.unity updated and MainMenu.unity created.\n\nBuild Settings have been updated with both scenes.",
                "OK");
        }

        // -------------------------------------------------------------------------
        // Boot scene — add singleton managers if missing
        // -------------------------------------------------------------------------

        private static void UpdateBootScene()
        {
            if (!System.IO.File.Exists(BootScenePath))
            {
                Debug.LogWarning($"[SceneSetupTool] Boot scene not found at {BootScenePath}, skipping.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);

            AddIfMissing<GameManager>("GameManager");
            AddIfMissing<SceneController>("SceneController");
            AddIfMissing<SaveManager>("SaveManager");
            AddIfMissing<BootManager>("BootManager");

            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SceneSetupTool] Boot.unity updated.");
        }

        private static void AddIfMissing<T>(string goName) where T : Component
        {
#pragma warning disable CS0618
            if (Object.FindObjectOfType<T>() != null) return;
#pragma warning restore CS0618
            var go = new GameObject(goName);
            go.AddComponent<T>();
            Debug.Log($"[SceneSetupTool] Added {goName} to Boot scene.");
        }

        // -------------------------------------------------------------------------
        // MainMenu scene — create from scratch
        // -------------------------------------------------------------------------

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera — no AudioListener (Boot scene owns the listener; MainMenu loads additively)
            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var cam = cameraGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Depth;
            cam.cullingMask = 1 << LayerMask.NameToLayer("UI");
            cam.orthographic = true;
            cam.depth = 1;
            // AudioListener intentionally omitted — Boot camera provides it

            // EventSystem — use new Input System module when active, legacy otherwise
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esGO.AddComponent<InputSystemUIInputModule>();
#else
            esGO.AddComponent<StandaloneInputModule>();
#endif

            // Canvas
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Background
            var bg = CreatePanel("Background", canvasGO.transform, new Color(0.05f, 0.05f, 0.1f));
            StretchToParent(bg);

            // Title
            var titleGO = CreateUIObject("Title", bg.transform);
            SetAnchored(titleGO, new Vector2(0.5f, 0.75f), new Vector2(800, 100), Vector2.zero);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Stellar Command";
            titleTMP.fontSize = 72;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = Color.white;
            titleTMP.fontStyle = FontStyles.Bold;

            // Main buttons — centred in lower half
            var newGameBtn  = CreateButton("NewGameButton",  bg.transform, "New Game", new Vector2(0,  80));
            var settingsBtn = CreateButton("SettingsButton", bg.transform, "Settings", new Vector2(0,   0));
            var quitBtn     = CreateButton("QuitButton",     bg.transform, "Quit",     new Vector2(0, -80));

            // ConfirmDialog (overlay — child of Canvas so it renders on top)
            var confirmDialogGO   = CreateUIObject("ConfirmDialog", canvasGO.transform);
            var confirmDialogComp = confirmDialogGO.AddComponent<ConfirmDialog>();

            // Dim overlay panel (the _panel field — starts inactive at runtime via Awake)
            var dialogPanel = CreatePanel("Panel", confirmDialogGO.transform, new Color(0, 0, 0, 0.8f));
            StretchToParent(dialogPanel);

            // Dialog box centred in screen
            var dialogBox = CreatePanel("DialogBox", dialogPanel.transform, new Color(0.12f, 0.12f, 0.2f));
            SetAnchored(dialogBox, new Vector2(0.5f, 0.5f), new Vector2(560, 220), Vector2.zero);

            var msgGO = CreateUIObject("MessageText", dialogBox.transform);
            SetAnchored(msgGO, new Vector2(0.5f, 0.5f), new Vector2(500, 80), new Vector2(0, 40));
            var msgTMP = msgGO.AddComponent<TextMeshProUGUI>();
            msgTMP.text = "";
            msgTMP.fontSize = 22;
            msgTMP.alignment = TextAlignmentOptions.Center;
            msgTMP.color = Color.white;
            msgTMP.enableWordWrapping = true;

            var confirmBtn = CreateButton("ConfirmButton", dialogBox.transform, "Confirm", new Vector2(-90, -60));
            var cancelBtn  = CreateButton("CancelButton",  dialogBox.transform, "Cancel",  new Vector2( 90, -60));

            // Wire ConfirmDialog serialized fields
            var confirmSO = new SerializedObject(confirmDialogComp);
            confirmSO.FindProperty("_panel").objectReferenceValue        = dialogPanel;
            confirmSO.FindProperty("_messageText").objectReferenceValue  = msgTMP;
            confirmSO.FindProperty("_confirmButton").objectReferenceValue = confirmBtn.GetComponent<Button>();
            confirmSO.FindProperty("_cancelButton").objectReferenceValue  = cancelBtn.GetComponent<Button>();
            confirmSO.ApplyModifiedPropertiesWithoutUndo();

            // Start the panel inactive (Awake also does this, but set it here for editor clarity)
            dialogPanel.SetActive(false);

            // MainMenuController (sits outside Canvas — plain GameObject)
            var controllerGO = new GameObject("MainMenuController");
            var controller   = controllerGO.AddComponent<MainMenuController>();

            var ctrlSO = new SerializedObject(controller);
            ctrlSO.FindProperty("_newGameButton").objectReferenceValue  = newGameBtn.GetComponent<Button>();
            ctrlSO.FindProperty("_settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
            ctrlSO.FindProperty("_quitButton").objectReferenceValue     = quitBtn.GetComponent<Button>();
            ctrlSO.FindProperty("_confirmDialog").objectReferenceValue  = confirmDialogComp;
            ctrlSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[SceneSetupTool] MainMenu.unity created at {MainMenuScenePath}");
        }

        // -------------------------------------------------------------------------
        // Build Settings
        // -------------------------------------------------------------------------

        private static void UpdateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();

            EnsureScene(scenes, BootScenePath,     insertAt: 0);
            EnsureScene(scenes, MainMenuScenePath, insertAt: 1);

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[SceneSetupTool] Build Settings updated.");
        }

        private static void EnsureScene(List<EditorBuildSettingsScene> scenes, string path, int insertAt)
        {
            if (scenes.Any(s => s.path == path)) return;

            var entry = new EditorBuildSettingsScene(path, true);
            int idx = Mathf.Min(insertAt, scenes.Count);
            scenes.Insert(idx, entry);
        }

        // -------------------------------------------------------------------------
        // UI helpers
        // -------------------------------------------------------------------------

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var go    = CreateUIObject(name, parent);
            var image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        private static void StretchToParent(GameObject go)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin  = Vector2.zero;
            rect.anchorMax  = Vector2.one;
            rect.offsetMin  = Vector2.zero;
            rect.offsetMax  = Vector2.zero;
        }

        private static void SetAnchored(GameObject go, Vector2 anchor, Vector2 size, Vector2 position)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin       = anchor;
            rect.anchorMax       = anchor;
            rect.pivot           = new Vector2(0.5f, 0.5f);
            rect.sizeDelta       = size;
            rect.anchoredPosition = position;
        }

        private static GameObject CreateButton(string name, Transform parent, string label, Vector2 position)
        {
            var go    = CreatePanel(name, parent, new Color(0.18f, 0.18f, 0.28f));
            SetAnchored(go, new Vector2(0.5f, 0.5f), new Vector2(300, 55), position);

            var btn    = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.28f, 0.28f, 0.45f);
            colors.pressedColor     = new Color(0.12f, 0.12f, 0.2f);
            btn.colors = colors;

            var labelGO   = CreateUIObject("Text", go.transform);
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin  = Vector2.zero;
            labelRect.anchorMax  = Vector2.one;
            labelRect.offsetMin  = Vector2.zero;
            labelRect.offsetMax  = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            return go;
        }
    }
}
