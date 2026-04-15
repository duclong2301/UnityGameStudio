namespace {{PROJECT_NAMESPACE}}.GameFoundation.UI
{
    /// <summary>
    /// Full-screen scene-level UI. Show/Hide is driven by GameState only — do not
    /// call from gameplay code. Only UIManager may trigger transitions.
    /// </summary>
    public abstract class UISceneBase : UIBase
    {
        public override UILayer Layer => UILayer.Scene;
    }
}
