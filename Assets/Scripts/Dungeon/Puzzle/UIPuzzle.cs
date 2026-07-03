using UI;
using UnityEngine;

namespace Dungeon {
    /// <summary>
    /// Puzzles that simply open a UI
    /// </summary>
    public class UIPuzzle : Puzzle {
        [SerializeField]
        GameObject UIPrefab;

        PuzzlePopup puzzle;

        /// <summary>
        /// Assumption: the UI before this, which is PuzzleStarter UI, can always be closed
        /// </summary>
        public override void StartPuzzle() {
            puzzle = StageController.PlayerControl.ForceCreatePopup(UIPrefab).GetComponent<PuzzlePopup>();
        }
    }
}