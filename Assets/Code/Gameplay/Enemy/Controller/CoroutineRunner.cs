using System.Collections;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class CoroutineRunner : MonoBehaviour
    {
        public void Run(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }

        public void StopAll()
        {
            StopAllCoroutines();
        }
    }
}