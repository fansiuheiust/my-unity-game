using UnityEngine;

/// <summary>
/// A class for serializing the stats of a mob
/// </summary>
[System.Serializable]
public class SerializedMobStats {
    [field: SerializeField]
    public BaseStats @base;
    [field: SerializeField]
    public ScalingStats scaling;
    [field: SerializeField]
    public InitialHashedScaling[] hashedScaling;



    [System.Serializable]
    public struct InitialHashedScaling {
        [field: SerializeField]
        public HashedScalingStats stats;
        [field: SerializeField]
        public float data;
    }



}
