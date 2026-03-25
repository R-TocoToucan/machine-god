using UnityEngine;

namespace StellarCommand.Core
{
    /// <summary>
    /// Entry point of the application.
    /// Attach to a GameObject in Boot.unity.
    /// Initializes all persistent managers, then loads the Main Menu asynchronously.
    /// </summary>
    public class BootManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string _mainMenuScene = "MainMenu";

        private async void Start()
        {
            Debug.Log("[BootManager] Initializing...");

            // Singletons self-initialize via Awake/OnInitialize (MonoSingleton pattern).
            // SaveManager.OnInitialize auto-loads save from disk (added in Plan 01-01).
            // By the time Start() runs, all Awakes have completed.

            // Verify critical singletons are alive
            if (SaveManager.Instance == null)
            {
                Debug.LogError("[BootManager] SaveManager not found in Boot scene!");
                return;
            }
            if (GameManager.Instance == null)
            {
                Debug.LogError("[BootManager] GameManager not found in Boot scene!");
                return;
            }
            if (SceneController.Instance == null)
            {
                Debug.LogError("[BootManager] SceneController not found in Boot scene!");
                return;
            }

            Debug.Log("[BootManager] All singletons initialized. Loading Main Menu...");

            // Transition to Main Menu (Boot scene stays loaded — singletons persist)
            await SceneController.Instance.LoadSceneAsync(_mainMenuScene);
            GameManager.Instance.SetState(GameManager.GameState.MainMenu);

            Debug.Log("[BootManager] Main Menu loaded.");
        }
    }
}
