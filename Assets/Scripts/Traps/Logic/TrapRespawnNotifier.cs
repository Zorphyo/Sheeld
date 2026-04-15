using UnityEngine;

namespace Traps.Logic
{
    public class TrapRespawnNotifier : MonoBehaviour
    {
        
        private ArenaTrapSpawner spawner;
        private ArenaTrapSpawner.TrapDefinition trapDefinition;
        private GameObject trapInstance;
        private bool hasNotified = false;

        public void Initialize(ArenaTrapSpawner spawner,
            ArenaTrapSpawner.TrapDefinition trapDefinition,
            GameObject trapInstance)
        {
            this.spawner = spawner;
            this.trapDefinition = trapDefinition;
            this.trapInstance = trapInstance;
        }

        private void OnDestroy()
        {
            if (hasNotified)
                return;

            if (spawner == null)
                return;

            hasNotified = true;
            spawner.NotifyTrapDestroyed(trapDefinition, trapInstance);
        }
    }
}