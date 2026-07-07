using UnityEngine;
using UnityEngine.UI;
using UI.Puzzle.LightAsSteelUtil;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace UI.Puzzle {
    namespace LightAsSteelUtil {
        public class Wagon {
            public int Row { get; private set; }
            public int Col { get; private set; }
            public int flightDur;
            public readonly bool horizontal;
            public readonly Train owner;
            public Wagon(int row, int col, bool horizontal, Train owner) {
                Row = row;
                Col = col;
                flightDur = 0;
                this.horizontal = horizontal;
                this.owner = owner;
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
                    wagons[i] = new(horizontal ? row : row - i, horizontal ? col - i : col, horizontal, this);
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
                    if (Row >= 0 && Col >= 0 && Row < toNull.GetLength(0) && Col < toNull.GetLength(1))
                        toNull[Row, Col] = null;
                    wagon.Forward();
                    // send a grounded wagon flying if it lies on 1 of its ramps
                    if (wagon.flightDur == 0 && (horizontal && ramps.Contains(wagon.Col-1) || !horizontal && ramps.Contains(wagon.Row-1)))
                        wagon.flightDur = LightAsSteel.RampDur;
                    // update the relevant grid if the wagon is within it
                    Wagon[,] toSet = wagon.flightDur > 0 ? air : ground;
                    if (Row >= 0 && Col >= 0 && Row < toSet.GetLength(0) && Col < toSet.GetLength(1)) {
                        if (toSet[Row, Col] != null)
                            crashed = toSet[Row, Col];
                        toSet[Row, Col] = wagon;
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
                    if (Row >= 0 && Col >= 0 && Row < toNull.GetLength(0) && Col < toNull.GetLength(1))
                        toNull[Row, Col] = null;
                }
            }
        }
    }
    
    public class LightAsSteel : PuzzlePopup {
        [SerializeField]
        RectTransform tileContainer;
        [SerializeField, Min(1)]
        int rows, cols;

        LightAsSteelTile[,] tiles;
        Button[,] buttons;


        [SerializeField]
        int rampDur;

        public static int RampDur { get; private set; }
        

        protected override void Awake() {
            base.Awake();
            RampDur = rampDur;
            InitializeTiles();
        }

        void InitializeTiles() {
            tiles = new LightAsSteelTile[rows, cols];
            buttons = new Button[rows, cols];
            foreach (var t in tileContainer.GetComponentsInChildren<LightAsSteelTile>()) {
                tiles[t.Row, t.Col] = t;
                buttons[t.Row, t.Col] = t.GetComponent<Button>();
                buttons[t.Row, t.Col].interactable = false;
                buttons[t.Row, t.Col].onClick.AddListener(()=>OnButtonClick(t.Row, t.Col));
            }
        }
        void OnButtonClick(int r, int c) {

        }
    }
}