using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AmmoScript : NetworkBehaviour
{
#pragma warning disable 0414
    [SyncVar] private float ammoAmountToGive;
#pragma warning restore 0414
    [SyncVar] public float ammoAmount;
    [SyncVar] [SerializeField] private GunScript.GunTypes ammoType;

    [SerializeField] private Sprite[] ammoSprites;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake() {
        if (ammoType == GunScript.GunTypes.AK) {
            spriteRenderer.sprite = ammoSprites[1];
            ammoAmountToGive = 30;
        } else if (ammoType == GunScript.GunTypes.M4) {
            spriteRenderer.sprite = ammoSprites[0];
            ammoAmountToGive = 30;
        } else if (ammoType == GunScript.GunTypes.Micro) {
            spriteRenderer.sprite = ammoSprites[2];
            ammoAmountToGive = 15;
        }
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.transform.tag.Contains("Player")) {
            PlayerController colsPlayerController = col.gameObject.GetComponent<PlayerController>();
            if (colsPlayerController != null) {
                CmdGiveAmmoTo(col.gameObject.GetComponent<PlayerController>().netId, ammoAmount, this.netId);
            }
        }
    }

    [Command]
    void CmdGiveAmmoTo(NetworkInstanceId applyAmmoTo, float amountToGive, NetworkInstanceId ammoToDestroy) {
        RpcGiveAmmoTo(applyAmmoTo, amountToGive, ammoToDestroy);
    }

    [ClientRpc]
    void RpcGiveAmmoTo(NetworkInstanceId applyAmmoTo, float amountToGive, NetworkInstanceId ammoToDestroy) {
        GameObject ObjToApplyAmmoTo = ClientScene.FindLocalObject(applyAmmoTo).gameObject;
        PlayerController objsPlayerController = ObjToApplyAmmoTo.GetComponent<PlayerController>();

        GameObject ammoGiver = ClientScene.FindLocalObject(ammoToDestroy);
        AmmoScript ammosScript = ammoGiver.GetComponent<AmmoScript>();

        //Get Player Controller and apply apply to it's weapon script
        if (ammosScript.ammoType == GunScript.GunTypes.AK) {
            objsPlayerController.akAmmoCarrying += amountToGive;
        } else if (ammosScript.ammoType == GunScript.GunTypes.M4) {
            objsPlayerController.m4AmmoCarrying += amountToGive;
        } else if (ammosScript.ammoType == GunScript.GunTypes.Micro) {
            objsPlayerController.microAmmoCarrying += amountToGive;
        } else if (ammosScript.ammoType == GunScript.GunTypes.Snipper) {
            objsPlayerController.snipperAmmoCarrying += amountToGive;
        }

        Destroy(ammoGiver);
    }

}
