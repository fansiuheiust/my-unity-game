using UnityEngine;

namespace Combat {
    public class ExplosiveProjectile : Projectile {
        [SerializeField, Min(0.1f)]
        float explosiveRadius = 0.5f;
        public override void Delete() {
            Collider[] c = Physics.OverlapSphere(transform.position, explosiveRadius);
            foreach (Collider collider in c) {
                if (collider.TryGetComponent(out Mob m)) {
                    Hit(m, false);
                }
            }
        }
    }
}