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
        Physics2D.IgnoreCollision(this.GetComponent<BoxCollider2D>(), colToStopIgnoring, false);
        boxCol.enabled = true;
    }

#endregion
}