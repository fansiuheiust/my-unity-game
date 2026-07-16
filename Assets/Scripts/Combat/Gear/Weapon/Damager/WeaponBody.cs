using System;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

namespace Combat {
    /// <summary>
    /// The contact point of a weapon
    /// </summary>
    public class WeaponBody : MonoBehaviour {

        public float attackTime = 0f;

        // basic info and their initialization
        /// <summary>
        /// Whoever the closest parent that has Mob component
        /// </summary>
        public Mob Owner { get; private set; } = null;
        protected Collider Collider { get; private set; }

        protected virtual DamageType DamageType => DamageType.Melee;

        BladeStance _stance = BladeStance.None;

        readonly HashSet<Mob> attackedMobs = new();
        public BladeStance Stance {
            get => _stance;
            set {
                // x -> S
                switch (_stance) {
                    // x = attack
                    case BladeStance.Attack:
                        attackedMobs.Clear();
                        break;

                }

                // S -> y
                switch (value) {
                    // y = {attack, block}
                    case BladeStance.Attack:
                    case BladeStance.Block:
                        Collider.enabled = true;
                        break;
                    // y = {idle, none}
                    case BladeStance.Idle:
                    case BladeStance.None:
                        Collider.enabled = false;
                        break;
                }
                _stance = value;
            }
        }

        private void Awake() {
            Owner = Mob.FindParentingMob(transform);
            Collider = GetComponent<Collider>();
        }

        // blocking
        /// <summary>
        /// whether it can be blocked by certain objects (not piercing)
        /// </summary>
        public bool isBlockable = true;

        /// <summary>
        /// invoked when an attack is interrupted<br />
        /// Mob0: the mob who interrupted the attack
        /// </summary>
        public event Action<Mob> OnAttackInterrupted;

        /// <summary>
        /// check returns
        /// </summary>
        /// <param name="blocker">the weapon that is trying to block the attack</param>
        /// <returns>true if the blocker can block the weapon, base: true iff isBlockable</returns>
        public virtual bool IsBlockAvailable(WeaponBody blocker) => isBlockable && !attackedMobs.Contains(blocker.Owner);

        public void InterruptAttack(Mob interrupter) {
            OnAttackInterrupted?.Invoke(interrupter);
        }

        // attacking
        /// <summary>
        /// damage multiplier based on weapon action
        /// </summary>
        public float multiplier = 1f;

        /// <summary>
        /// handles collision, i.e. when the weapon hits someone
        /// </summary>
        /// <param name="collider"></param>
        void OnTriggerEnter(Collider collider) {
            Hit(collider.gameObject);
        }

        /// <summary>
        /// base method for handling interaction with any gameObject, it only does damage dealing to mob targets
        /// </summary>
        /// <param name="target">the object that got hit</param>
        protected virtual void Hit(GameObject target) {
            switch (Stance) {
                case BladeStance.Block:
                    // OnBlockHit(target);
                    break;
                case BladeStance.Attack:
                    Mob m = Mob.FindParentingMob(target.transform);
                    // only hit when the target is not a weapon and it has a parenting mob
                    if (target.TryGetComponent(out WeaponBody body) && body.Stance == BladeStance.Block && IsBlockAvailable(body)) {
                        InterruptAttack(body.Owner);
                    } else if (target.GetComponent<WeaponBody>() == null && m != null && !attackedMobs.Contains(m)) {
                        Hit(m);
                        attackedMobs.Add(m);
                    }
                    break;
            }
        }

        /// <summary>
        /// base method for handling mob interaction, only deals damage to the target mob
        /// </summary>
        /// <param name="target">the mob that it hit</param>
        /// <returns>True if damage is dealt</returns>
        protected virtual bool Hit(Mob target) {
            if (Owner.CanAttack(target)) {
                Owner.DealDamage(target, DamageType, multiplier);
                Owner.DealKnockback(target, 0.5f * attackTime);
                return true;
            }
            return false;
        }

        void OnBlockHit(GameObject obj) {
            if (obj.TryGetComponent(out WeaponBody other) && other.IsBlockAvailable(this)) {
                other.InterruptAttack(Owner);
            }
        }

    }
    /// <summary>
    /// <para><c>None</c>: currently not acting or undergoing any actions.</para>
    /// <para><c>Attack</c>: self-documenting.</para>
    /// <para><c>Block</c>: self-documenting.</para>
    /// <para><c>Idle</c>: undergoing an action, but no need to trigger collision.</para>
    /// </summary>
    public enum BladeStance {
        None, Attack, Block, Idle
    }
}