using System;
using System.IO;
using UnityEngine;

namespace StellarCommand.Core
{
    /// <summary>
    /// Static utility for atomic file writes with backup support.
    /// Writes to .tmp first, then uses File.Replace to swap atomically.
    /// </summary>
    public static class AtomicFileWriter
    {
        /// <summary>
        /// Atomically write JSON content to the specified path.
        /// Creates a .tmp file first, then replaces the target.
        /// If the target already exists, the old content is preserved as .bak.
        /// </summary>
        public static void WriteAtomic(string finalPath, string json)
        {
            string tmpPath = finalPath + ".tmp";
            string bakPath = finalPath + ".bak";

            try
            {
                // Ensure directory exists
                string dir = Path.GetDirectoryName(finalPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // Write to temporary file first
                File.WriteAllText(tmpPath, json);

                if (File.Exists(finalPath))
                {
                    // Atomic replace: tmp -> final, old final -> bak
                    File.Replace(tmpPath, finalPath, bakPath);
                }
                else
                {
                    // No existing file -- just move tmp into place
                    File.Move(tmpPath, finalPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AtomicFileWriter] Failed to write atomically to '{finalPath}': {e.Message}");

                // Clean up tmp file on failure
                if (File.Exists(tmpPath))
                {
                    try { File.Delete(tmpPath); }
                    catch { /* best effort cleanup */ }
                }
            }
        }

        /// <summary>
        /// Load GameSaveData from the specified path with .bak fallback.
        /// If main file is corrupt or missing, tries .bak.
        /// If both fail, returns GameSaveData.CreateNew().
        /// </summary>
        public static GameSaveData LoadWithFallback(string finalPath)
        {
            string bakPath = finalPath + ".bak";

            // Try main file first
            if (File.Exists(finalPath))
            {
                try
                {
                    string json = File.ReadAllText(finalPath);
                    GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                    if (data != null)
                    {
                        Debug.Log($"[AtomicFileWriter] Loaded save from main file: {finalPath}");
                        return data;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[AtomicFileWriter] Main file corrupt or unreadable: {e.Message}");
                }
            }

            // Fallback to .bak file
            if (File.Exists(bakPath))
            {
                try
                {
                    string json = File.ReadAllText(bakPath);
                    GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                    if (data != null)
                    {
                        Debug.LogWarning($"[AtomicFileWriter] Recovered save from backup: {bakPath}");
                        return data;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AtomicFileWriter] Backup file also corrupt: {e.Message}");
                }
            }

            // Both failed -- create new
            Debug.LogWarning("[AtomicFileWriter] No valid save found. Creating new save data.");
            return GameSaveData.CreateNew();
        }
    }
}
