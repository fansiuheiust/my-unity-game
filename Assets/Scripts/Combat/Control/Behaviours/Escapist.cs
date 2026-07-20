using UnityEngine;

namespace Combat.Behaviours {
    /// <summary>
    /// Always escapes from the target, attacking when the target gets too close
    /// </summary>
    public class Escapist : MobBehaviour {
        protected override void SwitchState() {
            if (Delta.magnitude <= AttackRange) {
                State = MobState.Attack;
                return;
            }
            State = MobState.Escape;
        }
    }
}