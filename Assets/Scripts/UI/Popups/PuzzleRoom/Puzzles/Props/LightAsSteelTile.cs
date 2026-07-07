using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Puzzle {
    public class LightAsSteelTile : MonoBehaviour {
        [field: SerializeField]
        public int Row { get; private set; }
        [field: SerializeField]
        public int Col { get; private set; }
    }
}