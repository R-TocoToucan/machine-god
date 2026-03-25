using NUnit.Framework;
using UnityEngine;

namespace StellarCommand.Core.Tests.EditMode
{
    [TestFixture]
    public class GameManagerTests
    {
        private GameManager _gameManager;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("TestGameManager");
            _gameManager = go.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameManager.gameObject);
        }

        [Test]
        public void InitialState_IsBoot()
        {
            Assert.AreEqual(GameManager.GameState.Boot, _gameManager.CurrentState);
        }

        [Test]
        public void SetState_FiresOnStateChanged()
        {
            int fireCount = 0;
            _gameManager.OnStateChanged += (oldState, newState) => fireCount++;

            _gameManager.SetState(GameManager.GameState.MainMenu);

            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void SetState_SameState_DoesNotFire()
        {
            int fireCount = 0;
            _gameManager.OnStateChanged += (oldState, newState) => fireCount++;

            _gameManager.SetState(GameManager.GameState.Boot);

            Assert.AreEqual(0, fireCount);
        }

        [Test]
        public void OnStateChanged_ReceivesOldAndNewState()
        {
            GameManager.GameState receivedOld = GameManager.GameState.Boot;
            GameManager.GameState receivedNew = GameManager.GameState.Boot;

            _gameManager.OnStateChanged += (oldState, newState) =>
            {
                receivedOld = oldState;
                receivedNew = newState;
            };

            _gameManager.SetState(GameManager.GameState.MainMenu);

            Assert.AreEqual(GameManager.GameState.Boot, receivedOld);
            Assert.AreEqual(GameManager.GameState.MainMenu, receivedNew);
        }
    }
}
