using Combat;
using Loot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Interactable {
    public class MobTrial : Trial {
        [field: SerializeField, Range(0f, float.MaxValue)] public float WaveDuration { get; private set; }
        /// <summary>
        /// The area that mobs can spawn in
        /// </summary>
        [field: SerializeField, Range(1f, float.MaxValue)] public float SpawnRadius { get; private set; } = 8;
        /// <summary>
        /// Number of mobs to kill, kindly set it to 0 if it should be determined automatically
        /// </summary>
        [field: SerializeField] public uint ToKill { get; private set; } = 0;
        uint _remaining = 0;
        uint _currentWave = 0;
        uint _remainingThisWave = 0;
        bool _canSkipWave = false;
        
        [SerializeField] MobWaveInfo[] waveInfos;

        public Lootpool<GameObject>[] mobPresets;

        GameObject[] _courtesies = null;
        static GameObject _courtesyPrefab = null;
        List<Mob> _remainingMobs = new();
        Coroutine _waveSpawer = null;

        /// <summary>
        /// Determines whether a 3 second courtesy animation can be played
        /// </summary>
        bool Animatable => WaveDuration >= 4f;

        /// <summary>
        /// an exception thrower, but will determine how many mobs need to be killed by default
        /// </summary>
        private void Awake() {
            if (waveInfos == null || waveInfos.Length == 0)
                throw new System.Exception("There must be at least one wave");
            foreach (Lootpool<GameObject> preset in mobPresets)
                foreach (Lootpool<GameObject>.LootEntry entry in preset.entries)
                    if (!entry.item.TryGetComponent(out Mob _))
                        throw new System.Exception($"All mob options must be a mob, but {entry.item} is not");
            foreach (var waveInfo in waveInfos)
                if (waveInfo.presetIndex >= mobPresets.Length)
                    throw new System.Exception("preset index of a wave must be in range ");
            if (WaveDuration * waveInfos.Length > Duration)
                throw new System.Exception("Trial cannot complete before all waves are spawned, shorten WaveDuration");
            uint totalMobs = (uint)waveInfos.Sum(x => x.numMob);
            if (ToKill == 0)
                ToKill = totalMobs;
            if (_courtesyPrefab == null)
                _courtesyPrefab = (GameObject)Resources.Load("Prefabs/Interactable/MobCourtesy");
        }

        public override void Interact(Mob _) {
            base.Interact(_);
            _remaining = ToKill;
            _waveSpawer = StartCoroutine(SpawnWaves());
        }

        protected override void Complete(bool success) {
            if (_courtesies is not null) foreach (GameObject go in _courtesies)
                Destroy(go);
            _courtesies = null;
            foreach (Mob m in _remainingMobs)
                Destroy(m.gameObject);
            _remainingMobs.Clear();
            _currentWave = 0; // reset current wave such that redo is not cooked
            base.Complete(success);
        }

        IEnumerator SpawnWaves() {
            
            for (; _currentWave < waveInfos.Length; _currentWave++) {
                _canSkipWave = false;
                _remainingThisWave = waveInfos[_currentWave].numMob;
                // choose good random positions
                Vector3[] spots = new Vector3[waveInfos[_currentWave].numMob];
                _courtesies = new GameObject[waveInfos[_currentWave].numMob];
                for (int i = 0; i < waveInfos[_currentWave].numMob; i++) {
                    Collider[] hit = null;
                    int count = 0;
                    do {
                        spots[i] = transform.position + new Vector3(Random.Range(-SpawnRadius, SpawnRadius), 0, Random.Range(-SpawnRadius, SpawnRadius));
                        count++;
                    } while (Physics.OverlapSphereNonAlloc(spots[i], count, hit) != 0 && count < 10);
                    if (count >= 10)
                        Debug.Log("Bad spot");
                }
                if (Animatable) {
                    for (int i = 0; i < waveInfos[_currentWave].numMob; i++) {
                        _courtesies[i] = Instantiate(_courtesyPrefab);
                        _courtesies[i].transform.position = spots[i];
                    }
                    for (int j = 3; j > 0; j--) {
                        Debug.Log(j);
                        yield return new WaitForSeconds(1);
                    }
                    for (int i = 0; i < waveInfos[_currentWave].numMob; i++) {
                        Destroy(_courtesies[i]);
                    }
                    _courtesies = null;
                }
                for (int i = 0; i < waveInfos[_currentWave].numMob; i++) {
                    GameObject go = Instantiate(mobPresets[waveInfos[_currentWave].presetIndex].Draw());
                    go.transform.position = spots[i];
                    _remainingMobs.Add(go.GetComponent<Mob>());
                    go.GetComponent<Mob>().OnDeath.AddListener((m, _) => { OnMobKilled(m); });
                }
                _canSkipWave = true;
                yield return new WaitForSeconds(WaveDuration - (Animatable ? 3:0));
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
            _remainingThisWave--;
            _remaining--;
            if (_remaining <= 0) {
                EarlyComplete(true);
                return; // return such that if the trial is completed, the next wave won't be spawned
            }
            
            if (_remainingThisWave <= 0 && _canSkipWave) {
                StopCoroutine(_waveSpawer);
                _currentWave++;
                _waveSpawer = StartCoroutine(SpawnWaves());
            }
        }




        [System.Serializable]
        struct MobWaveInfo {
            public uint numMob;
            public uint presetIndex;
        }
    }
}