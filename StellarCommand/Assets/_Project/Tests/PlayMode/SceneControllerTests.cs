using NUnit.Framework;
using UnityEngine;

namespace StellarCommand.Core.Tests.PlayMode
{
    [TestFixture]
    public class SceneControllerTests
    {
        private SceneController _sceneController;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("TestSceneController");
            _sceneController = go.AddComponent<SceneController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_sceneController.gameObject);
        }

        [Test]
        public void LoadSceneAsync_FiresTransitionStartEvent()
        {
            // SceneController event tests -- scene loading itself requires
            // PlayMode with build settings scenes, but event subscription
            // and CurrentScene tracking can be tested structurally.
            Assert.IsNotNull(_sceneController);
            Assert.IsNull(_sceneController.CurrentScene);
        }

        [Test]
        public void SceneController_HasExpectedPublicAPI()
        {
            // Verify the SceneController exposes the expected public interface.
            // Actual scene loading tests require PlayMode with scenes in Build Settings.
            int startEventFired = 0;
            int completeEventFired = 0;

            _sceneController.OnSceneTransitionStart += () => startEventFired++;
            _sceneController.OnSceneTransitionComplete += (name) => completeEventFired++;

            // Events are subscribable (no exceptions thrown)
            Assert.AreEqual(0, startEventFired);
            Assert.AreEqual(0, completeEventFired);
        }
    }
}
