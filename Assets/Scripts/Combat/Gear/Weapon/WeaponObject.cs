using UnityEngine;
using System;
using NUnit.Framework;
using System.Security.Cryptography;
using UnityEditor.SceneManagement;
using static UnityEngine.UI.GridLayoutGroup;

namespace Combat {
    public abstract class WeaponObject : MonoBehaviour {

        /// <summary>
        /// Whoever the closest parent that has Mob component
        /// </summary>
        protected Mob Owner { get; private set; } = null;

        /// <summary>
        /// This should be invoked even if derived objects override
        /// </summary>
        protected virtual void Awake() {
            Owner = Mob.FindParentingMob(transform);
            Owner.OnAttackClick += AttackClicked;
            Owner.OnAttackLift += AttackLifted;
            Owner.OnBlockClick += BlockClicked;
            Owner.OnBlockLift += BlockLifted;
            Owner.OnBlockRotate += BlockRotated;
            Owner.OnWeaponUnequip += Delete;
        }


        void Delete() {
            Owner.OnAttackClick -= AttackClicked;
            Owner.OnAttackLift -= AttackLifted;
            Owner.OnBlockClick -= BlockClicked;
            Owner.OnBlockLift -= BlockLifted;
            Owner.OnBlockRotate -= BlockRotated;
            Owner.OnWeaponUnequip -= Delete;
            Owner = null;
            Destroy(gameObject);
        }

        /// <summary>
        /// <para>This just cancels the attack action as it is not implemented</para>
        /// <para>(to stop seeing this even if you implemented for a derived class, document the overridden method</para>
        /// </summary>
        /// <param name="attackTime">1/(final attack speed)</param>
        public virtual void AttackClicked(float attackTime) {
            ResetAttackControl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attackTime">1/(final attack speed)</param>
        public virtual void AttackLifted(float attackTime) {

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
        /// Must be called before an attack to invoke event
        /// </summary>
        protected void StartAttack() {
            Owner.OnAttackStart.Invoke(Owner);
        }
        /// <summary>
        /// Must be called after an attack to invoke event
        /// </summary>
        protected void EndAttack() {
            Owner.OnAttackEnd.Invoke(Owner);
        }
        /// <summary>
        /// Raises OnAttackControlReset
        /// </summary>
        protected void ResetAttackControl() {
            Owner.ResetAttackControl();
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