using UnityEngine;
using UnityEngine.Events;

namespace Movement3D.Beat
{
    public class BeatController : MonoBehaviour
    {
        [SerializeField] private float bpm;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Intervals[] intervals;
        //[SerializeField] private float hitSteps;

        private void Update()
        {
            foreach (var interval in intervals)
            {
                float sampledTime = audioSource.timeSamples / (audioSource.clip.frequency * interval.GetIntervalLength(bpm));
                interval.CheckForNewInterval(sampledTime);
            }
        
            /*
            if (Input.GetKeyDown(KeyCode.Space))
            {
                int sampleRate = audioSource.clip.frequency;
                int currentSample = audioSource.timeSamples;

                float beatIntervalSeconds = 60f / (bpm * 1f);
                int samplesPerBeat = Mathf.RoundToInt(beatIntervalSeconds * sampleRate);

                int nearestBeatSample = Mathf.RoundToInt((float)currentSample / samplesPerBeat) * samplesPerBeat;
                int sampleHitWindow = Mathf.RoundToInt(0.15f * sampleRate); // 150ms window

                if (Mathf.Abs(currentSample - nearestBeatSample) <= sampleHitWindow)
                {
                    Debug.Log("Hit!");
                }
                else
                {
                    Debug.Log("Miss!");
                }
            }*/
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