namespace StellarCommand.Core
{
    [System.Serializable]
    public class GameSaveData
    {
        public int saveVersion = 1;
        public long lastSaveTimestamp;

        // Future phases add fields here:
        // public List<QuestInstanceData> quests = new();
        // public List<ResourceEntry> resources = new();

        public static GameSaveData CreateNew()
        {
            return new GameSaveData
            {
                saveVersion = 1,
                lastSaveTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }

        public void MigrateFrom(int fromVersion)
        {
            // Stub -- add migration logic when saveVersion increments
            // Example: if (fromVersion < 2) { /* migrate v1 -> v2 */ }
        }
    }
}
