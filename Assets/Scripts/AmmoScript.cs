using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AmmoScript : NetworkBehaviour {

    [SyncVar] public float ammoAmount;
    [SerializeField] private GunScript.GunTypes ammoType;

    private void OnCollisionEnter2D(Collision2D col) {
        if (col.transform.tag.Contains("Player")) {
            if (col.gameObject.GetComponent<PlayerController>().holdingGun) {
                if (col.gameObject.GetComponent<PlayerController>().weaponsGunScript.gunType == this.ammoType) {
                    CmdGiveAmmoTo(col.gameObject.GetComponent<PlayerController>().netId, ammoAmount, this.netId);
                }
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

        //Get Player Controller and apply apply to it's weapon script
        ObjToApplyAmmoTo.GetComponent<PlayerController>().weaponsGunScript.otherAmmoCount += amountToGive;
        Destroy(ClientScene.FindLocalObject(ammoToDestroy));
    }

}
