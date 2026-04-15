using UnityEngine;
using UnityEngine.UI;
using TMPro;
using {{PROJECT_NAMESPACE}}.GameFoundation.UI;

namespace {{PROJECT_NAMESPACE}}.UI.Scenes
{
    public class UILoadingScene : UIBase 
    {
        public override UILayer Layer =>  UILayer.Loading;

        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text statusLabel;

        protected override void OnShow(object data)
        {
            if (statusLabel != null) statusLabel.text = "Loading...";
            if (progressBar != null) progressBar.value = 0f;
        }
        public void SetProgress(float progressPercentage, string status)
        {
            if (progressBar != null) progressBar.value = Mathf.Clamp01(progressPercentage);
            if (status != null && statusLabel != null) statusLabel.text = status;
        }
    }
}
