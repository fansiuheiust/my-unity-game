using UnityEngine;

namespace Combat.Miniboss {
    public class SwarmEgg : Projectile {
        [SerializeField]
        GameObject swarmPrefab;
        
        System.Func<GameObject, GameObject> spawner;

        public void SetSpawner(System.Func<GameObject, GameObject> spawner) {
            this.spawner = spawner;
        }

        public override void Delete() {
            GameObject spawn = spawner(swarmPrefab);
            spawn.transform.position = transform.position;
            base.Delete();
        }
    }
}