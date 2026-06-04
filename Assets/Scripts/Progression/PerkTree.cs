using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using Unity.VisualScripting;

namespace Progression {
    public class PerkTree {
        readonly Dictionary<string, Perk> perks = new();
        public PerkTree(Perk[] perks) {
            if (perks.Any(x => perks.Any(y => (x != y && x.id == y.id))))
                throw new System.Exception("2 perks cannot have the same ID.");
            foreach (Perk p in perks)
                this.perks.Add(p.id, p);
        }

        public bool Contains(string id) => perks.ContainsKey(id);

        public bool Unlockable(Perk toCheck) {
            if (toCheck == null) return false;
            if (!perks.ContainsKey(toCheck.id)) return false;
            if (toCheck.Level == toCheck.maxLevel) return false;
            foreach (Dependency d in toCheck.dependencies) {
                if (!perks.ContainsKey(d.id)) return false; // it is impossible to unlock if the dependency is not a valid perk anyways
                Perk dependent = perks[d.id];
                switch (d.type) {
                    case DependencyType.Existential:
                        if (dependent.Level == 0) return false;
                        break;
                    case DependencyType.Max:
                        if (dependent.maxLevel != dependent.Level) return false;
                        break;
                    case DependencyType.Levelled:
                        if (dependent.Level <= toCheck.Level && dependent.Level != dependent.maxLevel) return false;
                        break;
                }
            }
            foreach (string e in toCheck.exclusions) {
                if (perks.ContainsKey(e) && Unlocked(e)) return false;
            }
            return true;
        }

        public bool Unlockable(string id) {
            if (!perks.ContainsKey(id)) return false;
            return Unlockable(perks[id]);
        }

        public void LevelUp(Perk p) {
            if (p == null) return;
            if (!perks.ContainsKey(p.id)) return;
            if (!Unlockable(p)) return;
            p.LevelUp();
        }
        public void LevelUp(string id) =>
            LevelUp(perks[id]);

        /// <summary>
        /// returns the perk by perk ID
        /// </summary>
        /// <param name="id">ID of the returned perk</param>
        public Perk this[string id] => perks[id];
        /// <summary>
        /// Whether a perk has been unlocked or not (locked means level 0 here)
        /// </summary>
        /// <param name="id">ID of the perk</param>
        public bool Unlocked(string id) => perks[id].Level != 0;
        /// <summary>
        /// level of each unlocked perk
        /// </summary>
        public Dictionary<string, uint> PerkData => perks.Where(x=>x.Value.Level != 0).ToDictionary(x=>x.Key, x=>x.Value.Level);
    }

    /// <summary>
    /// <c>Existential</c>: Unlocked when the dependency is at least level 1<br />
    /// <c>Max</c>: Unlocked when the dependency is maxxed<br />
    /// <c>Levelled</c>: Each level in dependency unlocks a level, full unlock if dependency's level is maxxed<br />
    /// </summary>
    public enum DependencyType {
        Existential, Max, Levelled
    }
    public readonly struct Dependency {
        public readonly string id;
        public readonly DependencyType type;
        public Dependency(string id, DependencyType type) {
            this.id = id;
            this.type = type;
        }
    }
    /// <summary>
    /// A single perk
    /// </summary>
    public class Perk {
        public readonly string id;
        public readonly string name = "";
        /// <summary>
        /// Description that includes the {statname}
        /// </summary>
        public readonly string rawDescription = "";
        /// <summary>
        /// Type of coin to update this perk
        /// </summary>
        public readonly CoinType type;
        readonly PerkStats stats = new();
        public uint Level { get; private set; } = 0;
        public readonly uint maxLevel;
        public readonly Dependency[] dependencies;
        /// <summary>
        /// A list of IDs of abilities that this ability cannot be unlocked (level>0) with simultaneously
        /// </summary>
        public readonly string[] exclusions = new string[0];
        /// <summary>
        /// How much coin of rarity is needed to level up for level [l+1], 
        /// </summary>
        readonly (uint tier, uint value)[] costs;
        /// <summary>
        /// should only be used for testing purpose
        /// </summary>
        internal Perk(string id, uint maxLevel = 1, params Dependency[] dependencies) {
            this.id = id;
            this.maxLevel = maxLevel;
            this.dependencies = dependencies;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawDescription">use {} to encapsulate attributes by their specified name. e.g. Gain {Extra Loot} more loots.</param>
        public Perk(string id, string name, string rawDescription, PerkStats stats, CoinType type, (uint tier, uint value)[] costs, uint maxLevel, Dependency[] dependencies, string[] exclusions): this(id, maxLevel, dependencies) {
            if (costs.Length != maxLevel) throw new System.Exception("Number of costs must be equal to the number of levels for perk " + id);
            this.costs = costs;
            this.name = name;
            this.rawDescription = rawDescription;
            this.type = type;
            this.stats = stats;
            this.exclusions = exclusions.ToArray();
        }

        /// <summary>
        /// Levels up a perk, bypassing dependency/exclusion check
        /// </summary>
        /// <param name="level">number of level to add</param>
        internal void LevelUp(uint level = 1) {
            if ((Level+=level) > maxLevel)
                Level = maxLevel;
        }

        /// <summary>
        /// Cost to level up once
        /// </summary>
        public (CoinType type, uint tier, uint value) Cost => (type, costs[Level].tier, costs[Level].value); // say I want to level up to l, i should read l-1, which is just Level
        public (CoinType type, uint tier, uint value) CostAt(uint level) => (type, costs[level - 1].tier, costs[level - 1].value);

        /// <summary>
        /// Returns a Perk's attribute at its level
        /// </summary>
        /// <param name="name">name of the attribute</param>
        public float this[string name] => stats[name].Value(Level);
    }
}
