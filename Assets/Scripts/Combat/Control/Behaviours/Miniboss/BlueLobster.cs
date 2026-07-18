using Combat.Miniboss;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Combat.Behaviours {
    public class BlueLobster : Aggressive {
        bool _activeAbility = false;
        bool ActiveAbility {
            get => _activeAbility;
            set {
                bool og = _activeAbility;
                _activeAbility = value;
                if (og != value) {
                    if (value)
                        onAbilityStart.Invoke();
                    else
                        onAbilityEnd.Invoke();
                }
            }
        }

        [SerializeField]
        GameObject tpCourtesyPrefab;
        [SerializeField]
        GameObject rangedClawPrefab;
        [SerializeField]
        GameObject spinClawPrefab;

        [SerializeField]
        float abilityIntervalMin = 4f, abilityIntervalMax = 20f;

        [SerializeField]
        float tpAttackBoost = 0.5f, tpAttackCourtesy = 0.8f;

        [SerializeField]
        float clawAttackCourtesy = 1f, clawTime = 1f;


        [SerializeField]
        float spinClawRadius = 3f, spinClawTime = 0.75f, spinClawRangeBoost = 1f;

        [SerializeField, Min(1)]
        int thrownClawsPerShellSmash = 10, centerOutClawsPerShellSmash = 8;

        [SerializeField]
        float clawThrowDuration = 3f, centerOutClawTime = 1f;
        [SerializeField]
        float clawThrowVelocity = 10f, clawCenterOutVelocity = 50f;
        [SerializeField]
        float shellSmashPause = 1.5f;
        [SerializeField]
        float shellSmashAtkScale = 0.75f, shellSmashDefScale = -0.5f, shellSmashSpeedScale = 0.5f;


        public UnityEvent onAbilityStart, onAbilityEnd;

        AbilityChooser abilityChooser;

        

        IEnumerator TPAttack() {
            Mob t = Target;
            ActiveAbility = true;
            PauseStateSwitch();
            State = MobState.Idle;

            yield return new WaitForSeconds(tpAttackCourtesy/2f);

            Vector3 targetPos = t.transform.position + t.GetComponent<Rigidbody>().linearVelocity * tpAttackCourtesy/2f;
            GameObject tpCourtesy = Instantiate(tpCourtesyPrefab);
            tpCourtesy.transform.position = targetPos;
            yield return new WaitForSeconds(tpAttackCourtesy / 2f);

            Destroy(tpCourtesy);
            transform.position = targetPos;
            Owner.GainStats(null, new(atk: tpAttackBoost));
            State = MobState.Attack;
            yield return new WaitForSeconds(AttackTime);

            Owner.LoseStats(null, new(atk: tpAttackBoost));
            ResumeStateSwitch();
            ActiveAbility = false;
            yield break;
        }

        IEnumerator Clawler() {
            Mob t = Target;
            FaceTarget();
            ActiveAbility = true;
            PauseStateSwitch();
            Weapon w = Owner.EquippedWeapon;
            Owner.UnequipWeapon();
            State = MobState.Idle;
            Projectile claw1 = Instantiate(rangedClawPrefab).GetComponent<Projectile>(), claw2 = Instantiate(rangedClawPrefab).GetComponent<Projectile>();
            claw1.transform.position = claw2.transform.position = Owner.transform.position + Owner.Rotatable.forward*2;
            yield return new WaitForSeconds(clawAttackCourtesy);

            claw1.Set(Owner, 1f, (t.transform.position-claw1.transform.position) * 2 / clawTime);
            claw2.Set(Owner, 1f, (t.transform.position + t.GetComponent<Rigidbody>().linearVelocity * clawTime - claw2.transform.position) / clawTime);
            yield return new WaitForSeconds(clawTime*2);

            if (claw1 != null)
                claw1.Delete();
            if (claw2 != null)
                claw2.Delete();
            Owner.Equip(w);
            ResumeStateSwitch();
            ActiveAbility = false;
            yield break;
        }

        IEnumerator ClawSpin() {
            Mob t = Target;
            ActiveAbility = true;
            PauseStateSwitch();
            State = MobState.Idle;
            Weapon w = Owner.EquippedWeapon;
            Owner.UnequipWeapon();

            Vector3 targetFront = t.transform.position + t.Rotatable.forward * spinClawRadius;
            Vector3 left = targetFront - spinClawRadius * t.Rotatable.right, right = targetFront + spinClawRadius * t.Rotatable.right;

            SpinClaw leftClaw = Instantiate(spinClawPrefab).GetComponent<SpinClaw>(), rightClaw = Instantiate(spinClawPrefab).GetComponent<SpinClaw>();

            leftClaw.transform.position = left;
            leftClaw.Set(Owner, AttackTime,spinClawRadius);
            rightClaw.transform.position = right;
            rightClaw.Set(Owner, AttackTime, spinClawRadius);

            yield return new WaitForSeconds(1);
            leftClaw.Spin(spinClawTime);
            rightClaw.Spin(spinClawTime);

            yield return new WaitForSeconds(spinClawTime);

            Owner.Equip(w);
            Dictionary<HashedScalingStats, float> d = new() { { HashedScalingStats.AttackRange, spinClawRangeBoost} };
            Owner.GainStats(null, new ScalingStats(otherScaling: d));
            State = MobState.Attack;
            yield return new WaitForSeconds(AttackTime);

            Owner.LoseStats(null, new ScalingStats(otherScaling: d));
            ResumeStateSwitch();
            ActiveAbility = false;
            yield break;
        }

        protected override void Awake() {
            base.Awake();
            abilityUser = StartCoroutine(Ability());
            Owner.OnDamageTake.AddListener(OnDamageTaken);
            abilityChooser = new((TPAttack, ()=>!Owner.IsStunned), (Clawler, ()=>true), (ClawSpin, ()=>true));
            
        }

        Coroutine abilityUser = null;
        IEnumerator Ability() {
            while (true) {
                yield return new WaitForSeconds(Random.Range(abilityIntervalMin, abilityIntervalMax));
                if (!ActiveAbility && Target != null && abilityChooser.Next(out var f)) {
                    StartCoroutine(f());
                }
            }
        }


        bool usedHalfHPAbility = false;
        void OnDamageTaken(Mob source, float amount) {
            if (Owner.HP < Owner.Stats.MaxHp/2 && !usedHalfHPAbility) {
                usedHalfHPAbility = true;
                if (ActiveAbility) {
                    onAbilityEnd.AddListener(HalfHPAbilityStarter);
                } else {
                    HalfHPAbilityStarter();
                }
            }
        }

        void HalfHPAbilityStarter() {
            onAbilityEnd.RemoveListener(HalfHPAbilityStarter);
            StartCoroutine(ShellSmash());
        }
        IEnumerator ShellSmash() {
            Debug.Log("Unleashing Shell Smash");
            Mob t = Target;
            ActiveAbility = true;
            PauseStateSwitch();
            State = MobState.Idle;

            Owner.transform.position += 1 * Vector3.up;
            Owner.GetComponent<Rigidbody>().useGravity = false;
            Owner.AddEffect<Immunity>().Apply(3 * shellSmashPause + clawThrowDuration + centerOutClawTime);
            yield return new WaitForSeconds(shellSmashPause);

            float intervalPerThrow = clawThrowDuration / thrownClawsPerShellSmash;
            Projectile[] claws = new Projectile[thrownClawsPerShellSmash];
            for (int i = 0; i < thrownClawsPerShellSmash; i++) {
                Vector3 vel = (t.transform.position + (Random.Range(0, 2) == 0? t.GetComponent<Rigidbody>().linearVelocity: Vector3.zero) - transform.position).normalized * clawThrowVelocity;
                Face(t.transform);
                claws[i] = Instantiate(rangedClawPrefab).GetComponent<Projectile>();
                claws[i].transform.position = Owner.transform.position + Owner.Rotatable.forward*2;
                claws[i].Set(Owner, 1, vel);
                yield return new WaitForSeconds(intervalPerThrow);
            }

            yield return new WaitForSeconds(shellSmashPause);

            foreach (Projectile c in claws)
                if (c != null)
                    c.Delete();
            claws = new Projectile[centerOutClawsPerShellSmash];
            intervalPerThrow = centerOutClawTime / centerOutClawsPerShellSmash;
            for (int i = 0; i < centerOutClawsPerShellSmash; i++) {
                Owner.Rotatable.localEulerAngles = new Vector3(0, 360f/centerOutClawsPerShellSmash*i, 0);
                claws[i] = Instantiate(rangedClawPrefab).GetComponent<Projectile>();
                claws[i].transform.position = Owner.transform.position + Owner.Rotatable.forward * 2;
                claws[i].transform.forward = Owner.Rotatable.forward;
                yield return new WaitForSeconds(intervalPerThrow);
            }
            foreach (Projectile c in claws)
                c.Set(Owner, 1, c.transform.forward * clawCenterOutVelocity);
            yield return new WaitForSeconds(shellSmashPause);

            foreach (var c in claws)
                if (c != null)
                    c.Delete();


            Owner.GainStats(null, new ScalingStats(atk: shellSmashAtkScale, def: shellSmashDefScale, walkSpeed: shellSmashSpeedScale));
            Owner.GetComponent<Rigidbody>().useGravity = true;
            ResumeStateSwitch();
            ActiveAbility = false;
            yield break;
        }

        private void OnDestroy() {
            StopAllCoroutines();
        }
    }
}