using System.Collections;
using UnityEngine;
namespace Combat {
    public abstract class Effect : MonoBehaviour {
        protected Mob Owner { get; private set; }
        public Coroutine Countdown { get; private set; }
        float duration;

        protected virtual void Awake() {
            Owner = GetComponent<Mob>();
        }

        /// <summary>
        /// Jumpstarts the effect
        /// </summary>
        /// <param name="duration">Self-documenting</param>
        protected void Apply(float duration) {
            this.duration = duration;
            Begin();
            Countdown = StartCoroutine(StartCountdown());
        }

        /// <summary>
        /// Removes this effect before cooldown ends
        /// </summary>
        public void Cleanse() {
            StopCoroutine(Countdown);
            End();
            Owner.RemoveExpiredEffect(this);
        }

        /// <summary>
        /// Self-documenting, but note that the responsibility of destroying this script lies on the hand of Mob
        /// </summary>
        /// <returns></returns>
        IEnumerator StartCountdown() {
            yield return new WaitForSeconds(duration);
            End();
            Owner.RemoveExpiredEffect(this);
        }

        protected abstract void Begin();
        protected abstract void End();
    }
}