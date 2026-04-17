using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace {{PROJECT_NAMESPACE}}.GameFoundation
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicManager : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource = null;
        [SerializeField] AudioClip defaultClip = null;
        [SerializeField] Slider volumeSlider = null;

        [Header("Options")]
        [SerializeField] Toggle musicToogle = null;

        [SerializeField] float maxVolume = 0.75f;

        protected static float volume = 0.75f;
        public static float Volume
        {
            get
            {
                return volume;
            }
            private set
            {
                if (value != volume)
                {
                    volume = value;
                    PlayerPrefs.SetFloat("MusicManager", volume);
                    PlayerPrefs.Save();
                    OnVolumeChanged?.Invoke(volume);
                }
            }
        }

        protected static AudioClip currrentClip = null;

        public static float LastTime;
        public static float CurrentTime
        {
            get
            {
                if (instance && instance.audioSource)
                {
                    return instance.audioSource.time;
                }
                return 0;
            }
        }

        public delegate void VolumeChangedDelegate(float volume);
        public static event VolumeChangedDelegate OnVolumeChanged;

        public static bool IsPlaying
        {
            get
            {
                if (instance != null && instance.audioSource != null)
                    return instance.audioSource.isPlaying;
                return false;
            }
        }

        private static MusicManager instance = null;

        public static Transform Transform
        {
            get
            {
                return instance.transform;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            Volume = PlayerPrefs.GetFloat("MusicManager", maxVolume);

            if (volumeSlider)
            {
                volumeSlider.maxValue = maxVolume;
                volumeSlider.value = Volume;

                volumeSlider.onValueChanged.AddListener((value) =>
                {
                    audioSource.volume = value;
                    Volume = value;

                    if (value == 0)
                        audioSource.Pause();
                    else if (!audioSource.isPlaying)
                        audioSource.Play();
                    else
                        audioSource.UnPause();
                });
            }
            else if (musicToogle)
            {
                if (Volume == 0)
                    musicToogle.isOn = false;
                else
                    musicToogle.isOn = true;

                musicToogle.onValueChanged.AddListener((s) =>
                {
                    Volume = s ? maxVolume : 0;
                });
            }
            else
            {
                Debug.LogWarning("[MusicManager] volumeSlider or musicToggle NOT FOUND!");
            }

            if (defaultClip != null)
                Init(defaultClip);
        }

        public static void Play(string fileName, float fadeTime = 0.25f, bool loop = true)
        {
            var clip = Resources.Load(fileName, typeof(AudioClip)) as AudioClip;
            if (clip != null)
            {
                clip.name = fileName;
                Init(clip, fadeTime, loop);
            }
        }

        public static void Init(AudioClip clip, float fadeTime = 1.5f, bool loop = true)
        {
            if (instance != null)
            {
                instance.audioSource.DOKill();

                if (currrentClip == null)
                {
                    currrentClip = clip;
                    instance.audioSource.clip = currrentClip;
                    Play(0, fadeTime, loop);
                }
                else
                {
                    if (clip.name == currrentClip.name)
                    {
                        FadeIn(fadeTime);
                    }
                    else
                    {
                        Stop(0.55f, false, () =>
                        {
                            Resources.UnloadUnusedAssets();
                            currrentClip.UnloadAudioData();
                            currrentClip = null;
                            Init(clip, fadeTime, loop);
                        });
                    }
                }
            }
        }

        public static void Play(float time = 0, float fadeTime = 0.5f, bool loop = true)
        {
            if (instance != null && instance.audioSource != null)
            {
                instance.audioSource.DOKill(true);
                instance.audioSource.time = time;
                instance.audioSource.loop = loop;
                instance.audioSource.volume = 0;
                instance.audioSource.pitch = 1;
                FadeIn(fadeTime);
            }
        }

        public static void Pause()
        {
            if (instance != null && instance.audioSource != null)
            {
                instance.audioSource.Pause();
            }
        }

        public static void UnPause(float fadeTime = 0.125f)
        {
            if (instance != null && instance.audioSource != null)
            {
                instance.audioSource.UnPause();
            }
        }

        public static void Stop(float fadeTime = 0.25f, bool pitch = false, Action onDone = null)
        {
            if (instance != null && instance.audioSource != null)
            {
                instance.audioSource.DOKill(true);
                instance.audioSource.UnPause();
                instance.audioSource.DOFade(0, fadeTime)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(UpdateType.Normal, true)
                    .OnUpdate(() =>
                    {
                        if (pitch)
                            instance.audioSource.pitch = instance.audioSource.volume;
                    })
                    .OnComplete(() =>
                    {
                        instance.audioSource.Stop();
                        instance.audioSource.pitch = 1;
                        onDone?.Invoke();
                    });
            }
        }

        public static void FadeIn(float fadeTime = 0.5f, Action onDone = null)
        {
            if (instance != null && instance.audioSource != null && instance.audioSource.isPlaying == false)
            {
                instance.audioSource.DOKill(true);
                instance.audioSource.volume = 0;
                instance.audioSource.pitch = 1;
                instance.audioSource.Play();
                instance.audioSource.DOFade(Volume, fadeTime)
                        .SetEase(Ease.InCubic)
                        .SetUpdate(UpdateType.Normal, true)
                        .OnComplete(() =>
                        {
                            onDone?.Invoke();
                        });
            }
        }

        public static void FadeOut(float fadeTime = 0.25f, Action onDone = null)
        {
            if (instance != null && instance.audioSource != null)
            {
                instance.audioSource.DOKill(true);
                instance.audioSource.DOFade(0, fadeTime)
                        .SetEase(Ease.OutCubic)
                        .SetUpdate(UpdateType.Normal, true)
                        .OnComplete(() =>
                        {
                            onDone?.Invoke();
                        });
            }
        }

        public static void SetPitch(float pitch, float fadeTime = 0.5f)
        {
            if (instance != null && instance.audioSource != null)
            {
                instance.audioSource.DOKill(true);
                instance.audioSource.DOPitch(pitch, fadeTime)
                    .SetEase(Ease.InOutCirc)
                    .SetUpdate(UpdateType.Normal, true);
            }
        }

        public static void ScaleVolume(float value, float duration = 1.5f)
        {
            if (instance != null && instance.audioSource != null)
            {
                var nextVolume = Volume * value;
                instance.audioSource.DOKill(true);
                instance.audioSource.DOFade(nextVolume, duration);
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("{{PROJECT_NAMESPACE}}/GameFoundation/Music Manager/Create Music Manager")]
        public static void CreateMusicManager()
        {
            if (FindAnyObjectByType<MusicManager>() != null)
            {
                Debug.LogWarning("Music Manager already exists in the scene!");
                return;
            }
            string[] guids = UnityEditor.AssetDatabase.FindAssets("MusicManager t:prefab");
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
                var obj = new GameObject("MusicManager");
                obj.AddComponent<MusicManager>();
                string path = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.MonoScript.FromMonoBehaviour(obj.GetComponent<MusicManager>()));
                path = path.Replace("MusicManager.cs", "MusicManager.prefab");
                var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, path);
                if (prefab != null)
                {
                    DestroyImmediate(obj);
                    UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                    Debug.Log("Music Manager prefab created at: " + path);
                }
                else
                {
                    Debug.LogError("Failed to create Music Manager prefab at: " + path);
                }
            }
        }
#endif
    }
}
