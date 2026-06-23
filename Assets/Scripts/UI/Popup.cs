using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI {
    public class Popup : MonoBehaviour {
        [SerializeField] Button exitButton;
        public UnityEvent OnExit;
        public virtual void OnExitPressed() {
            OnExit.Invoke();
            Destroy(gameObject);
        }
    }
}