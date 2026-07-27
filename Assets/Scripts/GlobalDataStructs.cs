// this file contains data types/structures that should be used globally

using Combat;
/// <summary>
/// Not Mythical game engine pull
/// </summary>
public static class Global {
    public static readonly string[] Rarities = new string[] {
        "Common", "Rare", "Epic", "Legendary", "Mythical"
    };

    /// <summary>
    /// Upper case string of the key for triggering an ability
    /// </summary>
    /// <param name="key">Trigger key of the ability</param>
    public static string AbilityKey(AbilityTriggerKey key) => key switch {
        
        AbilityTriggerKey.Damage => "E",
        AbilityTriggerKey.Ultimate => "Q",
        AbilityTriggerKey.Movement => "X",
        AbilityTriggerKey.Misc => "C",
        AbilityTriggerKey.Weapon => "Z",
        _ => ""
    };

    /// <summary>
    /// String of a scaling stat
    /// </summary>
    /// <param name="h">hashed scaling stat to query</param>
    /// <exception cref="System.NotImplementedException"> if the string of the hashed scaling stat is still not added</exception>
    public static string HashedScalingStat(ScalingAttribute h) => h switch {
        ScalingAttribute.PhysicalDmg => "Physical Damage",
        ScalingAttribute.ProjectileDmg => "Projectile Damage",
        ScalingAttribute.MagicDmg => "Magic Damage",
        ScalingAttribute.AttackRange => "Attack Range",
        ScalingAttribute.ManaCostReduction => "Mana Cost Reduction",
        _ => throw new System.NotImplementedException($"Please add string of {h} to GlobalDataStructures.cs")
    };

}