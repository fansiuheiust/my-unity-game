using UnityEngine;

namespace Combat {
    public class Projectile : MonoBehaviour {
        protected Mob Owner { get; private set; } = null;
        protected Collider Collider { get; private set; }

        protected virtual DamageType DamageType => DamageType.Projectile;

        uint pierceLeft;
        float multiplier;

        private void Awake() {
            Collider = GetComponent<Collider>();
        }

        public void Set(uint pierceLeft, float multiplier) {
            this.pierceLeft = pierceLeft;
            this.multiplier = multiplier;
        }

        private void OnCollisionEnter(Collision collision) {
            if (Owner == null) return;
            Hit(collision.collider.gameObject);
        }

        protected virtual void Hit(GameObject gameObject) {
            if (gameObject.TryGetComponent(out Mob m)) {
                Hit(m);
            } else {
                // hitting a non-mob
                Die();
            }
        }

        protected virtual bool Hit(Mob m) {
            if (Owner.CanAttack(m)) {
                Owner.DealDamage(m, DamageType, multiplier);
                pierceLeft--;
                if (pierceLeft < 0) {
                    // ran out of pierces
                    Die();
                } else {
                    Physics.IgnoreCollision(Collider, m.GetComponent<Collider>());
                }
                return true;
            }
            return false;
        }

        protected virtual void Die() {
            Destroy(gameObject);
        }
    }
}
