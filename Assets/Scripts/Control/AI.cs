using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class AI : MonoBehaviour {
    /// <summary>
    /// Second per target find
    /// </summary>
    [SerializeField] protected float findInterval;
    /// <summary>
    /// Radius of target find
    /// </summary>
    [SerializeField] protected float searchRadius;
    
    /// <summary>
    /// Self-documenting
    /// </summary>
    protected Mob Owner { get; private set; } = null;
    Mob _target;
    /// <summary>
    /// The mob the AI should act on
    /// </summary>
    protected virtual Mob Target {
        get {
            return _target;
        }
        set {
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
    protected abstract bool Predicate(Mob m);

    private void Awake() {
        Owner = GetComponent<Mob>();
        if (!Owner) throw new NullReferenceException($"{gameObject} does not have an attached Mob component.");


        TargetFinder = StartCoroutine(FindTarget());
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
    /// <summary>
    /// Sets the mob's move direction
    /// </summary>
    protected Vector3 MoveDirection { set {
            Owner.MoveDirection = value;
        } 
    }
    protected void Jump() {
        Owner.Jump();
    }

    /// <summary>
    /// Used when attack 'key' should be 'clicked'
    /// </summary>
    protected void ClickAttack() {
        Owner.AttackClick();
    }
    /// <summary>
    /// Used when attack 'key' should be 'lifted'
    /// </summary>
    protected void LiftAttack() {
        Owner.AttackLift();
    }

    /// <summary>
    /// Used when block 'key' should be 'clicked'
    /// </summary>
    protected void ClickBlock() {
        Owner.BlockClick();
    }
    /// <summary>
    /// Used when block 'key' should be 'lifted'
    /// </summary>
    protected void LiftBlock() {
        Owner.BlockLift();
    }

}
