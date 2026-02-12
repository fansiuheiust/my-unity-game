using NUnit.Framework;

using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem.Controls;

namespace Progression {
    public class PerkTree {
        readonly Perk[] perks;
        public PerkTree(Perk[] perks) {
            if (perks.Any(x => perks.Any(y => (x != y && x.id == y.id))))
                throw new System.Exception("2 perks cannot have the same ID.");
            this.perks = perks;
        }

        public bool Unlockable(Perk toCheck) {
            if (toCheck == null) return false;
            if (!perks.Contains(toCheck)) return false;
            foreach (Dependency d in toCheck.dependencies) {
                Perk dependent = perks.Where(x => x.id == d.id).FirstOrDefault();
                if (dependent == null) return false; // it is impossible to unlock if the dependency is not a valid perk anyways
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
            return true;
        }

        public bool Unlockable(string id) =>
            Unlockable(perks.Where(x => x.id == id).FirstOrDefault());

        public void LevelUp(Perk p) {
            if (p == null) return;
            if (!perks.Contains(p)) return;
            if (!Unlockable(p)) return;
            p.LevelUp();
        }
        public void LevelUp(string id) =>
            LevelUp(perks.Where(x => x.id == id).FirstOrDefault());
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
        public int Level { get; private set; } = 0;
        public readonly int maxLevel;
        public readonly Dependency[] dependencies;
        public Perk(string id, int maxLevel = 1, params Dependency[] dependencies) {
            this.id = id;
            this.maxLevel = maxLevel;
            this.dependencies = dependencies;
        }
           
        public void LevelUp() {
            Level++;
        }
    }
}
