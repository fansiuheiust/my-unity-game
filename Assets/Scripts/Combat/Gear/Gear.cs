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
        public readonly string id = "";
        public readonly string name;
        public readonly BaseStats @base;
        public readonly ScalingStats scaling;
        public readonly Ability ability;
        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="id">ID of the gear</param>
        /// <param name="name">Name of the gear</param>
        /// <param name="base">Base stats of the gear, null if no base stats</param>
        /// <param name="scaling">Scaling stats of the gear, null if no scaling stats</param>
        /// <param name="ability">Ability of the gear, null if none, will be owned</param>
        public Gear(string id, string name, BaseStats @base, ScalingStats scaling, Ability ability) {
            this.id = id;
            this.name = name;
            this.@base = @base is null? new BaseStats(): @base.Clone();
            this.scaling = scaling is null? new ScalingStats(): scaling.Clone();
            this.ability = ability;
        }
        
        /// <summary>
        /// Creates a new gear with base stats multiplied
        /// </summary>
        /// <param name="multiplier">multiplier to the base stats</param>
        public virtual Gear Scaled(float multiplier) => new Gear(id, name, multiplier * @base, scaling, ability);

        protected Gear() { }
    }

    /// <summary>
    /// Self-documenting
    /// </summary>
    [System.Serializable]
    public class Armor : Gear {
        public readonly ArmorType type;
        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="name">Name of the armor</param>
        /// <param name="base">Base stats of the gear, null if no base stats</param>
        /// <param name="scaling">Scaling stats of the gear, null if no scaling stats</param>
        /// <param name="type">Type of the armor (helmet, chestplate, leggings, boots)</param>
        public Armor(string id, string name, BaseStats @base, ScalingStats scaling, ArmorType type, Ability ability = null) : base(id, name, @base, scaling, ability) {
            this.type = type;
        }
        protected Armor() { }

        public override Gear Scaled(float multiplier) => new Armor(id, name, multiplier * @base, scaling, type, ability);
    }

    /// <summary>
    /// Self-documenting
    /// </summary>
    [System.Serializable]
    public abstract class Weapon : Gear {
        public readonly WeaponSpeed weaponSpeed;
        public readonly float weaponRange;
        protected string prefabName = "Default";
        /// <summary>
        /// Self-documenting
        /// </summary>
        /// <param name="name">Name of the weapon</param>
        /// <param name="base">Base stats of the gear, null if no base stats</param>
        /// <param name="scaling">Scaling stats of the gear, null if no scaling stats</param>
        /// <param name="dmgRatio">Damage it deals in terms of percentage of the equipper's atk</param>
        /// <param name="weaponSpeed">Self-documenting</param>
        public Weapon(string id, string name, BaseStats @base, ScalingStats scaling, WeaponSpeed weaponSpeed, float weaponRange, string prefabName, Ability ability): base(id, name, @base, scaling, ability) {
            this.weaponSpeed = weaponSpeed;
            this.weaponRange = weaponRange;
            this.prefabName = prefabName;
        }
        protected Weapon() { }

        /// <returns>
        /// The base attack speed of the weapon
        /// </returns>
        public float BaseAttackSpeed => FindBaseAttackSpeed(weaponSpeed);

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

        public GameObject WeaponPrefab => (GameObject)Resources.Load($"Prefabs/Weapon/{WeaponFolderPath}/{prefabName}");
        protected abstract string WeaponFolderPath { get; }
    }

    [System.Serializable]
    public class Melee : Weapon {
        public Melee(string id, string name, BaseStats @base, ScalingStats scaling, WeaponSpeed weaponSpeed, float weaponRange, string prefabName = "Default", Ability ability = null) : base(id, name, @base, scaling, weaponSpeed, weaponRange, prefabName, ability) {

        }
        protected Melee() { }

        protected override string WeaponFolderPath => "Melee";
        public override Gear Scaled(float multiplier) => new Melee(id, name, multiplier * @base, scaling, weaponSpeed, weaponRange, prefabName, ability);
    }

    [System.Serializable]
    public class Ranged: Weapon {
        public readonly uint pierce = 0;
        public Ranged(string id, string name, BaseStats @base, ScalingStats scaling, WeaponSpeed weaponSpeed, float weaponRange, uint pierce, string prefabName = "Default", Ability ability = null): base(id, name, @base, scaling, weaponSpeed, weaponRange, prefabName, ability) {
            this.pierce = pierce;
        }

        protected Ranged() { }
        protected override string WeaponFolderPath => "Ranged";
        public override Gear Scaled(float multiplier) => new Ranged(id, name, multiplier * @base, scaling, weaponSpeed, weaponRange, pierce, prefabName, ability);
    }
}