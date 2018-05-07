using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour
{
    #region vars

    [SerializeField] private GameObject akPrefab;
    [SerializeField] private GameObject akAmmoPrefab;
    private List<GameObject> akInstances = new List<GameObject>();
    private List<GameObject> akAmmoInstances = new List<GameObject>();

    [SerializeField] private GameObject m4Prefab;
    [SerializeField] private GameObject m4AmmoPrefab;
    private List<GameObject> m4Instances = new List<GameObject>();
    private List<GameObject> m4AmmoInstances = new List<GameObject>();

    [SerializeField] private GameObject microPrefab;
    [SerializeField] private GameObject microAmmoPrefab;
    private List<GameObject> microInstances = new List<GameObject>();
    private List<GameObject> microAmmoInstances = new List<GameObject>();

    [SerializeField] private GameObject snipperPrefab;
    [SerializeField] private GameObject snipperAmmoPrefab;
    private List<GameObject> snipperInstances = new List<GameObject>();
    private List<GameObject> snipperAmmoInstances = new List<GameObject>();

    [SerializeField] private GameObject houseRoot;
    [SerializeField] private List<GameObject> weaponSpawnerObjs = new List<GameObject>();

    [SerializeField] private GameObject windowPrefab;
    private List<GameObject> windowInstances = new List<GameObject>();

    #endregion

    #region funcs

    public override void OnStartServer() {
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("Spawning weapons!");
        SpawnWeapons();
        SpawnWindows();
    }

    void SpawnWeapons() {
        GameObject[] weaponSpawners = GameObject.FindGameObjectsWithTag("WeaponSpawner");

        weaponSpawnerObjs.AddRange(weaponSpawners);
        int i = 1;

        foreach (GameObject weaponSpawnerObj in weaponSpawnerObjs) {
            int weaponChance = Random.Range(1, 14);

            if (weaponChance >= 1 && weaponChance <= 4) {
                microInstances.Add((GameObject)Instantiate(microPrefab, weaponSpawnerObj.transform.position, Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.rotation.eulerAngles.z))));

                microAmmoInstances.Add((GameObject)Instantiate(microAmmoPrefab, weaponSpawnerObj.transform.TransformPoint(Random.insideUnitCircle * 0.75f), Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.rotation.eulerAngles.z))));
            } else if (weaponChance >= 5 && weaponChance <= 7) {
                m4Instances.Add((GameObject)Instantiate(m4Prefab, weaponSpawnerObj.transform.position, Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.rotation.eulerAngles.z))));

                m4AmmoInstances.Add((GameObject)Instantiate(m4AmmoPrefab, weaponSpawnerObj.transform.TransformPoint(Random.insideUnitCircle * 0.75f), Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.rotation.eulerAngles.z))));
            } else if (weaponChance >= 8 && weaponChance <= 9) {
                akInstances.Add((GameObject)Instantiate(akPrefab, weaponSpawnerObj.transform.position, Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.rotation.eulerAngles.z))));

                akAmmoInstances.Add((GameObject)Instantiate(akAmmoPrefab, weaponSpawnerObj.transform.TransformPoint(Random.insideUnitCircle * 0.75f), Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.rotation.eulerAngles.z))));
            } else if (weaponChance == 10) {
                snipperInstances.Add((GameObject)Instantiate(snipperPrefab, weaponSpawnerObj.transform.position, Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.rotation.eulerAngles.z))));

                snipperAmmoInstances.Add((GameObject)Instantiate(snipperAmmoPrefab, weaponSpawnerObj.transform.TransformPoint(Random.insideUnitCircle * 0.75f), Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.rotation.eulerAngles.z))));
            }

            i++;
        }


        #region Spawning instantiated weapons on network.
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
        #endregion

        foreach (GameObject ammoObj in akAmmoInstances) {
            NetworkServer.Spawn(ammoObj);
        }

        foreach (GameObject ammoObj in m4AmmoInstances) {
            NetworkServer.Spawn(ammoObj);
        }

        foreach (GameObject ammoObj in microAmmoInstances) {
            NetworkServer.Spawn(ammoObj);
        }

        foreach (GameObject ammoObj in snipperAmmoInstances) {
            NetworkServer.Spawn(ammoObj);
        }
    }

    void SpawnWindows() {
        List<GameObject> windowSpawnObjs = new List<GameObject>();
        windowSpawnObjs.AddRange(GameObject.FindGameObjectsWithTag("WindowSpawner"));
        windowSpawnObjs.AddRange(GameObject.FindGameObjectsWithTag("WindowSpawner2"));

        foreach(GameObject obj in windowSpawnObjs) {
            windowInstances.Add(Instantiate(windowPrefab, obj.transform.position, Quaternion.Euler(new Vector3(0.0f, 0.0f, 90.0f))) as GameObject);
            if (obj.CompareTag("WindowSpawner2")) {
                windowInstances[windowInstances.Count - 1].transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
            }

            Explodable windowScript = windowInstances[windowInstances.Count - 1].GetComponent<Explodable>();
            windowScript.allowRuntimeFragmentation = true;
        }
    }

    #endregion

}
