using UnityEngine;
using Movement3D.Helpers;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;

namespace Movement3D.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private List<AudioInfo> audios;

        protected override void Awake()
        {
            base.Awake();

            foreach (var audioInfo in audios)
            {
                audioInfo.source = gameObject.AddComponent<AudioSource>();
                audioInfo.source.loop = audioInfo.loop;
                audioInfo.source.clip = audioInfo.clip;
                audioInfo.source.volume = audioInfo.volume;
                audioInfo.source.pitch = audioInfo.pitch;
                audioInfo.source.outputAudioMixerGroup = audioInfo.mixer;

                audioInfo.source.playOnAwake = false;
                if (audioInfo.playOnAwake) audioInfo.source.Play();
            }
        }

        public AudioInfo GetAudio(string audioName)
        {
            foreach (var audioInfo in audios)
            {
                if (audioInfo.name == audioName) 
                    return audioInfo;
            }
            Debug.LogWarning($"No audio found with name {audioName}");
            return null;
        }

        public void Play(string audioName)
        {
            var audioInfo = GetAudio(audioName);
            audioInfo?.source.Play();
        }

        public void Stop(string audioName)
        {
            var audioInfo = GetAudio(audioName);
            audioInfo?.source.Stop();
        }
        
        public void Pause(string audioName)
        {
            var audioInfo = GetAudio(audioName);
            audioInfo?.source.Pause();
        }

        public void UnPause(string audioName)
        {
            var audioInfo = GetAudio(audioName);
            audioInfo?.source.UnPause();
        }
    }
}