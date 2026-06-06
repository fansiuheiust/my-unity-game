using Combat;
using System.Linq;
using UnityEngine;


namespace Progression.Balance {
    [CreateAssetMenu(fileName = "AbilityData", menuName = "Scriptable Objects/AbilityData")]
    public class AbilityData : ScriptableObject {
        [SerializeField]
        SerializedAbility[] abilities;
        public Ability[] Abilities => abilities.Select(x => x.Ability).ToArray();
    }

    [System.Serializable]
    class SerializedAbility {
        [SerializeField, Tooltip("ID must match the identifier of the corresponding AbilityObject")]
        string id;
        [SerializeField]
        string name;
        [SerializeField, Tooltip("Use {<attribute name>} if you wish an attribute to appear")]
        string rawDescription;
        [SerializeField, Min(0f)]
        float cooldown;
        [SerializeField, Min(0f)]
        float manaCost;
        [SerializeField]
        AbilityTriggerKey triggerKey;
        [SerializeField]
        SingleValuedAttribute[] attributes;

        internal Ability Ability { 
            get {
                Attribute[] attributes = new Attribute[this.attributes.Length];
                for (int i = 0; i < attributes.Length; i++) {
                    attributes[i] = this.attributes[i].type switch {
                        PerkAttributeType.Integer => new IntAttribute(this.attributes[i].name, new int[] { (int)this.attributes[i].value }),
                        PerkAttributeType.Decimal => new DecimalAttribute(this.attributes[i].name, new float[] { this.attributes[i].value }),
                        _ => new PercentageAttribute(this.attributes[i].name, new float[] { this.attributes[i].value/100f })
                    };
                }
                return new(id, name, rawDescription, cooldown, manaCost, triggerKey, new(attributes), AbilityDatabase.abilityObjects[id]);
            } 
        }
    }

    [System.Serializable]
    class SingleValuedAttribute {
        public string name;
        public PerkAttributeType type;
        [Tooltip("Make sure the value matches the attribute type")]
        public float value;
    }
}