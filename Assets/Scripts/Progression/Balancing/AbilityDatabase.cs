using Combat;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Progression.Balance {
    public static class AbilityDatabase {
        /// <summary>
        /// List of <c>AbilityObject</c>s hashed by its identifier
        /// </summary>
        public static readonly Dictionary<string, System.Type> abilityObjects;

        /// <summary>
        /// List of <c>Ability</c>'s hashed by its ID
        /// </summary>

        public static readonly Dictionary<string, Combat.Ability> abilities;
        static AbilityDatabase() {
            abilityObjects = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(Combat.AbilityObject).IsAssignableFrom(t))
                .ToDictionary(t => t.Name, t => t);
            AbilityData data = (AbilityData)Resources.Load($"Data/Abilities/Default");
            abilities = data.Abilities.ToDictionary(x=>x.id, x=>x);
            foreach (Ability x in data.PerkAbilities)
                abilities.Add(x.id, x);
        }
    }
}