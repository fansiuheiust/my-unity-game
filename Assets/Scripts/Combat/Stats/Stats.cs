using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



namespace Combat {
    // for every field addition in BaseStats, follow this:
    // 1. add field
    // 2. modify ctor in BOTH parameter and body initializaiton
    // 3. modify the +/- operator overload
    // 4. modify ScalingStats' ctor in BOTH paramter and MIL
    // 5. modify ScalingStats' +/-/* operator overload
    // 6. modify FinalStats ctor
    // 7. modify FinalStats Normalize()
    // NO! The stats implemented here are the most primitive stats
    // TODO: implement the same thing for BaseStats when it is needed
    // from now on, you just need:
    // 1: create new element in enum
    // 2: set the lower and upper bound of the value in FinalStats

    [System.Serializable]

    public enum BaseAttribute {
        Atk, Def, MaxHp, MaxMana, ManaRegen
    }

    public enum ScalingAttribute {
        WalkSpeed, AtkSpeed, CritRate, CritDmg, DmgReduction, Knockback, KnockbackResistance, PhysicalDmg, ProjectileDmg, MagicDmg, AttackRange, ManaCostReduction
    }


    /// <summary>
    /// The stats that are presented as numbers
    /// </summary>
    [System.Serializable]
    public class BaseStats {
        public event System.Action<BaseAttribute> OnBaseStatChange;

        public static readonly float BaseEpsilon = 0.0625f;
        protected virtual float Epsilon => BaseEpsilon;

        protected readonly Dictionary<BaseAttribute, float> baseStats = new();

        public BaseStats(Dictionary<BaseAttribute, float> baseStats = null) {
            if (baseStats != null)
                this.baseStats = baseStats.ToDictionary(d=>d.Key, d=>d.Value);
        }

        /// <summary>
        /// deep copy
        /// </summary>
        /// <returns></returns>
        public BaseStats Clone() {
            return new BaseStats(baseStats);
        }

        public float this[BaseAttribute stat] {
            get => baseStats.ContainsKey(stat) ? baseStats[stat] : 0;
            protected set {
                if (baseStats.ContainsKey(stat)) baseStats[stat] = value;
                else baseStats.Add(stat, value);

                if (Mathf.Abs(this[stat]) < Epsilon && baseStats.ContainsKey(stat))
                    baseStats.Remove(stat);
            }
        }

        public void Gain(BaseAttribute stat, float value) {
            if (Mathf.Abs(value) > Epsilon) {
                this[stat] += value;

                OnBaseStatChange?.Invoke(stat);
            }
        }

        public void Lose(BaseAttribute stat, float value) => Gain(stat, -value);

        public void Gain(BaseStats other) {
            foreach (var x in other.baseStats) {
                Gain(x.Key, x.Value);
            }
        }
        public void Lose(BaseStats other) {
            foreach (var x in other.baseStats) {
                Lose(x.Key, x.Value);
            }
        }

        /*
        public static BaseStats operator +(BaseStats a, BaseStats b) {
            BaseStats ri = a.Clone();
            ri.Gain(b);
            return ri;
        }
        public static BaseStats operator -(BaseStats a, BaseStats b) {
            BaseStats ri = a.Clone();
            ri.Lose(b);
            return ri;
        }
        */
        public static BaseStats operator *(float a, BaseStats b) {
            Dictionary<BaseAttribute, float> other = new();
            foreach (BaseAttribute stat in typeof(BaseAttribute).GetEnumValues()) {
                float result = a * b[stat];
                if (Mathf.Abs(result) > BaseEpsilon)
                    other.Add(stat, result);
            }
            return new(other);
        }
    }

    /// <summary>
    /// The stats that are used as percentage
    /// </summary>
    [System.Serializable]
    public class ScalingStats : BaseStats {
        public event System.Action<ScalingAttribute> OnScalingStatChange;

        public static readonly float ScalingEpsilon = 0.00006103515625f;

        protected override float Epsilon => ScalingEpsilon;

        readonly Dictionary<ScalingAttribute, float> scalingStats = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="atk">Self-documenting</param>
        /// <param name="def">Self-documenting</param>
        /// <param name="maxHp">Self-documenting</param>
        /// <param name="maxMana">Self-documenting</param>
        /// <param name="manaRegen">Self-documenting</param>
        /// <param name="walkSpeed">Self-documenting</param>
        /// <param name="atkSpeed">Self-documenting</param>
        /// <param name="critRate">Self-documenting</param>
        /// <param name="critDmg">Self-documenting</param>
        /// <param name="dmgReduction">Self-documenting</param>
        /// <param name="knockback">Self-documenting</param>
        /// <param name="knockbackResistance">Self-documenting</param>
        /// <param name="otherScaling">Hashed scaling stats. If not given an argument, an new one will be allocated.</param>
        public ScalingStats(Dictionary<BaseAttribute, float> baseStats = null, Dictionary<ScalingAttribute, float> otherScaling = null) : base(baseStats) {
            if (otherScaling is not null)
                scalingStats = otherScaling.ToDictionary(x=>x.Key, x=>x.Value);
        }

        public new ScalingStats Clone() {
            return new ScalingStats(baseStats, scalingStats);
        }

        public void Gain(ScalingAttribute stat, float value) {
            if (Mathf.Abs(value) > Epsilon) {
                this[stat] += value;
                OnScalingStatChange?.Invoke(stat);
            }
        }
        public void Lose(ScalingAttribute stat, float value) => Gain(stat, -value);
        public void Gain(ScalingStats other) {
            Gain((BaseStats)other);
            foreach (var x in other.scalingStats) {
                Gain(x.Key, x.Value);
            }
        }
        public void Lose(ScalingStats other) {
            Lose((BaseStats)other);
            foreach (var x in other.scalingStats) {
                Lose(x.Key, x.Value);
            }
        }


        /*
        public static ScalingStats operator +(ScalingStats a, ScalingStats b) {
            ScalingStats ri = a.Clone();
            ri.Gain(b);
            return ri;
        }
        public static ScalingStats operator -(ScalingStats a, ScalingStats b) {
            ScalingStats ri = a.Clone();
            ri.Gain(b);
            return ri;
        }
        */
        public static FinalStats operator *(BaseStats a, ScalingStats b) => new(a, b);


        public float this[ScalingAttribute stat] {
            get => scalingStats.ContainsKey(stat) ? scalingStats[stat] : 0;
            protected set {
                if (scalingStats.ContainsKey(stat)) scalingStats[stat] = value;
                else scalingStats.Add(stat, value);

                if (Mathf.Abs(this[stat]) < Epsilon && scalingStats.ContainsKey(stat))
                    scalingStats.Remove(stat);
            }
        }
    }

    /// <summary>
    /// The stats obtained by scaling the BaseStats by the (1+percentage), and keeping the percentage stats the same
    /// </summary>
    [System.Serializable]
    public class FinalStats {
        readonly BaseStats @base;
        readonly ScalingStats scaling;
        readonly Dictionary<BaseAttribute, float> baseStats = new();
        readonly Dictionary<ScalingAttribute, float> scalingStats = new();
        public FinalStats(BaseStats @base, ScalingStats scale) {
            this.@base = @base;
            scaling = scale;
            foreach (BaseAttribute baseStat in typeof(BaseAttribute).GetEnumValues()) {
                Compute(baseStat);
            }
            foreach (ScalingAttribute scalingStat in typeof(ScalingAttribute).GetEnumValues()) {
                Compute(scalingStat);
            }

            @base.OnBaseStatChange += Compute;
            scaling.OnBaseStatChange += Compute;
            scaling.OnScalingStatChange += Compute;
        }

        /// <summary>
        /// Recomputes for a signle attribute
        /// </summary>
        /// <param name="att">attribute to recompute</param>
        public void Compute(BaseAttribute att) {
            float result = @base[att] > 0 ? @base[att] * Mathf.Max(0, 1 + scaling[att]) : @base[att] * Mathf.Max(0f, 1 - scaling[att]);
            result = (!BaseMins.ContainsKey(att) || result > BaseMins[att]) ?
                (!BaseMaxs.ContainsKey(att) || result < BaseMaxs[att]) ?
                    result :
                BaseMaxs[att] :
            BaseMins[att];
            // cases with action: larger than epsilon and not in dict, larger than epsilon and in dict, smaller than epsilon and in dict
            if (Mathf.Abs(result) > BaseStats.BaseEpsilon) {
                if (baseStats.ContainsKey(att))
                    baseStats[att] = result;
                else
                    baseStats.Add(att, result);
            } else if (baseStats.ContainsKey(att))
                baseStats.Remove(att);
        }

        public void Compute(ScalingAttribute att) {
            float result = scaling[att];
            result = (!ScalingMins.ContainsKey(att) || result > ScalingMins[att]) ?
                (!ScalingMaxs.ContainsKey(att) || result < ScalingMaxs[att]) ?
                    result :
                ScalingMaxs[att] :
            ScalingMins[att];
            if (Mathf.Abs(result) > ScalingStats.ScalingEpsilon) {
                if (scalingStats.ContainsKey(att))
                    scalingStats[att] = result;
                else
                    scalingStats.Add(att, result);
            } else if (scalingStats.ContainsKey(att))
                scalingStats.Remove(att);
        }

        public static readonly Dictionary<BaseAttribute, float> BaseMins = new() {
            { BaseAttribute.Atk, 0 },
            { BaseAttribute.Def, 0 },
            { BaseAttribute.MaxHp, 1 },
            { BaseAttribute.MaxMana, 1 }
        }, BaseMaxs = new() {

        };

        public static readonly Dictionary<ScalingAttribute, float> ScalingMins = new() {
            { ScalingAttribute.WalkSpeed, -1 },
            { ScalingAttribute.AtkSpeed, -0.5f },
            { ScalingAttribute.CritRate, 0 },
            { ScalingAttribute.CritDmg, -1 },
            { ScalingAttribute.Knockback, -1 },
            {ScalingAttribute.KnockbackResistance, -10 },
            { ScalingAttribute.PhysicalDmg, -1 },
            { ScalingAttribute.ProjectileDmg, -1 },
            { ScalingAttribute.MagicDmg, -1 },
            { ScalingAttribute.AttackRange, -0.75f },
        }, ScalingMaxs = new() {
            { ScalingAttribute.WalkSpeed, 10 },
            { ScalingAttribute.AtkSpeed, 1 },
            { ScalingAttribute.DmgReduction, 0.96875f },
            { ScalingAttribute.Knockback, 10 },
            { ScalingAttribute.KnockbackResistance, 1 },
            { ScalingAttribute.AttackRange, 1.5f },
            { ScalingAttribute.ManaCostReduction, 0.95f }
        };

        public float this[BaseAttribute stat] => baseStats.ContainsKey(stat) ? baseStats[stat] : 0;
        public float this[ScalingAttribute stat] => scalingStats.ContainsKey(stat)? scalingStats[stat]: 0;

        public float Crit { get => Random.value < this[ScalingAttribute.CritRate] ? this[ScalingAttribute.CritDmg] : 0; }
    }
}

