
using UnityEngine;

/// <summary>
/// Controls coins and perks
/// TODO: expand this such that perks can be placed
/// </summary>
[CreateAssetMenu(fileName = "Perks", menuName = "Scriptable Objects/Perks")]
public class Perks : ScriptableObject {
    [field: SerializeField, Tooltip("What 1 coin of a tier is equivalent to 1 tier lower")]
    public uint CoinDecompositionRatio { get; private set; }
    [field: SerializeField, Tooltip("How many level points a coin is equivalent to"), Min(0f)]
    public float CoinPerLevelPoint { get; private set; }
}
