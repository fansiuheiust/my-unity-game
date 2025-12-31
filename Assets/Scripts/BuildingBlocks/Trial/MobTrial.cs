using Combat;
using Loot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace BuildingBlocks {
    public class MobTrial : Trial {
        [field: SerializeField, Range(0f, float.MaxValue)] public float WaveDuration { get; private set; }
        /// <summary>
        /// How long should the spawn warning time be (note that <c>WaseDuration</c> must be higher than this variable by 1 for warning to trigger
        /// </summary>
        [field: SerializeField, Range(1f, 5f)] public uint SpawnWarnTime { get; private set; } = 2;
        /// <summary>
        /// The area that mobs can spawn in
        /// </summary>
        [field: SerializeField, Range(1f, float.MaxValue)] public float SpawnRadius { get; private set; } = 8;
        /// <summary>
        /// Number of mobs to kill, kindly set it to 0 if it should be determined automatically
        /// </summary>
        [field: SerializeField] public uint ToKill { get; private set; } = 0;
        
        [SerializeField] MobWaveInfo[] waveInfos;

        public Lootpool<GameObject>[] mobPresets;

        uint _remaining = 0;
        uint _currentWave = 0;
        uint _remainingThisWave = 0;
        bool _canSkipWave = false;

        Coroutine _waveSpawnerCoroutine = null;
        WaveSpawner _waveSpawner = null;

        /// <summary>
        /// Determines whether a 3 second courtesy animation can be played
        /// </summary>
        bool Warnable => WaveDuration >= SpawnWarnTime + 1f;

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
            _waveSpawner = gameObject.AddComponent<WaveSpawner>();
            _waveSpawner.OnMobKill.AddListener(OnMobKilled);
        }

        public override void Interact(Mob _) {
            base.Interact(_);
            _remaining = ToKill;
            _waveSpawnerCoroutine = StartCoroutine(SpawnWaves());
        }

        protected override void Complete(bool success) {
            _waveSpawner.Clean(false);
            _currentWave = 0; // reset current wave such that redo is not cooked
            base.Complete(success);
        }

        IEnumerator SpawnWaves() {
            _remainingThisWave = 0;
            for (; _currentWave < waveInfos.Length; _currentWave++) {
                _canSkipWave = false;
                _remainingThisWave += waveInfos[_currentWave].numMob;
                yield return _waveSpawner.Spawn(mobPresets[waveInfos[_currentWave].presetIndex], waveInfos[_currentWave].numMob, SpawnRadius, Warnable, SpawnWarnTime);
                _canSkipWave = true;
                yield return new WaitForSeconds(WaveDuration - (Warnable ? SpawnWarnTime:0));
            }
            yield break;
        }


        protected override void StopCoroutinesEarly() {
            base.StopCoroutinesEarly();
            StopCoroutine(_waveSpawnerCoroutine);
            _waveSpawnerCoroutine = null;
        }

        void OnMobKilled() {
            if (!IsOngoing) return;
            _remainingThisWave--;
            _remaining--;
            if (_remaining <= 0) {
                EarlyComplete(true);
                return; // return such that if the trial is completed, the next wave won't be spawned
            }
            
            if (_remainingThisWave <= 0 && _canSkipWave) {
                _canSkipWave = false;
                StopCoroutine(_waveSpawnerCoroutine);
                _currentWave++;
                _waveSpawnerCoroutine = StartCoroutine(SpawnWaves());
            }
        }




        [System.Serializable]
        struct MobWaveInfo {
            public uint numMob;
            public uint presetIndex;
        }
    }
}