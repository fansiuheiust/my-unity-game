using System.Collections;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace UI.Puzzle {
    public class Counting : PuzzlePopup {

        static readonly string ValueColor = "#00FFFF", AdditionColor  = "#00FF00", DeductionColor = "#FF0000";

        [SerializeField]
        Transform buttonList;
        [SerializeField]
        TextMeshProUGUI scoreDisplay, timeDisplay;
        [SerializeField]
        TextMeshProUGUI resetWarning;


        [SerializeField, Min(1)]
        int initialScore = 10;
        [SerializeField, Min(1)]
        int additiveMin = 5, additiveMax = 12;
        [SerializeField, Min(1)]
        float multiplicativeMin = 1.5f, multiplicativeMax = 3f;
        [SerializeField, Min(1)]
        int time = 5;
        [SerializeField, Min(1), Tooltip("Note that choices here refers to number of times player has to choose, NOT number of buttons")]
        int numChoices = 5;
        [SerializeField, Min(0.1f), Tooltip("A brief time when the buttons remain disabled to prevent undesirable clicking")]
        float choiceBufferTime = 0.1f;
        [SerializeField, Min(0.5f), Tooltip("Warns player when there is <value> seconds left")]
        float choiceWarnTime = 0.5f;

        float TimePerChoice => (float)time / numChoices;

        Button[] buttons;
        Operator[] operators;
        float[] values;

        int _score = 0;
        int Score {
            get => _score;
            set {
                scoreDisplay.text = $"Score: <color={(Score < value? AdditionColor: DeductionColor)}>{value}</color>";
                _score = value;
            }
        }

        int optimalScore;


        protected override void Awake() {
            base.Awake();
            Score = initialScore;
            optimalScore = initialScore;
            InitButtons();
            SetButtonInteractable(false);
            StartCoroutine(UpdateButtons());
            StartCoroutine(Timer());
        }
        void InitButtons() {
            buttons = buttonList.GetComponentsInChildren<Button>();
            operators = new Operator[buttons.Length];
            values = new float[buttons.Length];
            for (int i = 0; i < buttons.Length; i++) {
                int copy = i;
                buttons[i].onClick.AddListener(() => OnButtonClicked(copy));
            }
        }
        void UpdateButtonDisplay() {
            for (int i = 0; i < buttons.Length; i++) {
                string toPrint = operators[i] switch { Operator.Plus => "+", Operator.Minus => "-", Operator.Times => "*", _ => "/"};
                if (operators[i] == Operator.Plus || operators[i] == Operator.Minus)
                    toPrint += $"{values[i]:F0}";
                else
                    toPrint += $"{values[i]:F1}";
                buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = toPrint;
            }
        }
        void SetButtonInteractable(bool interactable) {
            foreach (Button button in buttons) button.interactable = interactable;
        }

        void OnButtonClicked(int i) {
            SetButtonInteractable(false);
            Score = Evaluate(operators[i], Score, values[i]);
        }


        void UpdateOptimal() {
            int nextOptimal = optimalScore;
            for (int i =0; i < buttons.Length; i++) {
                int candidate = Evaluate(operators[i], optimalScore, values[i]);
                if (candidate > nextOptimal)
                    nextOptimal = candidate;
            }
            optimalScore = nextOptimal;
        }

        void GameOver() {
            SetButtonInteractable(false);
            scoreDisplay.text = $"Score: <color={ValueColor}>{Score}</color>/{optimalScore}";
            timeDisplay.text = "Time's Up!";
            Clear(Score, optimalScore);
        }

        IEnumerator UpdateButtons() {
            float stayTime = TimePerChoice - choiceBufferTime - choiceWarnTime;
            if (stayTime < 0) throw new System.Exception("Choice buffer time + choice warning time exceed stay time");
            resetWarning.text = $"Choices resetting in {choiceWarnTime:F1}s";
            resetWarning.alpha = 0;
            for (int _ = 0; _ < numChoices; _++) {
                SetButtonInteractable(false);
                for (int i = 0; i < buttons.Length; i++) {
                    operators[i] = (Operator)Random.Range(0, 4);
                    values[i] = operators[i] == Operator.Plus || operators[i] == Operator.Minus ? Random.Range(additiveMin, additiveMax+1) : Mathf.Round(Random.Range(multiplicativeMin, multiplicativeMax)*10)/10;
                }
                UpdateButtonDisplay();
                UpdateOptimal();
                yield return new WaitForSeconds(choiceBufferTime);
                SetButtonInteractable(true);
                yield return new WaitForSeconds(stayTime);
                resetWarning.alpha = 1f;
                yield return new WaitForSeconds(choiceWarnTime);
                resetWarning.alpha = 0;
            }
            yield break;
        }
        
        IEnumerator Timer() {
            for (int i = 0; i< time; i++) {
                timeDisplay.text = $"Time: <color={ValueColor}>{i}</color>/{time}s";
                yield return new WaitForSeconds(1);
            }
            GameOver();
            yield break;
        }



        enum Operator {
            Plus, Minus, Times, Divide
        }
        int Evaluate(Operator op, int a, float b) => op switch {
            Operator.Plus => a + (int)b,
            Operator.Minus => a - (int)b,
            Operator.Times => Mathf.RoundToInt(a * b),
            _ => Mathf.RoundToInt(a / b)
        };
    }
}