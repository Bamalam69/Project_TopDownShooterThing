using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour
{
    #region vars

    [SerializeField] private GameObject akPrefab;
    private List<GameObject> akInstances = new List<GameObject>();

    [SerializeField] private GameObject m4Prefab;
    private List<GameObject> m4Instances = new List<GameObject>();

    [SerializeField] private GameObject microPrefab;
    private List<GameObject> microInstances = new List<GameObject>();

    [SerializeField] private GameObject snipperPrefab;
    private List<GameObject> snipperInstances = new List<GameObject>();

    [SerializeField] private GameObject houseRoot;
    [SerializeField] private List<GameObject> weaponSpawnerObjs = new List<GameObject>();

    #endregion

    #region funcs

    public override void OnStartServer() {
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("Spawning weapons!");
        SpawnWeapons();
        //SpawnWindows();
    }

    void SpawnWeapons() {

        weaponSpawnerObjs.AddRange(GameObject.FindGameObjectsWithTag("WeaponSpawner"));

        foreach (GameObject weaponSpawnerObj in weaponSpawnerObjs) {
            int weaponChance = Random.Range(1, 10);
            if (weaponChance >= 1 && weaponChance <= 4) {
                microInstances.Add((GameObject)Instantiate(microPrefab));
                microInstances[microInstances.Count - 1].transform.position = weaponSpawnerObj.transform.position;
            } else if (weaponChance >= 5 && weaponChance <= 7) {
                m4Instances.Add((GameObject)Instantiate(m4Prefab));
                m4Instances[m4Instances.Count - 1].transform.position = weaponSpawnerObj.transform.position;
            } else if (weaponChance >= 8 && weaponChance <= 9) {
                akInstances.Add((GameObject)Instantiate(akPrefab));
                akInstances[akInstances.Count - 1].transform.position = weaponSpawnerObj.transform.position;
            } else if (weaponChance == 10) {
                snipperInstances.Add((GameObject)Instantiate(akPrefab));
                snipperInstances[snipperInstances.Count - 1].transform.position = weaponSpawnerObj.transform.position;
            }
        }

        foreach (GameObject weapon in microInstances) {
            NetworkServer.Spawn(weapon);
        }

        foreach (GameObject weapon in akInstances) {
            NetworkServer.Spawn(weapon);
        }

        foreach (GameObject weapon in m4Instances) {
            NetworkServer.Spawn(weapon);
        }

        foreach (GameObject weapon in snipperInstances) {
            NetworkServer.Spawn(weapon);
        }
    }

    //void SpawnWindows() {
    //    houseRoot.GetComponent<WindowSpawner>().SpawnWindows();
    //}

    #endregion

}
