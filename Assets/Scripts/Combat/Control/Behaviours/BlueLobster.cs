using System.Collections;
using UnityEngine;

namespace Combat.Behaviours {
    public class BlueLobster : Aggressive {

        bool activeAbility = false;

        [SerializeField]
        GameObject tpCourtesyPrefab;
        [SerializeField]
        GameObject rangedClawPrefab;

        [SerializeField]
        float abilityInterval = 10f;

        [SerializeField]
        float tpAttackBoost = 0.5f, tpAttackCourtesy = 0.8f;

        [SerializeField]
        float clawAttackCourtesy = 1f, clawTime = 1f;
        IEnumerator TPAttack() {
            Mob t = Target;
            activeAbility = true;
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
            activeAbility = false;
            yield break;
        }

        IEnumerator Clawler() {
            Mob t = Target;
            activeAbility = true;
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
            activeAbility = false;
            yield break;
        }

        protected override void Awake() {
            base.Awake();
            StartCoroutine(Ability());
        }

        IEnumerator Ability() {
            while (true) {
                yield return new WaitForSeconds(abilityInterval);
                if (!activeAbility && !Owner.IsStunned && Target != null)
                    StartCoroutine(Clawler());
            }
        }

    }
}