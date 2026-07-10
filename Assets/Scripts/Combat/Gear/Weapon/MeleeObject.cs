using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;
namespace Combat {
    /// <summary>
    /// Attached to the Melee weapon gameObject, used for handling the melee weapon's movement
    /// </summary>
    public class MeleeObject : WeaponObject {
        /// <summary>
        /// blade of the melee weapon
        /// </summary>
        WeaponBody _blade;

        /// <summary>
        /// Change to the localPosition of model when started blocking
        /// </summary>
        Vector3 _blockChange = new(0.4f, -0.4f, 0);

        /// <summary>
        /// The active attack animation
        /// </summary>
        Coroutine _attackAnimation;

        protected override void Awake() {
            base.Awake();
            _blade = transform.Find("Model").Find("Blade").GetComponent<WeaponBody>();
        }

        protected override void ChangeAttackRange(float newRange) {
            _blade.transform.localScale = new(_blade.transform.localScale.x, _blade.transform.localScale.y, newRange);
            _blade.transform.localPosition = new(_blade.transform.localPosition.x, _blade.transform.localPosition.y, newRange / 2);
        }
    }
}