using Progression;
using UnityEngine;
using UnityEngine.UIElements;

namespace Combat {

    /// <summary>
    /// <c>Damage</c>: Ability that deals damage on the enemy, triggered by pressing <c>E</c><br />
    /// <c>Ultimate</c>: Ability that requires charging, triggered by pressing <c>Q</c><br />
    /// <c>Movement</c>: Ability that moves the user, triggered by pressing <c>X</c><br />
    /// <c>Misc</c>: Miscalleneous abilities, e.g. buffing self, debuffing enemy, etc, triggered by pressing <c>C</c><br />
    /// </summary>
    public enum AbilityTriggerKey {
        Damage, Ultimate, Movement, Misc, None
    }
    public class Ability {
        public readonly string id;
        public readonly string name;
        readonly string rawDescription;
        readonly float cooldown;
        readonly float manaCost;
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

        public virtual float Cooldown => cooldown;
        public virtual float ManaCost => manaCost;

        /// <summary>
        /// Returns an attribute's value
        /// </summary>
        /// <param name="name">name of the attribute</param>
        public virtual float this[string name] => stats[name].Value();
    }

    public class PerkAbility: Ability {
        public readonly Perk perk;
        public PerkAbility(Perk perk, AbilityTriggerKey triggerKey, System.Type ability): base(perk.id, perk.name, perk.rawDescription, -1, -1, triggerKey, null, ability) {
            this.perk = perk;
        }
        public override float this[string name] => perk[name];

        public override float Cooldown => perk.ContainsAttribute("Cooldown") ?perk["Cooldown"]: 0;
        public override float ManaCost => perk.ContainsAttribute("ManaCost")? perk["ManaCost"]: 0;

    }
}