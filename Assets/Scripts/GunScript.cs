using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GunScript : NetworkBehaviour
{
#region vars
    private float angle;
    private Rigidbody2D rb;
    private Transform playerParent;
    public Camera cam;

    public Sprite[] akSprites;
    public Sprite[] m4Sprites;
    public Sprite[] microSprites;

    public Transform playerEquippedTo;
    public PlayerController playersPlayerController;

    private BoxCollider2D boxCol;

    public bool equipped;

    public enum gunTypes
    {
        AK, M4, Micro, Snipper
    }

    public gunTypes gunType;

    private Quaternion rotation;

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
            if (transform.parent == null) Debug.LogError("Equipped with no parent!");

            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            if (rect.Contains(Input.mousePosition) && cam != null) {
                Vector2 target = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
                Vector2 myPos = new Vector2(transform.position.x, transform.position.y);
                Vector2 direction = (target - myPos).normalized;

                rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            }
        } else {
            playerEquippedTo = null;
           if (justDropped && rb.velocity.magnitude < 2.0f) {
               StopIgnoringCols();
               justDropped = false;
           }
        }
    }

    void FixedUpdate() {
        if (equipped) {
            if (playersPlayerController.isLocalPlayer) {
                RotateGun();
            }
        }
    }
    #endregion

#region custom Functions
    void RotateGun() {
        rb.MoveRotation(rotation.eulerAngles.z);
    }

    public void StopIgnoringCols() {
        Physics2D.IgnoreCollision(this.GetComponent<BoxCollider2D>(), colToStopIgnoring, false);
    }
#endregion
}