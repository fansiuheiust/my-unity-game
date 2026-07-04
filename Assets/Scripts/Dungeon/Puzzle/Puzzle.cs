using UnityEngine;
using UnityEngine.Events;

namespace Dungeon {
    public abstract class Puzzle : MonoBehaviour {
        /// <summary>
        /// <para>Invoked when the puzzle room is cleared</para>
        /// <c>int1</c>: score <br />
        /// <c>int2</c>: optimal score
        /// </summary>
        public UnityEvent<int, int> OnClear;
        public abstract void StartPuzzle();

        protected virtual void Clear(int score, int optimal) {
            OnClear.Invoke(score, optimal);
        }
    }
}