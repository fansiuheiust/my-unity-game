using UnityEngine;
using UnityEngine.Events;

namespace UI {
    public abstract class PuzzlePopup : MonoBehaviour {
        /// <summary>
        /// <para>Called when the dungeon is cleared.</para>
        /// <c>int1</c>: score<br />
        /// <c>int2</c>: optimal score<br />
        /// </summary>
        public UnityEvent<int, int> OnClear;
        public UnityEvent OnExit;
        bool cleared = false;

        protected virtual void Awake() {
            GetComponent<Popup>().OnExit.AddListener(OnUIExited);
        }

        public void OnUIExited() {
            if (!cleared)
                Clear(0, 1);
            OnExit.Invoke();
        }

        protected void Clear(int score, int optimalScore) {
            if (cleared)
                throw new System.Exception("Double clear");
            OnClear.Invoke(score, optimalScore);
            cleared = true;
        }
    }
}