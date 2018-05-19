using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class NetworkManager_Custom : NetworkManager
{

    public NetworkInstanceId localPlayer; //Local player. Gets found once connected.

    private void OnEnable() {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
        if (scene.buildIndex == 0) {
            StartCoroutine(SetupMenuSceneButtons());
        } else {
            StartCoroutine(SetupGameSceneButtons());
        }
    }

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

    IEnumerator SetupMenuSceneButtons() {
        yield return new WaitForSeconds(0.3f);
        Button startHostButton = GameObject.FindGameObjectWithTag("StartHost").GetComponent<Button>();
        startHostButton.onClick.RemoveAllListeners();
        startHostButton.onClick.AddListener(StartupHost);

        Button joinGame = GameObject.FindGameObjectWithTag("JoinGame").GetComponent<Button>();
        joinGame.onClick.RemoveAllListeners();
        joinGame.onClick.AddListener(JoinGame);
    }

    IEnumerator SetupGameSceneButtons() {
        yield return new WaitForSeconds(0.3f);
        GameObject.FindGameObjectWithTag("DisconnectButton").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.FindGameObjectWithTag("DisconnectButton").GetComponent<Button>().onClick.AddListener(StartDisconnect);

        Button particlePlayButton = GameObject.FindGameObjectWithTag("ParticlePlayer").GetComponent<Button>();
        particlePlayButton.onClick.AddListener(PlayParticle);
    }

    void PlayParticle() {
        PlayerController playersController = ClientScene.FindLocalObject(localPlayer).GetComponent<PlayerController>();
        if (playersController == null) {
            return;
        } else {
            Debug.Log("Playing particle!");
            playersController.bloodParticleSystemGO.SetActive(true);
            StartCoroutine(StopParticle());
        }
    }

    IEnumerator StopParticle() {
        yield return new WaitForSeconds(0.5f);

        PlayerController playersController = ClientScene.FindLocalObject(localPlayer).GetComponent<PlayerController>();

        if (playersController != null) {
            playersController.bloodParticleSystemGO.gameObject.SetActive(false);
        }
    }

    void PreDisconnect() {
        StartDisconnect();
    }

    void StartDisconnect() {
        StartCoroutine(Disconnect());
    }

    IEnumerator Disconnect() {
        Debug.Log("Disconnecting!");
        ClientScene.FindLocalObject(localPlayer).GetComponent<PlayerController>().CmdDropEverything(localPlayer);
        yield return new WaitForSeconds(0.2f);
        NetworkManager.singleton.StopHost();
    }
}