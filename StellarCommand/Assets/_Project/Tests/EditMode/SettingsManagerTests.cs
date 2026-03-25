using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace StellarCommand.Core.Tests.EditMode
{
    [TestFixture]
    public class SettingsManagerTests
    {
        private const string TestKey = "test.settingsmanager.key";
        private const string NonexistentKey = "test.settingsmanager.nonexistent";

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(TestKey);
            PlayerPrefs.DeleteKey(NonexistentKey);
        }

        [Test]
        public void SetFloat_PersistsToPlayerPrefs()
        {
            PlayerPrefs.DeleteKey(TestKey);

            PlayerPrefs.SetFloat(TestKey, 0.5f);
            PlayerPrefs.Save();

            float result = PlayerPrefs.GetFloat(TestKey, 0f);
            Assert.AreEqual(0.5f, result, 0.001f,
                "SetFloat should persist value retrievable via GetFloat");
        }

        [Test]
        public void GetFloat_ReturnsDefault_WhenKeyMissing()
        {
            PlayerPrefs.DeleteKey(NonexistentKey);

            float result = PlayerPrefs.GetFloat(NonexistentKey, 0.75f);
            Assert.AreEqual(0.75f, result, 0.001f,
                "GetFloat should return default value when key does not exist");
        }

        [Test]
        public void AllSettingsKeys_AreUnique()
        {
            var fields = typeof(SettingsKeys).GetFields(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            var constFields = fields
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .ToList();

            Assert.IsTrue(constFields.Count > 0, "SettingsKeys should have at least one constant");

            var values = constFields.Select(f => (string)f.GetRawConstantValue()).ToList();
            var uniqueValues = new HashSet<string>(values);

            Assert.AreEqual(constFields.Count, uniqueValues.Count,
                "All SettingsKeys constants must have unique values");
        }
    }
}
