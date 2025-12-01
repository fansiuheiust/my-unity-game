using UnityEngine;

public class HostileMelee : MobAI {
    [SerializeField] protected float AttackRadius = 2;
    protected override bool Predicate(Mob m) => m is Player;
    
    void Update() {
        if (Target == null) return;
        FollowTarget();
    }

    /// <summary>
    /// Moves towards target every update
    /// </summary>
    void FollowTarget() {
        Vector3 delta = Target.transform.position - transform.position;
        if (delta.magnitude < 2) delta = Vector3.zero;
        MoveDirection = Vector3.Scale(delta, new Vector3(1, 0, 1));
    }
}
