using UnityEngine;
using Combat;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Progression.Balance {
    [CreateAssetMenu(fileName = "Perks", menuName = "Scriptable Objects/Gears")]
    public class GearData: ScriptableObject {
        [SerializeField] SerializedArmor[] armors;
        [SerializeField] SerializedMelee[] melees;
        [SerializeField] SerializedRanged[] rangeds;

        public Dictionary<string, Armor> Armors => armors.Select(x => (Armor)x.Gear).ToDictionary(x => x.id);
        public Dictionary<string, Weapon> Weapons {
            get {
                Dictionary<string, Weapon> ri = melees.Select(x => (Weapon)x.Gear).ToDictionary(x => x.id),
                    ri2 = rangeds.Select(x => (Weapon)x.Gear).ToDictionary(x => x.id);
                return ri.Concat(ri2).ToDictionary(x=>x.Key, x=>x.Value);
            }
        }
        public Dictionary<string, Gear> AllGears {
            get {
                var armors = this.armors.Select(x => x.Gear);
                var weapons = melees.Select(x => x.Gear).Concat(rangeds.Select(x => x.Gear));
                return armors.Concat(weapons).ToDictionary(x => x.id);
            }
        }
    }
    [System.Serializable]
    abstract class SerializedGear {
        [SerializeField] protected string id;
        [SerializeField] protected string name;
        [SerializeField] protected SerializedMobStats stats;
        [SerializeField, Tooltip("Leave it blank if no ability")] protected string abilityID = "";
        internal virtual Gear Gear {
            get {
                stats.InsertHasedStats();
                return null;
            }
        }

        protected Ability Ability => abilityID != "" ? AbilityDatabase.abilities[abilityID]: null;
    }

    [System.Serializable]
    abstract class SerializedWeapon: SerializedGear {
        [SerializeField] protected WeaponSpeed weaponSpeed;
        [SerializeField, Min(0.5f)] protected float weaponRange;
        [SerializeField] protected string prefabName = "Default";
    }

    [System.Serializable]
    class SerializedArmor: SerializedGear {
        [SerializeField] protected ArmorType type;
        internal override Gear Gear {
            get {
                Gear _ = base.Gear;
                return new Combat.Armor(id, name, stats.@base, stats.scaling, type, Ability);
            }
        }
    }

    [System.Serializable]
    class SerializedMelee: SerializedWeapon {
        internal override Gear Gear {
            get {
                Gear _ = base.Gear;
                return new Combat.Melee(id, name, stats.@base, stats.scaling, weaponSpeed, weaponRange, prefabName, Ability);
            }
        }
    }

    [System.Serializable]
    class SerializedRanged: SerializedWeapon {
        [SerializeField] protected uint pierce;
        internal override Gear Gear {
            get {
                Gear _ = base.Gear;
                return new Combat.Ranged(id, name, stats.@base, stats.scaling, weaponSpeed, weaponRange, pierce, prefabName, Ability);
            }
        }
    }

}