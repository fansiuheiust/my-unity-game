using System.Collections.Generic;
using UnityEngine;


namespace Dungeon {
    public class MinibossRoom : Room {
        [SerializeField, Tooltip("Outer nest: floor since it is availabe; Inner nest: the list of bosses for that floor")]
        BossArray[] bosses;
    }

    [System.Serializable]
    class BossArray {
        public string[] bosses;
    }
}