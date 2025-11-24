using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using System.ComponentModel;
using Unity.Collections;

public enum DamageType {
    Melee, Projectile, Magic, True
}

/// <summary>
/// Handles stats computation and changes of a mob
/// </summary>
[Serializable]
public class MobStats {
    public BaseStats Base { get; private set; }
    public ScalingStats Scaling { get; private set; }
    /// <summary>
    /// Computed every time a stat is changed
    /// </summary>
    [field: SerializeField]
    [field: Unity.Collections.ReadOnly]
    public FinalStats Final { get; private set; }
    [field: SerializeField]
    public float Hp { get; private set; }
    public float Mana { get; private set; }
    /// <summary>
    /// Can be seen as weapon damage (Weapon's damage: x% of atk)
    /// </summary>
    public float DmgRatio { get; private set; }
    /// <summary>
    /// Invoked once per stats change
    /// <para>arg0: the new walk speed</para>
    /// </summary>
    public event Action<float> OnMovementSpeedChange;

    /// <summary>
    /// Self-documenting
    /// </summary>
    /// <param name="base">Initial base stats of the mob, it will be owned by MobStats</param>
    /// <param name="scaling">Initial scaling stats of hte mob, it will be owned by MobStats</param>
    public MobStats(BaseStats @base, ScalingStats scaling) {
        Base = @base;
        Scaling = scaling;
        ComputeFinalStats();
        Hp = Final.MaxHp;
        Mana = Final.MaxMana;
    }

    /// <summary>
    /// Self-documenting
    /// </summary>
    /// <param name="serializedMobStats">The mob stats as set in the inspector, hashed stats array must NOT contain double entry</param>
    public MobStats(SerializedMobStats serializedMobStats) {
        Base = serializedMobStats.@base;
        Scaling = serializedMobStats.scaling;
        Dictionary<HashedScalingStats, float> d = new();
        foreach (var s in serializedMobStats.hashedScaling) {
            d.Add(s.stats, s.data);
        }
        Scaling.InitializeHash(d);
        
        ComputeFinalStats();
        Hp = Final.MaxHp;
        Mana = Final.MaxMana;
    }

    // damage calculation
    public float UnclassifiedDmg => DmgRatio * Final.Atk * (1 + Final.Crit);
    public float MeleeDmg => UnclassifiedDmg * (1+Final[HashedScalingStats.PhysicalDmg]);
    public float ProjectileDmg => UnclassifiedDmg * (1+Final[HashedScalingStats.ProjectileDmg]);
    public float MagicDmg => UnclassifiedDmg * (1 + Final[HashedScalingStats.MagicDmg]);

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
    public float DmgTakenMultiplier => (1-Final.Def / (100 + Final.Def)) * (1-Final.DmgReduction);



    // stats change events
    /// <summary>
    /// Equips the player with a gear
    /// </summary>
    /// <param name="base">base stats of the gear, null if BaseStats is unchanged</param>
    /// <param name="scaling">scaling stats of the gear, null if ScalingStats is unchanged</param>
    public void GainStats(BaseStats @base, ScalingStats scaling) {
        if (@base is not null)
            Base += @base;
        if (scaling is not null)
            Scaling += scaling;
        ComputeFinalStats();
    }
    /// <summary>
    /// Overload of <c>GainStats(BaseStats, ScalingStats)</c> for equipping weapon
    /// </summary>
    /// <param name="base">base stats of the weapon</param>
    /// <param name="scaling">scaling stats of the weapon</param>
    /// <param name="weaponDmg">damage (% of atk) of the weapon</param>
    public void GainStats(BaseStats @base, ScalingStats scaling, float weaponDmg) {
        DmgRatio = weaponDmg;
        GainStats(@base, scaling);
    }
    /// <summary>
    /// Unequips a mob's weapon
    /// </summary>
    public void UnequipWeapon() {
        DmgRatio = 0.05f;
    }
    /// <summary>
    /// Unequips the player with a gear, including weapon
    /// </summary>
    /// <param name="base">base stats of the gear, null if BaseStats is unchanged</param>
    /// <param name="scaling">scaling stats of the gear, null if ScalingStats is unchanged</param>
    public void LoseStats(BaseStats @base, ScalingStats scaling) {
        if (@base is not null)
            Base -= @base;
        if (scaling is not null)
            Scaling -= scaling;
        ComputeFinalStats();
    }

    /// <summary>
    /// Computes the final stats and invokes <c>OnStatsChange</c>.
    /// Called once per gear change
    /// </summary>
    public void ComputeFinalStats() {
        Final = Base * Scaling;
        if (OnMovementSpeedChange is not null)
            OnMovementSpeedChange(Final.WalkSpeed);
    }

    // Damage taking

    /// <summary>
    /// Subtracts HP based on the amount of damage taken, the damage will be reduced based on the taker's stats.
    /// Note that a mob should be considered dead if their hp drops below 1.
    /// </summary>
    /// <param name="amount">amount of damage taken</param>
    /// <param name="damageType">Type of damage, damage reduction won't apply if it is DamageType.True</param>
    public void TakeDamage(float amount, DamageType damageType) {
        Hp -= amount * (damageType == DamageType.True? 1: DmgTakenMultiplier);
    }

    /// <summary>
    /// Subtracts HP based on the damage dealer's stats, the damage will be reduced based on the taker's stats
    /// Note that a mob should be considered dead if their hp drops below 1.
    /// </summary>
    /// <param name="source">Stats of the source mob</param>
    /// <param name="damageType">Type of damage, damage reduction won't apply if it is DamageType.True</param>
    public void TakeDamage(MobStats source, DamageType damageType) {
        TakeDamage(damageType switch {
            DamageType.Melee => source.MeleeDmg,
            DamageType.Projectile => source.ProjectileDmg,
            DamageType.Magic => source.MagicDmg,
            DamageType.True => source.UnclassifiedDmg,
            _ => throw new ArgumentOutOfRangeException(nameof(DamageType), $"Invalid Damage Type: {damageType}"),
        }, damageType);
    }

}


