using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class AI : MonoBehaviour {
    [SerializeField] protected float findInterval;
    [SerializeField] protected float searchRadius;
    
    /// <summary>
    /// Self-documenting
    /// </summary>
    protected Mob Owner { get; private set; } = null;
    Mob _target;
    /// <summary>
    /// The mob the AI should act on
    /// </summary>
    protected Mob Target {
        get {
            return _target;
        }
        private set {
            if (value == null) {
                TargetFinder = StartCoroutine(FindTarget());
            } else {
                StopCoroutine(TargetFinder);
            }
            _target = value;
        }
    }

    /// <summary>
    /// Stores the ongoing coroutine that finds a target
    /// </summary>
    protected Coroutine TargetFinder { get; set; }

    /// <summary>
    /// Criteria of the mob to be treated as a target
    /// </summary>
    protected abstract Func<Mob, bool> Predicate { get; }

    private void Awake() {
        Owner = GetComponent<Mob>();
        if (!Owner) throw new NullReferenceException($"{gameObject} does not have an attached Mob component.");
    }
    
    IEnumerator FindTarget() {
        while (true) {
            Collider[] candidates = Physics.OverlapSphere(transform.position, searchRadius);
            foreach (Collider x in candidates) {
                Mob m = x.GetComponent<Mob>();
                if (m != null && Predicate(m))
                    Target = m;
            }
            yield return new WaitForSeconds(findInterval);
        }
    }

    // Controls
    
}
