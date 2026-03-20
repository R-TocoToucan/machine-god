using UnityEngine;
using UnityEngine.SceneManagement;

namespace StellarCommand.Core
{
    /// <summary>
    /// Entry point of the application.
    /// Attach to a GameObject in Boot.unity.
    /// Initializes all persistent managers, then loads the main scene.
    /// </summary>
    public class BootManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string _firstSceneName = "BattleMap";

        private void Start()
        {
            Debug.Log("[BootManager] Initializing...");

            // Managers are initialized via their own Awake/OnInitialize.
            // Add any cross-manager setup here if needed in the future.

            Debug.Log("[BootManager] Boot complete. Loading main scene...");
            SceneManager.LoadScene(_firstSceneName);
        }
    }
}
