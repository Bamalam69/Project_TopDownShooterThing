using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [HideInInspector] public Transform playerToFollow;
    
    void FixedUpdate() {
        if (playerToFollow != null)
        transform.position = Vector3.Lerp(transform.position, playerToFollow.position + new Vector3(0, 0, -10), 0.7f);
    }

    public void SetTarget(Transform target) {
        playerToFollow = target;
    }
} 