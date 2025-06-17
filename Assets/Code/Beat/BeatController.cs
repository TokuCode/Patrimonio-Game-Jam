using UnityEngine;
using UnityEngine.Events;
using Movement3D.Helpers;
using Movement3D.Audio;

namespace Movement3D.Beat
{
    public class BeatController : Singleton<BeatController>
    {
        [SerializeField] private float bpm;
        [SerializeField] private Intervals[] intervals;
        [SerializeField] private float hitWindow = .15f; // 150ms window
        
        private AudioSource _audioSource;

        private void Start() => _audioSource = AudioManager.Instance.GetAudio("Tambor").source;

        private void Update()
        {
            foreach (var interval in intervals)
            {
                float sampledTime = _audioSource.timeSamples / (_audioSource.clip.frequency * interval.GetIntervalLength(bpm));
                interval.CheckForNewInterval(sampledTime);
            }
        }

        public bool WasOnBeat(float hitSteps = .26f)
        {
            int sampleRate = _audioSource.clip.frequency;
            int currentSample = _audioSource.timeSamples;

            float beatIntervalSeconds = 60f / (bpm * hitSteps);
            int samplesPerBeat = Mathf.RoundToInt(beatIntervalSeconds * sampleRate);

            int nearestBeatSample = Mathf.RoundToInt((float)currentSample / samplesPerBeat) * samplesPerBeat;
            int sampleHitWindow = Mathf.RoundToInt(hitWindow * sampleRate);

            if (Mathf.Abs(currentSample - nearestBeatSample) <= sampleHitWindow)
                return true;
            
            return false;
        }
    }

    [System.Serializable]
    public class Intervals
    {
        [SerializeField] private float steps;
        [SerializeField] private UnityEvent trigger;
        private int _lastInterval;

        public float GetIntervalLength(float bpm)
        {
            return 60f / (bpm * steps);
        }

        public void CheckForNewInterval(float interval)
        {
            if (Mathf.FloorToInt(interval) != _lastInterval)
            {
                _lastInterval = Mathf.FloorToInt(interval);
                trigger.Invoke();
            }
        }
    }
}