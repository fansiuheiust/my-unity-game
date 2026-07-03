using BuildingBlocks;
using UnityEngine;

namespace Dungeon {
    public class PuzzleRoom : Room {
        public string Puzzle { get; private set; }
        [SerializeField]
        PuzzleStarter starter;
        public void Awake() {
            // choose a random puzzle available
            Puzzle = StageController.DungeonData.Puzzles((uint)Random.Range(0, (int)StageController.DungeonData.NumPuzzles-1));
            starter.puzzleID = Puzzle;
        }
    }
}