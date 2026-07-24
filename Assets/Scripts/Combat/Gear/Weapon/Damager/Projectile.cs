using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Combat {
    public class Projectile : MonoBehaviour {
        protected Mob Owner { get; private set; } = null;
        protected Collider Collider { get; private set; }
        protected Rigidbody RB { get; private set; }

        protected virtual DamageType DamageType => DamageType.Projectile;

        public uint PierceLeft { get; private set; }
        float multiplier;

        /// <summary>
        /// <para>Called when the projectile hit a target</para>
        /// <c>Mob</c>: The mob it hit <br />
        /// </summary>
        public UnityEvent<Mob> onHit;
        /// <summary>
        /// Called when the projectile is about to be destroyed
        /// </summary>
        public UnityEvent onDelete;

        HashSet<Mob> hitMobs = new();


        public virtual Vector3 velocity {
            get => RB.linearVelocity;
            set => RB.linearVelocity = value;
        }

        private void Awake() {
            Collider = GetComponent<Collider>();
            RB = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pierces">number of pierces, zero means infer from the equipped weapon</param>
        public void Set(Mob owner, float multiplier, Vector3 velocity, uint pierces = 0) {
            Owner = owner;
            Owner.OnWeaponUnequip += Delete;
            hitMobs.Add(owner);
            Physics.IgnoreCollision(Owner.GetComponent<Collider>(), Collider);
            PierceLeft = pierces != 0? pierces: (Owner.EquippedWeapon != null && Owner.EquippedWeapon is Ranged r? r.pierce: 1);
            this.multiplier = multiplier;
            RB.AddForce(velocity, ForceMode.VelocityChange);
        }

        private void OnTriggerEnter(Collider collider) {
            if (Owner == null) return;
            Hit(collider.gameObject);
        }

        protected virtual void Hit(GameObject gameObject) {
            Mob m = Mob.FindParentingMob(gameObject.transform);
            // replace below with else if and place if here when adding hitting a blade/shield
            if (m != null || gameObject.TryGetComponent(out m)) {
                if (hitMobs.Add(m)) {
                    Hit(m);
                }
            } else if (gameObject.TryGetComponent(out Projectile p)) {
                // hitting a fellow projectile
                if (Owner != p.Owner)
                    Delete();
            } else {
                // hitting a non-mob
                Delete();
            }
        }

        protected virtual bool Hit(Mob m) {
            if (Owner.CanAttack(m)) {
                Owner.DealDamage(m, DamageType, multiplier);
                Owner.DealKnockback(m, m.transform.position - RB.linearVelocity, 0.5f);
                PierceLeft--;
                onHit.Invoke(m);
                if (PierceLeft == 0) {
                    // ran out of pierces
                    Delete();
                }
                return true;
            }
            return false;
        }

        public virtual void Delete() {
            onDelete.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy() {
            Owner.OnWeaponUnequip -= Delete;
            Owner = null;
        }
    }
}
