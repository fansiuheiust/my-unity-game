using System.Collections;
using System.Collections.Generic;
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

    }

    public enum HashedScalingStats {
        PhysicalDmg, ProjectileDmg, MagicDmg, AttackRange
    }


    /// <summary>
    /// The stats that are presented as numbers
    /// </summary>
    [System.Serializable]
    public class BaseStats {
        [field: SerializeField]
        public float Atk { get; protected set; }
        [field: SerializeField]
        public float Def { get; protected set; }
        [field: SerializeField]
        public float MaxHp { get; protected set; }
        [field: SerializeField]
        public float MaxMana { get; protected set; }
        [field: SerializeField]
        public float ManaRegen { get; protected set; }
        public BaseStats(float atk = 0, float def = 0, float maxHp = 0, float maxMana = 0, float manaRegen = 0) {
            Atk = atk;
            Def = def;
            MaxHp = maxHp;
            MaxMana = maxMana;
            ManaRegen = manaRegen;
        }
        public static BaseStats operator +(BaseStats a, BaseStats b) {
            return new(a.Atk + b.Atk, a.Def + b.Def, a.MaxHp + b.MaxHp, a.MaxMana + b.MaxMana, a.ManaRegen + b.ManaRegen);
        }
        public static BaseStats operator -(BaseStats a, BaseStats b) {
            return new(a.Atk - b.Atk, a.Def - b.Def, a.MaxHp - b.MaxHp, a.MaxMana - b.MaxMana, a.ManaRegen - b.ManaRegen);
        }
    }

    /// <summary>
    /// The stats that are used as percentage
    /// </summary>
    [System.Serializable]
    public class ScalingStats : BaseStats {
        [field: SerializeField]
        public float WalkSpeed { get; protected set; }
        [field: SerializeField]
        public float AtkSpeed { get; protected set; }
        [field: SerializeField]
        public float CritRate { get; protected set; }
        [field: SerializeField]
        public float CritDmg { get; protected set; }
        [field: SerializeField]
        public float DmgReduction { get; protected set; }
        [field: SerializeField]
        public float Knockback { get; protected set; }
        [field: SerializeField]
        public float KnockbackResistance { get; protected set; }
        public Dictionary<HashedScalingStats, float> OtherScaling { get; protected set; } = new();

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
        /// <param name="otherScaling">Hashed scaling stats. If not given an argument, an new one will be allocated. Otherwise, OtherScaling will OWN otherScaling</param>
        public ScalingStats(float atk = 0, float def = 0, float maxHp = 0, float maxMana = 0, float manaRegen = 0, float walkSpeed = 0, float atkSpeed = 0, float critRate = 0, float critDmg = 0, float dmgReduction = 0, float knockback = 0, float knockbackResistance = 0, Dictionary<HashedScalingStats, float> otherScaling = null) : base(atk, def, maxHp, maxMana, manaRegen) {
            WalkSpeed = walkSpeed;
            AtkSpeed = atkSpeed;
            CritRate = critRate;
            CritDmg = critDmg;
            DmgReduction = dmgReduction;
            Knockback = knockback;
            KnockbackResistance = knockbackResistance;
            if (otherScaling is not null)
                OtherScaling = otherScaling;
        }

        public static readonly float ScalingEpsilon = 0.00006103515625f;

        public float Crit { get => Random.value < CritRate ? CritDmg : 0; }
        public static ScalingStats operator +(ScalingStats a, ScalingStats b) {
            Dictionary<HashedScalingStats, float> other = new();
            foreach (HashedScalingStats stat in typeof(HashedScalingStats).GetEnumValues()) {
                float result = a[stat] + b[stat];
                if (Mathf.Abs(result) > ScalingEpsilon)
                    other.Add(stat, result);
            }
            return new(a.Atk + b.Atk, a.Def + b.Def, a.MaxHp + b.MaxHp, a.MaxMana + b.MaxMana, a.ManaRegen + b.ManaRegen, a.WalkSpeed + b.WalkSpeed, a.AtkSpeed + b.AtkSpeed, a.CritRate + b.CritRate, a.CritDmg + b.CritDmg, a.DmgReduction + b.DmgReduction, a.Knockback + b.Knockback, a.KnockbackResistance + b.KnockbackResistance, other);
        }
        public static ScalingStats operator -(ScalingStats a, ScalingStats b) {
            Dictionary<HashedScalingStats, float> other = new();
            foreach (HashedScalingStats stat in typeof(HashedScalingStats).GetEnumValues()) {
                float result = a[stat] - b[stat];
                if (Mathf.Abs(result) > ScalingEpsilon)
                    other.Add(stat, result);
            }
            return new(a.Atk - b.Atk, a.Def - b.Def, a.MaxHp - b.MaxHp, a.MaxMana - b.MaxMana, a.ManaRegen - b.ManaRegen, a.WalkSpeed - b.WalkSpeed, a.AtkSpeed - b.AtkSpeed, a.CritRate - b.CritRate, a.CritDmg - b.CritDmg, a.DmgReduction - b.DmgReduction, a.Knockback - b.Knockback, a.KnockbackResistance - b.KnockbackResistance, other);
        }
        public static FinalStats operator *(BaseStats a, ScalingStats b) => new(a, b);


        public float this[HashedScalingStats stat] {
            get => OtherScaling.ContainsKey(stat) ? OtherScaling[stat] : 0;
            protected set {
                if (OtherScaling.ContainsKey(stat)) OtherScaling[stat] = value;
                else OtherScaling.Add(stat, value);
            }
        }
        public void InitializeHash(Dictionary<HashedScalingStats, float> d) { if (d is not null) OtherScaling = d; }


    }

    /// <summary>
    /// The stats obtained by scaling the BaseStats by the (1+percentage), and keeping the percentage stats the same
    /// </summary>
    [System.Serializable]
    public class FinalStats : ScalingStats {
        public FinalStats(BaseStats @base, ScalingStats scale) : base(@base.Atk * (1 + scale.Atk), @base.Def * (1 + scale.Def), @base.MaxHp * (1 + scale.MaxHp), @base.MaxMana * (1 + scale.MaxMana), @base.ManaRegen * (1 + scale.ManaRegen), scale.WalkSpeed, scale.AtkSpeed, scale.CritRate, scale.CritDmg, scale.DmgReduction, scale.Knockback, scale.KnockbackResistance) {
            Normalize();
            NormalizeHashedScale(scale);
        }
        public static readonly Dictionary<HashedScalingStats, float> OtherScalingMins = new() {
        { HashedScalingStats.PhysicalDmg, -1 },
        { HashedScalingStats.ProjectileDmg, -1 },
        { HashedScalingStats.MagicDmg, -1 },
        { HashedScalingStats.AttackRange, -0.75f },
    }, OtherScalingMaxs = new() {
        { HashedScalingStats.AttackRange, 1.5f },
    };
        void Normalize() {
            Atk = Atk > 0 ? Atk : 0;
            Def = Def > 0 ? Def : 0;
            MaxHp = MaxHp > 1 ? MaxHp : 1;
            MaxMana = MaxMana > 1 ? MaxMana : 1;
            WalkSpeed = WalkSpeed > -1 ? WalkSpeed < 10 ? WalkSpeed : 10 : -1;
            AtkSpeed = AtkSpeed > -0.5f ? AtkSpeed < 1 ? AtkSpeed : 1 : -0.5f;
            CritRate = CritRate > 0 ? CritRate : 0;
            CritDmg = CritDmg > -1 ? CritDmg : -1;
            DmgReduction = DmgReduction < 0.96875f ? DmgReduction : 0.96875f;
            Knockback = Knockback > -1 ? Knockback < 10 ? Knockback : 10 : -1;
            KnockbackResistance = KnockbackResistance > -10 ? KnockbackResistance < 1 ? KnockbackResistance : 1 : -10;

        }

        void NormalizeHashedScale(ScalingStats scale) {
            foreach (var x in new List<HashedScalingStats>(scale.OtherScaling.Keys)) {
                float value = scale.OtherScaling[x];
                OtherScaling.Add(x,
                    (!OtherScalingMins.ContainsKey(x) || value > OtherScalingMins[x]) ?
                        (!OtherScalingMaxs.ContainsKey(x) || value < OtherScalingMaxs[x]) ?
                            value :
                        OtherScalingMaxs[x] :
                    OtherScalingMins[x]
                );
            }
        }
        public new float this[HashedScalingStats scale] => OtherScaling.ContainsKey(scale)? OtherScaling[scale]: 0;
    }
}

