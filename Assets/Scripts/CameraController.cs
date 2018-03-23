using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [HideInInspector] public Transform playerToFollow;
    [HideInInspector] public float timeToLerp = 0.7f;
    [HideInInspector] public float defaultTime = 0.7f;
    [HideInInspector] public float size = 6.0f;
    [HideInInspector] public float defaultSize = 6.0f;

    private Camera cam;

    void Start() {
        cam = GetComponentInChildren<Camera>();
    }

    void FixedUpdate() {
        if (playerToFollow != null) {
            transform.position = Vector3.Lerp(transform.position, playerToFollow.position + new Vector3(0, 0, -10), timeToLerp);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, size, 0.5f);
        }
    }

    public void SetTarget(Transform target) {
        playerToFollow = target;
    }
} 