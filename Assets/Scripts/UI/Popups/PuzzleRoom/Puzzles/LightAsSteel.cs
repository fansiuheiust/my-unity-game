using UnityEngine;
using UnityEngine.UI;
using UI.Puzzle.LightAsSteelUtil;
using System.Collections.Generic;
using System.Collections;
using TMPro;

namespace UI.Puzzle {
    namespace LightAsSteelUtil {
        public class Wagon {
            public int Row { get; private set; }
            public int Col { get; private set; }
            public int flightDur;
            public readonly bool horizontal;
            public readonly Train owner;
            public readonly bool isHead;
            public Wagon(int row, int col, bool horizontal, Train owner, bool isHead) {
                Row = row;
                Col = col;
                flightDur = 0;
                this.horizontal = horizontal;
                this.owner = owner;
                this.isHead = isHead;
            }
            public void Forward() {
                if (horizontal) {
                    Col++;
                } else {
                     Row++;
                }
                if (flightDur > 0)
                    flightDur--;
            }
        }

        public class Train {
            /// <summary>
            /// row of head wagon
            /// </summary>
            public int Row { get; private set;}
            /// <summary>
            /// column of head wagon
            /// </summary>
            public int Col { get; private set;}
            public readonly bool horizontal;
            /// <summary>
            /// Invariant: the array is ordered by front first, back last
            /// </summary>
            public readonly Wagon[] wagons;
            readonly HashSet<int> ramps = new();

            /// <summary>
            /// true if the head wagon is on air
            /// </summary>
            public bool OnAir => wagons[0].flightDur > 0;

            public Train(int row, int col, bool horizontal, int numWagons) {
                Row = row;
                Col = col;
                this.horizontal = horizontal;
                wagons = new Wagon[numWagons];
                for (int i = 0; i < numWagons; i++) {
                    wagons[i] = new(horizontal ? row : row - i, horizontal ? col - i : col, horizontal, this, i == 0);
                }
            }
            /// <summary>
            /// Moves the train forward by 1 unit
            /// </summary>
            /// <param name="ground">The grid of wagons on ground</param>
            /// <param name="air">The grid of wagons on air</param>
            /// <returns>the wagon that the train crashed with</returns>
            public Wagon Forward(Wagon[,] ground, Wagon[,] air) {
                Wagon crashed = null;
                foreach (var wagon in wagons) {
                    // update the relevant grid if the wagon is within it
                    Wagon[,] toNull = wagon.flightDur > 0 ? air : ground;
                    if (wagon.Row >= 0 && wagon.Col >= 0 && wagon.Row < toNull.GetLength(0) && wagon.Col < toNull.GetLength(1) && toNull[wagon.Row, wagon.Col] != null && toNull[wagon.Row, wagon.Col].owner == this)
                        toNull[wagon.Row, wagon.Col] = null;
                    wagon.Forward();
                    // send a grounded wagon flying if it lies on 1 of its ramps
                    if (wagon.flightDur == 0 && (horizontal && ramps.Contains(wagon.Col-1) || !horizontal && ramps.Contains(wagon.Row-1)))
                        wagon.flightDur = LightAsSteel.RampDur;
                    // update the relevant grid if the wagon is within it
                    Wagon[,] toSet = wagon.flightDur > 0 ? air : ground;
                    if (wagon.Row >= 0 && wagon.Col >= 0 && wagon.Row < toSet.GetLength(0) && wagon.Col < toSet.GetLength(1)) {
                        //                                         not the last wagon of another train
                        if (toSet[wagon.Row, wagon.Col] != null && toSet[wagon.Row, wagon.Col].owner.wagons[^1] != toSet[wagon.Row, wagon.Col])
                            crashed = toSet[wagon.Row, wagon.Col];
                        toSet[wagon.Row, wagon.Col] = wagon;
                    }
                }
                Row = wagons[0].Row;
                Col = wagons[0].Col;
                return crashed;
            }
            /// <summary>
            /// Places a ramp at the head of the train
            /// </summary>
            public void PlaceRamp() {
                if (horizontal)
                    ramps.Add(Col);
                else
                    ramps.Add(Row);
            }

            public void RemoveFromGrid(Wagon[,] ground, Wagon[,] air) {
                foreach (Wagon wagon in wagons) {
                    Wagon[,] toNull = wagon.flightDur > 0? air : ground;
                    if (wagon.Row >= 0 && wagon.Col >= 0 && wagon.Row < toNull.GetLength(0) && wagon.Col < toNull.GetLength(1))
                        toNull[wagon.Row, wagon.Col] = null;
                }
            }
        }
    }
    
    public class LightAsSteel : PuzzlePopup {

        static string ValueColor = "#0ff", CollisionColor="#f00", SurvivedColor="#0f0";

        [SerializeField]
        RectTransform tileContainer;
        [SerializeField]
        TextMeshProUGUI rampDurDisplay;
        [SerializeField]
        TextMeshProUGUI spawnedDisplay, collisionDisplay, survivedDisplay;
        [SerializeField]
        TextMeshProUGUI gameOverDisplay;

        [SerializeField, Min(1)]
        int rows, cols;

        LightAsSteelTile[,] tiles;
        Button[,] buttons;

        Wagon[,] groundGrid, airGrid;
        HashSet<Train> trains = new();


        [SerializeField]
        int rampDur;
        [SerializeField, Tooltip("Row is 0 for vertical trains, Col is 0 for horizontal trains")]
        int[] horizontalSpawns, verticalSpawns;

        [SerializeField, Min(1)]
        int numTrains = 34;
        [SerializeField, Min(1)]
        int minTrainLength = 2, maxTrainLength = 5;
        [SerializeField, Min(1), Tooltip("Number of cycles for each train spawn")]
        int cyclesPerSpawn = 4;
        [SerializeField, Min(1), Tooltip("May not attain minSpawnPerCycle if there is no available positions")]
        int minSpawnPerInterval = 1, maxSpawnPerInterval = 4;
        [SerializeField, Min(0.5f)]
        float cycleInterval = 0.75f;

        public static int RampDur { get; private set; }

        int _collisions;

        int Collisions {
            get => _collisions;
            set {
                _collisions = value;
                collisionDisplay.text = $"Collisions: <color={CollisionColor}>{value}</color>";
            }
        }

        int _survivals;
        int Survivals {
            get => _survivals;
            set {
                _survivals = value;
                survivedDisplay.text = $"Survivals: <color={SurvivedColor}>{value}</color>";
            }
        }

        int _spawned;
        int Spawned {
            get => _spawned;
            set {
                _spawned = value;
                spawnedDisplay.text = $"Spawned: <color={ValueColor}>{value}</color>/{numTrains}";
            }
        }
        

        protected override void Awake() {
            base.Awake();
            Collisions = 0;
            Survivals = 0;
            Spawned = 0;
            rampDurDisplay.text = $"Ramp Jump: <color={ValueColor}>{rampDur}</color> tiles";
            RampDur = rampDur;
            InitializeTiles();
            StartCoroutine(Loop());
        }
        IEnumerator Loop() {
            for (int i = 3; i > 0; i--) {
                gameOverDisplay.text = i.ToString();
                yield return new WaitForSeconds(1);
            }
            gameOverDisplay.text = "";
            while (true) {
                GameCycle();
                if (Collisions + Survivals == numTrains) {
                    GameOver();
                    break;
                }
                yield return new WaitForSeconds(cycleInterval);
            }
        }

        void GameOver() {
            gameOverDisplay.text = "Game over!";
            Clear(Survivals, numTrains);
        }


        void InitializeTiles() {
            tiles = new LightAsSteelTile[rows, cols];
            buttons = new Button[rows, cols];
            groundGrid = new Wagon[rows, cols];
            airGrid = new Wagon[rows, cols];
            foreach (var t in tileContainer.GetComponentsInChildren<LightAsSteelTile>()) {
                tiles[t.Row, t.Col] = t;
                buttons[t.Row, t.Col] = t.GetComponent<Button>();
                buttons[t.Row, t.Col].interactable = false;
                buttons[t.Row, t.Col].onClick.AddListener(()=>OnButtonClick(t.Row, t.Col));
            }
        }


        static readonly string GroundHead = "#080", GroundNormal = "#0F0", AirHead = "#00F", AirNormal = "#0FF", Default = "#FFF";
        void UpdateButtons() {
            for (int i =0; i < rows;i++) {
                for (int j = 0; j < cols; j++) {
                    if (buttons[i, j] == null) continue;
                    buttons[i, j].interactable = false;
                    string toUse = Default;
                    if (groundGrid[i, j] != null) {
                        toUse = groundGrid[i, j].isHead ? GroundHead : GroundNormal;
                        if (groundGrid[i, j].isHead)
                            buttons[i, j].interactable = true;
                    }
                    if (airGrid[i, j] != null)
                        toUse = airGrid[i, j].isHead? AirHead: AirNormal;
                    buttons[i, j].GetComponent<Image>().color = GlobalColor.Parse(toUse);
                }
            }
        }

        void OnButtonClick(int r, int c) {
            // note that a button is only clickable if it contains the ground train head
            groundGrid[r, c].owner.PlaceRamp();
        }


        int cycles = 0;
        void GameCycle() {
            HashSet<Train> toDelete = new();
            foreach (var t in trains) {
                if (toDelete.Contains(t)) continue;
                Wagon collided = t.Forward(groundGrid, airGrid);
                if (collided != null) {
                    toDelete.Add(t);
                    Collisions++;
                    if (!toDelete.Contains(collided.owner)) {
                        toDelete.Add(collided.owner);
                        Collisions ++;
                    }
                }
                // out of range entirely
                else if (t.wagons[^1].Row >= rows || t.wagons[^1].Col >= cols) {
                    toDelete.Add(t);
                    Survivals++;
                }
            }
            foreach (var t in toDelete) {
                t.RemoveFromGrid(groundGrid, airGrid);
                trains.Remove(t);
            }

            if (cycles % cyclesPerSpawn == 0) {
                // spawn new trains if possible
                List<int> availableVerticles = new(), availableHorizontals = new();
                foreach (int v in verticalSpawns)
                    if (groundGrid[0, v] == null && groundGrid[1, v] == null)
                        availableVerticles.Add(v);
                foreach (int h in horizontalSpawns)
                    if (groundGrid[h,0] == null && groundGrid[h, 1] == null)
                        availableHorizontals.Add(h);
                int numSpawn = Mathf.Min(Random.Range(minSpawnPerInterval, maxSpawnPerInterval+1), availableHorizontals.Count + availableVerticles.Count, numTrains - Spawned);
                Shuffle(availableHorizontals);
                Shuffle(availableVerticles);
                int currV = 0, currH = 0;
                int actualSpawn = 0;
                for (int i = 0; i < numSpawn; i++) {
                    Train t;
                    if (Random.Range(0, 2) == 0 && currH < availableHorizontals.Count) {
                        // choose Horizontal
                        t = new(availableHorizontals[currH++], 0, true, Random.Range(minTrainLength, maxTrainLength+1));
                    } else if (currV < availableVerticles.Count) {
                        t = new(0, availableVerticles[currV++], false, Random.Range(minTrainLength, maxTrainLength+1));
                    } else {
                        break;
                    }
                    groundGrid[t.Row, t.Col] = t.wagons[0];
                    trains.Add(t);
                    actualSpawn++;
                }
                Spawned += actualSpawn;
            }
            cycles++;

            UpdateButtons();
        }

        void Shuffle<T>(List<T> list) {
            for (int i = list.Count-1; i > 0; i--) {
                int k = Random.Range(0, i+1);
                (list[k], list[i]) = (list[i], list[k]);
            }
        }
    }
}