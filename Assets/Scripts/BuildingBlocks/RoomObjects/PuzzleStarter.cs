using Combat;
using UI;
using UnityEngine;

namespace BuildingBlocks {
    public class PuzzleStarter : MonoBehaviour, IInteractable {
        /// <summary>
        /// for locking during UI
        /// </summary>
        bool usingUI = false;
        public bool IsInteractable => !usingUI && !started;
        [SerializeField] GameObject puzzleIntroPrefab;
        Popup puzzleIntro = null;
        /// <summary>
        /// started puzzle
        /// </summary>
        bool started = false;
        public void Interact(Mob m) {
            usingUI = true;
            puzzleIntro = StageController.PlayerControl.CreatePopup(puzzleIntroPrefab);
            puzzleIntro.OnExit.AddListener(OnIntroClose);
        }

        void OnPuzzleBegin() {
            started = true;
        }
        void OnIntroClose() {
            usingUI = false;
        }
    }
}