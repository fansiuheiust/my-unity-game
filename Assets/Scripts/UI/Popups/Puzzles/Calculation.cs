using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class Calculation : PuzzlePopup {
        [SerializeField]
        Transform choiceLayout;
        [SerializeField]
        TextMeshProUGUI expression;
        [SerializeField]
        TextMeshProUGUI timer;


        [SerializeField, Min(2)]
        int minLength;
        [SerializeField, Min(2)]
        int maxLength;
        [SerializeField, Min(0)]
        int valueMin, valueMax;
        [SerializeField, Min(1)]
        int timeLimit;
        [SerializeField, Min(20), Tooltip("Largest difference between incorrect and correct choice")]
        int choiceVariation;

        int length;
        int[] values;
        Operator[] operations;
        int correctAnswer;

        Button[] choices;
        int correctChoice;


        int remainingTime;
        

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
            correctAnswer = Evaluate(0, length - 1);

            choices = choiceLayout.GetComponentsInChildren<Button>();
            correctChoice = Random.Range(0, choices.Length-1);
            UpdateButtonValues();
            for (int i = 0; i < choices.Length; i++)
                choices[i].onClick.AddListener((i == correctChoice)? ()=>OnResponse(true): ()=>OnResponse(false));

            UpdateExpression();
            

            remainingTime = timeLimit;
            timer.text = timeLimit.ToString();
            _timer = StartCoroutine(Timer());
            
        }

        void OnResponse(bool correct) {
            DisableGame();
            if (!correct) {
                Clear(0, timeLimit);
                timer.color = Color.red;
                timer.text = "Wrong!";
            } else {
                Clear(Mathf.Min(remainingTime + (int)Mathf.Ceil(timeLimit/2), timeLimit), timeLimit);
                timer.color = Color.green;
                timer.text = $"Correct!";
            }
        }
        void Timeout() {
            DisableGame();
            Clear(0, timeLimit);
            timer.color = Color.red;
            timer.text = "Time's up!";
        }

        void DisableGame() {
            foreach (Button button in choices) {
                button.interactable = false;
                button.onClick.RemoveAllListeners();
            }
            StopCoroutine(_timer);
            _timer = null;
        }

        void UpdateButtonValues() {
            choices[correctChoice].GetComponentInChildren<TextMeshProUGUI>().text = correctAnswer.ToString();
            int[] distractions = new int[choices.Length - 1];
            for (int i = 0; i < distractions.Length; i++) {
                bool appeared, equalsCorrect;
                do {
                    distractions[i] = Random.Range(correctChoice - choiceVariation, correctChoice + choiceVariation);
                    equalsCorrect = distractions[i] == correctAnswer;
                    appeared = false;
                    for (int j = 0; j < i; j++)
                        if (distractions[j] == distractions[i]) {
                            appeared = true;
                            break;
                        }
                } while (appeared || equalsCorrect);
            }
            for (int i = 0, j = 0; i < choices.Length; i++, j++) {
                if (i == correctChoice) {
                    j--;
                    continue;
                }
                choices[i].GetComponentInChildren<TextMeshProUGUI>().text = distractions[j].ToString();

            }
        }

        void UpdateExpression() {
            string toPrint = "";
            for (int i = 0; i < length; i++) {
                toPrint += values[i].ToString();
                if (i < length - 1)
                    toPrint += $" {Op(operations[i])} ";
            }
            expression.text = toPrint;
        }

        Coroutine _timer = null;
        IEnumerator Timer() {
            while (true) {
                yield return new WaitForSeconds(1);
                remainingTime--;
                timer.text = remainingTime.ToString();
                if (remainingTime < 0)
                    Timeout();
            }
        }


        /// <summary>
        /// Evaluates a range
        /// </summary>
        /// <param name="start">index of the starting value</param>
        /// <param name="end">index of the ending value</param>
        int Evaluate(int start, int end) {
            // 0 1 2 3 4 5
            // 1 3 5 8 6 7
            //  - * + + *
            //  0 1 2 3 4
            if (start == end)
                return values[start];
            // find first +-
            for (int i = end-1; i >= start; i--) {
                if (operations[i] == Operator.Plus || operations[i] == Operator.Minus) {
                    return Evaluate(operations[i], Evaluate(start, i), Evaluate(i+1, end));
                }
            }
            for (int i = end-1;  i >= start; i--) {
                if (operations[i] == Operator.Multiply)
                    return Evaluate(start, i) * Evaluate(i+1, end);
            }
            throw new System.Exception("Code should not reach here");
        }
        int Evaluate(Operator op, int a, int b) => op switch {
            Operator.Plus => a + b,
            Operator.Minus => a - b,
            _ => a * b
        };

        enum Operator {
            Plus, Minus, Multiply
        }
        char Op(Operator x) => x switch { Operator.Plus => '+', Operator.Minus => '-', _ => '*' };
    }
}