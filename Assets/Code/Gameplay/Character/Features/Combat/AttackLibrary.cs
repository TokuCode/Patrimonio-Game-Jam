using System.Collections.Generic;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public static class AttackLibrary
    {
        private static Dictionary<string, FullAttack> _attacks;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            _attacks = new Dictionary<string, FullAttack>();
            var attacks = Resources.LoadAll<FullAttack>("");
            
            foreach (var attack in attacks)
            {
                if (string.IsNullOrWhiteSpace(attack.attackName)) continue;
                _attacks.Add(attack.attackName, attack);
            }
        }

        public static FullAttack GetAttack(string name)
        {
            if(!_attacks.TryGetValue(name, out FullAttack attack)) return null;
            return attack;
        }
    }
}