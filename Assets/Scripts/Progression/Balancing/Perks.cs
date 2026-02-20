
using Progression;
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



        [System.Serializable]
        public class Attribute {
            [field: SerializeField]
            public PerkAttributeType Type { get; private set; }
            [field: SerializeField]
            public string Name { get; private set; }
            [field: SerializeField, Tooltip("Make sure you use the data type specified in Type")]
            public float Value { get; private set; }
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