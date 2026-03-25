using System;
using UnityEngine;

namespace StellarCommand.Core
{
    public class GameManager : MonoSingleton<GameManager>
    {
        public enum GameState { Boot, MainMenu, Playing, Paused }

        public GameState CurrentState { get; private set; } = GameState.Boot;

        public event Action<GameState, GameState> OnStateChanged;

        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            var oldState = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(oldState, newState);

            Debug.Log($"[GameManager] State changed: {oldState} -> {newState}");
        }

        protected override void OnInitialize()
        {
            // GameManager is a state hub only — no gameplay logic here.
        }
    }
}
