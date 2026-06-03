
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
    public class PerkData : ScriptableObject {
        [field: SerializeField, Tooltip("What 1 coin of a tier is equivalent to 1 tier lower")]
        public uint CoinDecompositionRatio { get; private set; }
        [field: SerializeField, Tooltip("How many level points a coin is equivalent to"), Min(0f)]
        public float CoinPerLevelPoint { get; private set; }

        [SerializeField]
        SerializedPerk[] floorPerks;
        [SerializeField]
        SerializedPerk[] rngPerks;
        [SerializeField]
        SerializedPerk[] classPerks;
        public PerkTree FloorPerkTree => new(floorPerks.Select(x=>x.UseDefaultCoinType? x.ToPerk(CoinType.Floor): x.AsPerk).ToArray());
        public PerkTree RNGPerkTree => new(rngPerks.Select(x=>x.UseDefaultCoinType? x.ToPerk(CoinType.RNG): x.AsPerk).ToArray());
        public PerkTree ClassPerkTree => new(classPerks.Select(x => x.UseDefaultCoinType ? x.ToPerk(CoinType.Class) : x.AsPerk).ToArray());
    }

    [System.Serializable]
    public class SerializedPerk {
        [SerializeField]
        string id;
        [SerializeField]
        string name;
        [SerializeField]
        string rawDescription;
        [field: SerializeField, Tooltip("Whether the type of coin should use the coin type according to a perk tree's category")]
        public bool UseDefaultCoinType { get; private set; } = true;
        [SerializeField]
        CoinType coinType;
        [SerializeField]
        Attribute[] stats;

        [SerializeField]
        uint maxLevel;
        [SerializeField]
        Dependency[] serializedDependencies;
        [SerializeField]
        string[] exclusions;
        [SerializeField]
        Cost[] serializedCosts;

        public Perk ToPerk(CoinType coinType) {
            PerkAttribute[] attributes = new PerkAttribute[stats.Length];
            for (int i = 0; i < stats.Length; i++)
                attributes[i] = stats[i].type switch {
                    PerkAttributeType.Integer => new IntAttribute(stats[i].name, stats[i].values.Select(x => (int)x).ToArray()),
                    PerkAttributeType.Percentage => new PercentageAttribute(stats[i].name, stats[i].values.Select(x => x / 100).ToArray()),
                    _ => new DecimalAttribute(stats[i].name, stats[i].values)
                };  
            Progression.Dependency[] dependencies = new Progression.Dependency[serializedDependencies.Length];
            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = new Progression.Dependency(serializedDependencies[i].id, serializedDependencies[i].type);
            (uint tier, uint value)[] costs = new (uint, uint)[serializedCosts.Length];
            for (int i = 0; i < costs.Length; i++) {
                costs[i].tier = serializedCosts[i].tier;
                costs[i].value = serializedCosts[i].value;
            }
            return new Perk(id, name, rawDescription, new(attributes), coinType, costs, maxLevel, dependencies, exclusions);
        }
        public Perk AsPerk => ToPerk(coinType);

        [System.Serializable]
        class Attribute {
            public PerkAttributeType type;
            public string name;
            [Tooltip("Make sure you use the data type specified in Type")]
            public float[] values;
        }
        [System.Serializable]
        class Dependency {

            public string id;
            public DependencyType type;
        }
        [System.Serializable]
        class Cost {
            public uint tier;
            public uint value;
        }
    }
    public enum PerkAttributeType {
        Integer, Decimal, Percentage
    }
}