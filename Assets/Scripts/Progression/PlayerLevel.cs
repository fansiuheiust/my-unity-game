using NUnit.Framework.Constraints;
using Progression.Balance;
using UnityEngine;

namespace Progression {
    public class PlayerLevel : MonoBehaviour {
        public readonly Leveling levelingData;
        
        public PlayerLevel(Leveling levelingData) {
            this.levelingData = levelingData;
        }
        public int Level { get; private set; } = 0;

        uint _point = 0;
        /// <summary>
        /// Number of points the player needs for the next level, common corresponds to 1, point per non-common coin is indeterminate
        /// </summary>
        public uint Point {
            get => _point;
            private set {
                if (_point < 0) throw new System.Exception("Point reached negative");
                _point = value;
                while (Level < levelingData.MaxLevel && Point >= (uint)levelingData.LevelCurve.Evaluate(Level+1)) {
                    _point -= (uint)levelingData.LevelCurve.Evaluate(Level+1);
                    Level++;
                }
            }
        }
        public void AddPoint(uint num) {
            Point += num;
        }
    }
}