using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateFloor : MonoBehaviour {

    private float grassSize = 2.54f;

    private float mapWidth = 15.0f;
    private float mapHeight = 15.0f;

    [SerializeField] private GameObject grassPrefab;
    public Sprite[] grassSprites;
    private List<GameObject> grassInstances = new List<GameObject>();

    // Use this for initialization
    void Start () {
        GenerateGrassLand();
	}

    #region funcs

    void GenerateGrassLand() {


        for (float _x = 0; _x < mapWidth; _x++) {
            for (float _y = 0; _y < mapHeight; _y++) {
                grassInstances.Add(Instantiate(grassPrefab, new Vector3(_x * grassSize, _y * grassSize), Quaternion.identity) as GameObject);
            }
        }

        RandomGrassSprites();
    }
    #endregion

    void RandomGrassSprites() {
        foreach (GameObject obj in grassInstances) {
            SpriteRenderer spr = obj.GetComponent<SpriteRenderer>();
            spr.sprite = grassSprites[Random.Range(1, 9)];
        }
    }
}
