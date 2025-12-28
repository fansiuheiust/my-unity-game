using Combat;
using Loot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Interactable {
    public class MobTrial : Trial {
        [field: SerializeField] public float WaveDuration { get; private set; }
        /// <summary>
        /// The area that mobs can spawn in
        /// </summary>
        [field: SerializeField] public float SpawnRadius { get; private set; } = 8;
        /// <summary>
        /// Number of mobs to kill, kindly set it to 0 if it should be determined automatically
        /// </summary>
        [field: SerializeField] public uint ToKill { get; private set; } = 0;
        uint _remaining = 0;
        [SerializeField] MobWaveInfo[] waveInfos;

        List<Mob> _remainingMobs = new();
        Coroutine _waveSpawer = null;

        /// <summary>
        /// an exception thrower, but will determine how many mobs need to be killed by default
        /// </summary>
        private void Awake() {
            if (waveInfos == null || waveInfos.Length == 0)
                throw new System.Exception("There must be at least one wave");
            foreach (var waveInfo in waveInfos)
                foreach (Lootpool<GameObject>.LootEntry entry in waveInfo.mobOptions.entries)
                    if (!entry.item.TryGetComponent(out Mob _))
                        throw new System.Exception($"All mob options must be a mob, but {entry.item} is not");
            if (WaveDuration * waveInfos.Length > Duration)
                throw new System.Exception("Trial cannot complete before all waves are spawned, shorten WaveDuration");
            if (SpawnRadius <= 1)
                throw new System.Exception("Spawn radius is too small, make it greater than 1");
            if (ToKill == 0)
                ToKill = (uint)waveInfos.Sum(x => x.numMob);
        }

        public override void Interact(Mob _) {
            base.Interact(_);
            _remaining = ToKill;
            _waveSpawer = StartCoroutine(SpawnWaves());
        }

        protected override void Complete(bool success) {
            foreach (Mob m in _remainingMobs)
                Destroy(m.gameObject);
            _remainingMobs.Clear();
            base.Complete(success);
        }

        IEnumerator SpawnWaves() {
            foreach (var waveInfo in waveInfos) {
                for (int i = 0; i < waveInfo.numMob; i++) {
                    // choose a good random position
                    Vector3 chosenSpot;
                    Collider[] hit = null;
                    int count = 0;
                    do {
                        chosenSpot = transform.position + new Vector3(Random.Range(-SpawnRadius, SpawnRadius), 0, Random.Range(-SpawnRadius, SpawnRadius));
                        count++;
                    } while (Physics.OverlapSphereNonAlloc(chosenSpot, count, hit) != 0 && count < 10);
                    if (count >= 10)
                        Debug.Log("Bad spot");
                    GameObject go = Instantiate(waveInfo.mobOptions.Draw());
                    go.transform.position = chosenSpot;
                    _remainingMobs.Add(go.GetComponent<Mob>());
                    go.GetComponent<Mob>().OnDeath.AddListener((m, _) => { OnMobKilled(m); });
                }
                yield return new WaitForSeconds(WaveDuration);
            }
            yield break;
        }


        protected override void StopCoroutinesEarly() {
            base.StopCoroutinesEarly();
            StopCoroutine(_waveSpawer);
            _waveSpawer = null;
        }

        void OnMobKilled(Mob m) {
            if (!IsOngoing) return;
            _remainingMobs.Remove(m);
            _remaining--;
            if (_remaining <= 0)
                EarlyComplete(true);

        }




        [System.Serializable]
        struct MobWaveInfo {
            public uint numMob;
            public Lootpool<GameObject> mobOptions;
        }
    }
}