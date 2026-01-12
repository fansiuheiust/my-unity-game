using Combat;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace BuildingBlocks {
    public class Spike : TriggerEnterable {
        [field: SerializeField, Tooltip("Damage"), Min(0f)] public float Damage { get; private set; }

        void Awake() {
            if (!TryGetComponent(out Rigidbody _))
                throw new System.Exception("Parent must have Rigidbody");
            Transform[] children = GetComponentsInChildren<Transform>();
            foreach (Transform child in children) {
                child.gameObject.layer = 2; // to roost player into touching the spike
            }
        }
        protected override void TriggerEffect(Mob m) {
            m.TakeDamage(Damage, null, DamageType.Melee);
            Debug.Log("Ouch");
        }
    }
}
