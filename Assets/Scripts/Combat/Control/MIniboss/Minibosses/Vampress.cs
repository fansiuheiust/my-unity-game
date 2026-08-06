using System.Collections;
using UnityEngine;

namespace Combat.Miniboss {
    public class Vampress : Miniboss {
        [SerializeField]
        GameObject majorCourtesyPrefab;
        [SerializeField]
        GameObject majorPrefab;
        [SerializeField]
        GameObject swarmEggPrefab;
        [SerializeField]
        int swarmSpawnCount = 10;
        [SerializeField]
        float swarmSpawnTime = 1;
        [SerializeField]
        float swarmEggSpeed = 40;


        int spawnCount = 0;
        protected override (System.Func<IEnumerator> ability, System.Func<bool> predicate)[] Abilities => new (System.Func<IEnumerator>, System.Func<bool>)[] {
            (Major, ()=>true), (Swarm, ()=>spawnCount < 40)
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
            courtesy.transform.position = transform.position+Owner.Rotatable.forward;

            yield return new WaitForSeconds(1);
            GameObject major = SpawnMinion(majorPrefab);
            major.transform.position = courtesy.transform.position;

            if (courtesy != null)
                Destroy(courtesy);

            Behaviour.ResumeStateSwitch();
            EndAbility();
        }

        IEnumerator Swarm() {
            interruptedAction = () => Behaviour.ResumeStateSwitch();
            Behaviour.PauseStateSwitch();
            Behaviour.State = MobState.Idle;
            Behaviour.FaceTarget();

            yield return new WaitForSeconds(0.5f);
            float waitTime = swarmSpawnTime / swarmSpawnCount;
            for (int i = 0; i < swarmSpawnCount; i++) {
                SwarmEgg egg = Instantiate(swarmEggPrefab).GetComponent<SwarmEgg>();
                egg.transform.position = transform.position+Owner.Rotatable.forward;
                egg.Set(Owner, 0, swarmEggSpeed * ((Target.transform.position - egg.transform.position).normalized + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-.25f, .25f), Random.Range(-.25f, .25f))), 67);
                egg.SetSpawner(SpawnMinion);
                yield return new WaitForSeconds(waitTime);
            }

            Behaviour.ResumeStateSwitch();
            EndAbility();
            yield break;
        }
    }
}