using System;
using UnityEngine;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.UI
{
    /// <summary>
    /// Abstract base for every UI element. Delegates animation to an optional
    /// <see cref="IUIAnim"/> component on the same GameObject. No animator → SetActive fallback.
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        public abstract UILayer Layer { get; }
        public bool IsVisible { get; private set; }

        private IUIAnim _anim;
        private bool _animResolved;

        protected virtual void Awake() { OnInitialize(); }
        protected virtual void OnDestroy() { OnDispose(); }

        protected virtual void OnInitialize() { }
        protected virtual void OnDispose() { }
        protected virtual void OnShow(object data) { }
        protected virtual void OnHide() { }

        private IUIAnim ResolveAnim()
        {
            if (_animResolved) return _anim;
            _anim = GetComponent<IUIAnim>();
            _animResolved = true;
            return _anim;
        }

        internal void ShowInternal(object data = null)
        {
            if (IsVisible) return;
            IsVisible = true;
            gameObject.SetActive(true);
            OnShow(data);

            var anim = ResolveAnim();
            if (anim != null) anim.Show();
        }

        internal void HideInternal(Action onDone = null)
        {
            if (!IsVisible) { onDone?.Invoke(); return; }
            IsVisible = false;
            OnHide();

            var anim = ResolveAnim();
            if (anim != null)
            {
                anim.Hide(() => { gameObject.SetActive(false); onDone?.Invoke(); });
            }
            else
            {
                gameObject.SetActive(false);
                onDone?.Invoke();
            }
        }
    }
}
