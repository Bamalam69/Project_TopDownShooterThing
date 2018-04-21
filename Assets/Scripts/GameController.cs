using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour
{
    [SerializeField] private GameObject akPrefab;
    private GameObject akInstance;

    [SerializeField] private GameObject m4Prefab;
    private GameObject m4Instance;

    [SerializeField] private GameObject microPrefab;
    private GameObject microInstance;

    [SerializeField] private GameObject snipperPrefab;
    private GameObject snipperInstance;

    [SerializeField] private GameObject houseRoot;

    public override void OnStartServer() {
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("Spawning weapons!");
        SpawnWeapons();
        SpawnWindows();
    }

    void SpawnWeapons() {
        akInstance = (GameObject)Instantiate(akPrefab);
        NetworkServer.Spawn(akInstance);

        m4Instance = (GameObject)Instantiate(m4Prefab);
        NetworkServer.Spawn(m4Instance);

        microInstance = (GameObject)Instantiate(microPrefab);
        NetworkServer.Spawn(microInstance);

        snipperInstance = (GameObject)Instantiate(snipperPrefab);
        NetworkServer.Spawn(snipperInstance);
    }

    void SpawnWindows() {
        houseRoot.GetComponent<WindowSpawner>().SpawnWindows();
    }
}
