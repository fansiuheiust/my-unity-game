using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Combat {
    /// <summary>
    /// <para><c>None</c>: currently not acting or undergoing any actions.</para>
    /// <para><c>Attack</c>: self-documenting.</para>
    /// <para><c>Block</c>: self-documenting.</para>
    /// <para><c>Idle</c>: undergoing an action, but no need to trigger collision.</para>
    /// </summary>
    public enum BladeStance {
        None, Attack, Block, Idle
    }
    public class Blade : WeaponBody {
        BladeStance _stance = BladeStance.None;
        public float attackTime = 0f;



        protected override DamageType DamageType => DamageType.Melee;


        List<Mob> attackeds = new();
        /// <summary>
        /// Self-documenting, but comes with a setter for executing stuff when hopping from a stance
        /// </summary>
        public BladeStance Stance {
            get => _stance;
            set {
                // x -> S
                switch (_stance) {
                    // x = attack
                    case BladeStance.Attack:
                        foreach (Mob m in attackeds) {
                            if (m != null)
                                Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), m.GetComponent<Collider>(), false);
                        }
                        attackeds.Clear();
                        break;

                }

                // S -> y
                switch (value) {
                    // y = {attack, block}
                    case BladeStance.Attack:
                    case BladeStance.Block:
                        Collider.isTrigger = false;
                        break;
                    // y = {idle, none}
                    case BladeStance.Idle:
                    case BladeStance.None:
                        Collider.isTrigger = true;
                        break;
                }
                _stance = value;
            }
        }

        protected override void Hit(GameObject target) {
            switch (Stance) {
                case BladeStance.Block:
                    OnBlockHit(target);
                    break;
                case BladeStance.Attack:
                    base.Hit(target);
                    break;
            }
        }

        /// <summary>
        /// Also deals knockback
        /// </summary>
        /// <param name="target">self-documenting</param>
        /// <returns>Whether the target was damaged upon hitting</returns>
        protected override bool Hit(Mob target) {

            bool baseHit = base.Hit(target);
            if (baseHit)
                Owner.DealKnockback(target, 0.5f * attackTime);
            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), target.GetComponent<Collider>());
            attackeds.Add(target);
            return baseHit;
        }

        void OnBlockHit(GameObject obj) {
            if (obj.TryGetComponent(out WeaponBody other) && other.IsBlockAvailable(this)) {
                other.InterruptAttack(Owner);
            }
        }
        public override bool IsBlockAvailable(WeaponBody blocker) => base.IsBlockAvailable(blocker) && !attackeds.Contains(blocker.Owner);
    }
}