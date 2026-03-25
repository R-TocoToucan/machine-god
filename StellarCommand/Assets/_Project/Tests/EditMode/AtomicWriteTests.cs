using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace StellarCommand.Core.Tests.EditMode
{
    [TestFixture]
    public class AtomicWriteTests
    {
        private string _testDir;
        private string _testFilePath;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "StellarCommandTests");
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
            Directory.CreateDirectory(_testDir);
            _testFilePath = Path.Combine(_testDir, "test_save.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        [Test]
        public void AtomicWrite_CreatesFile()
        {
            string json = "{\"saveVersion\":1}";
            AtomicFileWriter.WriteAtomic(_testFilePath, json);

            Assert.IsTrue(File.Exists(_testFilePath));
            Assert.AreEqual(json, File.ReadAllText(_testFilePath));
        }

        [Test]
        public void AtomicWrite_ReplacesExistingFile()
        {
            string oldJson = "{\"saveVersion\":1}";
            string newJson = "{\"saveVersion\":2}";

            File.WriteAllText(_testFilePath, oldJson);
            AtomicFileWriter.WriteAtomic(_testFilePath, newJson);

            Assert.AreEqual(newJson, File.ReadAllText(_testFilePath));
        }

        [Test]
        public void AtomicWrite_CreatesBakFile()
        {
            string oldJson = "{\"saveVersion\":1}";
            string newJson = "{\"saveVersion\":2}";
            string bakPath = _testFilePath + ".bak";

            File.WriteAllText(_testFilePath, oldJson);
            AtomicFileWriter.WriteAtomic(_testFilePath, newJson);

            Assert.IsTrue(File.Exists(bakPath));
            Assert.AreEqual(oldJson, File.ReadAllText(bakPath));
        }

        [Test]
        public void AtomicWrite_OnLoadFailure_FallsBackToBak()
        {
            // Write a valid .bak file
            string validJson = JsonUtility.ToJson(GameSaveData.CreateNew());
            string bakPath = _testFilePath + ".bak";
            File.WriteAllText(bakPath, validJson);

            // Write corrupt main file
            File.WriteAllText(_testFilePath, "NOT JSON {{{corrupt");

            // Attempt load via the fallback reader
            GameSaveData result = AtomicFileWriter.LoadWithFallback(_testFilePath);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.saveVersion);
            Assert.Greater(result.lastSaveTimestamp, 0);
        }
    }
}
