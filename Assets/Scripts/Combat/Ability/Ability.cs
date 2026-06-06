using Progression;
using UnityEngine;


namespace Combat {

    public enum AbilityTriggerKey {
        E, Q, X, C, None
    }
    public class Ability {
        public readonly string id;
        public readonly string name;
        readonly string rawDescription;
        public readonly float cooldown;
        public readonly float manaCost;
        public readonly AbilityTriggerKey triggerKey;
        readonly Stats stats;
        public readonly System.Type ability;
        
        /// <summary>
        /// Note that <c>stats</c> will be reference-copied
        /// </summary>
        /// <param name="stats">will be reference-copied</param>
        public Ability(string id, string name, string rawDescription, float cooldown, float manaCost, AbilityTriggerKey triggerKey, Stats stats, System.Type ability) {
            this.id = id;
            this.name = name;
            this.rawDescription = rawDescription;
            this.cooldown = cooldown;
            this.manaCost = manaCost;
            this.triggerKey = triggerKey;
            this.stats = stats;
            this.ability = ability;
        }

        /// <summary>
        /// Returns an attribute's value
        /// </summary>
        /// <param name="name">name of the attribute</param>
        public virtual float this[string name] => stats[name].Value();
    }

    public class PerkAbility: Ability {
        public readonly Perk perk;
        /// <summary>
        /// </summary>
        public PerkAbility(string id, string name, string rawDescription, float cooldown, float manaCost, AbilityTriggerKey triggerKey, Perk perk, System.Type type): base(id, name, rawDescription, cooldown, manaCost, triggerKey, null, type) {
            this.perk = perk;
        }
        public override float this[string name] => perk[name];
    }
}