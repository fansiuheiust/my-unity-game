using Unity.VisualScripting;
using UnityEngine;
using Combat;
using System.Collections.Generic;
using Progression.Balance;
using System.Linq;
using UnityEditor;

namespace Loot {

    public enum Class {
        Generic, Melee, Ranged
    }
    /// <summary>
    /// A temporary database for storing gears, just for testing
    /// </summary
    public static class GearDatabase {

        static Dictionary<string, Gear> gears;
        static Dictionary<string, Gear> scaledGears;
        static GearDatabase() {
            gears = new();
            foreach (Class c in System.Enum.GetValues(typeof(Class))) {
                GearData data = (GearData)Resources.Load($"Data/Gears/{c}/Default");
                gears = gears.Concat(data.AllGears).ToDictionary(x=>x.Key, x=>x.Value);
            }
            scaledGears = gears.ToDictionary(x => x.Key, x => x.Value.Scaled(StageController.LevelingData.ItemBaseStatsMultiplier.Evaluate(StageController.PlayerLevel.Level)));
            StageController.PlayerLevel.PlayerLevelChanged += OnPlayerLevelChange;
        }

        static void OnPlayerLevelChange(uint level) {
            scaledGears = gears.ToDictionary(x => x.Key, x => x.Value.Scaled(StageController.LevelingData.ItemBaseStatsMultiplier.Evaluate(level)));
        }

        /// <summary>
        /// returns a gear by ID
        /// </summary>
        /// <param name="id">ID of the gear</param>
        public static Gear Get(string id) => gears[id];
        /// <summary>
        /// returns gear after being scaled to level
        /// </summary>
        /// <param name="id">ID of the gear</param>
        public static Gear GetScaled(string id) => scaledGears[id];
    }
}