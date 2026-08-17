using Loot;
using Progression.Balance;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat.Miniboss {
    public class BlueLobster : Miniboss {
        [SerializeField]
        GameObject tpCourtesyPrefab;
        [SerializeField]
        GameObject rangedClawPrefab;
        [SerializeField]
        GameObject spinClawPrefab;

        [SerializeField]
        float tpAttackBoost = 0.5f, tpAttackCourtesy = 0.8f, tpDistanceThreshold = 10f;

        [SerializeField]
        float clawAttackCourtesy = 1f, clawTime = 1f, clawlerDistanceThreshold = 7f;


        [SerializeField]
        float spinClawRadius = 3f, spinClawTime = 0.75f, spinClawRangeBoost = 1f;

        [SerializeField, Min(1)]
        int thrownClawsPerShellSmash = 10, flungClawsPerShellSmash = 8;

        [SerializeField]
        float clawThrowDuration = 3f, clawFlingTime = 1f;
        [SerializeField]
        float clawThrowVelocity = 10f, clawFlingVelocity = 50f;
        [SerializeField]
        float shellSmashPause = 1.5f;
        [SerializeField]
        float shellSmashAtkScale = 0.75f, shellSmashDefScale = -0.5f, shellSmashSpeedScale = 0.5f;


        protected override void Awake() {
            base.Awake();
            Owner.OnDamageTake.AddListener(OnDamageTaken);
        }

        protected override (System.Func<IEnumerator> ability, System.Func<bool> predicate)[] Abilities => new (System.Func<IEnumerator>, System.Func<bool>)[] {
            (TPAttack, ()=>!Owner.IsStunned && Behaviour.Delta.magnitude >= tpDistanceThreshold), (Clawler, ()=>Behaviour.Delta.magnitude >= clawlerDistanceThreshold), (ClawSpin, ()=>true)
        };

        IEnumerator TPAttack() {
            Behaviour.PauseStateSwitch();
            Behaviour.State = MobState.Idle;

            yield return new WaitForSeconds(tpAttackCourtesy / 2f);

            Vector3 targetPos = Target.transform.position + Target.GetComponent<Rigidbody>().linearVelocity * tpAttackCourtesy / 2f;
            GameObject tpCourtesy = InstantiateProp(tpCourtesyPrefab);
            tpCourtesy.transform.position = targetPos;
            yield return new WaitForSeconds(tpAttackCourtesy / 2f);

            DestroyProp(tpCourtesy);
            transform.position = targetPos;
            Owner.ScalingStats.Gain((BaseAttribute.Atk, tpAttackBoost));
            interruptedAction =  ()=>Owner.ScalingStats.Lose((BaseAttribute.Atk, tpAttackBoost));
            Behaviour.State = MobState.Attack;
            yield return new WaitForSeconds(Behaviour.AttackTime);

            Owner.ScalingStats.Lose((BaseAttribute.Atk, tpAttackBoost));
            Behaviour.ResumeStateSwitch();
            EndAbility();
            yield break;
        }

        IEnumerator Clawler() {
            Behaviour.FaceTarget();
            Behaviour.PauseStateSwitch();
            Weapon w = Owner.EquippedWeapon;
            Owner.UnequipWeapon();

            interruptedAction = () => { if (Owner.EquippedWeapon is null) Owner.Equip(w); };

            Behaviour.State = MobState.Idle;
            Projectile claw1 = InstantiateProp(rangedClawPrefab).GetComponent<Projectile>(), claw2 = InstantiateProp(rangedClawPrefab).GetComponent<Projectile>();
            claw1.transform.position = claw2.transform.position = Owner.transform.position + Owner.Rotatable.forward * 2;
            yield return new WaitForSeconds(clawAttackCourtesy);

            claw1.Set(Owner, 1f, (Target.transform.position - claw1.transform.position) * 2 / clawTime, 1);
            claw2.Set(Owner, 1f, (Target.transform.position + Target.GetComponent<Rigidbody>().linearVelocity * clawTime - claw2.transform.position) / clawTime, 1);
            yield return new WaitForSeconds(clawTime * 2);

            if (claw1 != null)
                DestroyProp(claw1.gameObject);
            if (claw2 != null)
                DestroyProp(claw2.gameObject);
            Owner.Equip(w);
            Behaviour.ResumeStateSwitch();
            EndAbility();
            yield break;
        }

        IEnumerator ClawSpin() {
            Mob t = Target;
            Behaviour.PauseStateSwitch();
            Behaviour.State = MobState.Idle;
            Weapon w = Owner.EquippedWeapon;
            float atkTime = Behaviour.AttackTime;
            Owner.UnequipWeapon();
            interruptedAction = () => { if (Owner.EquippedWeapon is null) Owner.Equip(w); };

            Vector3 targetFront = t.transform.position + t.Rotatable.forward * spinClawRadius;
            Vector3 left = targetFront - spinClawRadius / 1.5f * t.Rotatable.right, right = targetFront + spinClawRadius / 1.5f * t.Rotatable.right;

            SpinClaw leftClaw = InstantiateProp(spinClawPrefab).GetComponent<SpinClaw>(), rightClaw = InstantiateProp(spinClawPrefab).GetComponent<SpinClaw>();

            leftClaw.transform.position = left;
            leftClaw.Set(Owner, atkTime, spinClawRadius);
            rightClaw.transform.position = right;
            rightClaw.Set(Owner, atkTime, spinClawRadius);

            yield return new WaitForSeconds(1);
            leftClaw.Spin(spinClawTime);
            rightClaw.Spin(spinClawTime);

            yield return new WaitForSeconds(spinClawTime);

            Owner.Equip(w);
            Dictionary<ScalingAttribute, float> d = new() { { ScalingAttribute.AttackRange, spinClawRangeBoost } };
            Owner.GainStats(null, new ScalingStats(otherScaling: d));
            interruptedAction = () => Owner.LoseStats(null, new ScalingStats(otherScaling: d));
            Behaviour.State = MobState.Attack;
            yield return new WaitForSeconds(Behaviour.AttackTime);

            Owner.LoseStats(null, new ScalingStats(otherScaling: d));
            Behaviour.ResumeStateSwitch();
            EndAbility();
            yield break;
        }

        bool usedHalfHPAbility = false;
        void OnDamageTaken(Mob source, float amount) {
            if (Owner.HP < Owner.Stats[BaseAttribute.MaxHp] / 2 && !usedHalfHPAbility) {
                usedHalfHPAbility = true;
                StartNewAbility(ShellSmash);
            }
        }
        IEnumerator ShellSmash() {
            Debug.Log("Unleashing Shell Smash");
            Mob t = Target;
            Behaviour.PauseStateSwitch();
            Behaviour.State = MobState.Idle;

            Owner.transform.position += 1 * Vector3.up;
            Owner.GetComponent<Rigidbody>().useGravity = false;
            Owner.GetComponent<MobMovement>().enabled = false;
            interruptedAction = () => { 
                Owner.IsImmune = false;
                Owner.GetComponent<Rigidbody>().useGravity = true;
                Owner.GetComponent<MobMovement>().enabled = true; 
            };
            yield return new WaitForSeconds(0.5f);
            Owner.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

            Owner.IsImmune = true;
            yield return new WaitForSeconds(shellSmashPause);

            float intervalPerThrow = clawThrowDuration / thrownClawsPerShellSmash;
            Projectile[] claws = new Projectile[thrownClawsPerShellSmash];
            for (int i = 0; i < thrownClawsPerShellSmash; i++) {
                Vector3 vel = (t.transform.position + (Random.Range(0, 2) == 0 ? t.GetComponent<Rigidbody>().linearVelocity : Vector3.zero) - transform.position).normalized * clawThrowVelocity;
                Behaviour.Face(t.transform);
                claws[i] = InstantiateProp(rangedClawPrefab).GetComponent<Projectile>();
                claws[i].transform.position = Owner.transform.position + Owner.Rotatable.forward * 2;
                claws[i].Set(Owner, 1, vel, 1);
                yield return new WaitForSeconds(intervalPerThrow);
            }

            yield return new WaitForSeconds(shellSmashPause);

            foreach (Projectile c in claws)
                if (c != null)
                    DestroyProp(c.gameObject);
            claws = new Projectile[flungClawsPerShellSmash];
            intervalPerThrow = clawFlingTime / flungClawsPerShellSmash;
            for (int i = 0; i < flungClawsPerShellSmash; i++) {
                Owner.Rotatable.localEulerAngles = new Vector3(0, 360f / flungClawsPerShellSmash * i, 0);
                claws[i] = InstantiateProp(rangedClawPrefab).GetComponent<Projectile>();
                claws[i].transform.position = Owner.transform.position + Owner.Rotatable.forward * 2;
                claws[i].transform.forward = Owner.Rotatable.forward;
                yield return new WaitForSeconds(intervalPerThrow);
            }
            foreach (Projectile c in claws)
                c.Set(Owner, 1, c.transform.forward * clawFlingVelocity, 1);
            yield return new WaitForSeconds(shellSmashPause);

            foreach (var c in claws)
                if (c != null)
                    DestroyProp(c.gameObject);

            Owner.ScalingStats.Gain((BaseAttribute.Atk, shellSmashAtkScale), (BaseAttribute.Def, shellSmashDefScale));
            Owner.IsImmune = false;
            Owner.ScalingStats.Gain((ScalingAttribute.WalkSpeed, shellSmashSpeedScale));
            Owner.GetComponent<Rigidbody>().useGravity = true;
            Owner.GetComponent<MobMovement>().enabled = true;
            Behaviour.ResumeStateSwitch();
            EndAbility();
            yield break;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            Owner.OnDamageTake.RemoveListener(OnDamageTaken);
        }
    }
}