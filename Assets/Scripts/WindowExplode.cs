using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WindowExplode : NetworkBehaviour
{
	void OnCollisionEnter2D(Collision2D col) {
        if (col.transform.CompareTag("bullet")) {
            CmdExplode();
        }
    }

    [Command]
    void CmdExplode() {
        RpcExplode();
    }

    [ClientRpc]
    void RpcExplode() {
        Debug.Log("Breaking object!");

        GetComponent<Rigidbody2D>().isKinematic = false;

        Explodable explodeScript = GetComponent<Explodable>();
        explodeScript.explode();
    }
}
