using BuildingBlocks;
using UI;
using UnityEngine;

namespace Dungeon {
    public class PuzzleRoom : Room {
        [field: SerializeField]
        public string Puzzle { get; private set; }
        [SerializeField]
        PuzzleStarter starter;
        [SerializeField]
        GameObject outroPrefab;
        Puzzle puzzle;

        PuzzleOutro outro;
        public void Awake() {
            starter.puzzleID = Puzzle;
            starter.OnBegin.AddListener(StartPuzzle);
            puzzle = GetComponent<Puzzle>();
            puzzle.OnExit.AddListener(OnPuzzleExited);
        }

        void StartPuzzle() {
            starter.OnBegin.RemoveListener(StartPuzzle);
            puzzle.OnClear.AddListener(OnPuzzleCleared);
            puzzle.StartPuzzle();
        }

        int score, optimalScore;
        void OnPuzzleCleared(int score, int optimalScore) {
            puzzle.OnClear.RemoveListener(OnPuzzleCleared);
            this.score = score;
            this.optimalScore = optimalScore;
        }

        void OnPuzzleExited() {
            puzzle.OnExit.RemoveListener(OnPuzzleExited);
            outro = StageController.PlayerControl.EnqueuePopup(outroPrefab).GetComponent<PuzzleOutro>();
            outro.Init(score, optimalScore);
        }
    }
}