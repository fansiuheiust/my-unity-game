using System.Collections;
using UnityEngine;


namespace Combat.Abilities {
    public class Speedy : AbilityObject {
        float speedBoost;
        float duration;
        protected override void SubscribeToOwner() {
        }

        protected override void SetFields(Ability ability) {
            speedBoost = ability["Speed Boost"];
            duration = ability["Duration"];
        }

        Coroutine activeInstance = null;
        protected override void AbilityBehaviour() {
            activeInstance = StartCoroutine(Speed());
        }

        IEnumerator Speed() {
            Owner.GainStats(null, new(walkSpeed: speedBoost));
            yield return new WaitForSeconds(duration);
            RemoveSpeedBoost();
        }

        void RemoveSpeedBoost() {
            Owner.LoseStats(null, new(walkSpeed: speedBoost));
            activeInstance = null;
        }

        protected override void AbilityRemovalBehaviour() {
            if (activeInstance != null) {
                StopCoroutine(activeInstance);
                RemoveSpeedBoost();
            }
        }
    }
}