using UnityEngine;

namespace Combat.Abilities {
    public class ShellSmash : AbilityObject {

        ScalingStats delta;
        float hpThreshold;
        bool triggered = false;
        protected override void SetFields(Ability ability) {
            delta = new ScalingStats(atk: ability["Attack Increase"], walkSpeed: ability["Walk Speed Increase"], atkSpeed: ability["Attack Speed Increase"], def: -ability["Defence Reduction"]);
            hpThreshold = ability["HP Threshold"];
        }

        protected override void SubscribeToOwner() {
            Owner.OnDamageTake.AddListener(OnDamageTaken);
        }


        void OnDamageTaken(Mob _, float __) {
            if (Owner.HP / Owner.Stats.MaxHp < hpThreshold) {
                AbilityBehaviour();
            }
        }
        protected override void AbilityBehaviour() {
            if (triggered) return;
            triggered = true;
            Owner.OnDamageTake.RemoveListener(OnDamageTaken);

            Owner.GainStats(null, delta);
        }

        protected override void AbilityRemovalBehaviour() {
            if (Owner == null) return; // if owner died
            if (triggered)
                Owner.LoseStats(null, delta);
        }
    }
}