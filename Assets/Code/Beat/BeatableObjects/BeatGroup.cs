using UnityEngine;
using System.Collections.Generic;

namespace Movement3D.Beat
{
    public class BeatGroup : MonoBehaviour
    {
        [SerializeField] private List<GameObject> beatObjects;

        public void TriggerBeat()
        {
            foreach (var obj in beatObjects)
            {
                if (obj.GetComponent<IBeatReactable>() == null) return;
                obj.GetComponent<IBeatReactable>().OnBeat();
            }
        }
    }
}