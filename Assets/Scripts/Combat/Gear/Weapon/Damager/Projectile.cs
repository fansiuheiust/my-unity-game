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
            Set(StageController.Player, 0, 1);
        }

        public void Set(Mob owner, uint pierceLeft, float multiplier) {
            Owner = owner;
            Owner.OnWeaponUnequip += Delete;
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
                Debug.Log("Hit the ground");
                Delete();
            }
        }

        protected virtual bool Hit(Mob m) {
            Physics.IgnoreCollision(Collider, m.GetComponent<Collider>());
            Debug.Log("Projectile hit!");
            if (Owner.CanAttack(m)) {
                Debug.Log("Hit a mob!");
                Owner.DealDamage(m, DamageType, multiplier);
                if (pierceLeft == 0) {
                    // ran out of pierces
                    Debug.Log("Ran out of pierces");
                    Delete();
                } else {
                    pierceLeft--;
                }
                return true;
            }
            return false;
        }

        protected virtual void Delete() {
            Owner.OnWeaponUnequip -= Delete;
            Destroy(gameObject);
        }
    }
}
