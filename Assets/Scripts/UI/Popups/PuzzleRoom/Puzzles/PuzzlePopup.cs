using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Puzzle {
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
            GetComponent<Popup>().CanExit = false;
        }

        public void OnUIExited() {
            if (!cleared)
                throw new System.Exception("Exited UI before puzzle completion");
            OnExit.Invoke();
        }

        protected void Clear(int score, int optimalScore) {
            if (cleared)
                throw new System.Exception("Double clear");
            OnClear.Invoke(score, optimalScore);
            cleared = true;
            GetComponent<Popup>().CanExit = true;
        }

        public static void Shuffle<T>(IList<T> list) {
            for (int i = list.Count - 1; i > 0; i--) {
                int k = Random.Range(0, i + 1);
                (list[k], list[i]) = (list[i], list[k]);
            }
        }
    }
}