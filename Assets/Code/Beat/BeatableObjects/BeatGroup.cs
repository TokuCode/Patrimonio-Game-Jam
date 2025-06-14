using UnityEngine;
using System.Collections.Generic;

namespace Movement3D.Beat
{
    public class BeatGroup : MonoBehaviour
    {
        private List<IBeatReactable> beatObjects = new();

        private void Awake()
        {
            foreach (Transform child in transform)
            {
                IBeatReactable beatReactable = child.GetComponent<IBeatReactable>();
                if (beatReactable != null)
                {
                    beatObjects.Add(beatReactable);
                }
            }
        }

        public void TriggerBeat()
        {
            foreach (var obj in beatObjects)
            {
                obj.OnBeat();
            }
        }
    }
}