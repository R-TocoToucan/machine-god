using UnityEngine;
using UnityEngine.UI;
using StellarCommand.Core;

namespace StellarCommand.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject settingsPanel;

        private void Start()
        {
            continueButton.interactable = SaveManager.Instance.HasSave();

            newGameButton.onClick.AddListener(OnNewGame);
            continueButton.onClick.AddListener(OnContinue);
            settingsButton.onClick.AddListener(OnSettings);
            quitButton.onClick.AddListener(OnQuit);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void OnNewGame()
        {
            SaveManager.Instance.DeleteSave();
            SceneTransitionManager.Instance.LoadScene("Bridge");
        }

        private void OnContinue()
        {
            // TODO: Load saved scene/state
            SceneTransitionManager.Instance.LoadScene("Bridge");
        }

        private void OnSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
