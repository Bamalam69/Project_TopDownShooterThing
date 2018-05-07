using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BulletScript : NetworkBehaviour {

    //Initialized in inspector
    [SyncVar] public float damageAmount;

    public bool hasCollidedWithWall;

    private void Start() {
        hasCollidedWithWall = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!collision.transform.CompareTag("Window")) {
            hasCollidedWithWall = true;
        }
    }

}
