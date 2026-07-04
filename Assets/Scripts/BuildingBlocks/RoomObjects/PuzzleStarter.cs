using Combat;
using UI;
using UnityEngine;
using UnityEngine.Events;

namespace BuildingBlocks {
    public class PuzzleStarter : MonoBehaviour, IInteractable {
        /// <summary>
        /// for locking during UI
        /// </summary>
        bool usingUI = false;
        [System.NonSerialized]
        public string puzzleID;
        public bool IsInteractable => !usingUI && !started;
        [SerializeField] GameObject puzzleIntroPrefab;
        PuzzleIntro puzzleIntro = null;
        /// <summary>
        /// started puzzle
        /// </summary>
        bool started = false;

        public UnityEvent OnBegin;
        public void Interact(Mob m) {
            usingUI = true;
            puzzleIntro = StageController.PlayerControl.EnqueuePopup(puzzleIntroPrefab).GetComponent<PuzzleIntro>();
            puzzleIntro.GetComponent<Popup>().OnExit.AddListener(OnIntroClose);
            
            puzzleIntro.StartButton.onClick.AddListener(OnPuzzleBegin);
            puzzleIntro.PuzzleID = puzzleID;
        }

        void OnPuzzleBegin() {
            started = true;
            puzzleIntro.GetComponent<Popup>().OnExitPressed();
            OnBegin.Invoke();
            Destroy(gameObject);
        }
        void OnIntroClose() {
            usingUI = false;
        }
    }
}