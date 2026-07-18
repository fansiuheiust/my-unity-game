using UnityEngine;

namespace Combat {
    public abstract class Attack : MonoBehaviour {
        protected Mob Owner { get; private set; } = null;
        protected WeaponObject WeaponObject { get; private set; } = null;
        bool _attacking = false;
        protected bool Attacking => _attacking;

        protected virtual void Awake() {
            Owner = Mob.FindParentingMob(transform);
            WeaponObject = GetComponent<WeaponObject>();
            Owner.OnAttackClick += AttackClicked;
            Owner.OnAttackLift += AttackLifted;
            Owner.OnWeaponUnequip += Delete;
        }

        void Delete() {
            if (Attacking) ResetAttackControl();
            Owner.OnAttackClick -= AttackClicked;
            Owner.OnAttackLift -= AttackLifted;
            Owner.OnWeaponUnequip -= Delete;
            Owner = null;
        }


        /// <summary>
        /// <para>This just cancels the attack action as it is not implemented</para>
        /// <para>(to stop seeing this even if you implemented for a derived class, document the overridden method)</para>
        /// </summary>
        /// <param name="attackTime">1/(final attack speed)</param>
        public virtual void AttackClicked(float attackTime) {
            ResetAttackControl();
        }

        /// <summary>
        /// Does nothing by default.
        /// </summary>
        /// <param name="attackTime">1/(final attack speed)</param>
        public virtual void AttackLifted(float attackTime) {

        }

        /// <summary>
        /// Must be called before an attack to invoke event
        /// </summary>
        protected void StartAttack() {
            Owner.OnAttackStart.Invoke(Owner);
            _attacking = true;
        }
        /// <summary>
        /// Must be called after an attack to invoke event
        /// </summary>
        protected void EndAttack() {
            Owner.OnAttackEnd.Invoke(Owner);
            _attacking = false;
        }
        /// <summary>
        /// Raises OnAttackControlReset
        /// </summary>
        protected void ResetAttackControl() {
            Owner.ResetAttackControl();
        }
    }
}