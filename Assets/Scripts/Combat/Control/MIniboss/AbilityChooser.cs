using Dungeon;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;



namespace Combat.Miniboss {
    public class AbilityChooser {
        (System.Func<IEnumerator> ability, System.Func<bool> predicate)[] abilities;
        int curr = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="abilities">A list of abilities by their IEnumerator methods and their predicates (conditions for ability activation)</param>
        /// 
        public AbilityChooser(params (System.Func<IEnumerator>, System.Func<bool>)[] abilities) {
            this.abilities = abilities.ToArray();
        }
        /// <summary>
        /// Reshuffles the abilities
        /// </summary>
        public void Reset() {
            curr = 0;
            UI.Puzzle.PuzzlePopup.Shuffle(abilities);
        }

        /// <summary>
        /// Chooses the next available ability.<br />
        /// Reshuffles if reached the end of the current permutation.
        /// </summary>
        /// <returns>The next available ability to use, null if an ability is active or no abilities fulfill the predicate</returns>
        public bool Next(out System.Func<IEnumerator> f) {
            f = null;
            for (; curr < abilities.Length; curr++) {
                if (abilities[curr].predicate()) {
                    f = abilities[curr].ability;
                    // make curr point to the next ability to consider
                    curr++;
                    break;
                }
            }
            if (curr >= abilities.Length)
                Reset();

            return f is not null;
        }
    }
}