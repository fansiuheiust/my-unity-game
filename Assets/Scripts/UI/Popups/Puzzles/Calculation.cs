using UnityEngine;

namespace UI {
    public class Calculation : PuzzlePopup {
        [SerializeField, Min(1)]
        int minLength;
        [SerializeField, Min(1)]
        int maxLength;
        [SerializeField, Min(0)]
        int valueMin, valueMax;
        int length;
        int[] values;
        Operator[] operations;

        protected override void Awake() {
            base.Awake();
            length = Random.Range(minLength, maxLength);
            values = new int[length];
            operations = new Operator[length-1];
            for (int i = 0; i < length; i++) {
                values[i] = Random.Range(valueMin, valueMax);
            }
            for (int i = 0; i < length - 1; i++)
                operations[i] = (Operator)Random.Range(0, 3);
        }

        enum Operator {
            Plus, Minus, Multiply, Divide
        }
    }
}