using System.Collections.Generic;
using UnityEngine;

namespace Traps.TrapUsageData
{
    public class TrapStatsManager : MonoBehaviour
    {
        public static TrapStatsManager Instance;

        private Dictionary<TrapType, int> triggeredCounts = new Dictionary<TrapType, int>();
        private Dictionary<TrapType, int> interactedCounts = new Dictionary<TrapType, int>();
        private Dictionary<TrapType, int> playerHitCounts = new Dictionary<TrapType, int>();
        private Dictionary<TrapType, int> enemyHitCounts = new Dictionary<TrapType, int>();
        private Dictionary<TrapType, int> playerDamageCounts = new Dictionary<TrapType, int>();
        private Dictionary<TrapType, int> enemyDamageCounts = new Dictionary<TrapType, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void RecordTrapEvent(TrapType trapType, TrapEventType eventType)
        {
            switch (eventType)
            {
                case TrapEventType.Triggered:
                    AddCount(triggeredCounts, trapType);
                    break;

                case TrapEventType.Interacted:
                    AddCount(interactedCounts, trapType);
                    break;

                case TrapEventType.HitPlayer:
                    AddCount(playerHitCounts, trapType);
                    break;

                case TrapEventType.HitEnemy:
                    AddCount(enemyHitCounts, trapType);
                    break;

                case TrapEventType.DamagedPlayer:
                    AddCount(playerDamageCounts, trapType);
                    break;

                case TrapEventType.DamagedEnemy:
                    AddCount(enemyDamageCounts, trapType);
                    break;
            }

            Debug.Log("Trap Data: " + trapType + " recorded " + eventType);
        }

        private void AddCount(Dictionary<TrapType, int> dictionary, TrapType trapType)
        {
            if (!dictionary.ContainsKey(trapType))
            {
                dictionary[trapType] = 0;
            }

            dictionary[trapType]++;
        }

        public int GetTriggeredCount(TrapType trapType)
        {
            return triggeredCounts.ContainsKey(trapType) ? triggeredCounts[trapType] : 0;
        }

        public int GetInteractedCount(TrapType trapType)
        {
            return interactedCounts.ContainsKey(trapType) ? interactedCounts[trapType] : 0;
        }

        public int GetPlayerHitCount(TrapType trapType)
        {
            return playerHitCounts.ContainsKey(trapType) ? playerHitCounts[trapType] : 0;
        }

        public int GetEnemyHitCount(TrapType trapType)
        {
            return enemyHitCounts.ContainsKey(trapType) ? enemyHitCounts[trapType] : 0;
        }

        public int GetPlayerDamageCount(TrapType trapType)
        {
            return playerDamageCounts.ContainsKey(trapType) ? playerDamageCounts[trapType] : 0;
        }

        public int GetEnemyDamageCount(TrapType trapType)
        {
            return enemyDamageCounts.ContainsKey(trapType) ? enemyDamageCounts[trapType] : 0;
        }

        public void ResetRoundStats()
        {
            triggeredCounts.Clear();
            interactedCounts.Clear();
            playerHitCounts.Clear();
            enemyHitCounts.Clear();
            playerDamageCounts.Clear();
            enemyDamageCounts.Clear();
        }
    }
}