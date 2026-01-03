using Combat;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace BuildingBlocks {
    public class Spike : MonoBehaviour {
        public UnityEvent<Mob> OnMobStep = new();
        [field: SerializeField, Tooltip("Time between damage"), Min(0.05f)] public float Period { get; private set; }
        [field: SerializeField, Tooltip("Damage, duh"), Min(0f)] public float Damage { get; private set; }

        /// <summary>
        /// Mobs that are currently taking damage from the spike
        /// </summary>
        readonly LinkedList<Mob> _affecteds = new(); 
        void Awake() {
            if (!TryGetComponent(out Rigidbody _))
                throw new System.Exception("Parent must have Rigidbody");
            Transform[] children = GetComponentsInChildren<Transform>();
            foreach (Transform child in children) {
                child.gameObject.layer = 2; // to roost player into touching the spike
            }
        }

        void OnTriggerEnter(Collider collider) {
            if (collider.TryGetComponent(out Mob m) && !_affecteds.Contains(m)) {
                _affecteds.AddLast(m);
                StartCoroutine(Step(m));
            }
        }

        private void OnTriggerExit(Collider collider) {
            if (collider.TryGetComponent(out Mob m)) {
                _affecteds.Remove(m);
            }
        }

        IEnumerator Step(Mob m) {
            do {
                if (m.IsDestroyed()) {
                    _affecteds.Remove(m);
                    yield break;
                }
                OnMobStep.Invoke(m);
                m.TakeDamage(Damage, null, DamageType.Melee);
                yield return new WaitForSeconds(Period);
            } while (_affecteds.Contains(m));
        }
    }
}
