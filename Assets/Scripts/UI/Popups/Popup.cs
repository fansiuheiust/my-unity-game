using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI {
    public class Popup : MonoBehaviour {
        [SerializeField] Button exitButton;
        public UnityEvent OnExit;
        public virtual void OnExitPressed() {
            if (!CanExit) return;
            OnExit.Invoke();
            Destroy(gameObject);
        }

        bool _canExit = true;
        public bool CanExit {
            get => _canExit;
            set {
                _canExit = value;
                exitButton.interactable = value;
            }
        }
    }
}