using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorExploder : MonoBehaviour {

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("bullet")) {
            Explodable exploder = GetComponent<Explodable>();
            exploder.explode();
            foreach(GameObject fragment in exploder.fragments) {
                Destroy(fragment, 3.0f);
            }
        }
    }
}
