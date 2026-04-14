using UnityEngine;
using UnityEngine.UI;
using {{PROJECT_NAMESPACE}}.GameFoundation.UI;

namespace {{PROJECT_NAMESPACE}}.UI.Popups
{
    public class PopupSettings : UIPopupBase
    {
        [SerializeField] private Button closeButton;

        protected override void OnInitialize()
        {
            if (closeButton != null) closeButton.onClick.AddListener(() => Hide());
        }
    }
}
