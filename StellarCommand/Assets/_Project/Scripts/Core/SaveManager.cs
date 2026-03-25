using System;
using System.IO;
using UnityEngine;

namespace StellarCommand.Core
{
    /// <summary>
    /// Unified save system for GameSaveData.
    /// Uses atomic writes (write-to-tmp, File.Replace) with .bak fallback.
    /// Auto-loads on boot, auto-saves on focus loss.
    /// </summary>
    public class SaveManager : MonoSingleton<SaveManager>
    {
        private GameSaveData _currentData;
        private string _saveDir;
        private const string SaveFileName = "game.json";

        /// <summary>
        /// Read access to the current in-memory save data.
        /// </summary>
        public GameSaveData Data => _currentData;

        protected override void OnInitialize()
        {
            _saveDir = Path.Combine(Application.persistentDataPath, "Saves");

            if (!Directory.Exists(_saveDir))
            {
                Directory.CreateDirectory(_saveDir);
                Debug.Log($"[SaveManager] Save directory created at: {_saveDir}");
            }

            // Auto-load existing save on boot (per FOUND-05)
            LoadFromDisk();
        }

        // -- Public API -------------------------------------------------------

        /// <summary>
        /// Serialize current save data to disk using atomic write.
        /// </summary>
        public void Save()
        {
            try
            {
                _currentData.lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string json = JsonUtility.ToJson(_currentData, true);
                AtomicFileWriter.WriteAtomic(GetPath(), json);
                Debug.Log("[SaveManager] Game saved.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
            }
        }

        /// <summary>
        /// Load save data from disk with .bak fallback.
        /// If no valid save exists, creates new via GameSaveData.CreateNew().
        /// </summary>
        public void LoadFromDisk()
        {
            string path = GetPath();
            _currentData = AtomicFileWriter.LoadWithFallback(path);
            MigrateIfNeeded();
            Debug.Log($"[SaveManager] Save loaded (version {_currentData.saveVersion}).");
        }

        /// <summary>
        /// Check whether the save file exists on disk.
        /// </summary>
        public bool Exists()
        {
            return File.Exists(GetPath());
        }

        /// <summary>
        /// Wipe all save files and reset to a fresh save.
        /// </summary>
        public void DeleteAll()
        {
            string path = GetPath();
            string tmpPath = path + ".tmp";
            string bakPath = path + ".bak";

            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(tmpPath)) File.Delete(tmpPath);
            if (File.Exists(bakPath)) File.Delete(bakPath);

            _currentData = GameSaveData.CreateNew();
            Debug.LogWarning("[SaveManager] All save files deleted. Fresh save created.");
        }

        // -- Auto-save triggers -----------------------------------------------

        /// <summary>
        /// Auto-save when the application loses focus (per D-04).
        /// Other auto-save triggers (habit completion, resource update, report submit)
        /// are wired by consuming systems via SaveManager.Instance.Save().
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && _currentData != null)
            {
                Save();
                Debug.Log("[SaveManager] Auto-saved on focus loss.");
            }
        }

        // -- Internal ---------------------------------------------------------

        private void MigrateIfNeeded()
        {
            int currentVersion = GameSaveData.CreateNew().saveVersion;
            if (_currentData.saveVersion < currentVersion)
            {
                Debug.Log($"[SaveManager] Migrating save from v{_currentData.saveVersion} to v{currentVersion}.");
                _currentData.MigrateFrom(_currentData.saveVersion);
                _currentData.saveVersion = currentVersion;
                Save();
            }
        }

        private string GetPath()
        {
            return Path.Combine(_saveDir, SaveFileName);
        }
    }
}
