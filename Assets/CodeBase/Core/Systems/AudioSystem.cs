using System;
using CodeBase.Core.Systems.Save;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace CodeBase.Core.Systems
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSystem : MonoBehaviour, ISerializableDataSystem
    {
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioClip mainMelodyClip;
        [SerializeField] private AudioClip gameMelodyClip;
        [Header("Fade Parameters")]
        [SerializeField] private float fadeDuration = 1.0f;
        [Inject] private ISaveSystem _saveSystem;
        private float _musicVolume;

        public float MusicVolume
        {
            get => _musicVolume;
            private set => _musicVolume = value < 0 ? 0 : value;
        }

        public float SoundsVolume { get; private set; }

        public event Action<float> OnSoundsVolumeChanged;

        private void Start()
        {
            if (_saveSystem != null)
                _saveSystem.AddSystem(this);
        }

        private void OnDestroy()
        {
            // Kill all DOTween tweens on AudioSource to prevent WebGL errors during scene unload
            if (musicAudioSource != null)
                musicAudioSource.DOKill();
        }

        public void PlayGameMelody() => PlayMusic(gameMelodyClip);
        
        public void PlayMainMenuMelody() => PlayMusic(mainMelodyClip);

        private void PlayMusic(AudioClip music)
        {
            if (music == null)
            {
                Debug.LogWarning("[AudioSystem] Trying to play null AudioClip");
                return;
            }
            
            if (musicAudioSource == null)
            {
                Debug.LogWarning("[AudioSystem] musicAudioSource is null");
                return;
            }
            
            // In WebGL, AudioClip loading is asynchronous
            // If not loaded, request loading but don't play yet to avoid errors
            if (music.loadState != AudioDataLoadState.Loaded)
            {
                Debug.LogWarning($"[AudioSystem] AudioClip {music.name} is not loaded yet (loadState: {music.loadState}). Skipping playback for WebGL safety.");
                
                // Request loading for next time, but don't play now
                if (music.loadState == AudioDataLoadState.Unloaded)
                {
                    music.LoadAudioData();
                }
                
                return;
            }
            
            musicAudioSource.clip = music;
            FadeIn(musicAudioSource, MusicVolume, fadeDuration);
        }
        
    public void StopMusic()
    {
        if (musicAudioSource != null)
        {
            // Kill any running tweens before stopping to prevent WebGL errors
            musicAudioSource.DOKill();
            
            if (musicAudioSource.isPlaying)
                musicAudioSource.Stop();
        }
    }

        public void SetMusicVolume(float volume)
        {
            Debug.Log("Set MusicVolume - " + volume);
            MusicVolume = volume > 0 ? volume : 0;
            musicAudioSource.volume = MusicVolume;
        }

        public void SetSoundsVolume(float volume)
        {
            SoundsVolume = volume > 0 ? volume : 0;
            OnSoundsVolumeChanged?.Invoke(SoundsVolume);
        }

        private void FadeOut(AudioSource audioSource, float duration)
        {
            if (audioSource == null)
            {
                Debug.LogWarning("[AudioSystem] FadeOut: audioSource is null");
                return;
            }
            
            // Kill any existing tweens before starting new one
            audioSource.DOKill();
                
            audioSource.DOFade(0, duration)
                .SetLink(gameObject)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (audioSource != null)
                        audioSource.Stop();
                });
        }

        private void FadeIn(AudioSource audioSource, float targetVolume, float duration)
        {
            if (audioSource == null)
            {
                Debug.LogWarning("[AudioSystem] FadeIn: audioSource is null");
                return;
            }
            
            // Kill any existing tweens before starting new one
            audioSource.DOKill();
                
            if (!audioSource.isPlaying) 
                audioSource.Play();
            
            audioSource.volume = 0;
            audioSource.DOFade(targetVolume, duration)
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        public UniTask LoadData(SerializableDataContainer dataContainer)
        {
            MusicVolume = dataContainer.TryGet(nameof(MusicVolume), out float musicVolume) ? musicVolume : 0;
            SoundsVolume = dataContainer.TryGet(nameof(SoundsVolume), out float soundsVolume) ? soundsVolume : 0;
            
            musicAudioSource.volume = MusicVolume;
            return UniTask.CompletedTask;
        }

        public void SaveData(SerializableDataContainer dataContainer)
        {      
            dataContainer.SetData(nameof(MusicVolume), MusicVolume);
            dataContainer.SetData(nameof(SoundsVolume), SoundsVolume);
        }
    }
}