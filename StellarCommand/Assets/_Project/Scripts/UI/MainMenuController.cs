namespace StellarCommand.Core
{
    using UnityEngine;
    using UnityEngine.UI;

    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        [Header("Dialogs")]
        [SerializeField] private ConfirmDialog _confirmDialog;

        [Header("Scene Names")]
        [SerializeField] private string _gameSceneName = "Game";
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        private void Start()
        {
            _newGameButton.onClick.AddListener(OnNewGameClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnNewGameClicked()
        {
            // Per D-07: modal confirmation before wipe
            _confirmDialog.Show(
                "All progress will be deleted. This cannot be undone.",
                onConfirm: OnNewGameConfirmed,
                onCancel: null
            );
        }

        private async void OnNewGameConfirmed()
        {
            SaveManager.Instance.DeleteAll();
            // Reload from Boot to reinitialize everything
            await SceneController.Instance.LoadSceneAsync("Boot", _mainMenuSceneName);
            GameManager.Instance.SetState(GameManager.GameState.Boot);
        }

        private void OnSettingsClicked()
        {
            // Settings UI will be wired in Plan 01-03
            Debug.Log("[MainMenu] Settings clicked -- not yet implemented.");
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
