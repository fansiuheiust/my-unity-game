using Unity.VisualScripting;
using UnityEngine;
using Combat;
using System.Collections.Generic;
using Progression.Balance;
using System.Linq;

namespace Loot {

    public enum Class {
        Generic, Melee, Ranged
    }
    /// <summary>
    /// A temporary database for storing gears, just for testing
    /// </summary
    public static class GearDatabase {

        static Dictionary<string, Gear> gears; 
        static GearDatabase() {
            gears = new();
            foreach (Class c in System.Enum.GetValues(typeof(Class))) {
                GearData data = (GearData)Resources.Load($"Data/Gears/{c}/Default");
                gears = gears.Concat(data.AllGears).ToDictionary(x=>x.Key, x=>x.Value);
            }
        }
        public static Gear GetById(string id) => gears[id];
    }
}