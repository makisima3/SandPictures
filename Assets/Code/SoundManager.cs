using UnityEngine;

namespace Code
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        
        [SerializeField] private AudioClip audioClip;

        private AudioSource _audioSource;
        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();

            Instance = this;
        }

        public void PlaySound()
        {
            _audioSource.PlayOneShot(audioClip);
        }

        public void StartPlay()
        {
            _audioSource.clip = audioClip;
            _audioSource.loop = true;
            _audioSource.Play();
        }
        
        public void StopPlay()
        {
            _audioSource.Pause();
        }

    }
}