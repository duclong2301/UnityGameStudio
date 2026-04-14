using System;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.UI
{
    public enum UIAnimStatus { Initial, Hide, Hiding, Showing, Show }

    public interface IUIAnim
    {
        UIAnimStatus Status { get; }
        bool IsAnimating { get; }
        void Show(Action onStart = null, Action onDone = null);
        void Hide(Action onDone = null);
        event Action<UIAnimStatus> OnStatusChanged;
    }
}
