using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BulletScript : NetworkBehaviour {

    //Initialized in inspector
    [SyncVar] public float damageAmount;

}
