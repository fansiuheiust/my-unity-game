using UnityEngine;
namespace Combat {
    public class SampleClassAbility : AbilityObject {
        int toPrint = 0;
        protected override void SetFields(Ability ability) {
            toPrint = (int)ability["Power"];
        }
        protected override void SubscribeToOwner() { }

        protected override void AbilityBehaviour() {
            Debug.Log($"Power: {toPrint}");
        }
        protected override void AbilityRemovalBehaviour() { }
    }
}