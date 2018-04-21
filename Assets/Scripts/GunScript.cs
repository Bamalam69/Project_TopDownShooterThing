using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GunScript : NetworkBehaviour
{

#region vars

    private Rigidbody2D rb;
    private Transform playerParent;
    public Camera cam;

    public Sprite[] akSprites;
    public Sprite[] m4Sprites;
    public Sprite[] microSprites;
    public Sprite[] snipperSprites;

    //public Transform playerEquippedTo;
    public PlayerController playersPlayerController;

    public BoxCollider2D boxCol;

    public bool equipped;

    public enum GunTypes
    {
        AK, M4, Micro, Snipper
    }

    public GunTypes gunType;

    public bool justDropped;
    public PolygonCollider2D colToStopIgnoring;

    private Quaternion angle;

#endregion

    #region compulsory functions

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        
        boxCol = GetComponent<BoxCollider2D>();
        justDropped = false;
    }

    void Update() {
        //Check for a parent. If so, check if mouse is in screen. If so, get angle to point the gun toward.
        if (equipped) {
            //Calculate direction to aim at:
            Vector2 target = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));

            Vector2 myPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 direction = (target - myPos).normalized;

            angle = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        } else {
            //playerEquippedTo = null;
           if (justDropped && rb.velocity.magnitude < 1.5f) {
               StopIgnoringCols();
               justDropped = false;
           }
        }
    }
    #endregion

#region custom Functions

    public void StopIgnoringCols() {
        Physics2D.IgnoreCollision(boxCol, colToStopIgnoring, false);
    }

    private void Rotate() {
        rb.MoveRotation(angle.eulerAngles.z);
    }

#endregion
}