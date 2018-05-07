using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowExplode : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D col) {
        if (col.transform.CompareTag("bullet")) {
            if (!col.gameObject.GetComponent<BulletScript>().hasCollidedWithWall) { 
                Explode();
            }
        }
    }

    void Explode() {
        //Debug.Log("Breaking object! objNetId == " + objNetId);

        Explodable explodeScript = GetComponent<Explodable>();

        explodeScript.explode();
        //Physics2D.IgnoreLayerCollision(10, 14);
        foreach (GameObject fragment in explodeScript.fragments) {
            //Rigidbody2D fragRb = fragment.GetComponent<Rigidbody2D>();
            //fragRb.velocity += bulletObj.GetComponent<Rigidbody2D>().GetPointVelocity(bulletObj.TransformPoint(point)) * Random.Range(0.1f, 1.0f);
            //fragRb.isKinematic = false;
            //fragRb.drag = 2.0f;
            //fragRb.angularDrag = 2.0f;

            Destroy(fragment, 5.0f);
        }
    }
}
