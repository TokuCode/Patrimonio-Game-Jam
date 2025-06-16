using UnityEngine;
using UnityEngine.Audio;

namespace Movement3D.Audio
{
    [System.Serializable]
    public class AudioInfo
    {
        public string name;
        public AudioClip clip;
        public bool loop;
        public bool playOnAwake;
        
        [Range(0f, 1f)] public float volume;
        [Range(-3f, 3f)] public float pitch = 1f;
        [HideInInspector] public AudioSource source;
        public AudioMixerGroup mixer;
    }
}