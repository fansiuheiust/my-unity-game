using Unity.VisualScripting;
using UnityEngine;
using Combat;

namespace Loot {
    /// <summary>
    /// A temporary database for storing gears, just for testing
    /// </summary
    public static class GearDatabase {
        public static Gear GetById(string id) => id switch {
            "long_sword" => new Melee("long_sword", "Long Sword", new BaseStats(atk: 1), new ScalingStats(def: -0.1f, otherScaling: new() { { HashedScalingStats.AttackRange, 0.4f } }), WeaponSpeed.Slow, 3),
            "dagger" => new Melee("dagger", "Dagger", new BaseStats(atk: 10), new ScalingStats(def: -0.4f), WeaponSpeed.Fast, 0.5f),
            "bow" => new Ranged("bow", "Bow", new BaseStats(atk: 10), new ScalingStats(), WeaponSpeed.Slow, 40f, 1),
            _ => new Melee("starter_gear", "Starter's Gear", new BaseStats(atk: 3), new ScalingStats(atk: 0.1f), WeaponSpeed.Normal, 2),
        };
    }
}