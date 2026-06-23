
using NUnit.Framework;
using Progression;
using System.ComponentModel.Design.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

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
        [field: SerializeField, Min(1f), Tooltip("For rooms of the same rarity tier (floor 2 from 1, floor 4 from 3, ...), how much more times should perk cost from previous floor")]
        public float FloorPerkCostMultiplier { get; private set; }
        public PerkTree FloorPerkTree {
            get {
                List<SerializedPerk> perks = new();
                for (uint i =1; i <= 9; i++) { // note that no perks can be used for the last floor
                    perks.AddRange(floorPerks.Select(x => x.Modified(i.ToString(), $" (floor {i})", (i-1)/2, i%2==1? 1: FloorPerkCostMultiplier)));
                }
                return new(perks.Select(x => x.UseDefaultCoinType ? x.ToPerk(CoinType.Floor) : x.AsPerk).ToArray());
            }
        }
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
            Progression.Attribute[] attributes = new Progression.Attribute[stats.Length];
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
        /// <summary>
        /// Returns the perk but with all names, including those in dependencies and exclusions, appended, deep copies everything
        /// </summary>
        /// <param name="additionID">what should be appended for the perk IDs</param>
        /// <param name="additionName">what should be appended for the perk names</param>
        /// <param name="costTierUp">How much tier should the cost be upped</param>
        /// <param name="costMultiplier">How much should the cost be multiplied wtih</param>
        public SerializedPerk Modified(string additionID, string additionName, uint costTierUp = 0, float costMultiplier = 1) {
            SerializedPerk newPerk = (SerializedPerk)MemberwiseClone();
            newPerk.id += additionID;
            newPerk.name += additionName;
            newPerk.stats = newPerk.stats.Select(x=>new Attribute { type = x.type, name = x.name, values = x.values.ToArray() }).ToArray();
            newPerk.serializedDependencies = newPerk.serializedDependencies.Select(x => new Dependency() { id = x.id + additionID, type = x.type }).ToArray();
            newPerk.exclusions = newPerk.exclusions.Select(x=>x+additionID).ToArray();
            newPerk.serializedCosts = newPerk.serializedCosts.Select(x => new Cost() { tier = x.tier + costTierUp, value = (uint)Mathf.RoundToInt(x.value * costMultiplier) }).ToArray();
            return newPerk;
        }

        public Perk AsPerk => ToPerk(coinType);

        [System.Serializable]
        class Attribute {
            public string name;
            public PerkAttributeType type;
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