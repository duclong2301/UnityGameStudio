using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace {{PROJECT_NAMESPACE}}.GameFoundation
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] protected bool loadAllSoundsAtStart = false;
        [SerializeField][Tooltip("Add sound to Resources/Sounds")] protected string soundPath = "Sounds";

        [SerializeField] protected AudioSource audioSource = null;
        [SerializeField] protected AudioClip defaultClip;

        [SerializeField] AudioMixerGroup mixerGroup = null;

        protected static float volume = 1f;
        public static float Volume
        {
            get
            {
                return volume;
            }
            private set
            {
                if (volume != value)
                {
                    volume = value;
                    PlayerPrefs.SetFloat("SoundManager", volume);
                    PlayerPrefs.Save();
                    if (instance && instance.defaultClip)
                    {
                        instance.audioSource.volume = volume;
                        instance.SetAudioMixerVolume(volume);
                        if (instance.audioSource.isPlaying == false)
                            instance.audioSource.Play();
                    }
                }
            }
        }

        public delegate void VolumeChangedDelegate(float volume);
        public static event VolumeChangedDelegate OnVolumeChanged = null;

        [SerializeField]
        private Slider soundSlider = null;

        [Header("Options")]
        [SerializeField]
        private Toggle soundToogle = null;

        public float maxVolume = 1.0f;

        private static Dictionary<string, AudioClip> allSounds = new Dictionary<string, AudioClip>();

        [SerializeField]
        private int soundObjTempList = 10;

        Transform parrentTransform = null;

        protected static SoundManager instance { get; set; }

        public static string TAG
        {
            get
            {
                if (instance != null)
                    return "[" + instance.GetType().Name + "] ";
                return "";
            }
        }

        public void Awake()
        {
            instance = this;
            parrentTransform = transform;
        }

        private void Start()
        {
            if (loadAllSoundsAtStart)
            {
                LoadAllSounds();
                InitSoundObjTempList();
            }

            Volume = PlayerPrefs.GetFloat(name, maxVolume);

            if (soundSlider)
            {
                soundSlider.maxValue = maxVolume;
                soundSlider.value = Volume;

                soundSlider.onValueChanged.AddListener((value) =>
                {
                    Volume = value;
                });
            }
            else if (soundToogle)
            {
                if (Volume == 0)
                    soundToogle.isOn = false;
                else
                    soundToogle.isOn = true;

                soundToogle.onValueChanged.AddListener((s) =>
                {
                    Volume = s ? maxVolume : 0;
                });
            }
            else
            {
                Debug.LogWarning("[SoundManager] volumeSlider or soundToggle NOT FOUND!");
            }

            if (defaultClip)
            {
                audioSource.clip = defaultClip;
                audioSource.volume = Volume;
            }
        }

        void SetAudioMixerVolume(float volume)
        {
            if (mixerGroup != null)
            {
                float val = Mathf.Max(0.0001f, Mathf.Log10(volume) * 20);
                mixerGroup.audioMixer.SetFloat("Volume", val);
            }
        }

        public void ToggleSound(bool isOn)
        {
            if (isOn)
                Play("sfx_click");
        }

        public static void LoadAllSounds(string path = "")
        {
            if (string.IsNullOrEmpty(path) && instance)
                path = instance.soundPath;

            var resources = Resources.LoadAll<AudioClip>(path);
            if (resources != null)
            {
                foreach (var i in resources)
                {
                    if (!allSounds.ContainsKey(i.name))
                        allSounds.Add(i.name, i);
                }
                resources = null;
            }
            else
            {
                Debug.LogError("[LoadAllSounds] " + path + " is not correct!?");
            }
        }

        public void SetPlay(string fileName)
        {
            Play(fileName);
        }

        public void SetPlay(AudioClip audioClip)
        {
            PlayClip(audioClip);
        }

        public static void Play(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && instance && Volume > 0)
            {
                if (!allSounds.ContainsKey(fileName))
                {
                    var sound = Resources.Load<AudioClip>(instance.soundPath + "/" + fileName);
                    if (sound != null && !allSounds.ContainsKey(fileName))
                        allSounds.Add(fileName, sound);
                }

                if (allSounds.ContainsKey(fileName))
                    PlayTemp(allSounds[fileName]);
                else
                    Debug.LogWarning(TAG + " There is no sound file with the name [" + fileName + "] in any of the Resources folders.\n Check that the spelling of the fileName (without the extension) is correct or if the file exists under a Resources folder");
            }
        }

        public static void PlayClip(AudioClip clip, bool setPos = false, Vector3 pos = new Vector3(), float _maxDistance = 500, float _minDistance = 1)
        {
            var tempGO = new SoundObj("TempAudio - " + clip.name, instance?.parrentTransform);
            if (setPos)
                tempGO.gameObject.transform.position = pos;
            tempGO.aSource.maxDistance = _maxDistance;
            tempGO.aSource.minDistance = _minDistance;
            tempGO.aSource.PlayOneShot(clip, Volume);
            Destroy(tempGO.gameObject, clip.length);
        }

        private static List<SoundObj> soundObjList = new List<SoundObj>();

        public void InitSoundObjTempList()
        {
            for (int i = 0; i < soundObjTempList; i++)
            {
                soundObjList.Add(new SoundObj("SoundObj", parrentTransform));
            }
        }

        public static void PlayTemp(AudioClip clip, bool setPos = false, Vector3 pos = new Vector3())
        {
            var check = soundObjList.FirstOrDefault(x => x.aSource.clip == clip && x.aSource.isPlaying == false);
            if (check == null)
                check = soundObjList.FirstOrDefault(x => x.aSource.isPlaying == false);
            if (check != null)
            {
                if (setPos)
                    check.gameObject.transform.position = pos;
                check.aSource.PlayOneShot(clip, Volume);
            }
            else
            {
                PlayClip(clip);
            }
        }

        public class SoundObj
        {
            public GameObject gameObject;
            public AudioSource aSource;

            public SoundObj(string name, Transform parent = null)
            {
                gameObject = new GameObject(name);
                if (parent)
                    gameObject.transform.parent = parent;
                aSource = gameObject.AddComponent<AudioSource>();
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("{{PROJECT_NAMESPACE}}/GameFoundation/Sound Manager/Create Sound Manager")]
        public static void CreateSoundManager()
        {
            if (FindAnyObjectByType<SoundManager>() != null)
            {
                Debug.LogWarning("Sound Manager already exists in the scene!");
                return;
            }
            string[] guids = UnityEditor.AssetDatabase.FindAssets("SoundManager t:prefab");
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
                var obj = new GameObject("SoundManager");
                obj.AddComponent<SoundManager>();
                string path = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.MonoScript.FromMonoBehaviour(obj.GetComponent<SoundManager>()));
                path = path.Replace("SoundManager.cs", "SoundManager.prefab");
                var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, path);
                if (prefab != null)
                {
                    DestroyImmediate(obj);
                    UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                    Debug.Log("Sound Manager prefab created at: " + path);
                }
                else
                {
                    Debug.LogError("Failed to create Sound Manager prefab at: " + path);
                }
            }
        }
#endif
    }
}
