namespace {{PROJECT_NAMESPACE}}.GameFoundation.UI
{
    public abstract class UILoadingBase : UIBase
    {
        public override UILayer Layer => UILayer.Loading;

        public void Show(object data = null) => ShowInternal(data);
        public void Hide() => HideInternal();

        public abstract void SetProgress(float value01);
    }
}
