using System.Collections;
using UnityEngine;

namespace Combat.Miniboss {
    public class Vampress : Miniboss {
        [SerializeField]
        GameObject majorCourtesyPrefab;
        [SerializeField]
        GameObject majorPrefab;
        [SerializeField]
        int swarmSpawnCount = 0;


        int spawnCount = 0;
        protected override (System.Func<IEnumerator> ability, System.Func<bool> predicate)[] Abilities => new (System.Func<IEnumerator>, System.Func<bool>)[] {
            (Major, ()=>true)        
        };
        GameObject SpawnMinion(GameObject prefab) {
            GameObject returnItem = Instantiate(prefab);
            spawnCount++;
            prefab.GetComponent<Mob>().OnDeath.AddListener(OnMobDied);
            return returnItem;
        }

        void OnMobDied(Mob _, Mob __) {
            if (spawnCount > 0)
                spawnCount--;
        }

        IEnumerator Major() {
            interruptedAction = () => { Behaviour.ResumeStateSwitch(); };
            Behaviour.PauseStateSwitch();
            Behaviour.State = MobState.Idle;
            Behaviour.FaceTarget();
            GameObject courtesy = InstantiateProp(majorCourtesyPrefab);
            courtesy.transform.position = transform.position+transform.forward;

            yield return new WaitForSeconds(1);
            GameObject major = SpawnMinion(majorPrefab);
            major.transform.position = courtesy.transform.position;

            if (courtesy != null)
                Destroy(courtesy);

            Behaviour.ResumeStateSwitch();
            yield return null;
            EndAbility();
        }
    }
}