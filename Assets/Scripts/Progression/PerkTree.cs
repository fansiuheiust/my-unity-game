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
                if (perks.ContainsKey(e) && perks[e].Level != 0) return false;
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
        public readonly PerkStats stats = new();
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
        }

        public void LevelUp() {
            if (++Level > maxLevel)
                Level--;
        }

        /// <summary>
        /// Cost to level up once
        /// </summary>
        public (CoinType type, uint tier, uint value) Cost => (type, costs[Level].tier, costs[Level].value); // say I want to level up to l, i should read l-1, which is just Level
        public (CoinType type, uint tier, uint value) CostAt(uint level) => (type, costs[level - 1].tier, costs[level - 1].value);
    }

    public static class Driver {
        public static void Main() {
            Perk[] perks = new Perk[] {
                new("a", 10),
                new("b", 4, new Dependency("a", DependencyType.Existential)),
                new("c", 3, new Dependency("a", DependencyType.Max)),
                new("d", 8, new Dependency("b", DependencyType.Levelled), new Dependency("c", DependencyType.Levelled)),
            };
            PerkTree pt = new(perks);
            Debug.Assert(pt.Unlockable("b") == false && pt.Unlockable("c") == false);
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            Debug.Assert(pt.Unlockable("b") == true && pt.Unlockable("c") == false);
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            pt.LevelUp("a");
            Debug.Assert(pt.Unlockable("b") == true && pt.Unlockable("c") == true);

            Debug.Assert(pt.Unlockable("d") == false);
            pt.LevelUp("b");
            Debug.Assert(pt.Unlockable("d") == false);
            pt.LevelUp("c");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == false);
            pt.LevelUp("c");
            pt.LevelUp("c");
            Debug.Assert(pt.Unlockable("d") == false);
            pt.LevelUp("b");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == false);
            pt.LevelUp("b");
            pt.LevelUp("b");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == true);
            pt.LevelUp("d");
            Debug.Assert(pt.Unlockable("d") == false);

            Perk p = new("tester", "Tester", "Lorem {Targets} Ipsum {Raw damage} {Bonus damage}", new PerkStats(
                new IntAttribute("Targets", 3, 5, 10),
                new DecimalAttribute("Raw damage", 12f, 15.5f, 22.77f),
                new PercentageAttribute("Bonus damage", 0.1f, 0.3f, 0.6f)
                ), CoinType.Floor,
                new (uint, uint)[3] {
                    (0, 3),
                    (0, 9),
                    (1, 4)
                }, 3, 
                new Dependency[] { new Dependency("prereq", DependencyType.Existential) }, new string[0]);
        }
    }
}
