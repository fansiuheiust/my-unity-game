using BuildingBlocks;
using UnityEngine;

namespace Dungeon {
    public class PuzzleRoom : Room {
        [field: SerializeField]
        public string Puzzle { get; private set; }
        [SerializeField]
        PuzzleStarter starter;
        public void Awake() {
            starter.puzzleID = Puzzle;
        }
    }
}