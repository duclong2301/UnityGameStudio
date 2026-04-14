using System.Collections;
using UnityEngine;
using {{PROJECT_NAMESPACE}}.GameFoundation.StateMachine;

namespace {{PROJECT_NAMESPACE}}.Gameplay
{
    /// <summary>
    /// Drives the Gameplay scene's state flow: enter Ready → wait 1s → transition to Play.
    /// UI reacts via <see cref="GameStateManager.OnGameStateChanged"/>.
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private float readyDuration = 1f;

        private IEnumerator Start()
        {
            GameStateManager.Ready();
            yield return new WaitForSeconds(readyDuration);
            GameStateManager.Play();
        }
    }
}
