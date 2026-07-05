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
        [SerializeField]
        GameObject coinRewardPrefab, buffRewardPrefab, gearRewardPrefab;

        [SerializeField, Min(0f)]
        float coinRewardThreshold = 0.5f, buffRewardThreshold = 0.75f, gearRewardThreshold = 1f;

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
            Cleared = true;
        }

        void OnPuzzleExited() {

            float optimality = (float)score / optimalScore;
            puzzle.OnExit.RemoveListener(OnPuzzleExited);
            outro = StageController.PlayerControl.EnqueuePopup(outroPrefab).GetComponent<PuzzleOutro>();
            outro.Init(score, optimalScore, optimality >= coinRewardThreshold, optimality >= buffRewardThreshold, optimality >= gearRewardThreshold);
            if (optimality >= coinRewardThreshold)
                StageController.PlayerControl.EnqueuePopup(coinRewardPrefab).GetComponent<CoinReward>().Init(optimality);
            if (optimality >= buffRewardThreshold)
                StageController.PlayerControl.EnqueuePopup(buffRewardPrefab); // TODO: once implemented, init this also
            if (optimality >= gearRewardThreshold)
                StageController.PlayerControl.EnqueuePopup(gearRewardPrefab); // TODO: once implemented, init this also

        }
    }
}