using UnityEngine;
using UnityEngine.SceneManagement;
using {{PROJECT_NAMESPACE}}.GameFoundation.Data;
using {{PROJECT_NAMESPACE}}.GameFoundation.StateMachine;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Bootstrap
{
    /// <summary>
    /// Runtime bootstrap. Ensures DataManager + GameStateManager are initialized
    /// before any gameplay scene loads.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string mainSceneName = "MainScene";
        [SerializeField] private float minLoadingSeconds = 1f;

        private void Start()
        {
            GameStateManager.Init();
            StartCoroutine(LoadMain());
        }

        private System.Collections.IEnumerator LoadMain()
        {
            yield return new WaitForSeconds(minLoadingSeconds);
            var op = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Single);
            while (!op.isDone) yield return null;
            GameStateManager.Main();
        }
    }
}
