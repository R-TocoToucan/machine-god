using NUnit.Framework;
using UnityEngine;

namespace StellarCommand.Core.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for boot flow validation.
    /// Full scene-loading tests require Boot and MainMenu scenes in Build Settings.
    /// These tests validate singleton initialization contracts structurally.
    /// </summary>
    [TestFixture]
    public class BootFlowTests
    {
        private GameObject _saveManagerGo;
        private GameObject _gameManagerGo;
        private GameObject _sceneControllerGo;

        [SetUp]
        public void SetUp()
        {
            // Create singleton instances to simulate Boot scene state.
            // MonoSingleton.Awake sets the Instance and calls OnInitialize.
            _saveManagerGo = new GameObject("TestSaveManager");
            _saveManagerGo.AddComponent<SaveManager>();

            _gameManagerGo = new GameObject("TestGameManager");
            _gameManagerGo.AddComponent<GameManager>();

            _sceneControllerGo = new GameObject("TestSceneController");
            _sceneControllerGo.AddComponent<SceneController>();
        }

        [TearDown]
        public void TearDown()
        {
            // DestroyImmediate to clean up in test context
            if (_sceneControllerGo != null)
                Object.DestroyImmediate(_sceneControllerGo);
            if (_gameManagerGo != null)
                Object.DestroyImmediate(_gameManagerGo);
            if (_saveManagerGo != null)
                Object.DestroyImmediate(_saveManagerGo);
        }

        [Test]
        public void Boot_InitializesAllSingletons()
        {
            // After Awake runs (triggered by AddComponent), singletons should be accessible
            Assert.IsNotNull(SaveManager.Instance, "SaveManager.Instance should not be null after boot");
            Assert.IsNotNull(GameManager.Instance, "GameManager.Instance should not be null after boot");
            Assert.IsNotNull(SceneController.Instance, "SceneController.Instance should not be null after boot");
        }

        [Test]
        public void Boot_AutoLoadsSave()
        {
            // SaveManager.OnInitialize calls LoadFromDisk, which creates fresh data if none exists.
            // After initialization, Data should never be null.
            Assert.IsNotNull(SaveManager.Instance.Data, "SaveManager.Data should not be null after boot (auto-load creates fresh save)");
        }
    }
}
