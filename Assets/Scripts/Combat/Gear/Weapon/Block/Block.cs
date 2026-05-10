using UnityEngine;


namespace Combat {
    public abstract class Block : MonoBehaviour {
        /// <summary>
        /// Whoever the closest parent that has Mob component
        /// </summary>
        protected Mob Owner { get; private set; } = null;

        protected WeaponObject WeaponObject { get; private set; } = null;

        protected virtual void Awake() {
            Owner = Mob.FindParentingMob(transform);
            WeaponObject = GetComponent<WeaponObject>();
            Owner.OnBlockClick += BlockClicked;
            Owner.OnBlockLift += BlockLifted;
            Owner.OnBlockRotate += BlockRotated;
            Owner.OnWeaponUnequip += Delete;
        }

        void Delete() {
            Owner.OnBlockClick -= BlockClicked;
            Owner.OnBlockLift -= BlockLifted;
            Owner.OnBlockRotate -= BlockRotated;
            Owner.OnWeaponUnequip -= Delete;

        }

        /// <summary>
        /// <para>This just cancels the block action as it is not implemented</para>
        /// <para>(to stop seeing this even if you implemented for a derived class, document the overridden method</para>
        /// </summary>
        public virtual void BlockClicked() {
            ResetBlockControl();
        }
        public virtual void BlockLifted() {

        }
        public virtual void BlockRotated(float angle) {

        }

        /// <summary>
        /// Must be called after a block to invoke event
        /// </summary>
        protected void StartBlock() {
            Owner.OnBlockStart.Invoke(Owner);
        }
        /// <summary>
        /// Must be called after a block to invoke event 
        /// </summary>
        protected void EndBlock() {
            Owner.OnBlockEnd.Invoke(Owner);
        }
        /// <summary>
        /// Reaises OnBlockControlReset
        /// </summary>
        protected void ResetBlockControl() {
            Owner.ResetBlockControl();
        }
    }
}