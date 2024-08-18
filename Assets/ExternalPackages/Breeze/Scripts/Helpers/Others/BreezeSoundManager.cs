using System.Linq;
using UnityEngine;

namespace Breeze.Core
{
    public class BreezeSoundManager : MonoBehaviour
    {
        public static BreezeSoundManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public static void TriggerSound(GameObject source, bool IsPlayer, BreezeEnums.Factions[] affectedFactions)
        {
            var systems = FindObjectsOfType<BreezeSystem>();
            foreach (var system in systems)
            {
                if (affectedFactions.Contains(system.CurrentAIFaction))
                {
                    system.NotifySound(source, IsPlayer);
                }
            }
        }
    }
}