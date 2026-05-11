using UnityEngine;

namespace Combat {
    public class RangedObject : WeaponObject {

        public float AttackRange { get; private set; }
        protected override void ChangeAttackRange(float newRange) {
            AttackRange = newRange;
        }
    }
}