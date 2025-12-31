using Combat;
using System.Collections;
using UnityEngine;

namespace BuildingBlocks {
    public class Bomb : MonoBehaviour, IInteractable {
        [field: SerializeField, Tooltip("How long it takes for the bomb to set off"), Min(0f)] public float DetonationTime { get; private set; }
        [field: SerializeField, Tooltip("Range of the explosion"), Min(1f)] public float Range { get; private set; }
        [field: SerializeField, Tooltip("How much damage should the bomb deal to nearby mobs"), Min(0f)] public float Damage { get; private set; }
        public bool IsInteractable => _untouched;
        bool _untouched = true;
        Coroutine _detonateCoroutine = null;
        public void Interact(Mob _) {
            _untouched = false;
            _detonateCoroutine = StartCoroutine(Detonate());
        }
        IEnumerator Detonate() {
            Debug.Log($"Set off detonation, detonating in {DetonationTime}s...");
            yield return new WaitForSeconds( DetonationTime );

            Collider[] colliders = Physics.OverlapSphere(transform.position, Range);
            foreach (Collider c in colliders) {
                if (c.TryGetComponent(out Mob m))
                    m.TakeDamage(Damage, null, DamageType.Melee);
                if (c.TryGetComponent(out Explodable e))
                    e.StartDisappear();
            }
            Destroy(gameObject);
        }
        public void Stop() {
            if (_untouched) return; // do nothing if not yet detonated
            _untouched = true;
            StopCoroutine(_detonateCoroutine);
            _detonateCoroutine = null;
        }
    }
}