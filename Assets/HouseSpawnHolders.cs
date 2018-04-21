using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseSpawnHolders : MonoBehaviour {

    public List<GameObject> weaponSpawners = new List<GameObject>();

    void Start() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).CompareTag("WeaponSpawner")) {
                weaponSpawners.Add(transform.GetChild(i).gameObject);
            }
        }
        print(weaponSpawners);
    }

}