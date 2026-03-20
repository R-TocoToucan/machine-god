using System.Collections;
using UnityEngine;

namespace StellarCommand.Core
{
    public class SceneTransitionManager : MonoSingleton<SceneTransitionManager>
    {
        [SerializeField] private float fadeDuration = 1f;

        private bool _isTransitioning;

        public void LoadScene(string sceneName)
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionCoroutine(sceneName));
        }

        public void LoadScene(int sceneIndex)
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionCoroutine(sceneIndex));
        }

        private IEnumerator TransitionCoroutine(string sceneName)
        {
            _isTransitioning = true;
            yield return StartCoroutine(FadeOut());
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            yield return StartCoroutine(FadeIn());
            _isTransitioning = false;
        }

        private IEnumerator TransitionCoroutine(int sceneIndex)
        {
            _isTransitioning = true;
            yield return StartCoroutine(FadeOut());
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
            yield return StartCoroutine(FadeIn());
            _isTransitioning = false;
        }

        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                // TODO: Apply fade-out visual (CanvasGroup alpha, post-process, etc.)
                yield return null;
            }
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                // TODO: Apply fade-in visual (CanvasGroup alpha, post-process, etc.)
                yield return null;
            }
        }
    }
}
