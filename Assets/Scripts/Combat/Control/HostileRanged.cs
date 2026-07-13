using System.Collections;
using UnityEngine;

namespace Combat {
    public class HostileRanged : Hostile {
        protected override void SwitchState() {
            State = State switch {
                MobState.Charge => MobState.Attack,
                MobState.Attack => MobState.Escape,
                _ => MobState.Charge
            };
        }
    }
}