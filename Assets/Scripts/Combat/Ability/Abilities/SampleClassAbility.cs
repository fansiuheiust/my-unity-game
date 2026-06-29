using UnityEngine;

namespace Combat {
    public class SampleClassAbility : AbilityObject {
        int toPrint = 0;
        float toPrintFloat = 0;
        float toPrintFloat2 = 0;
        protected override void SetFields(Ability ability) {
            toPrint = (int)ability["One Int Attribute"];
            toPrintFloat = ability["One Decimal Attribute"];
            toPrintFloat2 = ability["One Percentage Attribute"];
        }
        protected override void SubscribeToOwner() { }

        protected override void AbilityBehaviour() {
            Debug.Log($"Int: {toPrint}, Decimal: {toPrintFloat}, Percentage: {toPrintFloat2}");
        }
        protected override void AbilityRemovalBehaviour() { }
    }
}