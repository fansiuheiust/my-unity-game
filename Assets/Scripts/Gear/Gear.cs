using UnityEditor;
using UnityEngine;


public enum ArmorType {
    Helmet, Chestplate, Leggings, Boots
}
public enum WeaponSpeed {
    Slow, Normal, Fast
}

/// <summary>
/// Base class for gears like weapon and armor
/// </summary>
public class Gear {
    /// <summary>
    /// For now, ID is irrelevant
    /// </summary>
    public readonly string Id = ""; 
    public readonly string Name;
    public readonly BaseStats Base;
    public readonly ScalingStats Scaling;
    /// <summary>
    /// Self-documenting
    /// </summary>
    /// <param name="name">Name of the gear</param>
    /// <param name="base">Base stats of the gear, null if no base stats, it will be owned by Gear</param>
    /// <param name="scaling">Scaling stats of the gear, null if no scaling stats, it will be owned by Gear</param>
    public Gear(string name, BaseStats @base, ScalingStats scaling) {
        Name = name;
        Base = @base;
        Scaling = scaling;
    }
}

/// <summary>
/// Self-documenting
/// </summary>
public class Armor : Gear {
    public readonly ArmorType Type;
    /// <summary>
    /// Self-documenting
    /// </summary>
    /// <param name="name">Name of the armor</param>
    /// <param name="base">Base stats of the gear, null if no base stats, it will be owned by Armor</param>
    /// <param name="scaling">Scaling stats of the gear, null if no scaling stats, it will be owned by Armor</param>
    /// <param name="type">Type of the armor (helmet, chestplate, leggings, boots)</param>
    public Armor(string name, BaseStats @base, ScalingStats scaling, ArmorType type) : base(name, @base, scaling) {
        Type = type;
    }
}

/// <summary>
/// Self-documenting
/// </summary>
public abstract class Weapon: Gear {
    public readonly float DmgRatio;
    public readonly WeaponSpeed WeaponSpeed;
    /// <summary>
    /// Self-documenting
    /// </summary>
    /// <param name="name">Name of the weapon</param>
    /// <param name="base">Base stats of the gear, null if no base stats, it will be owned by Weapon</param>
    /// <param name="scaling">Scaling stats of the gear, null if no scaling stats, it will be owned by Weapon</param>
    /// <param name="dmgRatio">Damage it deals in terms of percentage of the equipper's atk</param>
    /// <param name="weaponSpeed">Self-documenting</param>
    public Weapon(string name, BaseStats @base, ScalingStats scaling, float dmgRatio, WeaponSpeed weaponSpeed) : base(name, @base, scaling) {
        DmgRatio = dmgRatio;
        WeaponSpeed = weaponSpeed;
    }


    /// <returns>
    /// The base attack speed of the weapon
    /// </returns>
    public float BaseAttackSpeed => FindBaseAttackSpeed(WeaponSpeed);

    /// <summary>
    /// Calculates the base attack speed based on WeaponSpeed
    /// </summary>
    /// <param name="weaponSpeed">self-documenting</param>
    /// <returns>
    /// Fast: 3; Normal: 2; Slow: 1.5
    /// </returns>
    public static float FindBaseAttackSpeed(WeaponSpeed weaponSpeed) => weaponSpeed switch { 
        WeaponSpeed.Slow=>1.5f,
        WeaponSpeed.Normal=>2f,
        WeaponSpeed.Fast => 3f,
        _ => throw new System.ArgumentOutOfRangeException(nameof(weaponSpeed), $"{weaponSpeed} is not a valid weapon speed"),
    };

    public abstract GameObject WeaponPrefab { get; }
}

public class Melee: Weapon {
    
    public Melee(string name, BaseStats @base, ScalingStats scaling, float dmgRatio, WeaponSpeed weaponSpeed): base(name, @base, scaling, dmgRatio, weaponSpeed) {

    }

    public override GameObject WeaponPrefab => (GameObject)Resources.Load("Prefabs/Weapon/Melee/Default");
}