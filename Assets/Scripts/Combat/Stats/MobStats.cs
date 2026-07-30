using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using System.ComponentModel;
using Unity.Collections;
using System.Linq;

namespace Combat {
    public enum DamageType {
        Melee, Projectile, Magic, True
    }

    /// <summary>
    /// Handles stats computation and changes of a mob
    /// </summary>
    [Serializable]
    public class MobStats {
        readonly BaseStats _base;
        readonly ScalingStats _scaling;
        /// <summary>
        /// Computed every time a stat is changed
        /// </summary>
        [field: SerializeField]
        [field: Unity.Collections.ReadOnly]
        readonly FinalStats _final;

        public BaseStats Base => _base;
        public ScalingStats Scaling => _scaling;
        public FinalStats Final => _final;


        [field: SerializeField]
        public float Hp { get; private set; }
        public float Mana { get; private set; }
        /// <summary>
        /// Invoked once per stats change
        /// <para>arg0: the new walk speed</para>
        /// </summary>
        public event Action<float> OnMovementSpeedChange;
        /// <summary>
        /// <para>Invoked once per stats change if Attack Range is present</para>
        /// <c>arg0</c>: the new attack range *scalar*
        /// </summary>
        public event Action<float> OnAttackRangeChange;

        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="base">Initial base stats of the mob</param>
        /// <param name="scaling">Initial scaling stats of hte mob</param>
        public MobStats(BaseStats @base, ScalingStats scaling) {
            _base = @base.Clone();
            _scaling = scaling.Clone();
            _final = new(_base, _scaling);
            Hp = _final[BaseAttribute.MaxHp];
            Mana = _final[BaseAttribute.MaxMana];
            _base.OnBaseStatChange += OnStatChanged;
            _scaling.OnBaseStatChange += OnStatChanged;
            _scaling.OnScalingStatChange += OnStatChanged;
        }

        /// <summary>
        /// Called when a BaseAttribute got changed in BaseStats or ScalingStats
        /// </summary>
        /// <param name="stat">the attribute that got changed</param>
        void OnStatChanged(BaseAttribute stat) {
            
        }

        /// <summary>
        /// Called when a ScalingAttribute got changed
        /// </summary>
        /// <param name="stat">the attribute that got changed</param>
        void OnStatChanged(ScalingAttribute stat) {
            switch (stat) {
                case ScalingAttribute.WalkSpeed:
                    OnMovementSpeedChange?.Invoke(_final[ScalingAttribute.WalkSpeed]);
                    break;
                case ScalingAttribute.AttackRange:
                    OnAttackRangeChange?.Invoke(_final[ScalingAttribute.AttackRange]);
                    break;
            }
        }

        public void RaiseStatChangeEvents() {
            OnMovementSpeedChange?.Invoke(_final[ScalingAttribute.WalkSpeed]);
            OnAttackRangeChange?.Invoke(_final[ScalingAttribute.AttackRange]);
        }


        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="serializedMobStats">The mob stats as set in the inspector, hashed stats array must NOT contain double entry</param>
        public MobStats(SerializedMobStats serializedMobStats): this(serializedMobStats.Base, serializedMobStats.Scaling) {
            
        }
        public void ResetHp() => Hp = _final[BaseAttribute.MaxHp];

        // damage calculation
        public float UnclassifiedDmg => _final[BaseAttribute.Atk] * (1 + _final.Crit);
        public float MeleeDmg => UnclassifiedDmg * (1 + _final[ScalingAttribute.PhysicalDmg]);
        public float ProjectileDmg => UnclassifiedDmg * (1 + _final[ScalingAttribute.ProjectileDmg]);
        public float MagicDmg => UnclassifiedDmg * (1 + _final[ScalingAttribute.MagicDmg]);

        /// <returns>
        /// Default definition of dead: hp < 1
        /// </returns>
        public bool IsDead => Hp < 1;

        /// <summary>
        /// This will be multiplied to the amount of damage taken by the mob.
        /// </summary>
        /// <example>
        /// If <c>DmgTakenMultiplier</c> = 0.6, final damage taken = original damage * 0.6
        /// </example>
        public float DmgTakenMultiplier => (1 - _final[BaseAttribute.Def] / (100 + _final[BaseAttribute.Def])) * (1 - _final[ScalingAttribute.DmgReduction]);

        // mana change
        /// <summary>
        /// Consumes mana if possible, will not when there is no mana
        /// </summary>
        /// <param name="mana">Amount of mana to be consumed</param>
        /// <returns>Whether mana is consumed, and the mana consumed</returns>
        public (bool consumed, float amount) ConsumeMana(float mana) {
            mana *= (1 - _final[ScalingAttribute.ManaCostReduction]);
            if (mana > Mana)
                return (false, 0);
            Mana -= mana;
            return (true, mana);
        }


        // stats change events
        /// <summary>
        /// Equips the player with a gear
        /// </summary>
        /// <param name="base">base stats of the gear, null if BaseStats is unchanged</param>
        /// <param name="scaling">scaling stats of the gear, null if ScalingStats is unchanged</param>
        public void GainStats(BaseStats @base, ScalingStats scaling) {
            if (@base is not null)
                _base.Gain(@base);
            if (scaling is not null)
                _scaling.Gain(scaling);
        }
        public void GainBaseStats(params (BaseAttribute, float)[] bases) {
            if (bases is not null)
                foreach (var (stat, value) in bases)
                    _base.Gain(stat, value);
        }

        public void LoseBaseStats(params (BaseAttribute, float)[] bases) => GainBaseStats(bases.Select(x => (x.Item1, -x.Item2)).ToArray());

        public void GainScalingStats(params (BaseAttribute, float)[] bases) {
            if (bases is not null)
                foreach (var (stat, value) in bases)
                    _scaling.Gain(stat, value);
        }
        public void LoseScalingStats(params (BaseAttribute, float)[] bases) => GainScalingStats(bases.Select(x=>(x.Item1, -x.Item2)).ToArray());
        public void GainScalingStats(params (ScalingAttribute, float)[] scalings) {
            if (scalings is not null)
                foreach (var (stat, value) in scalings)
                    _scaling.Gain(stat, value);
        }
        public void LoseScalingStats(params (ScalingAttribute, float)[] scalings) => GainScalingStats(scalings.Select(x=>(x.Item1, -x.Item2)).ToArray());


        /// <summary>
        /// Unequips the player with a gear, including weapon
        /// </summary>
        /// <param name="base">base stats of the gear, null if BaseStats is unchanged</param>
        /// <param name="scaling">scaling stats of the gear, null if ScalingStats is unchanged</param>
        public void LoseStats(BaseStats @base, ScalingStats scaling) {
            if (@base is not null)
                _base.Lose(@base);
            if (scaling is not null)
                _scaling.Lose(scaling);
        }

        // Damage taking

        /// <summary>
        /// Subtracts HP based on the amount of damage taken, the damage will be reduced based on the taker's stats.
        /// Note that a mob should be considered dead if their hp drops below 1.
        /// </summary>
        /// <param name="amount">amount of damage taken</param>
        /// <param name="damageType">Type of damage, damage reduction won't apply if it is DamageType.True</param>
        /// <returns>amount of damage taken</returns>
        public float TakeDamage(float amount, DamageType damageType) {
            float amt = amount * (damageType == DamageType.True ? 1 : DmgTakenMultiplier);
            Hp -= amount * (damageType == DamageType.True ? 1 : DmgTakenMultiplier);
            return amt;
        }

        /// <summary>
        /// Subtracts HP based on the damage dealer's stats, the damage will be reduced based on the taker's stats
        /// Note that a mob should be considered dead if their hp drops below 1.
        /// </summary>
        /// <param name="source">Stats of the source mob</param>
        /// <param name="damageType">Type of damage, damage reduction won't apply if it is DamageType.True</param>
        /// <param name="weaponMultiplier">Multiplier of the calculated damage based on the weapon's action</param>
        /// <returns>amount of damage taken</returns>
        public float TakeDamage(MobStats source, DamageType damageType, float weaponMultiplier = 1f) {
            return TakeDamage(weaponMultiplier * damageType switch {
                DamageType.Melee => source.MeleeDmg,
                DamageType.Projectile => source.ProjectileDmg,
                DamageType.Magic => source.MagicDmg,
                DamageType.True => source.UnclassifiedDmg,
                _ => throw new ArgumentOutOfRangeException(nameof(DamageType), $"Invalid Damage Type: {damageType}"),
            }, damageType);
        }

        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="amount">Amount of HP to receive</param>
        /// <param name="source">Who healed you</param>
        public void Heal(float amount, MobStats source) {
            Hp += amount;
        }

    }
}
