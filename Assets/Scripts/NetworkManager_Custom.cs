using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkManager_Custom : NetworkManager {

    public List<int> playerNetIds = new List<int>();

	public void StartupHost() {
        SetPort();
        NetworkManager.singleton.StartHost();
    }

    public void JoinGame() {
        SetIPAddress();
        SetPort();
        NetworkManager.singleton.StartClient();
    }

    void SetIPAddress() {
        string ipAddress = GameObject.FindGameObjectWithTag("InputFieldIPAdress").transform.Find("Text").GetComponent<Text>().text;
        NetworkManager.singleton.networkAddress = ipAddress;
    }

    void SetPort() {
        NetworkManager.singleton.networkPort = 7777;
    }

    private void OnLevelWasLoaded(int level) {
        if (level == 0) {
            SetupMenuSceneButtons();
        } else {
            SetupGameSceneButtons();
        }
    }

    void SetupMenuSceneButtons() {
        GameObject.FindGameObjectWithTag("StartHost").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.FindGameObjectWithTag("StartHost").GetComponent<Button>().onClick.AddListener(StartupHost);

        GameObject.FindGameObjectWithTag("JoinGame").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.FindGameObjectWithTag("JoinGame").GetComponent<Button>().onClick.AddListener(JoinGame);
    }

    void SetupGameSceneButtons() {
        GameObject.FindGameObjectWithTag("DisconnectButton").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.FindGameObjectWithTag("DisconnectButton").GetComponent<Button>().onClick.AddListener(NetworkManager.singleton.StopHost);
    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        Debug.Log(conn.clientOwnedObjects);
    }

}