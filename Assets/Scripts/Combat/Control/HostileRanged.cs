using System.Collections;
using UnityEngine;

namespace Combat {
    public class HostileRanged : Hostile {

        Transform _rotatable;


        protected override void Awake() {
            base.Awake();
            _rotatable = Owner.transform.Find("Rotatable");
        }
        protected override void SwitchState() {
            State = State switch {
                MobState.Charge => MobState.Attack,
                MobState.Attack => MobState.Escape,
                _ => MobState.Charge
            };
        }

        protected override void AttackAction() {
            StartCoroutine(Attack());
        }

        IEnumerator Attack() {
            ClickAttack();
            float afkTime = 1 / ((1 + Owner.Stats.Final.AtkSpeed) * Owner.EquippedWeapon.BaseAttackSpeed);
            for (float time = 0; time < afkTime; time += Time.deltaTime) {
                _rotatable.forward = Delta;
                yield return null;
            }
            LiftAttack();
        }
    }
}