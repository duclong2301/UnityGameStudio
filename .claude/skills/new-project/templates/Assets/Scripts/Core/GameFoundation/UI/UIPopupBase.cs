using System;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.UI
{
    /// <summary>
    /// Modal popup layer. Can be called from anywhere via <see cref="UIManager.ShowPopup{T}"/>.
    /// </summary>
    public abstract class UIPopupBase : UIBase
    {
        public override UILayer Layer => UILayer.Popup;

        public void Show(object data = null) => ShowInternal(data);
        public void Hide(Action onDone = null) => HideInternal(onDone);
    }
}
