using UnityEngine;

namespace Combat.Behaviours {
    /// <summary>
    /// Escapes most of the time, but suddenly charges at and attacks the target very fast
    /// </summary>
    public class Ambusher : MobBehaviour {
        [SerializeField, Tooltip("Out of how many state switch should the mob switch to ambush a player")] int ambushRatio = 6;

        [SerializeField, Tooltip("Walk speed increase when the mob is ambushing")] float speedIncrease = 1.0f;

        int currRatio = 1;
        protected override void SwitchState() {
            if (State == MobState.Charge) {
                if (Delta.magnitude < AttackRange) {
                    Owner.ScalingStats.Lose((ScalingAttribute.WalkSpeed, speedIncrease));
                    State = MobState.Attack;
                }
                return;
            }
            if (currRatio == 0) {
                State = MobState.Charge;
                Owner.ScalingStats.Gain((ScalingAttribute.WalkSpeed, speedIncrease));
            } else {
                State = MobState.Escape;
            }
            currRatio = (currRatio + 1) % ambushRatio;
        }
    }
}