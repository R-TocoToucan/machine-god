using UnityEngine;

namespace StellarCommand.Core
{
    /// <summary>
    /// Generic singleton base for all manager classes.
    /// Attach to a GameObject in Boot.unity.
    /// Persists across scene loads via DontDestroyOnLoad.
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[MonoSingleton] Instance of {typeof(T)} requested after application quit. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            Debug.LogError($"[MonoSingleton] No instance of {typeof(T)} found in scene. " +
                                           $"Make sure it exists in Boot.unity.");
                        }
                    }
                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[MonoSingleton] Duplicate instance of {typeof(T)} detected. Destroying this one.");
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            DontDestroyOnLoad(gameObject);

            OnInitialize();
        }

        /// <summary>
        /// Override this instead of Awake() in subclasses.
        /// </summary>
        protected virtual void OnInitialize() { }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
            }
        }
    }
}
