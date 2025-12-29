using Combat;
using Loot;
using System.Collections;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Interactable {
    /// <summary>
    /// Borrowed mechanics from Trial
    /// </summary>
    public class Spawner: Trial {

        [ReadOnly, SerializeField]
        string inspectorWarnText = "Please do NOT change IsSuccessByDefault (true) or IsRedoable (true) unless absolutely necessary";
        [field: SerializeField, Min(0), Tooltip("Smallest amount of mobs spawned in a single wave")] public uint MinMobSpawn { get; private set; } = 1;
        [field: SerializeField, Min(1), Tooltip("Largest amount of mobs spawned in a single wave")] public uint MaxMobSpawn { get; private set; } = 1;
        [field: SerializeField, Min(1), Tooltip("How many surviving spawned mobs can there be at any time")] public uint MaxConcurrentMob { get; private set; } = 12;
        [field: SerializeField, Min(1), Tooltip("The radius of the circle the mobs should be spawned at")] public float MobSpawnRadius { get; private set; } = 7f;
        
        [field: SerializeField, Min(2), Tooltip("Shortest time between 2 waves of mobs")] public float MinSpawnInterval { get; private set; } = 5f;
        [field: SerializeField, Min(2), Tooltip("Longest time between 2 waves of mobs")] public float MaxSpawnInterval { get; private set; } = 7f;


        [field: SerializeField, Min(1), Tooltip("How far away can the player be when deactivating the spawner")] public float DeactivateRadius { get; private set; } = 3f;
        [field: SerializeField, Min(1), Tooltip("How close should the player be to trigger spawning")] public float SpawnTriggerRadius = 10f;

        [SerializeField] Lootpool<GameObject> mobPreset;

        Mob _trialStarter;

        uint _numSpawned = 0;
        WaveSpawner _waveSpawner;
        Coroutine _mobSpawnCoroutine;
        Coroutine _proximityCheck = null;

        private void Awake() {

            if (MaxMobSpawn < MinMobSpawn)
                throw new System.Exception("MinMobSpawn cannot exceed MaxMobSpawn");
            if (MaxSpawnInterval < MinSpawnInterval)
                throw new System.Exception("MinSpawnInterval cannot exceed MaxSpawnInterval");
            _waveSpawner = gameObject.AddComponent<WaveSpawner>();
            _waveSpawner.OnMobKill.AddListener(() => { _numSpawned--; });
            _mobSpawnCoroutine = StartCoroutine(MobSpawn());
        }

        public override void Interact(Mob m) {
            _trialStarter = m;
            base.Interact(m);
            _proximityCheck = StartCoroutine(ProximityCheck());
        }

        protected override void Complete(bool success) {
            // fail if player's gets out of frame at the moment the 'trial' ends
            if ((_trialStarter.transform.position - transform.position).magnitude > DeactivateRadius)
                success = false;

            // since proximity check runs forever, so it should be ended on complete regardless of whether the 'trial' is ended early or not
            StopCoroutine(_proximityCheck);
            _proximityCheck = null;
            if (success) {
                _waveSpawner.Clean(true);
                StopCoroutine(_mobSpawnCoroutine);
                _mobSpawnCoroutine = null;
            }
            base.Complete(success);
        }

        /// <summary>
        /// Makes the spawner spawns mobs, responsibility of ending it lies on Complete(true)
        /// </summary>
        IEnumerator MobSpawn() {
            while (true) {
                uint numToSpawn = (uint)Random.Range(MinMobSpawn, MaxMobSpawn);
                numToSpawn = (numToSpawn + _numSpawned > MaxConcurrentMob)? MaxConcurrentMob - _numSpawned : numToSpawn;
                float spawnInterval = Random.Range(MinSpawnInterval, MaxSpawnInterval);
                if (numToSpawn <= 0) {
                    yield return new WaitForSeconds(spawnInterval);
                    continue;
                }
                Collider[] c = Physics.OverlapSphere(transform.position, SpawnTriggerRadius);
                // player not in range: delay spawning by 1 second
                if (c.Length == 0 || c.All(x=>!x.TryGetComponent(out Player _))) {
                    yield return new WaitForSeconds(1);
                    continue;
                }

                yield return _waveSpawner.Spawn(mobPreset, numToSpawn, MobSpawnRadius, true, 1, false);
                _numSpawned += numToSpawn;
                yield return new WaitForSeconds(spawnInterval-1);
            }
        }

        /// <summary>
        /// Checks if the player is in the deactivation radius every 1s
        /// </summary>
        /// <returns></returns>
        IEnumerator ProximityCheck() {
            while (true) {
                if ((transform.position - _trialStarter.transform.position).magnitude > DeactivateRadius)
                    EarlyComplete(false);

                yield return new WaitForSeconds(1);
            }
        }
    }
}
