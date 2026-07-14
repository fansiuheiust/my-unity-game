
using UnityEngine;
using System.Collections;

namespace Combat.Behaviours {
    /// <summary>
    /// Cycles between charging, attacking, and escaping
    /// </summary>
    public class BackAndForth : MobBehaviour {
        protected override void SwitchState() {
            if (State == MobState.Charge && Delta.magnitude <= AttackRange) {
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