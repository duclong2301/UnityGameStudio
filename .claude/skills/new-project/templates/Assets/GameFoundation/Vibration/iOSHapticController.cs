#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace {{PROJECT_NAMESPACE}}.GameFoundation
{
    // Internal class — use HapticFeedback as the public API.
    internal static class iOSHapticController
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _playSelectionHaptic();

        [DllImport("__Internal")]
        private static extern void _playNotificationHaptic(int type);

        [DllImport("__Internal")]
        private static extern void _playImpactHaptic(int style);
#endif

        public static void PlayImpact(HapticFeedback.ImpactStyle style)
        {
#if UNITY_IOS
            try
            {
                switch (style)
                {
                    case HapticFeedback.ImpactStyle.Light:
                    case HapticFeedback.ImpactStyle.Soft:
                        _playSelectionHaptic();
                        break;
                    case HapticFeedback.ImpactStyle.Medium:
                        _playImpactHaptic((int)HapticFeedback.ImpactStyle.Medium);
                        break;
                    case HapticFeedback.ImpactStyle.Heavy:
                        _playImpactHaptic((int)HapticFeedback.ImpactStyle.Heavy);
                        break;
                    case HapticFeedback.ImpactStyle.Rigid:
                        _playImpactHaptic((int)HapticFeedback.ImpactStyle.Rigid);
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[iOSHapticController] Error playing haptic. Style: {style}. Is the native plugin (iOSHaptics.mm) included? Exception: {e}");
            }
#endif
        }
    }
}
