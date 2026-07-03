using UnityEngine;
using UnityEngine.Events;

namespace UI {
    public abstract class PuzzlePopup : MonoBehaviour {
        /// <summary>
        /// Called when the dungeon is cleared.<br />
        /// <c>float</c>: optimality - how close to optimal was the player's solution
        /// </summary>
        public UnityEvent<float> OnClear;
        bool cleared = false;

        protected virtual void Awake() {
            GetComponent<Popup>().OnExit.AddListener(OnUIExited);
        }

        public void OnUIExited() {
            if (!cleared)
                Clear(0);
        }

        protected void Clear(float optimality) {
            if (cleared)
                throw new System.Exception("Double clear");
            OnClear.Invoke(optimality);
            cleared = true;
        }
    }
}