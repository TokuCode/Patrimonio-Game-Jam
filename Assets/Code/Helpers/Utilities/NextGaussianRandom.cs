using UnityEngine;

namespace Movement3D.Helpers
{
    public static class NextGaussianRandom
    {
        public static float NextGaussian()
        {
            float u, v, S;

            do
            {
                u = 2 * Random.Range(0, 1f) - 1;
                v = 2 * Random.Range(0, 1f) - 1;
                S = u * u + v * v;
            }
            while (S >= 1.0);

            float fac = Mathf.Sqrt(-2 * Mathf.Log(S) / S);
            return Mathf.Clamp01(u * fac);
        } 
    }
}