using UnityEngine;

namespace Combat.Behaviours {
    /// <summary>
    /// Always charges at the target, attacking when given the opportunity
    /// </summary>
    public class Aggressive : MobBehaviour {
        protected override void SwitchState() {
            if (Delta.magnitude <= AttackRange) {
                FaceTarget();
                State = MobState.Attack;
                return;
            }
            State = MobState.Charge;
        }
    }
}