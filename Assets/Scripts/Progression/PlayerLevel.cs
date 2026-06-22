using NUnit.Framework.Constraints;
using Progression.Balance;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

namespace Progression {
    public class PlayerLevel {
        /// <summary>
        /// Invoked every time when the player's level is changed, passes new level
        /// </summary>
        public event Action<uint> PlayerLevelChanged;
        public PlayerLevel() {
        }
        public PlayerLevel(uint level, uint point) {
            Level = level;
            _point = point;
        }

        public uint Level { get; private set; } = 0;

        uint _point = 0;
        /// <summary>
        /// Number of points the player needs for the next level, common corresponds to 1, point per non-common coin is indeterminate
        /// </summary>
        public uint Point {
            get => _point;
            private set {
                if (_point < 0) throw new System.Exception("Point reached negative");
                _point = value;
                uint initialLevel = Level;
                while (Level < StageController.LevelingData.MaxLevel && Point >= (uint)StageController.LevelingData.LevelCurve.Evaluate(Level+1)) {
                    _point -= (uint)StageController.LevelingData.LevelCurve.Evaluate(Level+1);
                    Level++;
                }
                if (Level != initialLevel)
                    PlayerLevelChanged?.Invoke(Level);
            }
        }
        public void AddPoint(uint num) {
            Point += num;
        }
    }
}