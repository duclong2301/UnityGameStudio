using UnityEngine;

namespace {{PROJECT_NAMESPACE}}.GameFoundation
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundFX : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource = null;
        [SerializeField] AudioClip audioClip = null;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if (audioClip == null)
                audioClip = audioSource.clip;
        }

        public void Play()
        {
            if (SoundManager.Volume > 0 && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(audioClip, SoundManager.Volume);
            }
        }
    }
}
