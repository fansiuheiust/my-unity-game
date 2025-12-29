using Combat;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Interactable {
    public class Trial : MonoBehaviour, IInteractable {
        /// <summary>
        /// How long the trial should last
        /// </summary>
        [field: SerializeField, Range(0, float.MaxValue)] public float Duration { get; private set; } = 30f;
        /// <summary>
        /// Whether the trial should be a success when the time runs out
        /// </summary>
        [field: SerializeField] public bool IsSuccessByDefault { get; private set; } = true;
        /// <summary>
        /// Whether the trial can be redone after the player fails it
        /// </summary>
        [field: SerializeField] public bool IsRedoable { get; private set; } = true;

        [field: SerializeField] public string TrialDescription { get; private set; }

        public UnityEvent OnTrialSuccess;
        public UnityEvent OnTrialFailure;

        Coroutine _timer = null;

        public bool IsOngoing => _timer != null;

        [DoNotSerialize] public bool IsInteractable { get; private set; } = true;

        public virtual void Interact(Mob _) {
            IsInteractable = false;
            // all these debug logs should be replaced by UI stuff in the future
            Debug.Log($"Begin trail: {TrialDescription} in {Duration}s");
            _timer = StartCoroutine(TrialTimer());
        }

        IEnumerator TrialTimer() {
            yield return new WaitForSeconds(Duration);
            // during the wait, interruption will result in the rest of the code not running
            Complete(IsSuccessByDefault);
        }

        /// <summary>
        /// Ends the trial early
        /// </summary>
        /// <param name="success">Whether the trial should be successful or not</param>
        public void EarlyComplete(bool success) {
            if (_timer == null) return; // timer == null => trial not in progress
            StopCoroutinesEarly();
            Complete(success);
        }

        protected virtual void Complete(bool success) {
            _timer = null;
            if (success) {
                OnTrialSuccess.Invoke();
                // all these debug logs should be replaced by UI stuff in the future
                Debug.Log("Success!");
                return;
            } 
            OnTrialFailure.Invoke();
            // all these debug logs should be replaced by UI stuff in the future
            Debug.Log("Fail!");
            IsInteractable = IsRedoable;
        }

        protected virtual void StopCoroutinesEarly() {
            StopCoroutine(_timer);
            _timer = null;
        }



    }
}