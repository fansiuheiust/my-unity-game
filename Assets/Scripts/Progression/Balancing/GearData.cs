using UnityEngine;
using Combat;
using System.Collections.Generic;
using System.Linq;

namespace Progression.Balance {
    public class GearData: ScriptableObject {
        [SerializeField] SerializedArmor[] armors;
        [SerializeField] SerializedMelee[] melees;
        [SerializeField] SerializedRanged[] rangeds;

        public Dictionary<string, Armor> Armors => armors.Select(x => (Armor)x.Gear).ToDictionary(x => x.Id);
        public Dictionary<string, Weapon> Weapons {
            get {
                Dictionary<string, Weapon> ri = melees.Select(x => (Weapon)x.Gear).ToDictionary(x => x.Id),
                    ri2 = rangeds.Select(x => (Weapon)x.Gear).ToDictionary(x => x.Id);
                return ri.Concat(ri2).ToDictionary(x=>x.Key, x=>x.Value);
            }
        }
        public Dictionary<string, Gear> AllGears {
            get {
                var armors = this.armors.Select(x => x.Gear);
                var weapons = this.armors.Select(x => x.Gear);
                return armors.Concat(weapons).ToDictionary(x => x.Id);
            }
        }
    }
    [System.Serializable]
    abstract class SerializedGear {
        [SerializeField] protected string id;
        [SerializeField] protected string name;
        [SerializeField] protected SerializedMobStats stats;
        internal virtual Gear Gear {
            get {
                stats.InsertHasedStats();
                return null;
            }
        }
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
                return new Combat.Armor(id, name, stats.@base, stats.scaling, type);
            }
        }
    }

    [System.Serializable]
    class SerializedMelee: SerializedWeapon {
        internal override Gear Gear {
            get {
                Gear _ = base.Gear;
                return new Combat.Melee(id, name, stats.@base, stats.scaling, weaponSpeed, weaponRange, prefabName);
            }
        }
    }

    [System.Serializable]
    class SerializedRanged: SerializedWeapon {
        [SerializeField] protected uint pierce;
        internal override Gear Gear {
            get {
                Gear _ = base.Gear;
                return new Combat.Ranged(id, name, stats.@base, stats.scaling, weaponSpeed, weaponRange, pierce, prefabName);
            }
        }
    }

}