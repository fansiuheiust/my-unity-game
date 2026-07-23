using Progression;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        Damage, Ultimate, Movement, Misc, Weapon, None
    }
    public class Ability {
        public readonly string id;
        public readonly string name;
        public readonly string rawDescription;
        readonly float cooldown;
        readonly float manaCost;
        public readonly AbilityTriggerKey triggerKey;
        readonly Stats stats;
        public readonly System.Type abilityObject;

        readonly Dictionary<string, GameObject> prefabs = new();
        
        /// <summary>
        /// Note that <c>stats</c> will be reference-copied
        /// </summary>
        /// <param name="stats">will be reference-copied</param>
        public Ability(string id, string name, string rawDescription, float cooldown, float manaCost, AbilityTriggerKey triggerKey, Stats stats, System.Type abilityObject, Dictionary<string, GameObject> prefabs) {
            this.id = id;
            this.name = name;
            this.rawDescription = rawDescription;
            this.cooldown = cooldown;
            this.manaCost = manaCost;
            this.triggerKey = triggerKey;
            this.stats = stats;
            this.abilityObject = abilityObject;
            if (prefabs is not null)
                this.prefabs.AddRange(prefabs);
        }

        public GameObject Prefab(string name) => prefabs[name];

        public virtual float Cooldown => cooldown;
        public virtual float ManaCost => manaCost;

        /// <summary>
        /// Returns an attribute's value
        /// </summary>
        /// <param name="name">name of the attribute</param>
        public float this[string name] => Attribute(name);

        public virtual float Attribute(string name) => stats[name].Value();

        public virtual string AttributeString(string name) => stats[name].ValueInString();
    }

    public class PerkAbility: Ability {
        public readonly Perk perk;
        public PerkAbility(Perk perk, AbilityTriggerKey triggerKey, System.Type ability, Dictionary<string, GameObject> prefabs): base(perk.id, perk.name, perk.rawDescription, -1, -1, triggerKey, null, ability, prefabs) {
            this.perk = perk;
        }
        public override float Attribute(string name) => perk[name];

        public override string AttributeString(string name) => perk.AttributeString(name);

        public override float Cooldown => perk.ContainsAttribute("Cooldown") ?perk["Cooldown"]: 0;
        public override float ManaCost => perk.ContainsAttribute("Mana Cost")? perk["Mana Cost"]: 0;

    }
}