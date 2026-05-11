using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;
using UnityEditor;
using UnityEngine;

namespace Combat {
    public enum ArmorType {
        Helmet, Chestplate, Leggings, Boots
    }
    public enum WeaponSpeed {
        Slow, Normal, Fast
    }

    /// <summary>
    /// Base class for gears like weapon and armor
    /// </summary>
    [System.Serializable]
    public class Gear {
        /// <summary>
        /// For now, ID is irrelevant
        /// </summary>
        
        public readonly string Id = "";
        [field: SerializeField] public readonly string Name;
        [field: SerializeField] public readonly BaseStats Base;
        [field: SerializeField] public readonly ScalingStats Scaling;
        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="name">Name of the gear</param>
        /// <param name="base">Base stats of the gear, null if no base stats, it will be owned by Gear</param>
        /// <param name="scaling">Scaling stats of the gear, null if no scaling stats, it will be owned by Gear</param>
        public Gear(string name, BaseStats @base, ScalingStats scaling): this("", name, @base, scaling) {
        }
        public Gear(string id, string name, BaseStats @base, ScalingStats scaling) {
            Id = id;
            Name = name;
            Base = @base;
            Scaling = scaling;
        }

        protected Gear() { }
    }

    /// <summary>
    /// Self-documenting
    /// </summary>
    [System.Serializable]
    public class Armor : Gear {
        [field: SerializeField] public readonly ArmorType Type;
        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="name">Name of the armor</param>
        /// <param name="base">Base stats of the gear, null if no base stats, it will be owned by Armor</param>
        /// <param name="scaling">Scaling stats of the gear, null if no scaling stats, it will be owned by Armor</param>
        /// <param name="type">Type of the armor (helmet, chestplate, leggings, boots)</param>
        public Armor(string id, string name, BaseStats @base, ScalingStats scaling, ArmorType type) : base(name, @base, scaling) {
            Type = type;
        }
        public Armor(string name, BaseStats @base, ScalingStats scaling, ArmorType type) : this("", name, @base, scaling, type) {
        }
        protected Armor() { }
    }

    /// <summary>
    /// Self-documenting
    /// </summary>
    [System.Serializable]
    public abstract class Weapon : Gear {
        [field: SerializeField] public readonly WeaponSpeed WeaponSpeed;
        [field: SerializeField] public readonly float WeaponRange;
        string prefabName = "Default";
        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="name">Name of the weapon</param>
        /// <param name="base">Base stats of the gear, null if no base stats, it will be owned by Weapon</param>
        /// <param name="scaling">Scaling stats of the gear, null if no scaling stats, it will be owned by Weapon</param>
        /// <param name="dmgRatio">Damage it deals in terms of percentage of the equipper's atk</param>
        /// <param name="weaponSpeed">Self-documenting</param>
        public Weapon(string name, BaseStats @base, ScalingStats scaling, WeaponSpeed weaponSpeed, float weaponRange, string prefabName) : this("", name, @base, scaling, weaponSpeed, weaponRange, prefabName) {
        }
        public Weapon(string id, string name, BaseStats @base, ScalingStats scaling, WeaponSpeed weaponSpeed, float weaponRange, string prefabName): base(id, name, @base, scaling) {
            WeaponSpeed = weaponSpeed;
            WeaponRange = weaponRange;
            this.prefabName = prefabName;
        }
        protected Weapon() { }

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
            WeaponSpeed.Slow => 1.5f,
            WeaponSpeed.Normal => 2f,
            WeaponSpeed.Fast => 3f,
            _ => throw new System.ArgumentOutOfRangeException(nameof(weaponSpeed), $"{weaponSpeed} is not a valid weapon speed"),
        };

        public void SetPrefabName(string name) => prefabName = name;

        public GameObject WeaponPrefab => (GameObject)Resources.Load($"Prefabs/Weapon/{weaponFolderPath}/{prefabName}");
        protected abstract string weaponFolderPath { get; }
    }

    [System.Serializable]
    public class Melee : Weapon {
        public Melee(string name, BaseStats @base, ScalingStats scaling, WeaponSpeed weaponSpeed, float weaponRange, string prefabName = "Default"): this("", name, @base, scaling, weaponSpeed, weaponRange, prefabName) {

        }
        public Melee(string id, string name, BaseStats @base, ScalingStats scaling, WeaponSpeed weaponSpeed, float weaponRange, string prefabName = "Default") : base(id, name, @base, scaling, weaponSpeed, weaponRange, prefabName) {

        }
        protected Melee() { }

        protected override string weaponFolderPath => "Melee";
    }

    [System.Serializable]
    public class Ranged: Weapon {
        [field: SerializeField] public readonly uint pierce = 0;
        public Ranged(string id, string name, BaseStats @base, ScalingStats scaling, WeaponSpeed weaponSpeed, float weaponRange, uint pierce, string prefabName = "Default"): base(id, name, @base, scaling, weaponSpeed, weaponRange, prefabName) {
            this.pierce = pierce;
        }

        protected Ranged() { }
        protected override string weaponFolderPath => "Ranged";
    }
}