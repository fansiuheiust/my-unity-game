using UnityEngine;

namespace Combat.Behaviours {
    /// <summary>
    /// Always charges at the target, attacking when given the opportunity
    /// </summary>
    public class Aggressive : MobBehaviour {
        protected override void SwitchState() {
            if (Delta.magnitude <= AttackRange) {
                State = MobState.Attack;
                return;
            }
            State = MobState.Charge;
        }
        protected override void FollowTarget() {
            base.FollowTarget();
            if (ActiveStateSwitch && State == MobState.Charge)
                SwitchState();
        }

        protected override void OnAttackControlReset() {
            base.OnAttackControlReset();
            if (ActiveStateSwitch)
                State = MobState.Charge;
        }

    }
}