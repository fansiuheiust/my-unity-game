using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class PuzzleIntro : MonoBehaviour {
        [SerializeField]
        Transform background;
        [field: SerializeField]
        public Button StartButton { get; private set; }
        [SerializeField]
        string pathToGuides;
        GameObject guide = null;

        string _puzzleID;
        public string PuzzleID { 
            get =>_puzzleID;
            set {
                _puzzleID = value;
                if (guide != null)
                    Destroy(guide);
                guide = Instantiate((GameObject)Resources.Load(pathToGuides+"/"+PuzzleID), background);
            }
        }
    }
}