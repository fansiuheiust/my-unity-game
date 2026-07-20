using System.Collections.Generic;
using UnityEngine;

namespace Combat {
    public class Projectile : MonoBehaviour {
        protected Mob Owner { get; private set; } = null;
        protected Collider Collider { get; private set; }
        protected Rigidbody RB { get; private set; }

        protected virtual DamageType DamageType => DamageType.Projectile;

        uint pierceLeft;
        float multiplier;


        HashSet<Mob> hitMobs = new();

        private void Awake() {
            Collider = GetComponent<Collider>();
            RB = GetComponent<Rigidbody>();
        }

        public void Set(Mob owner, float multiplier, Vector3 velocity) {
            Owner = owner;
            Owner.OnWeaponUnequip += Delete;
            hitMobs.Add(owner);
            Physics.IgnoreCollision(Owner.GetComponent<Collider>(), Collider);
            pierceLeft = (Owner.EquippedWeapon != null && Owner.EquippedWeapon is Ranged r? r.pierce: 1);
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
                if (pierceLeft == 0) {
                    // ran out of pierces
                    Delete();
                } else {
                    pierceLeft--;
                }
                return true;
            }
            return false;
        }

        public virtual void Delete() {
            Destroy(gameObject);
        }

        private void OnDestroy() {
            Owner.OnWeaponUnequip -= Delete;
            Owner = null;
        }
    }
}
