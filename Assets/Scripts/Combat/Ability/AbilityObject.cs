using Combat;
using System.Collections;
using UnityEngine;

namespace Combat {
    public abstract class AbilityObject : MonoBehaviour {
        protected Mob Owner { get; private set; }

        // cooldown handling
        protected float Cooldown { get; private set; }
        protected bool OnCooldown { get; private set; } = false;
        protected void EnterCooldown() {
            StartCoroutine(PerformCooldown());
        }
        IEnumerator PerformCooldown() {
            if (Cooldown < 0.001f)
                yield break;
            OnCooldown = true;
            yield return new WaitForSeconds(Cooldown);
            OnCooldown = false;
        }


        // initialization
        public void Init(Mob Owner, Ability ability) {
            this.Owner = Owner;
            Cooldown = ability.cooldown;
            SetFields(ability);
            SubscribeToOwner();
        }
        /// <summary>
        /// To set required data members according to an ability, not necessarily in <c>Init</c>, e.g. when perk upgrades for perk-based attributes
        /// </summary>
        protected abstract void SetFields(Ability ability);
        /// <summary>
        /// To subscribe methods to the owner's events, triggered when an owner is set
        /// </summary>
        protected abstract void SubscribeToOwner();
    }
}