using UnityEngine;
using System.Text.Json;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using System.Linq;

namespace Progression {


    
    /// <summary>
    /// Basically a collection of perk attributes, note that this is DIFFERENT from <c>Combat.Stats</c>
    /// </summary>
    public class Stats {
        readonly Dictionary<string, Attribute> attributes = new();
        /// <summary>
        /// Note that every attribute in <c>attributes</c> will be owned by the instantiated <c>PerkStats</c>.
        /// </summary>
        /// <param name="attributes"></param>
        public Stats(params Attribute[] attributes) {
            foreach (var attr in attributes) {
                this.attributes.Add(attr.name, attr);
            }
        }
        public bool Contains(string name) => attributes.ContainsKey(name);
        public Attribute this[string name] => attributes[name];
    }

    public abstract class Attribute {
        public readonly string name;
        public Attribute(string name) {
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
        /// <summary>
        /// value of the attribute to display at level 1
        /// </summary>
        public string ValueInString() => ValueInString(1);
        /// <summary>
        /// value of hte attribute at level 1
        /// </summary>
        public float Value() => Value(1);

        /// <summary>
        /// Whether the attribute only has 1 value
        /// </summary>
        public abstract bool IsConstant { get; }
    }

    public class IntAttribute: Attribute {
        protected readonly int[] values;
        public IntAttribute(string name, params int[] values): base(name) {
            this.values = values.ToArray();
        }
        public override string ValueInString(uint level) => IsConstant? values[0].ToString(): values[level-1].ToString();
        public override float Value(uint level) => IsConstant? values[0]: values[level-1];

        public override bool IsConstant => values.Length == 1;
    }

    public class DecimalAttribute : Attribute {
        protected readonly float[] values;
        public DecimalAttribute(string name, params float[] values) : base(name) {
            this.values = values.ToArray();
        }
        public override string ValueInString(uint level) => $"{(IsConstant ? values[0]: values[level - 1]):F2}";
        public override float Value(uint level) => IsConstant ? values[0] : values[level - 1];
        public override bool IsConstant => values.Length == 1;
    }

    public class PercentageAttribute: DecimalAttribute {
        public PercentageAttribute(string name, params float[] values) : base(name, values) {
        }
        public override string ValueInString(uint level) => $"{((IsConstant ? values[0] : values[level - 1]) * 100):F0}%";
    }

}