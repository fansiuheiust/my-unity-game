using Combat;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace BuildingBlocks {
    /// <summary>
    /// An abstract class that allows blocks to do something when mobs enter it
    /// </summary>
    public abstract class Enterable: MonoBehaviour {
        /// <summary>
        /// Triggers when a mob enters the block
        /// </summary>
        public UnityEvent<Mob> OnMobEnter = new();
        /// <summary>
        /// Triggers when a mob exits the block
        /// </summary>
        public UnityEvent<Mob> OnMobExit = new();
        /// <summary>
        /// Triggers when a mob triggers effect in the block
        /// </summary>
        public UnityEvent<Mob> OnEffectTrigger = new();

        /// <summary>
        /// Not for coding behaviour of the enterable
        /// </summary>
        /// <param name="m"></param>
        protected virtual void MobEnteredInternal(Mob m) {
            OnMobEnter.Invoke(m);
            OnMobEntered(m);
            TriggerEffect(m);
        }

        /// <summary>
        /// Called when a mob entered
        /// </summary>
        /// <param name="m"></param>
        protected abstract void OnMobEntered(Mob m);

        /// <summary>
        /// Not for coding behaviour of the enterable
        /// </summary>
        /// <param name="m"></param>
        protected virtual void MobExitedInternal(Mob m) {
            OnMobExit.Invoke(m);
            OnMobExited(m);
        }
        /// <summary>
        /// Called when a mob exits
        /// </summary>
        /// <param name="m"></param>
        protected abstract void OnMobExited(Mob m);

        protected void TriggerEffectInternal(Mob m) {
            OnEffectTrigger.Invoke(m);
            TriggerEffect(m);
        }
        /// <summary>
        /// Triggers the effect on the said mob
        /// </summary>
        /// <param name="m">The target mob</param>
        protected abstract void TriggerEffect(Mob m);
    }

    /// <summary>
    /// Enterables that triggers its effect continuously (i.e. once per interval)
    /// </summary>
    public abstract class EnterableContinuous : Enterable {
        [field: SerializeField, Tooltip("Time between trigger"), Min(0.01f)] public float Interval { get; private set; }

        /// <summary>
        /// Mobs that are currently triggering effect from the Enterable
        /// </summary>
        LinkedList<Mob> _affecteds = new();

        protected override void MobEnteredInternal(Mob m) {
            base.MobEnteredInternal(m);
            _affecteds.AddLast(m);
            StartCoroutine(EffectTriggerer(m));
        }

        protected override void MobExitedInternal(Mob m) {
            base.MobExitedInternal(m);
            _affecteds.Remove(m);
        }

        IEnumerator EffectTriggerer(Mob m) {
            do {
                // TODO: implement a better mob dead detector, and use it
                if (m.IsDestroyed()) {
                    _affecteds.Remove(m);
                    yield break;
                }
                TriggerEffectInternal(m);
                yield return new WaitForSeconds(Interval);
            } while (_affecteds.Contains(m));
            yield break;
        }
    }
}