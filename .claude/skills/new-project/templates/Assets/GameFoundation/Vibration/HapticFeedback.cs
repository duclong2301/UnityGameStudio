using System;
using UnityEngine;
using UnityEngine.UI;

namespace {{PROJECT_NAMESPACE}}.GameFoundation
{
    public class HapticFeedback : MonoBehaviour
    {
        [SerializeField]
        protected Toggle hapticToggle = null;

        protected static int isOn = -1;
        public static int IsOn
        {
            get
            {
                if (isOn == -1)
                    isOn = PlayerPrefs.GetInt("HapticFeedback", 1);
                return isOn;
            }
            set
            {
                if (value != isOn)
                {
                    isOn = value;
                    PlayerPrefs.SetInt("HapticFeedback", isOn);
                    PlayerPrefs.Save();
                }
            }
        }

        protected static float vibrationDelay = 0.1f;
        protected static float lastTime;

        public static HapticFeedback instance = null;

        protected void Awake()
        {
            instance = this;
        }

        public static void CheckIsOn()
        {
            if (instance != null && instance.hapticToggle)
            {
                instance.hapticToggle.onValueChanged.AddListener(instance.OnToggleChanged);
                instance.hapticToggle.isOn = IsOn > 0;
            }
        }

        private void OnToggleChanged(bool isOn)
        {
            if (isOn)
                PlayImpact(ImpactStyle.Light);
        }

        private void OnEnable()
        {
            if (hapticToggle)
                hapticToggle.isOn = IsOn > 0;
        }

        private void Start()
        {
            lastTime = Time.time;
        }

        public enum ImpactStyle { Light = 0, Medium = 1, Heavy = 2, Rigid = 3, Soft = 4 }

        public static void PlayImpact(ImpactStyle style)
        {
#if UNITY_EDITOR
            Debug.Log($"[HapticFeedback] PlayImpact({style}) called in Editor.");
#elif UNITY_IOS
            iOSHapticController.PlayImpact(style);
#elif UNITY_ANDROID
            AndroidHapticController.PlayImpact(style);
#endif
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("{{PROJECT_NAMESPACE}}/GameFoundation/Haptic Feedback/Create Haptic Feedback")]
        public static void CreateHapticFeedback()
        {
            if (FindAnyObjectByType<HapticFeedback>() != null)
            {
                Debug.LogWarning("Haptic Feedback already exists in the scene!");
                return;
            }
            string[] guids = UnityEditor.AssetDatabase.FindAssets("HapticFeedback t:prefab");
            if (guids.Length > 0)
            {
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
                if (prefab != null)
                {
                    UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                }
            }
            else
            {
                var obj = new GameObject("HapticFeedback");
                obj.AddComponent<HapticFeedback>();
                string path = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.MonoScript.FromMonoBehaviour(obj.GetComponent<HapticFeedback>()));
                path = path.Replace("HapticFeedback.cs", "HapticFeedback.prefab");
                var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, path);
                if (prefab != null)
                {
                    DestroyImmediate(obj);
                    UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                    Debug.Log("Haptic Feedback prefab created at: " + path);
                }
                else
                {
                    Debug.LogError("Failed to create Haptic Feedback prefab at: " + path);
                }
            }
        }
#endif
    }
}
