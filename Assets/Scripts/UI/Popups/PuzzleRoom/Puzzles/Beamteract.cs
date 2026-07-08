using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Puzzle {
    public class Beamteract : PuzzlePopup {
        [SerializeField]
        GameObject beamLinePrefab;
        
        [SerializeField]
        Button[] transmitters;
        [SerializeField]
        TextMeshProUGUI[] transmitterCharges;
        [SerializeField]
        Transform[] receivers;
        [SerializeField]
        Transform beamContainer;


        /// <summary>
        /// Inference: txOrder[0] is the identifier of the 1st transmitter, vice-versa for rxOrder
        /// </summary>
        int[] txOrder, rxOrder;

        static readonly Color[] Colors = new Color[] { Color.red, Color.green, Color.blue };

        protected override void Awake() {
            base.Awake();
            if (transmitters.Length != transmitterCharges.Length || transmitters.Length != receivers.Length) {
                throw new System.Exception("Uneven array length for the transmitters");
            }
            InitOrder();

        }

        void InitOrder() {
            txOrder = new int[transmitters.Length];
            rxOrder = new int[transmitters.Length];
            for (int i = 0; i < txOrder.Length; i++) {
                txOrder[i] = i;
                rxOrder[i] = i;
            }
            Shuffle(txOrder);

            // derangement of txOrder
            bool unequal;
            do {
                unequal = true;
                Shuffle(rxOrder);
                for (int i = 0; i < rxOrder.Length; i++)
                    if (txOrder[i] == rxOrder[i])
                        unequal = false;
            } while (!unequal);
            SetupGame();
        }

        void SetupGame() {
            for (int i = 0; i < transmitters.Length; i++) {
                transmitters[i].GetComponent<Image>().color = Colors[txOrder[i]];
                transmitters[i].GetComponentInChildren<TextMeshProUGUI>().text = "Transmitter " + txOrder[i].ToString();
                receivers[i].GetComponent<Image>().color = Colors[rxOrder[i]];
                receivers[i].GetComponentInChildren<TextMeshProUGUI>().text = "Receiver " + rxOrder[i].ToString();
                // j s.t. txOrder[i] == rxOrder[j]
                int j = 0;
                for (; j < rxOrder.Length; j++)
                    if (txOrder[i] == rxOrder[j])
                        break;
                DependencyLine line = Instantiate(beamLinePrefab, beamContainer).GetComponent<DependencyLine>();
                line.From = transmitters[i].transform.position;
                line.To = receivers[j].transform.position;
                line.Color = Colors[rxOrder[j]];
            }
        }
    }
}