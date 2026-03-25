namespace StellarCommand.Core
{
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneController : MonoSingleton<SceneController>
    {
        private string _currentScene;

        public string CurrentScene => _currentScene;
        public event Action OnSceneTransitionStart;
        public event Action<string> OnSceneTransitionComplete;

        public async Awaitable LoadSceneAsync(string sceneName, string unloadScene = null)
        {
            OnSceneTransitionStart?.Invoke();

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (!string.IsNullOrEmpty(unloadScene))
            {
                await SceneManager.UnloadSceneAsync(unloadScene);
            }

            _currentScene = sceneName;
            OnSceneTransitionComplete?.Invoke(sceneName);

            Debug.Log($"[SceneController] Loaded scene: {sceneName}");
        }
    }
}
