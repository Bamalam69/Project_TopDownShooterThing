using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateFloor : MonoBehaviour {

    private float grassSize = 2.54f;

    private const float mapWidth = 15.0f;
    private const float mapHeight = 15.0f;

    [SerializeField] private Transform environmentHolder;
    [SerializeField] private GameObject grassPrefab;
    public Sprite[] grassSprites;
    private List<GameObject> grassInstances = new List<GameObject>();

    // Use this for initialization
    void Start () {
        GenerateGrassLand();
	}

    #region funcs

    void GenerateGrassLand() {

        for (float _x = 100; _x != -100; _x--) {
            for (float _y = -100; _y != 100; _y++) {
                grassInstances.Add(Instantiate(grassPrefab, new Vector3(_x * grassSize, _y * grassSize), Quaternion.identity, environmentHolder) as GameObject);              
            }
        }
        RandomizeGrassSprites();
    }
    

    void RandomizeGrassSprites() {
        foreach (GameObject obj in grassInstances) {
            SpriteRenderer spr = obj.GetComponent<SpriteRenderer>();
            spr.sprite = grassSprites[Random.Range(1, 9)];
        }
    }

    #endregion
}
