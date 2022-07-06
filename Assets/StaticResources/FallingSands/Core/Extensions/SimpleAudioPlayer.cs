namespace CorgiFallingSands
{
    using UnityEngine;

    public class SimpleAudioPlayer : MonoBehaviour
    {
        // custom
        public AudioClip AudioClearScreen;
        public AudioClip AudioSelectMaterial;
        public AudioClip AudioGrabKnob;

        // internal 
        public AudioSource[] AudioSourcesBurst;
        [System.NonSerialized] private int _audioBurstIndex = 0;

        private AudioSource GetFreeAudioBurst()
        {
            var audioSource = AudioSourcesBurst[_audioBurstIndex];

            _audioBurstIndex += 1;
            if (_audioBurstIndex >= AudioSourcesBurst.Length)
            {
                _audioBurstIndex = 0;
            }

            return audioSource;
        }

        public void PlaySoundBurst(AudioClip clip, float volume = 1f)
        {
            var audioSource = GetFreeAudioBurst();
            audioSource.Stop();

            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.volume = volume;

            audioSource.Play();
        }
    }
}