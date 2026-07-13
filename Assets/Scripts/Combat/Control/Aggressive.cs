
using UnityEngine;
using System.Collections;

namespace Combat {
    public class Aggressive : MobBehaviour {
        protected override void SwitchState() {
            if (State == MobState.Charge && Delta.magnitude <= (Owner.EquippedWeapon is not null ? Owner.EquippedWeapon.weaponRange * (1 + Owner.Stats[HashedScalingStats.AttackRange]) : 0f)) {
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