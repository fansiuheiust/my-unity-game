using System.Collections.Generic;
using UnityEngine;
using Combat;

namespace Dungeon {
    public class MobRoom : Room {
        Transform _mobParent;
        List<Mob> mobs = new();
        void Awake() {
            _mobParent = transform.Find("Targets");
            for (int i = 0; i < _mobParent.childCount; i++) {
                if (_mobParent.GetChild(i).TryGetComponent(out Mob m) && IsValidEnemy(m)) {
                    mobs.Add(m);
                    m.OnDeath.AddListener(OnMobDied);
                }
            }
            if (mobs.Count == 0) throw new System.Exception("Mob room has no valid mobs");
        }

        /// <summary>
        /// See: returns. <br />
        /// For now, this will return true for all cases. But in the future, some mobs should not be marked as to kill, e.g. allies.
        /// </summary>
        /// <param name="m">Self-documenting</param>
        /// <returns>whether a mob should be in the to kill list</returns>
        protected bool IsValidEnemy(Mob m) => true;

        /// <summary>
        /// Called once a mob dies, takes it out of the mob list, and mark the room as cleared if there are no mobs left
        /// </summary>
        /// <param name="m">mob that is about to die</param>
        /// <param name="killer">irrelevant</param>
        void OnMobDied(Mob m, Mob killer) {
            mobs.Remove(m);
            if (mobs.Count == 0) {
                Cleared = true;
                Debug.Log("Cleared a room");
            }
        }
    }
}