using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    public enum HashedBaseStats {
        Atk, Def, MaxHp, MaxMana, ManaRegen
    }

    public enum HashedScalingStats {
        WalkSpeed, AtkSpeed, CritRate, CritDmg, DmgReduction, Knockback, KnockbackResistance, PhysicalDmg, ProjectileDmg, MagicDmg, AttackRange, ManaCostReduction
    }


    /// <summary>
    /// The stats that are presented as numbers
    /// </summary>
    [System.Serializable]
    public class BaseStats {

        public static readonly float BaseEpsilon = 0.0625f;

        protected readonly Dictionary<HashedBaseStats, float> baseStats = new();

        public BaseStats(Dictionary<HashedBaseStats, float> baseStats = null) {
            if (baseStats != null)
                this.baseStats = baseStats.ToDictionary(d=>d.Key, d=>d.Value);
        }

        /// <summary>
        /// deep copy
        /// </summary>
        /// <returns></returns>
        public BaseStats Clone() {
            return 1 * this;
        }

        public float this[HashedBaseStats stat] {
            get => baseStats.ContainsKey(stat) ? baseStats[stat] : 0;
            protected set {
                if (baseStats.ContainsKey(stat)) baseStats[stat] = value;
                else baseStats.Add(stat, value);
            }
        }

        public static BaseStats operator +(BaseStats a, BaseStats b) {
            Dictionary<HashedBaseStats, float> other = new();
            foreach (HashedBaseStats stat in typeof(HashedBaseStats).GetEnumValues()) {
                float result = a[stat] + b[stat];
                if (Mathf.Abs(result) > BaseEpsilon)
                    other.Add(stat, result);
            }
            return new(other);
        }
        public static BaseStats operator -(BaseStats a, BaseStats b) {
            Dictionary<HashedBaseStats, float> other = new();
            foreach (HashedBaseStats stat in typeof(HashedBaseStats).GetEnumValues()) {
                float result = a[stat] - b[stat];
                if (Mathf.Abs(result) > BaseEpsilon)
                    other.Add(stat, result);
            }
            return new(other);
        }
        public static BaseStats operator *(float a, BaseStats b) {
            Dictionary<HashedBaseStats, float> other = new();
            foreach (HashedBaseStats stat in typeof(HashedBaseStats).GetEnumValues()) {
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

        public static readonly float ScalingEpsilon = 0.00006103515625f;
        readonly Dictionary<HashedScalingStats, float> scalingStats = new();

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
        public ScalingStats(Dictionary<HashedBaseStats, float> baseStats = null, Dictionary<HashedScalingStats, float> otherScaling = null) : base(baseStats) {
            if (otherScaling is not null)
                scalingStats = otherScaling.ToDictionary(x=>x.Key, x=>x.Value);
        }

        public new ScalingStats Clone() {
            return new ScalingStats(baseStats, scalingStats);
        }

        public float Crit { get => Random.value < this[HashedScalingStats.CritRate] ? this[HashedScalingStats.CritDmg] : 0; }
        public static ScalingStats operator +(ScalingStats a, ScalingStats b) {
            Dictionary<HashedBaseStats, float> otherBase = new();
            foreach (HashedBaseStats stat in typeof(HashedBaseStats).GetEnumValues()) {
                float result = a[stat] + b[stat];
                if (Mathf.Abs(result) > ScalingEpsilon)
                    otherBase.Add(stat, result);
            }
            Dictionary<HashedScalingStats, float> otherScaling = new();
            foreach (HashedScalingStats stat in typeof(HashedScalingStats).GetEnumValues()) {
                float result = a[stat] + b[stat];
                if (Mathf.Abs(result) > ScalingEpsilon)
                    otherScaling.Add(stat, result);
            }
            return new(otherBase, otherScaling);
        }
        public static ScalingStats operator -(ScalingStats a, ScalingStats b) {
            Dictionary<HashedBaseStats, float> otherBase = new();
            foreach (HashedBaseStats stat in typeof(HashedBaseStats).GetEnumValues()) {
                float result = a[stat] - b[stat];
                if (Mathf.Abs(result) > ScalingEpsilon)
                    otherBase.Add(stat, result);
            }
            Dictionary<HashedScalingStats, float> otherScaling = new();
            foreach (HashedScalingStats stat in typeof(HashedScalingStats).GetEnumValues()) {
                float result = a[stat] - b[stat];
                if (Mathf.Abs(result) > ScalingEpsilon)
                    otherScaling.Add(stat, result);
            }
            return new(otherBase, otherScaling);
        }
        public static FinalStats operator *(BaseStats a, ScalingStats b) => new(a, b);


        public float this[HashedScalingStats stat] {
            get => scalingStats.ContainsKey(stat) ? scalingStats[stat] : 0;
            protected set {
                if (scalingStats.ContainsKey(stat)) scalingStats[stat] = value;
                else scalingStats.Add(stat, value);
            }
        }
    }

    /// <summary>
    /// The stats obtained by scaling the BaseStats by the (1+percentage), and keeping the percentage stats the same
    /// </summary>
    [System.Serializable]
    public class FinalStats {

        readonly Dictionary<HashedBaseStats, float> baseStats = new();
        readonly Dictionary<HashedScalingStats, float> scalingStats = new();
        public FinalStats(BaseStats @base, ScalingStats scale) {
            foreach (HashedBaseStats baseStat in typeof(HashedBaseStats).GetEnumValues()) {
                float result = @base[baseStat] > 0 ? @base[baseStat] * Mathf.Max(0, 1 + scale[baseStat]) : @base[baseStat] * Mathf.Max(0f, 1 - scale[baseStat]);
                result = (!BaseMins.ContainsKey(baseStat) || result > BaseMins[baseStat]) ?
                    (!BaseMaxs.ContainsKey(baseStat) || result < BaseMaxs[baseStat]) ?
                        result :
                    BaseMaxs[baseStat] :
                BaseMins[baseStat];
                if (Mathf.Abs(result) > BaseStats.BaseEpsilon)
                    baseStats.Add(baseStat, result);
            }
            foreach (HashedScalingStats scalingStat in typeof(HashedScalingStats).GetEnumValues()) {
                float result = scalingStats[scalingStat];
                result = (!ScalingMins.ContainsKey(scalingStat) || result > ScalingMins[scalingStat])?
                    (!ScalingMaxs.ContainsKey(scalingStat) || result < ScalingMaxs[scalingStat])?
                        result:
                    ScalingMaxs[scalingStat]:
                ScalingMins[scalingStat];
                if (Mathf.Abs(result) > ScalingStats.ScalingEpsilon)
                    scalingStats.Add(scalingStat, result);
            }
        }

        public static readonly Dictionary<HashedBaseStats, float> BaseMins = new() {
            { HashedBaseStats.Atk, 0 },
            { HashedBaseStats.Def, 0 },
            { HashedBaseStats.MaxHp, 1 },
            { HashedBaseStats.MaxMana, 1 }
        }, BaseMaxs = new() {

        };

        public static readonly Dictionary<HashedScalingStats, float> ScalingMins = new() {
            { HashedScalingStats.WalkSpeed, -1 },
            { HashedScalingStats.AtkSpeed, -0.5f },
            { HashedScalingStats.CritRate, 0 },
            { HashedScalingStats.CritDmg, -1 },
            { HashedScalingStats.Knockback, -1 },
            {HashedScalingStats.KnockbackResistance, -10 },
            { HashedScalingStats.PhysicalDmg, -1 },
            { HashedScalingStats.ProjectileDmg, -1 },
            { HashedScalingStats.MagicDmg, -1 },
            { HashedScalingStats.AttackRange, -0.75f },
        }, ScalingMaxs = new() {
            { HashedScalingStats.WalkSpeed, 10 },
            { HashedScalingStats.AtkSpeed, 1 },
            { HashedScalingStats.DmgReduction, 0.96875f },
            { HashedScalingStats.Knockback, 10 },
            { HashedScalingStats.KnockbackResistance, 1 },
            { HashedScalingStats.AttackRange, 1.5f },
            { HashedScalingStats.ManaCostReduction, 0.95f }
        };
        public float this[HashedScalingStats scale] => scalingStats.ContainsKey(scale)? scalingStats[scale]: 0;
    }
}

