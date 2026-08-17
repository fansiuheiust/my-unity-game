using Loot;
using Progression.Balance;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        GameObject bloodSpherePrefab;
        [SerializeField]
        string phase2WeaponID = "vampress2";


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

        [SerializeField]
        float atkMultPerCapture = 0.2f, defPerCapture = 8;

        [SerializeField]
        float bloodSphereBase = 2f;

        [SerializeField]
        float bloodSphereVelocity = 100f;

        [SerializeField]
        bool despawnsOutrangedSwarms = true;

        [SerializeField]
        int bloodRainCount = 10;
        [SerializeField]
        float bloodRainDuration = 2f;
        [SerializeField]
        float bloodRainHeight = 8f;
        [SerializeField]
        float bloodRainRadius = 20f;
        [SerializeField]
        float bloodSphereDamageMultiplier = 1f;


        readonly HashSet<Mob> spawns = new();

        Vector3 roomCenter;
        protected override (System.Func<IEnumerator> ability, System.Func<bool> predicate)[] Abilities => new (System.Func<IEnumerator>, System.Func<bool>)[] {
            (Major, ()=>true), (Swarm, ()=>spawns.Count < 40)
        };

        protected override void Awake() {
            base.Awake();
            Owner.OnDamageTake.AddListener(OnDamageTaken);
        }

        protected override void Start() {
            base.Start();
            roomCenter = transform.position;
        }

        GameObject SpawnMinion(GameObject prefab) {
            GameObject returnItem = Instantiate(prefab);
            spawns.Add(returnItem.GetComponent<Mob>());
            returnItem.GetComponent<Mob>().OnDeath.AddListener(OnMobDied);
            if (despawnsOutrangedSwarms)
                StartCoroutine(RemoveOutranged(returnItem.GetComponent<Mob>()));
            return returnItem;
        }

        void RemoveMinion(Mob m) {
            OnMobDied(m, null);
            Destroy(m.gameObject);
        }

        IEnumerator RemoveOutranged(Mob m) {
            yield return new WaitForSeconds(0.5f);
            if (m != null && Mathf.Max(Mathf.Abs(m.transform.position.x - roomCenter.x), Mathf.Abs(m.transform.position.z - roomCenter.z), Mathf.Abs(m.transform.position.y - roomCenter.y)) >= (StageController.DungeonData.RoomLength + StageController.DungeonData.WallThickness) / 2f) {
                RemoveMinion(m);
            }
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
            RemoveMinion(m);
            yield return new WaitForSeconds(captureTime);
            DestroyProp(courtesy);
            yield break;
        }

        IEnumerator BloodSpill() {

            interruptedAction = () => {
                Owner.IsImmune = false;
                Behaviour.ResumeStateSwitch();
                Owner.GetComponent<Rigidbody>().useGravity = true;
                Owner.GetComponent<MobMovement>().enabled = true;
            };
            Behaviour.PauseStateSwitch();
            Behaviour.State = MobState.Idle;

            Owner.GetComponent<Rigidbody>().useGravity = false;
            Owner.GetComponent<MobMovement>().enabled = false;
            Owner.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            Owner.IsImmune = true;
            Owner.transform.position += 3 * Vector3.up;
            yield return new WaitForSeconds(bloodSpillCourtesyTime);

            yield return SwarmInternal(4f);

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

            Owner.BaseStats.Gain(BaseAttribute.Def, captured * defPerCapture);
            Owner.ScalingStats.Gain(BaseAttribute.Atk, captured * atkMultPerCapture);

            Owner.Equip(GearDatabase.Get(phase2WeaponID));

            if (captured > captureCount) {
                Debug.Log("Bad thing happens");

                Projectile bloodSphere = Instantiate(bloodSpherePrefab).GetComponent<Projectile>();
                bloodSphere.GetComponent<Rigidbody>().useGravity = false;
                bloodSphere.transform.position = Owner.transform.position + Vector3.up;

                yield return new WaitForSeconds(bloodSpillCourtesyTime);

                bloodSphere.Set(Owner, bloodSphereBase, bloodSphereVelocity * (Target.transform.position - bloodSphere.transform.position).normalized, 1);

                yield return new WaitForSeconds(bloodSpillCourtesyTime);
            }

            SwitchAbilitySet(new (System.Func<IEnumerator>, System.Func<bool>)[] { (BloodRain, ()=>true)});

            interruptedAction();
            EndAbility();
            yield break;
        }

        IEnumerator BloodRain() {
            interruptedAction = () => {
                Behaviour.ResumeStateSwitch();
            };

            float durationPerBlood = bloodRainDuration / bloodRainCount;
            float verticalSpeed = Mathf.Sqrt(-2 * Physics.gravity.y * bloodRainHeight);// v^2 = u^2 + 2as => u = sqrt(2as)
            // v = u + at => t = u/a
            float otherSpeed = bloodRainRadius / (2* verticalSpeed / -Physics.gravity.y);
            for (int i =0; i < bloodRainCount; i++) {
                Vector3 vel = verticalSpeed * Vector3.up + otherSpeed * (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f))).normalized;

                Projectile bloodSphere = Instantiate(bloodSpherePrefab, Owner.transform.position, Quaternion.identity).GetComponent<Projectile>();

                bloodSphere.Set(Owner, bloodSphereDamageMultiplier, vel, 1);

                yield return new WaitForSeconds(durationPerBlood);
            }


            interruptedAction();

            EndAbility();
            yield break;
        }
    }
}