using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasLockRot : MonoBehaviour {

    private Quaternion rotation;
    private Vector3 offset = new Vector3(0f, 0.7f, 0f);

    public Transform playerToFollow;

	void Awake() {
        rotation = transform.rotation;
    }

    void Update() {
        transform.rotation = rotation;
        if (playerToFollow != null)
            transform.position = playerToFollow.position + offset;
    }
}