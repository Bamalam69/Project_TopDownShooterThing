using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    #region variables
    //Movement and player related things.
    private Rigidbody2D rb;
    private Vector3 movementVector;
    private float moveSpeed = 7.0f;
    private float angle;
    private RectTransform nameCanvas;

    [SerializeField] private GameObject bulletPrefab;

    [SerializeField] private GameObject akPrefab;
    [SerializeField] private GameObject m4Prefab;
    [SerializeField] private GameObject microPrefab;

    private PolygonCollider2D polCol2D;

    //private GameObject weaponInstance;
    private GameObject weaponHolding = null;

    public bool holdingGun;

    //The camera that spawns with the player.
    public GameObject camInstance;
    public Camera[] cams;

    private AudioListener audListeners;
    private NetworkIdentity networkIdentity;

    private Vector2 weaponPosition = new Vector2(0.04f, -0.166f);

    private float lastFired;
    private float akFireRate = 7.0f;
    private float m4FireRate = 7.0f;
    private float microFireRate = 10.0f;

    [SerializeField] private GameObject camManagerPrefab;

    #endregion

    #region compulsory Functions

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        holdingGun = false;

        nameCanvas = transform.Find("Name Canvas") as RectTransform;

        nameCanvas.GetComponent<CanvasLockRot>().playerToFollow = this.transform;
        nameCanvas.SetParent(null);

        polCol2D = GetComponent<PolygonCollider2D>();

        camInstance = Instantiate(camManagerPrefab);

        //get camera component
        cams = camInstance.GetComponentsInChildren<Camera>();

        audListeners = camInstance.GetComponentInChildren<AudioListener>();

        networkIdentity = GetComponent<NetworkIdentity>();
    }

    //Must make sure this is here and that it is called because otherwise isLocalPlayer may not become true.
    public override void OnStartLocalPlayer() {
        Debug.Log("Spawned!");

        audListeners.enabled = true;

        foreach (Camera cam in cams) {
            cam.enabled = isLocalPlayer;
        }

        nameCanvas.GetComponentInChildren<TextMeshProUGUI>().text = this.netId.ToString();

        camInstance.GetComponent<CameraController>().playerToFollow = this.transform;
        camInstance.transform.SetParent(null);
    }

    #region Collision events
    void OnCollisionEnter2D(Collision2D col) {
        if (!isLocalPlayer) return;
        if (col.gameObject.transform.CompareTag("Gun")) {
            if (col.transform.CompareTag("Gun") && !holdingGun && !col.gameObject.GetComponent<GunScript>().equipped && weaponHolding == null) {
                CmdEquip(col.gameObject.GetComponent<NetworkIdentity>(), this.networkIdentity);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (!isLocalPlayer) return;
        if (col.transform.CompareTag("TransCol")) {
            LockOnRoof(col, 0.2f, 0.2f);
            CameraLerpToHouse(col.transform, 0.2f, 4.0f);
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (!isLocalPlayer) return;
        if (col.transform.CompareTag("TransCol")) {
            LockOnRoof(col, 0.2f, 1.0f);
            CameraLerpToHouse(this.transform, 0.7f, 6.0f);
        }
    }

    #endregion

    void Update() {
        if (isLocalPlayer) {

            //Get keyboard input
            float _xMovement = Input.GetAxis("Horizontal");
            float _yMovement = Input.GetAxis("Vertical");

            movementVector = new Vector3(_xMovement, _yMovement);

            //Get mouse rotation and calculate rotation
            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            if (rect.Contains(Input.mousePosition)) {
                Vector3 dir = Input.mousePosition - cams[0].WorldToScreenPoint(transform.position);
                angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }

            if (holdingGun) {
                if (Input.GetMouseButton(0)) {
                    if ((weaponHolding.transform.name.Contains("AK")) || (weaponHolding.transform.name.Contains("M4"))) {
                        if (Time.time - lastFired > 1 / akFireRate) {
                            CmdShoot(this.networkIdentity, cams[0].ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y)));
                            lastFired = Time.time;
                        }
                    } else if (weaponHolding.transform.name.Contains("Micro")) {
                        if (Time.time - lastFired > 1 / microFireRate) {
                            CmdShoot(this.networkIdentity, cams[0].ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y)));
                            lastFired = Time.time;
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.G)) {
                    CmdTempDrop(this.GetComponent<NetworkIdentity>(), cams[0].ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y)));
                }
            }
        }
    }

    void FixedUpdate() {
        if (!isLocalPlayer) return;
        Move();
        Rotate();
    }

    #endregion

    #region custom Functions

    #region Functions called for movement.
    void Move() {
        rb.MovePosition(transform.position + movementVector * moveSpeed * Time.deltaTime);
    }

    void Rotate() {
        rb.MoveRotation(angle);
    }
    #endregion

    #region network Functions

    #region equip functions.
    [Command]
    void CmdEquip(NetworkIdentity gameObjectID, NetworkIdentity playerId) {
        RpcEquip(gameObjectID, playerId);
    }

    [ClientRpc]
    void RpcEquip(NetworkIdentity gunToEquip, NetworkIdentity playerId) {
        GameObject playerThatCalled = ClientScene.FindLocalObject(playerId.netId);
        GameObject weaponTouched = ClientScene.FindLocalObject(gunToEquip.netId);

        string objectName = weaponTouched.transform.name;

        SpriteRenderer weaponRenderer = weaponTouched.GetComponent<SpriteRenderer>();
        GunScript weaponScript = weaponTouched.GetComponent<GunScript>();

        if (weaponScript.gunType == GunScript.gunTypes.AK) {
            weaponRenderer.sprite = weaponScript.akSprites[1];
        } else if (weaponScript.gunType == GunScript.gunTypes.M4) {
            weaponRenderer.sprite = weaponScript.m4Sprites[1];
        } else if (weaponScript.gunType == GunScript.gunTypes.Micro) {
            weaponRenderer.sprite = weaponScript.microSprites[1];
        }

        weaponRenderer.sortingOrder = 0;

        Rigidbody2D weaponsRigidbody = weaponTouched.GetComponent<Rigidbody2D>();
        //weaponsRigidbody.isKinematic = true;
        //weaponsRigidbody.velocity = Vector2.zero;
        //weaponsRigidbody.angularVelocity = 0.0f;

        weaponTouched.transform.position = playerThatCalled.transform.TransformPoint(weaponPosition);
        weaponTouched.transform.rotation = playerThatCalled.transform.rotation;

        FixedJoint2D joint = playerThatCalled.GetComponent<FixedJoint2D>();
        if (joint == null) joint = playerThatCalled.AddComponent<FixedJoint2D>();
        joint.enabled = true;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = weaponsRigidbody;
        joint.anchor = weaponTouched.transform.position;
        joint.connectedAnchor = playerThatCalled.transform.position;

        weaponScript.playersPlayerController = playerThatCalled.GetComponent<PlayerController>();

        weaponScript.cam = weaponScript.playersPlayerController.cams[0];

        Physics2D.IgnoreCollision(weaponTouched.GetComponent<BoxCollider2D>(), this.GetComponent<PolygonCollider2D>(), true);
        weaponScript.boxCol.enabled = false;
        weaponScript.playersPlayerController.holdingGun = true;
        weaponScript.equipped = true;
        playerThatCalled.GetComponent<PlayerController>().weaponHolding = weaponTouched;
    }
    #endregion

    #region shooting functions

    [Command]
    void CmdShoot(NetworkIdentity playerCallingId, Vector2 target) {
        RpcShoot(playerCallingId, target);
    }

    [ClientRpc]
    void RpcShoot(NetworkIdentity playerCallingId, Vector2 target) {
        GameObject playerThatCalled = ClientScene.FindLocalObject(playerCallingId.netId);

        Vector2 myPos = new Vector2(playerThatCalled.GetComponent<PlayerController>().weaponHolding.transform.position.x, playerThatCalled.GetComponent<PlayerController>().weaponHolding.transform.position.y);
        Vector2 direction = (target - myPos).normalized;

        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        GameObject projectile = Instantiate(bulletPrefab, myPos, rotation);

        projectile.GetComponent<Rigidbody2D>().velocity = direction * 1500f * Time.deltaTime;

        Physics2D.IgnoreCollision(projectile.GetComponent<BoxCollider2D>(), playerThatCalled.GetComponent<PolygonCollider2D>(), true);

        Destroy(projectile, 2f);
    }

    #endregion

    #region Dropping functions

    [Command]
    void CmdTempDrop(NetworkIdentity gunObject, Vector2 target) {
        RpcTempDrop(gunObject, target);
    }

    [ClientRpc]
    void RpcTempDrop(NetworkIdentity playerCalling, Vector2 target) {
        GameObject playerCallingObj = ClientScene.FindLocalObject(playerCalling.netId);

        //Get the playerCalling's weaponHolding variable.
        PlayerController playersController = playerCallingObj.GetComponent<PlayerController>();
        GameObject playersWeapon = playersController.weaponHolding;

        GunScript weaponsScript = playersWeapon.GetComponent<GunScript>();
        weaponsScript.equipped = false;

        if (playersWeapon.transform.name.Contains("AK")) {
            playersWeapon.GetComponent<SpriteRenderer>().sprite = weaponsScript.akSprites[0];
        } else if (playersWeapon.transform.name.Contains("M4")) {
            playersWeapon.GetComponent<SpriteRenderer>().sprite = weaponsScript.m4Sprites[0];
        } else if (playersWeapon.transform.name.Contains("Micro")) {
            playersWeapon.GetComponent<SpriteRenderer>().sprite = weaponsScript.microSprites[0];
        }

        playersController.holdingGun = false;

        weaponsScript.colToStopIgnoring = playersController.polCol2D;
        weaponsScript.justDropped = true;

        //Calculate direction
        Vector2 myPos = new Vector2(playersController.weaponHolding.transform.position.x, playersController.weaponHolding.transform.position.y);
        Vector2 direction = (target - myPos).normalized;

        FixedJoint2D joint = playerCallingObj.GetComponent<FixedJoint2D>();
        Destroy(joint);

        Rigidbody2D weaponsRb = playersWeapon.GetComponent<Rigidbody2D>();
        weaponsRb.isKinematic = false;
        weaponsRb.AddForce(direction * 15.0f, ForceMode2D.Impulse);
        weaponsRb.AddTorque((Random.Range(0, 2) * 2 - 1) * 2.0f, ForceMode2D.Impulse);

        playersController.weaponHolding = null;
    }

    #endregion

    #endregion

    #region HousEntering lerping
    void LockOnRoof(Collider2D col, float time, float amount) {
        //Get the parent gameobject attached to the col.
        //Get the sprite renderer that is responsible for rendering the roof.
        //Apply transparency.

        GameObject rootObject = col.transform.parent.gameObject;
        SpriteRenderer[] spriteRenderers = rootObject.GetComponentsInChildren<SpriteRenderer>();

        SpriteRenderer roofRenderer = null;

        foreach (SpriteRenderer spr in spriteRenderers) {
            if (spr.gameObject.transform.CompareTag("Roof")) {
                roofRenderer = spr;
                break;
            }
        }

        StartCoroutine(lerpTransparency(roofRenderer, time, amount));
    }

    private IEnumerator lerpTransparency(SpriteRenderer spr, float time, float amount) {

        if (spr == null) {
            Debug.Log("No spr!");
            yield return null;
        }

        float elapsedTime = 0;

        while (elapsedTime < time) {
            spr.color = Color.Lerp(spr.color, new Color(1.0f, 1.0f, 1.0f, amount), (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    void CameraLerpToHouse(Transform posToLerpTo, float time, float size) {
        CameraController camsController = camInstance.GetComponent<CameraController>();

        camsController.playerToFollow = posToLerpTo;
        camsController.timeToLerp = time;
        camsController.size = size;
    }
    #endregion

    #endregion
}