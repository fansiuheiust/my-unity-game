using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Combat {
    /// <summary>
    /// A class for serializing the stats of a mob, <c>InsertHashedStats MUST be called before usage</c>
    /// </summary>
    [System.Serializable]
    public class SerializedMobStats {
        public BaseStats Base => new BaseStats(baseStats.ToDictionary(x=>x.stats, x=>x.@base));
        public ScalingStats Scaling => new ScalingStats(baseStats.ToDictionary(x => x.stats, x => x.scale), hashedScaling.ToDictionary(x => x.stats, x => x.data));
        public InitialHashedBase[] baseStats = new InitialHashedBase[0];
        public InitialHashedScaling[] hashedScaling = new InitialHashedScaling[0];

        [System.Serializable]
        public struct InitialHashedScaling {
            [field: SerializeField]
            public ScalingAttribute stats;
            [field: SerializeField]
            public float data;
        }

        [System.Serializable]
        public struct InitialHashedBase {
            public BaseAttribute stats;
            public float @base, scale;
        }


    }
}
