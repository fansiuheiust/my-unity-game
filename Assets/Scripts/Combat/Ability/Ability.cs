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
        public readonly AbilityTriggerKey triggerKey;
        readonly Stats stats;
        
        /// <summary>
        /// Note that <c>stats</c> will be reference-copied
        /// </summary>
        /// <param name="stats">will be reference-copied</param>
        public Ability(string id, string name, string rawDescription, float cooldown, AbilityTriggerKey triggerKey, Stats stats) {
            this.id = id;
            this.name = name;
            this.rawDescription = rawDescription;
            this.cooldown = cooldown;
            this.triggerKey = triggerKey;
            this.stats = stats;
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
        public PerkAbility(string id, string name, string rawDescription, float cooldown, AbilityTriggerKey triggerKey, Perk perk): base(id, name, rawDescription, cooldown, triggerKey, null) {
            this.perk = perk;
        }
        public override float this[string name] => perk[name];
    }
}