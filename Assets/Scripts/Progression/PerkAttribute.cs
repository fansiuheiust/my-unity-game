using UnityEngine;
using System.Text.Json;
using System.Collections.Generic;
using NUnit.Framework.Internal;

namespace Progression {

    /// <summary>
    /// Basically a collection of perk attributes
    /// </summary>
    public class PerkStats {
        readonly Dictionary<string, PerkAttribute> attributes = new();
        /// <summary>
        /// Note that every attribute in <c>attributes</c> will be owned by the instantiated <c>PerkStats</c>.
        /// </summary>
        /// <param name="attributes"></param>
        public PerkStats(params PerkAttribute[] attributes) {
            foreach (var attr in attributes) {
                this.attributes.Add(attr.name, attr);
            }
        }
        public bool Contains(string name) => attributes.ContainsKey(name);
        public PerkAttribute this[string name] => attributes[name];
    }
    public abstract class PerkAttribute {
        public readonly string name;
        public PerkAttribute(string name) {
            this.name = name;
        }
        /// <summary>
        /// value of the attribute to display at a level
        /// </summary>
        public abstract string ValueInString(uint level);
        /// <summary>
        /// value of the attribute at a level
        /// </summary>
        public abstract float Value(uint level);
    }

    public class IntAttribute: PerkAttribute {
        protected readonly int[] values;
        public IntAttribute(string name, params int[] values): base(name) {
            this.values = values;
        }
        public override string ValueInString(uint level) => values[level-1].ToString();
        public override float Value(uint level) => values[level-1];
    }

    public class DecimalAttribute : PerkAttribute {
        protected readonly float[] values;
        public DecimalAttribute(string name, params float[] values) : base(name) {
            this.values = values;
        }
        public override string ValueInString(uint level) => $"{values[level - 1]:F2}";
        public override float Value(uint level) => values[level - 1];
    }

    public class PercentageAttribute: DecimalAttribute {
        public PercentageAttribute(string name, params float[] values) : base(name, values) {
        }
        public override string ValueInString(uint level) => $"{(values[level - 1] * 100):F2}%";
    }

}