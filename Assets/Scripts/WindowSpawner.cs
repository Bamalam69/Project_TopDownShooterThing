using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WindowSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] windows;
    private Rigidbody2D rb;
    private Vector3 origin;

    public void SpawnWindows() { 
    GameObject windowInstance = (GameObject)Instantiate(windows[0], 
            new Vector3(3.641f, 2.644f, 0.0f), 
            Quaternion.Euler(new Vector3(0.0f, 0.0f, 90.0f)));

        Debug.Log("Window spawned!");

        windowInstance.transform.SetParent(this.transform);

        NetworkServer.Spawn(windowInstance);
    }

}
