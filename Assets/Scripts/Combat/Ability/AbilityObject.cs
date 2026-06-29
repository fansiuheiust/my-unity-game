using Combat;
using System.Collections;
using UnityEngine;

namespace Combat {
    public abstract class AbilityObject : MonoBehaviour {
        protected Mob Owner { get; private set; }

        // cooldown handling
        protected float Cooldown { get; private set; }
        protected float ManaCost { get; private set; }
        protected AbilityTriggerKey TriggerKey { get; private set; }
        protected bool OnCooldown { get; private set; } = false;

        void OnDestroy() {
            AbilityRemovalBehaviour();
            StopAllCoroutines();
            if (TriggerKey != AbilityTriggerKey.None)
                Owner.OnAbilityUseAttempt -= OnAbilityUseAttempted;
        }


        void OnAbilityUseAttempted(AbilityTriggerKey triggerKey) {
            if (triggerKey != TriggerKey) return;
            UnleashAbility();
        }
        protected void UnleashAbility() {
            if (OnCooldown) {
                return;
            }
            if (ManaCost > 0 && !Owner.ConsumeMana(ManaCost)) { // i.e. insufficient mana
                return;
            }

            AbilityBehaviour();

            StartCoroutine(PerformCooldown());
        }
        IEnumerator PerformCooldown() {
            if (Cooldown < 0.001f)
                yield break;
            OnCooldown = true;
            yield return new WaitForSeconds(Cooldown);
            OnCooldown = false;
        }

        protected abstract void AbilityBehaviour();

        // initialization
        public void Init(Mob Owner, Ability ability) {
            this.Owner = Owner;
            Cooldown = ability.Cooldown;
            ManaCost = ability.ManaCost;
            TriggerKey = ability.triggerKey;
            SetFields(ability);
            SubscribeToOwner();
            if (TriggerKey != AbilityTriggerKey.None)
                Owner.OnAbilityUseAttempt += OnAbilityUseAttempted;
        }
        /// <summary>
        /// To set required data members according to an ability, not necessarily in <c>Init</c>, e.g. when perk upgrades for perk-based attributes
        /// </summary>
        protected abstract void SetFields(Ability ability);
        /// <summary>
        /// To subscribe methods to the owner's events, triggered when an owner is set
        /// </summary>
        protected abstract void SubscribeToOwner();

        /// <summary>
        /// To do things when an ability is soon to be removed
        /// </summary>
        protected abstract void AbilityRemovalBehaviour();
    }
}