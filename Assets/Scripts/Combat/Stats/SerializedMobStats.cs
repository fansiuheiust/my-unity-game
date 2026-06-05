using System.Collections.Generic;
using UnityEngine;


namespace Combat {
    /// <summary>
    /// A class for serializing the stats of a mob, <c>InsertHashedStats MUST be called before usage</c>
    /// </summary>
    [System.Serializable]
    public class SerializedMobStats {
        [field: SerializeField]
        public BaseStats @base;
        [field: SerializeField]
        public ScalingStats scaling;
        [field: SerializeField]
        public InitialHashedScaling[] hashedScaling;

        public void InsertHasedStats() {
            Dictionary<HashedScalingStats, float> keyValuePairs = new();
            foreach (var hash in hashedScaling)
                keyValuePairs.Add(hash.stats, hash.data);
            scaling.InitializeHash(keyValuePairs);
        }

        [System.Serializable]
        public struct InitialHashedScaling {
            [field: SerializeField]
            public HashedScalingStats stats;
            [field: SerializeField]
            public float data;
        }


    }
}
