using NUnit.Framework;

namespace StellarCommand.Core.Tests.EditMode
{
    [TestFixture]
    public class GameSaveDataTests
    {
        [Test]
        public void SaveVersion_DefaultsToOne()
        {
            var data = new GameSaveData();
            Assert.AreEqual(1, data.saveVersion);
        }

        [Test]
        public void MigrateFrom_DoesNotThrow()
        {
            var data = new GameSaveData();
            Assert.DoesNotThrow(() => data.MigrateFrom(0));
        }

        [Test]
        public void CreateNew_SetsTimestamp()
        {
            var data = GameSaveData.CreateNew();
            Assert.Greater(data.lastSaveTimestamp, 0);
        }
    }
}
