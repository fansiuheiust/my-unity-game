using UnityEngine;
using System;
using NUnit.Framework;
using System.Security.Cryptography;
using UnityEditor.SceneManagement;
using static UnityEngine.UI.GridLayoutGroup;

namespace Combat {
    public abstract class WeaponObject : MonoBehaviour {

        /// <summary>
        /// Whoever the closest parent that has Mob component
        /// </summary>
        protected Mob Owner { get; private set; } = null;

        public Transform Model { get; private set; }

        /// <summary>
        /// For attack/block mutual exclusion
        /// </summary>
        [System.NonSerialized]public bool isActing = false;

        /// <summary>
        /// This should be invoked even if derived objects override
        /// </summary>
        protected virtual void Awake() {
            Owner = Mob.FindParentingMob(transform);
            Model = transform.Find("Model");
            Owner.OnWeaponUnequip += Delete;
            Owner.OnAttackRangeChange += ChangeAttackRange;
            if (!TryGetComponent(out Block _)) {
                Owner.OnBlockClick += ResetBlock;
            }

        }

        void Delete() {
            Owner.OnWeaponUnequip -= Delete;
            Owner.OnAttackRangeChange -= ChangeAttackRange;
            if (!TryGetComponent(out Block _)) {
                Owner.OnBlockClick -= ResetBlock;
            }
            Owner = null;
            Destroy(gameObject);
        }
        /// <summary>
        /// Resets block control of owner for when there is no block script
        /// </summary>
        void ResetBlock() {
            Owner.ResetBlockControl();
        }

        protected abstract void ChangeAttackRange(float newRange);
    }
}