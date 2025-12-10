using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Movement script derived from MobMovement to support movement according to camera rotation
/// </summary>
public class PlayerMovement : MobMovement {
    public void Rotate(Quaternion delta) {
        _movement = delta * _movement;
    }
}
