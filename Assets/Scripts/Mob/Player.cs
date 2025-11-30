using UnityEngine;

public class Player : Mob {
    /// <summary>
    /// Rotates player's movement
    /// </summary>
    /// <param name="rotation">rotation</param>
    public void RotateMovement(Quaternion rotation) {
        CastMovement<PlayerMovement>().Rotate(rotation);
    }
}
