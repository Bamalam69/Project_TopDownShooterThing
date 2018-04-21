using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootObjHolder : MonoBehaviour {

    public Transform parent;

    void Start() {
        parent = transform.parent;
    }
}
