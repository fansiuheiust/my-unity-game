using Combat;
using Loot;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Interactable {
    public class WaveSpawner : MonoBehaviour {
        public UnityEvent OnMobKill = new();
        
        GameObject[] _courtesies = null;
        List<Mob> _remainingMobs = new();


        static GameObject _courtesyPrefab = null;
        private void Awake() {
            if (_courtesyPrefab == null)
                _courtesyPrefab = (GameObject)Resources.Load("Prefabs/Interactable/MobCourtesy");
        }

        /// <summary>
        /// Invoked when the spawning mechanic should be turned off
        /// </summary>
        /// <param name="preserveMobs">Whether spawned mobs stay after cleaning</param>
        public void Clean(bool preserveMobs) {
            if (!preserveMobs) foreach (Mob m in _remainingMobs)
                    Destroy(m.gameObject);
            _remainingMobs.Clear();
            if (_courtesies is not null) foreach (GameObject go in _courtesies)
                Destroy(go);
        }

        /// <summary>
        /// Spawns a single wave of mobs
        /// </summary>
        /// <param name="mobPreset">Lootpool of prefabs, assumed to come with mob.</param>
        /// <param name="numMobs"></param>
        /// <param name="hasWarning">Warn before spawning mobs?</param>
        /// <param name="warnTime">How long a warning should be</param>
        public IEnumerator Spawn(Lootpool<GameObject> mobPreset, uint numMobs, float spawnRadius, bool hasWarning, uint warnTime, bool showWarnMsg = true) {
            // choose good random positions
            Vector3[] spots = new Vector3[numMobs];
            _courtesies = new GameObject[numMobs];
            for (int i = 0; i < numMobs; i++) {
                Collider[] hit = null;
                int count = 0;
                do {
                    // radius then rotation
                    float spawnMagnitude = Random.Range(1, spawnRadius);
                    Quaternion spawnRotation = Quaternion.Euler(0, Random.Range(0, 359.875f), 0);
                    spots[i] = transform.position + spawnRotation * new Vector3(spawnMagnitude, 0, 0);
                    count++;
                } while (Physics.OverlapSphereNonAlloc(spots[i], count, hit) != 0 && count < 10);
                if (count >= 10)
                    Debug.Log("Bad spot"); // no need to put this on UI
            }
            if (hasWarning) {
                for (int i = 0; i < numMobs; i++) {
                    _courtesies[i] = Instantiate(_courtesyPrefab);
                    _courtesies[i].transform.position = spots[i];
                }
                for (uint j = warnTime; j > 0; j--) {
                    if (showWarnMsg) Debug.Log(j);
                    yield return new WaitForSeconds(1);
                }
                for (int i = 0; i < numMobs; i++) {
                    Destroy(_courtesies[i]);
                }
                _courtesies = null;
            }
            for (int i = 0; i < numMobs; i++) {
                GameObject go = Instantiate(mobPreset.Draw());
                go.transform.position = spots[i];
                _remainingMobs.Add(go.GetComponent<Mob>());
                go.GetComponent<Mob>().OnDeath.AddListener((m, _) => { OnMobKilled(m); });
            }
            yield break;
        }

        void OnMobKilled(Mob m) {
            _remainingMobs.Remove(m);
            OnMobKill.Invoke();
        }
    }
}
