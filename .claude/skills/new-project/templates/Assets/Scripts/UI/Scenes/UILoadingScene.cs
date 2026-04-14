using UnityEngine;
using UnityEngine.UI;
using TMPro;
using {{PROJECT_NAMESPACE}}.GameFoundation.UI;

namespace {{PROJECT_NAMESPACE}}.UI.Scenes
{
    public class UILoadingScene : UISceneBase
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text statusLabel;

        protected override void OnShow(object data)
        {
            if (statusLabel != null) statusLabel.text = "Loading...";
            if (progressBar != null) progressBar.value = 0f;
        }

        public void SetProgress(float value01, string status = null)
        {
            if (progressBar != null) progressBar.value = Mathf.Clamp01(value01);
            if (status != null && statusLabel != null) statusLabel.text = status;
        }
    }
}
