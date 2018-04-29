using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using EZCameraShake;

public class PlayerController : NetworkBehaviour
{
    #region variables
    //Movement and player related things.
    private Rigidbody2D rb;
    private Vector3 movementVector;
    private float moveSpeed = 7.0f;
    private float angle;
    private RectTransform nameCanvas;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image reloadCircle;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] public float akAmmoCarrying;
    [SerializeField] public float m4AmmoCarrying;
    [SerializeField] public float microAmmoCarrying;
    [SerializeField] public float snipperAmmoCarrying;

    [SerializeField] private GameObject akPrefab;
    [SerializeField] private GameObject m4Prefab;
    [SerializeField] private GameObject microPrefab;

    private PolygonCollider2D polCol2D;

    //private GameObject weaponInstance;
    private GameObject weaponHolding = null;
    public GunScript weaponsGunScript = null;

    public bool holdingGun;

    //The camera that spawns with the player.
    public GameObject camInstance;
    public Camera[] cams;
    public CameraShaker camerashaker;

    private AudioListener audListeners;
    private NetworkIdentity networkIdentity;

    private Vector2 weaponPosition = new Vector2(0.04f, -0.166f);

    private float lastFired;
    private float akFireRate = 7.0f;
    private float m4FireRate = 7.0f;
    private float microFireRate = 10.0f;

    [SerializeField] private GameObject camManagerPrefab;

    [SerializeField] private bool editorDebug;

    [SerializeField] Collider2D debugCol;

    #endregion

    #region compulsory Functions

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        holdingGun = false;

        nameCanvas = transform.Find("Name Canvas") as RectTransform;

        nameCanvas.GetComponent<CanvasLockRot>().playerToFollow = this.transform;
        nameCanvas.SetParent(null);

        camInstance = Instantiate(camManagerPrefab);
        //get camera component
        cams = camInstance.GetComponentsInChildren<Camera>();

        audListeners = camInstance.GetComponentInChildren<AudioListener>();

        polCol2D = GetComponent<PolygonCollider2D>();

        networkIdentity = GetComponent<NetworkIdentity>();
    }

    //Must make sure this is here and that it is called because otherwise isLocalPlayer may not become true.
    public override void OnStartLocalPlayer() {
        Debug.Log("Spawned! " + netId);

        audListeners.enabled = true;

        foreach (Camera cam in cams) {
            cam.enabled = isLocalPlayer;
            camerashaker = cam.GetComponent<CameraShaker>();
        }

        camerashaker.enabled = true;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
            go.GetComponent<PlayerController>().nameCanvas.GetComponentInChildren<TextMeshProUGUI>().text = go.GetComponent<PlayerController>().netId.ToString();
        }

        camInstance.GetComponent<CameraController>().playerToFollow = this.transform;
        camInstance.transform.SetParent(null);

        ammoText.transform.parent.transform.gameObject.SetActive(true);
        ammoText.text = "";
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
            //CameraLerpToHouse(col.transform, 0.2f, 4.0f);
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

    #region update functions

    void Update() {
        if (isLocalPlayer) {

            //Get keyboard input
            float _xMovement = Input.GetAxis("Horizontal");
            float _yMovement = Input.GetAxis("Vertical");

            movementVector = new Vector3(_xMovement, _yMovement);

            //Get mouse rotation and calculate rotation
            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            if (rect.Contains(Input.mousePosition) && Application.isFocused) {
                Vector3 dir = Input.mousePosition - cams[0].WorldToScreenPoint(transform.position);
                angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }

            if (holdingGun) {
                if (Input.GetMouseButton(0) && weaponsGunScript.notReloading) {
                    if (weaponsGunScript.clipAmmoCount > 0) {
                        if ((weaponsGunScript.gunType == GunScript.GunTypes.AK) || (weaponsGunScript.gunType == GunScript.GunTypes.M4)) {
                            if (Time.time - lastFired > 1 / akFireRate) {
                                CmdShoot(this.networkIdentity, cams[0].ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y)));
                                lastFired = Time.time;
                                camerashaker.ShakeOnce(Random.Range(6.5f, 4.5f), 0.5f, 0.2f, 0.3f);
                                Debug.Log("Shaking " + weaponsGunScript.transform.name);
                            }
                        } else if (weaponHolding.transform.name.Contains("Micro")) {
                            if (Time.time - lastFired > 1 / microFireRate) {
                                CmdShoot(this.networkIdentity, cams[0].ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y)));
                                lastFired = Time.time;
                                camerashaker.ShakeOnce(Random.Range(3.5f, 2.5f), 0.5f, 0.2f, 0.3f);
                                Debug.Log("Shaking " + weaponsGunScript.transform.name);
                            }
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.R)) {
                    if (weaponsGunScript.gunType == GunScript.GunTypes.AK) {
                        if (akAmmoCarrying > 0) {
                            weaponsGunScript.notReloading = false;
                            StartCoroutine(LerpReloadCircle(weaponsGunScript.reloadTime));
                            Reload();
                            Debug.LogWarning("Reloading weapon!");
                        }
                    } else if (weaponsGunScript.gunType == GunScript.GunTypes.M4) {
                        if (m4AmmoCarrying > 0) {
                            weaponsGunScript.notReloading = false;
                            StartCoroutine(LerpReloadCircle(weaponsGunScript.reloadTime));
                            Reload();
                            Debug.LogWarning("Reloading weapon!");
                        }
                    } else if (weaponsGunScript.gunType == GunScript.GunTypes.Micro) {
                        if (microAmmoCarrying > 0) {
                            weaponsGunScript.notReloading = false;
                            StartCoroutine(LerpReloadCircle(weaponsGunScript.reloadTime));
                            Reload();
                            Debug.LogWarning("Reloading weapon!");
                        }
                    } else if (weaponsGunScript.gunType == GunScript.GunTypes.Snipper) {
                        if (snipperAmmoCarrying > 0) {
                            weaponsGunScript.notReloading = false;
                            StartCoroutine(LerpReloadCircle(weaponsGunScript.reloadTime));
                            Reload();
                            Debug.LogWarning("Reloading weapon!");
                        }
                    }
                }

                if (Input.GetMouseButtonDown(0)) {
                    if (weaponsGunScript.clipAmmoCount > 0) {
                        if (weaponHolding.transform.name.Contains("Snipper")) {
                            CmdShoot(this.networkIdentity, cams[0].ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y)));
                            if (weaponsGunScript.gunType == GunScript.GunTypes.Snipper) {
                                camerashaker.ShakeOnce(Random.Range(10.5f, 8.5f), 0.5f, 0.2f, 0.3f);
                                Debug.Log("Shaking " + weaponsGunScript.transform.name);
                            }
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.G)) {
                    CmdTempDrop(this.networkIdentity, cams[0].ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y)));
                }


                #region Displaying ammo
                if (weaponsGunScript.gunType == GunScript.GunTypes.AK) {
                    ammoText.text = "Ammo: " + weaponsGunScript.clipAmmoCount + " Rest: " + akAmmoCarrying;
                } else if (weaponsGunScript.gunType == GunScript.GunTypes.M4) {
                    ammoText.text = "Ammo: " + weaponsGunScript.clipAmmoCount + " Rest: " + m4AmmoCarrying;
                } else if (weaponsGunScript.gunType == GunScript.GunTypes.Micro) {
                    ammoText.text = "Ammo: " + weaponsGunScript.clipAmmoCount + " Rest: " + microAmmoCarrying;
                } else if (weaponsGunScript.gunType == GunScript.GunTypes.Snipper) {
                    ammoText.text = "Ammo: " + weaponsGunScript.clipAmmoCount + " Rest: " + snipperAmmoCarrying;
                }
                #endregion
            } else {
                ammoText.text = "";
            }
        }

        //Debugging stuff
        if (editorDebug) {
            Debug.Log(Physics2D.GetIgnoreCollision(this.GetComponent<PolygonCollider2D>(), debugCol));
        }
    }

    void FixedUpdate() {
        if (!isLocalPlayer) return;
        Move();
        Rotate();
    }

    #endregion

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

        if (weaponScript.gunType == GunScript.GunTypes.AK) {
            weaponRenderer.sprite = weaponScript.akSprites[1];
        } else if (weaponScript.gunType == GunScript.GunTypes.M4) {
            weaponRenderer.sprite = weaponScript.m4Sprites[1];
        } else if (weaponScript.gunType == GunScript.GunTypes.Micro) {
            weaponRenderer.sprite = weaponScript.microSprites[1];
        } else if (weaponScript.gunType == GunScript.GunTypes.Snipper) {
            weaponRenderer.sprite = weaponScript.snipperSprites[1];
        }

        weaponRenderer.sortingOrder = 0;

        Rigidbody2D weaponsRigidbody = weaponTouched.GetComponent<Rigidbody2D>();

        weaponTouched.transform.position = playerThatCalled.transform.TransformPoint(weaponPosition);
        weaponTouched.transform.rotation = playerThatCalled.transform.rotation;

        HingeJoint2D joint = playerThatCalled.GetComponent<HingeJoint2D>();
        if (joint == null) joint = playerThatCalled.AddComponent<HingeJoint2D>();
        joint.enabled = true;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = weaponsRigidbody;
        joint.anchor = new Vector2(0.0f, -0.2f);
        joint.connectedAnchor = Vector2.zero;

        weaponScript.playersPlayerController = playerThatCalled.GetComponent<PlayerController>();

        weaponScript.cam = weaponScript.playersPlayerController.cams[0];

        Physics2D.IgnoreCollision(weaponTouched.GetComponent<BoxCollider2D>(), playerThatCalled.GetComponent<PolygonCollider2D>(), true);
        weaponScript.boxCol.enabled = true;
        weaponScript.playersPlayerController.holdingGun = true;
        weaponScript.equipped = true;

        playerThatCalled.GetComponent<PlayerController>().weaponHolding = weaponTouched;
        playerThatCalled.GetComponent<PlayerController>().weaponsGunScript = weaponScript;

        weaponScript.notReloading = true;
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
        PlayerController playersController = playerThatCalled.GetComponent<PlayerController>();

        Debug.Log(playersController);

        Vector2 myPos = new Vector2(playersController.weaponHolding.transform.position.x, playersController.weaponHolding.transform.position.y);
        Vector2 direction = (target - myPos).normalized;

        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        GameObject projectile = Instantiate(bulletPrefab, myPos, rotation);

        projectile.GetComponent<Rigidbody2D>().velocity = direction * 1500f * Time.deltaTime;

        Physics2D.IgnoreCollision(projectile.GetComponent<BoxCollider2D>(), playerThatCalled.GetComponent<PolygonCollider2D>(), true);

        GunScript weaponsScript = playersController.weaponHolding.GetComponent<GunScript>();

        weaponsScript.clipAmmoCount -= 1;

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
        } else if (playersWeapon.transform.name.Contains("Snipper")) {
            playersWeapon.GetComponent<SpriteRenderer>().sprite = weaponsScript.snipperSprites[0];
        }

        playersController.holdingGun = false;

        weaponsScript.colToStopIgnoring = playersController.polCol2D;
        weaponsScript.justDropped = true;

        //Calculate direction
        Vector2 playerPos = new Vector2(playersController.weaponHolding.transform.position.x, playersController.weaponHolding.transform.position.y);
        Vector2 direction = (target - playerPos).normalized;

        HingeJoint2D joint = playerCallingObj.GetComponent<HingeJoint2D>();
        Destroy(joint);

        Physics2D.IgnoreCollision(playerCallingObj.GetComponent<PolygonCollider2D>(), playersWeapon.GetComponent<BoxCollider2D>(), true);

        Rigidbody2D weaponsRb = playersWeapon.GetComponent<Rigidbody2D>();
        weaponsRb.isKinematic = false;
        weaponsRb.velocity += direction * 15.0f;
        weaponsRb.angularVelocity = (Random.Range(0, 2) * 2 - 1) * 575.0f;

        playersController.weaponHolding = null;
        playersController.weaponsGunScript = null;
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

        StartCoroutine(LerpTransparency(roofRenderer, time, amount));
    }

    private IEnumerator LerpTransparency(SpriteRenderer spr, float time, float amount) {

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

    void Reload() {
        float ammoToAdd = weaponsGunScript.clipSize - weaponsGunScript.clipAmmoCount;

        if (weaponsGunScript.gunType == GunScript.GunTypes.AK) {
            if (akAmmoCarrying < ammoToAdd) {
                weaponsGunScript.clipAmmoCount += akAmmoCarrying;
                akAmmoCarrying -= akAmmoCarrying;
            } else {
                weaponsGunScript.clipAmmoCount += ammoToAdd;
                akAmmoCarrying -= ammoToAdd;
            }
        } else if (weaponsGunScript.gunType == GunScript.GunTypes.M4) {
            if (m4AmmoCarrying < ammoToAdd) {
                weaponsGunScript.clipAmmoCount += m4AmmoCarrying;
                m4AmmoCarrying -= m4AmmoCarrying;
            } else {
                weaponsGunScript.clipAmmoCount += ammoToAdd;
                m4AmmoCarrying -= ammoToAdd;
            }
        } else if (weaponsGunScript.gunType == GunScript.GunTypes.Micro) {
            if (microAmmoCarrying < ammoToAdd) {
                weaponsGunScript.clipAmmoCount += microAmmoCarrying;
                microAmmoCarrying -= microAmmoCarrying;
            } else {
                weaponsGunScript.clipAmmoCount += ammoToAdd;
                microAmmoCarrying -= ammoToAdd;
            }
        } else if (weaponsGunScript.gunType == GunScript.GunTypes.Snipper) {
            if (snipperAmmoCarrying < ammoToAdd) {
                weaponsGunScript.clipAmmoCount += snipperAmmoCarrying;
                snipperAmmoCarrying -= snipperAmmoCarrying;
            } else {
                weaponsGunScript.clipAmmoCount += ammoToAdd;
                snipperAmmoCarrying -= ammoToAdd;
            }
        }
    }

    private IEnumerator LerpReloadCircle(float time) {

        float elapsedTime = 0;

        while (elapsedTime < time) {
            if (reloadCircle.fillAmount != 1.0f) {
                reloadCircle.fillAmount = Mathf.Lerp(reloadCircle.fillAmount, 1.0f, (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            } else {
                weaponsGunScript.notReloading = true;
                reloadCircle.fillAmount = 0.0f;

                Reload();
                break;
            }
        }


    }

    #endregion
}