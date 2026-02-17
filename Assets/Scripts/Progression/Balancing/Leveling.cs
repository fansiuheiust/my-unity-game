using System.ComponentModel;
using UnityEngine;
namespace Progression.Balance {
    [CreateAssetMenu(fileName = "Leveling", menuName = "Scriptable Objects/Leveling")]
    public class Leveling : ScriptableObject {
        [field: SerializeField]
        public int MaxLevel { get; private set; } = 10;
        [field: SerializeField, Tooltip("Determines how much progression points are needed to level up for each level.")]
        public AnimationCurve LevelCurve { get; private set; }
        [field: SerializeField, Tooltip("Multiplier of base stats of items for each level")]
        public AnimationCurve ItemBaseStatsMultiplier { get; private set; }
    }
}
