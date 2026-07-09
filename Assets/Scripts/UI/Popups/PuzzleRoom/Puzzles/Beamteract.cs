using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Puzzle {
    public class Beamteract : PuzzlePopup {
        [SerializeField]
        GameObject beamLinePrefab;

        [SerializeField, Tooltip("Invariant: sorted by higher position first")]
        Button[] transmitters;
        [SerializeField, Tooltip("Invariant: sorted by higher position first")]
        TextMeshProUGUI[] transmitterCharges;
        [SerializeField, Tooltip("Invariant: sorted by higher position first")]
        Transform[] receivers;
        [SerializeField]
        Transform beamContainer;

        [SerializeField]
        TextMeshProUGUI chargeSpawnedDisplay, chargeReceivedDisplay;

        [SerializeField]
        TextMeshProUGUI stateDisplay;

        [SerializeField, Min(4)]
        int familiarizationTime = 5;
        [SerializeField, Min(1)]
        int numCharges = 50, transmitterCapacity = 8;
        [SerializeField, Min(0.0625f)]
        float updateInterval = 0.25f;
        [SerializeField]
        int intervalPerTransmission = 2;


        /// <summary>
        /// Inference: <c>txOrder[i]</c> is the identifier of the <c>i</c>-th transmitter, vice-versa for rxOrder
        /// </summary>
        int[] txOrder, rxOrder;
        /// <summary>
        /// <c>mutex[i, j]</c> <c>i</c>-th transmitter cannot send signal with <c>j</c>-th transmitter simultaneously
        /// </summary>
        bool[,] mutex;
        /// <summary>
        /// <c>lines[i]</c> stores the beam of the <c>i</c>-th transmitter
        /// </summary>
        Image[] lines;
        /// <summary>
        /// <c>beaming[i]</c> stores whether the <c>i</c>-th transmitter is beaming
        /// </summary>
        bool[] beaming;

        int[] _charges;

        int _receivedCharge = 0;
        int ReceivedCharge {
            get => _receivedCharge;
            set {
                _receivedCharge = value;
                chargeReceivedDisplay.text = $"Charges received: <color={NormalCharge}>{value}</color>";
            }
        }

        int _spawnedCharge = 0;
        int SpawnedCharge {
            get => _spawnedCharge;
            set {
                _spawnedCharge = value;
                chargeSpawnedDisplay.text = $"Charges spawned: <color={NormalCharge}>{value}</color>/{numCharges}";
            }
        }


        static readonly Color[] Colors = new Color[] { Color.red, Color.green, Color.blue, Color.white };
        static readonly string WastedCharge = "#f00", NormalCharge = "#0ff";

        protected override void Awake() {
            base.Awake();
            if (transmitters.Length != transmitterCharges.Length || transmitters.Length != receivers.Length) {
                throw new System.Exception("Uneven array length for the transmitters");
            }
            InitOrder();
            SetupGame();
            StartCoroutine(GameCycle());
        }

        void InitOrder() {
            txOrder = new int[transmitters.Length];
            rxOrder = new int[transmitters.Length];
            int pass = 0;
            Reroll:
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

            mutex = new bool[transmitters.Length, transmitters.Length];
            bool needReshuffling = true;
            for (int i = 0; i < transmitters.Length; i++) {
                int mutexCount = 0;
                for (int j = 0; j < transmitters.Length; j++) {
                    if (i == j) continue;
                    int rxi = 0, rxj = 0;
                    for (; rxi < transmitters.Length; rxi++)
                        if (txOrder[i] == rxOrder[rxi]) break;
                    for (; rxj < transmitters.Length; rxj++)
                        if (txOrder[j] == rxOrder[rxj]) break;

                    if (i < j && rxi > rxj || i > j && rxi < rxj) {
                        mutex[i, j] = true;
                        mutexCount++;
                    }
                }
                // no need to reshuffle if there exists a transmitter that has at least <intervalPerTransmission> non-mutexes
                if (transmitters.Length - mutexCount >= intervalPerTransmission)
                    needReshuffling = false;
            }
            pass++;
            if (pass >= 100) Debug.Log("Too many passes, resorting to difficult setup");
            if (needReshuffling && pass < 100) goto Reroll;
        }

        void SetupGame() {
            lines = new Image[transmitters.Length];
            beaming = new bool[transmitters.Length];
            _charges = new int[transmitters.Length];
            for (int i = 0; i < transmitters.Length; i++) {
                transmitters[i].GetComponent<Image>().color = Colors[txOrder[i]];
                transmitters[i].GetComponentInChildren<TextMeshProUGUI>().text = "Transmitter " + txOrder[i].ToString();
                receivers[i].GetComponent<Image>().color = Colors[rxOrder[i]];
                receivers[i].GetComponentInChildren<TextMeshProUGUI>().text = "Receiver " + rxOrder[i].ToString();
                int copy = i;
                transmitters[i].interactable = false;
                transmitters[i].onClick.AddListener(() => OnTransmitterClicked(copy));
                // j s.t. txOrder[i] == rxOrder[j]
                int j = 0;
                for (; j < rxOrder.Length; j++)
                    if (txOrder[i] == rxOrder[j])
                        break;
                DependencyLine line = Instantiate(beamLinePrefab, beamContainer).GetComponent<DependencyLine>();
                line.From = transmitters[i].transform.position;
                line.To = receivers[j].transform.position;
                Color c = Colors[rxOrder[j]];
                c.a = 0.5f;
                line.Color = c;
                lines[i] = line.GetComponent<Image>();
            }
            SpawnedCharge = 0;
            ReceivedCharge = 0;
            for (int i = 0; i < transmitters.Length; i++)
                TransmitterDisplay(i, NormalCharge);
        }


        int cycle = 0;
        IEnumerator GameCycle() {
            stateDisplay.text = "Familiarize yoursel!";
            yield return new WaitForSeconds(familiarizationTime);

            foreach (var l in lines)
                l.enabled = false;
            for (int i = 3; i > 0; i--) {
                stateDisplay.text = i.ToString();
                yield return new WaitForSeconds(1);
            }

            foreach (var b in transmitters)
                b.interactable = true;
            stateDisplay.text = "";
            while (true) {
                if (cycle == 0)
                    TransmitCharge();
                if (SpawnedCharge < numCharges)
                    SpawnCharge();
                cycle = (cycle + 1) % intervalPerTransmission;
                if (_charges.Sum() == 0) break;
                yield return new WaitForSeconds(updateInterval);
            }
            foreach (var b in transmitters)
                b.onClick.RemoveAllListeners();
            stateDisplay.text = "Game over!";
            Clear(ReceivedCharge, numCharges);
            yield break;
        }

        void TransmitCharge() {
            for (int i = 0; i < transmitters.Length; i++) {
                if (!beaming[i]) continue;
                bool decreased = DecCharge(i);
                bool intercepted = false;
                for (int j = 0; j < transmitters.Length; j++) {
                    if (beaming[j] && mutex[i, j]) {
                        intercepted = true;
                        break;
                    }
                }
                if (!decreased) continue;
                if (!intercepted)
                    ReceivedCharge++;
                else
                    TransmitterDisplay(i, WastedCharge);

            }
        }

        void SpawnCharge() {
            IncCharge(Random.Range(0, transmitters.Length));
            SpawnedCharge++;
        }


        /// <summary>
        /// Interaction logic with the <c>i</c>-th transmitter is clicked
        /// </summary>
        /// <param name="i">transmitter, ordered by higher position first</param>
        void OnTransmitterClicked(int i) {
            beaming[i] = !beaming[i];
            lines[i].enabled = beaming[i];
        }


        void TransmitterDisplay(int i, string color) {
            transmitterCharges[i].text = $"<color={color}>{_charges[i]}</color>/{transmitterCapacity}";
        }
        /// <summary>
        /// Increments the charges in the <c>i</c>-th transmitter
        /// </summary>
        /// <param name="i">ith transmitter by order top first</param>
        /// <returns>true if charge has been incremented</returns>
        bool IncCharge(int i) {
            bool ri = _charges[i] < transmitterCapacity;
            if (ri)
                _charges[i]++;
            TransmitterDisplay(i, ri? NormalCharge: WastedCharge);
            return ri;
        }
        /// <summary>
        /// Decrements the charges in the <c>i</c>-th transmitter
        /// </summary>
        /// <param name="i">ith transmitter by order top first</param>
        /// <returns>true if charge has been decremented</returns>
        bool DecCharge(int i) {
            bool ri = _charges[i] > 0;
            if (ri)
                _charges[i]--;
            TransmitterDisplay(i, NormalCharge);
            return ri;
        }
    }
}