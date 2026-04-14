using System.Collections;
using UnityEngine;
using {{PROJECT_NAMESPACE}}.GameFoundation.StateMachine;

namespace {{PROJECT_NAMESPACE}}.Gameplay
{
    /// <summary>
    /// Drives the Gameplay scene's state flow.
    ///
    /// Flow: Main (user clicks Play) → Init → [InitGame] → Ready → [Countdown] → Play
    ///
    /// Because the scene loads AFTER GameStateManager.Init() is called, this controller
    /// checks CurrentState in OnEnable to handle the case where Init fired before the
    /// scene was ready. Subclass and override <see cref="InitGame"/> for game-specific setup.
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private float readyDuration = 1f;

        protected virtual void OnEnable()
        {
            GameStateManager.OnGameStateChanged += HandleStateChanged;

            // Scene loaded while state is already Init — begin init flow immediately.
            if (GameStateManager.CurrentState == GameState.Init)
                StartCoroutine(InitGame());
        }

        protected virtual void OnDisable()
        {
            GameStateManager.OnGameStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState current, GameState last, object data)
        {
            if (current == GameState.Init)    StartCoroutine(InitGame());
            if (current == GameState.Ready)   StartCoroutine(Countdown());
            if (current == GameState.Restart) StartCoroutine(InitGame());
        }

        /// <summary>
        /// Override to initialize game-specific systems (spawn enemies, load level data, etc.).
        /// Base implementation finishes immediately and transitions to Ready.
        /// </summary>
        protected virtual IEnumerator InitGame()
        {
            yield return null; // Replace with actual async setup
            GameStateManager.Ready();
        }

        private IEnumerator Countdown()
        {
            yield return new WaitForSeconds(readyDuration);
            GameStateManager.Play();
        }
    }
}
