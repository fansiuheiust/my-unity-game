using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
namespace Combat {
    public class HostileMelee : Hostile {
        [SerializeField] protected float attackRadius = 2;

        protected override void AttackAction() {
            ClickAttack();
        }

        protected override void SwitchState() {
            if (State == MobState.Charge && Delta.magnitude <= attackRadius) {
                State = MobState.Attack;
                return;
            }

            State = State switch {
                MobState.Attack => MobState.Escape,
                _ => MobState.Charge
            };
        }
    }
}