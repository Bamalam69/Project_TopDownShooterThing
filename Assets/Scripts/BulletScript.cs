using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BulletScript : NetworkBehaviour {

    //Initialized in inspector
    [SyncVar] public float damageAmount;

    [SyncVar] public bool hasCollided;

    private void Start() {
        hasCollided = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!collision.transform.CompareTag("Window")) {
            hasCollided = true;
        }
    }

}
