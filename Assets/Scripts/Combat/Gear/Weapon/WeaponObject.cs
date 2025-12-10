using UnityEngine;
using System;
using NUnit.Framework;
using System.Security.Cryptography;
using UnityEditor.SceneManagement;

namespace Combat {
    public abstract class WeaponObject : MonoBehaviour {

        protected Mob owner;

        /// <summary>
        /// This should be invoked even if derived objects override
        /// </summary>
        protected virtual void Awake() {
            owner = transform.root.GetComponent<Mob>();
            owner.OnAttackClick += AttackClicked;
            owner.OnAttackLift += AttackLifted;
            owner.OnBlockClick += BlockClicked;
            owner.OnBlockLift += BlockLifted;
            owner.OnBlockRotate += BlockRotated;
            owner.OnWeaponUnequip += Delete;
        }


        void Delete() {
            owner.OnAttackClick -= AttackClicked;
            owner.OnAttackLift -= AttackLifted;
            owner.OnBlockClick -= BlockClicked;
            owner.OnBlockLift -= BlockLifted;
            owner.OnBlockRotate -= BlockRotated;
            owner.OnWeaponUnequip -= Delete;
            owner = null;
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
            owner.OnAttackStart.Invoke(owner);
        }
        /// <summary>
        /// Must be called after an attack to invoke event
        /// </summary>
        protected void EndAttack() {
            owner.OnAttackEnd.Invoke(owner);
        }
        /// <summary>
        /// Raises OnAttackControlReset
        /// </summary>
        protected void ResetAttackControl() {
            owner.ResetAttackControl();
        }
        /// <summary>
        /// Must be called after a block to invoke event
        /// </summary>
        protected void StartBlock() {
            owner.OnBlockStart.Invoke(owner);
        }
        /// <summary>
        /// Must be called after a block to invoke event 
        /// </summary>
        protected void EndBlock() {
            owner.OnBlockEnd.Invoke(owner);
        }
        /// <summary>
        /// Reaises OnBlockControlReset
        /// </summary>
        protected void ResetBlockControl() {
            owner.ResetBlockControl();
        }
    }
}