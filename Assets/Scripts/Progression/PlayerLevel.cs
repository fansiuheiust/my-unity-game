using NUnit.Framework.Constraints;
using Progression.Balance;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Progression {
    public class PlayerLevel {
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
                while (Level < StageController.LevelingData.MaxLevel && Point >= (uint)StageController.LevelingData.LevelCurve.Evaluate(Level+1)) {
                    _point -= (uint)StageController.LevelingData.LevelCurve.Evaluate(Level+1);
                    Level++;
                }
            }
        }
        public void AddPoint(uint num) {
            Point += num;
        }
    }
}