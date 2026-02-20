
using Progression;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using UnityEngine;
namespace Progression.Balance {
    /// <summary>
    /// Controls coins and perks
    /// TODO: expand this such that perks can be placed
    /// </summary>
    [CreateAssetMenu(fileName = "Perks", menuName = "Scriptable Objects/Perks")]
    public class Perks : ScriptableObject {
        [field: SerializeField, Tooltip("What 1 coin of a tier is equivalent to 1 tier lower")]
        public uint CoinDecompositionRatio { get; private set; }
        [field: SerializeField, Tooltip("How many level points a coin is equivalent to"), Min(0f)]
        public float CoinPerLevelPoint { get; private set; }
    }

    [System.Serializable]
    public class SerializedPerk {
        [field: SerializeField]
        public string ID { get; private set; }
        [field: SerializeField]
        public string Name { get; private set; }
        [field: SerializeField]
        public string RawDescription { get; private set; }
        [field: SerializeField]
        public CoinType CoinType { get; private set; }
        [field: SerializeField]
        public Attribute[] Stats { get; private set; }

        [field: SerializeField]
        public uint MaxLevel { get; private set; }
        [field: SerializeField]
        public Dependency[] Dependencies { get; private set; }
        [field: SerializeField]
        public string[] Exclusions { get; private set; }
        [field: SerializeField]
        public Cost[] Costs { get; private set; }

        public Perk ToPerk() {
            PerkAttribute[] attributes = new PerkAttribute[Stats.Length];
            for (int i = 0; i < Stats.Length; i++)
                attributes[i] = Stats[i].Type switch {
                    PerkAttributeType.Integer => new IntAttribute(Stats[i].Name, Stats[i].Values.Select(x => (int)x).ToArray()),
                    PerkAttributeType.Percentage => new PercentageAttribute(Stats[i].Name, Stats[i].Values.Select(x => x / 100).ToArray()),
                    _ => new DecimalAttribute(Stats[i].Name, Stats[i].Values)
                };  
            Progression.Dependency[] dependencies = new Progression.Dependency[Dependencies.Length];
            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = new Progression.Dependency(Dependencies[i].ID, Dependencies[i].Type);
            (uint tier, uint value)[] costs = new (uint, uint)[Costs.Length];
            for (int i = 0; i < costs.Length; i++) {
                costs[i].tier = Costs[i].Tier;
                costs[i].value = Costs[i].Value;
            }
            return new Perk(ID, Name, RawDescription, new(attributes), CoinType, costs, MaxLevel, dependencies, Exclusions);
        }

        [System.Serializable]
        public class Attribute {
            [field: SerializeField]
            public PerkAttributeType Type { get; private set; }
            [field: SerializeField]
            public string Name { get; private set; }
            [field: SerializeField, Tooltip("Make sure you use the data type specified in Type")]
            public float[] Values { get; private set; }
        }
        [System.Serializable]
        public class Dependency {
            [field: SerializeField]

            public string ID { get; private set; }
            [field: SerializeField]
            public DependencyType Type { get ; private set; }
        }
        [System.Serializable]
        public class Cost {
            [field: SerializeField]
            public uint Tier { get; private set; }
            [field: SerializeField]
            public uint Value { get; private set; }
        }
    }
    public enum PerkAttributeType {
        Integer, Decimal, Percentage
    }
}