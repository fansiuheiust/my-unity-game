using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

namespace Combat.Miniboss {
    public class Vampress : Miniboss {
        [SerializeField]
        GameObject majorCourtesyPrefab;
        [SerializeField]
        GameObject majorPrefab;
        [SerializeField]
        GameObject swarmEggPrefab;
        [SerializeField]
        GameObject bloodSpillCourtesyPrefab;
        [SerializeField]
        int swarmSpawnCount = 10;
        [SerializeField]
        float swarmSpawnTime = 1;
        [SerializeField]
        float swarmEggSpeed = 40;

        [SerializeField]
        float bloodSpillCourtesyTime = 2;
        [SerializeField]
        float captureTime = 0.5f;
        [SerializeField]
        int captureCount = 10;
        [SerializeField]
        float captureCooldown = 1f;

        readonly HashSet<Mob> spawns = new();
        protected override (System.Func<IEnumerator> ability, System.Func<bool> predicate)[] Abilities => new (System.Func<IEnumerator>, System.Func<bool>)[] {
            (Major, ()=>true), (Swarm, ()=>spawns.Count < 40)
        };

        protected override void Awake() {
            base.Awake();
            Owner.OnDamageTake.AddListener(OnDamageTaken);
        }

        GameObject SpawnMinion(GameObject prefab) {
            GameObject returnItem = Instantiate(prefab);
            spawns.Add(returnItem.GetComponent<Mob>());
            returnItem.GetComponent<Mob>().OnDeath.AddListener(OnMobDied);
            return returnItem;
        }

        void OnMobDied(Mob m, Mob _) {
            spawns.Remove(m);
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
                DestroyProp(courtesy);

            Behaviour.ResumeStateSwitch();
            EndAbility();
        }

        IEnumerator SwarmInternal(float eggMultiplier = 1) {
            yield return new WaitForSeconds(0.5f);
            int swarmSpawnCount = (int)(this.swarmSpawnCount * eggMultiplier);
            float waitTime = swarmSpawnTime / swarmSpawnCount;
            for (int i = 0; i < swarmSpawnCount; i++) {
                SwarmEgg egg = Instantiate(swarmEggPrefab).GetComponent<SwarmEgg>();
                egg.transform.position = transform.position+Owner.Rotatable.forward;
                egg.Set(Owner, 0, swarmEggSpeed * ((Target.transform.position - egg.transform.position).normalized + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-.5f, .5f), Random.Range(-.5f, .5f))), 67);
                egg.SetSpawner(SpawnMinion);
                yield return new WaitForSeconds(waitTime);
            }
        }
        IEnumerator Swarm() {
            interruptedAction = () => Behaviour.ResumeStateSwitch();
            Behaviour.PauseStateSwitch();
            Behaviour.State = MobState.Idle;
            Behaviour.FaceTarget();
            yield return SwarmInternal();
            Behaviour.ResumeStateSwitch();
            EndAbility();
            yield break;
        }


        bool usedBloodSpill = false;
        void OnDamageTaken(Mob _, float __) {
            if (!usedBloodSpill && Owner.HP * 2 < Owner.Stats[BaseAttribute.MaxHp]) {
                usedBloodSpill = true;
                StartNewAbility(BloodSpill);
            }
        }

        IEnumerator Capture(Mob m) {
            GameObject courtesy = InstantiateProp(bloodSpillCourtesyPrefab);
            courtesy.transform.position = m.transform.position;
            OnMobDied(m, null);
            Destroy(m.gameObject);
            yield return new WaitForSeconds(captureTime);
            DestroyProp(courtesy);
            yield break;
        }

        IEnumerator BloodSpill() {

            interruptedAction = () => {
                Behaviour.ResumeStateSwitch();
                Owner.GetComponent<Rigidbody>().useGravity = true;
                Owner.GetComponent<MobMovement>().enabled = true;
            };
            Behaviour.PauseStateSwitch();
            Behaviour.State = MobState.Idle;

            Owner.GetComponent<Rigidbody>().useGravity = false;
            Owner.GetComponent<MobMovement>().enabled = false;
            Owner.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            Owner.AddEffect<Immunity>().Apply(bloodSpillCourtesyTime * 3 + captureCount * captureCooldown);
            Owner.transform.position += 3 * Vector3.up;
            yield return new WaitForSeconds(bloodSpillCourtesyTime);

            yield return SwarmInternal(3f);

            yield return new WaitForSeconds(bloodSpillCourtesyTime);

            int captured = 0;
            for (int i = 0; i < captureCount; i++) {
                if (spawns.Count > 0) {
                    captured++;
                    StartCoroutine(Capture(spawns.ElementAt(Random.Range(0, spawns.Count))));
                } else {
                    break;
                }
                yield return new WaitForSeconds(captureCooldown);
            }

            while (spawns.Count > 0) {
                captured++;
                StartCoroutine(Capture(spawns.FirstOrDefault()));
            }
            yield return new WaitForSeconds(bloodSpillCourtesyTime);


            interruptedAction();
            EndAbility();
            yield break;
        }
    }
}