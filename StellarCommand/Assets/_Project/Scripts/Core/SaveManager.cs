using System;
using System.IO;
using UnityEngine;

namespace StellarCommand.Core
{
    /// <summary>
    /// Generic JSON save/load system.
    /// Saves to Application.persistentDataPath/Saves/
    /// Usage:
    ///   SaveManager.Instance.Save("quests", myQuestSaveData);
    ///   var data = SaveManager.Instance.Load<QuestSaveData>("quests");
    /// </summary>
    public class SaveManager : MonoSingleton<SaveManager>
    {
        private string _saveDir;

        protected override void OnInitialize()
        {
            _saveDir = Path.Combine(Application.persistentDataPath, "Saves");

            if (!Directory.Exists(_saveDir))
            {
                Directory.CreateDirectory(_saveDir);
                Debug.Log($"[SaveManager] Save directory created at: {_saveDir}");
            }
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Serialize and write data to disk as JSON.
        /// </summary>
        public void Save<T>(string key, T data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                string path = GetPath(key);
                File.WriteAllText(path, json);
                Debug.Log($"[SaveManager] Saved '{key}' → {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save '{key}': {e.Message}");
            }
        }

        /// <summary>
        /// Load and deserialize data from disk.
        /// Returns default(T) if file does not exist.
        /// </summary>
        public T Load<T>(string key)
        {
            string path = GetPath(key);

            if (!File.Exists(path))
            {
                Debug.Log($"[SaveManager] No save file found for '{key}'. Returning default.");
                return default;
            }

            try
            {
                string json = File.ReadAllText(path);
                T data = JsonUtility.FromJson<T>(json);
                Debug.Log($"[SaveManager] Loaded '{key}' ← {path}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load '{key}': {e.Message}");
                return default;
            }
        }

        /// <summary>
        /// Check whether a save file exists for the given key.
        /// </summary>
        public bool Exists(string key)
        {
            return File.Exists(GetPath(key));
        }

        /// <summary>
        /// Delete a save file by key.
        /// </summary>
        public void Delete(string key)
        {
            string path = GetPath(key);

            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[SaveManager] Deleted save file for '{key}'.");
            }
        }

        /// <summary>
        /// Wipe all save files. Use with caution.
        /// </summary>
        public void DeleteAll()
        {
            foreach (string file in Directory.GetFiles(_saveDir, "*.json"))
            {
                File.Delete(file);
            }
            Debug.LogWarning("[SaveManager] All save files deleted.");
        }

        // ── Internal ─────────────────────────────────────────────────────────────

        private string GetPath(string key)
        {
            return Path.Combine(_saveDir, $"{key}.json");
        }
    }
}
