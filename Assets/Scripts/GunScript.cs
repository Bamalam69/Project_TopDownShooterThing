using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GunScript : NetworkBehaviour
{

#region vars

    public Rigidbody2D rb;
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

    [SyncVar] public float clipAmmoCount;
    public float clipSize;
    public float reloadTime;
    public bool notReloading;

    #endregion

    #region compulsory functions

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        
        boxCol = GetComponent<BoxCollider2D>();
        justDropped = false;

        if (gunType == GunTypes.AK || gunType == GunTypes.M4) {
            clipAmmoCount = clipSize = 30;
            reloadTime = 1.5f;
        } else if (gunType == GunTypes.Micro) {
            clipAmmoCount = clipSize = 25;
            reloadTime = 0.5f;
        } else if (gunType == GunTypes.Snipper) {
            clipAmmoCount = clipSize = 5;
            reloadTime = 2.5f;
        }
    }

    void Update() {
        //Check for a parent. If so, check if mouse is in screen. If so, get angle to point the gun toward.
        if (equipped) {
            if (!playersPlayerController.isLocalPlayer) {
                transform.right = playersPlayerController.transform.right;
                return;
            }

            //Calculate direction to aim at:
            Vector2 target = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));

            Vector2 myPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 direction = (target - myPos).normalized;

            angle = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        } else {
            //playerEquippedTo = null;
           if (justDropped && rb.velocity.magnitude < 0.75f) {
                CmdStopIgnoringCols(this.netId, playersPlayerController.netId);
               justDropped = false;
           }
        }
    }
    #endregion

#region custom Functions

    [Command]
    private void CmdStopIgnoringCols(NetworkInstanceId objCalling, NetworkInstanceId objToStopIgnoring) {
        RpcStopIgnoringCols(objCalling, objToStopIgnoring);
    }

    [ClientRpc]
    public void RpcStopIgnoringCols(NetworkInstanceId objCalling, NetworkInstanceId objToStopIgnoring) {
        BoxCollider2D objsCallingsBoxCol = ClientScene.FindLocalObject(objCalling).GetComponent<BoxCollider2D>();
        PolygonCollider2D colToStopIgnore = ClientScene.FindLocalObject(objToStopIgnoring).GetComponent<PolygonCollider2D>();

        Physics2D.IgnoreCollision(objsCallingsBoxCol, colToStopIgnore, false);
    }

    private void FixedUpdate() {
        if (equipped) {
            if (!playersPlayerController.isLocalPlayer) return;
            Rotate();
        }
    }

    private void Rotate() {
        rb.MoveRotation(angle.eulerAngles.z);
    }

#endregion
}