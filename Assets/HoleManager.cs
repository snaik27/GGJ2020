using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleManager : MonoBehaviour
{
    //public GameManagerSingleton mainGame;

    public GameObject[] spawnLocations;
    public GameObject holeSpawner;
    public GameManagerSingleton gm;
    public float spawnTimer = 5f;
    private float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= spawnTimer)
        {
            SpawnNewHole();
            timer = 0f;
        }
    }
    void SpawnNewHole()
    {
        int randomTrack = Random.Range(0, spawnLocations.Length);
        holeSpawner.GetComponent<SpawnHole>().SpawnNewHole(spawnLocations[randomTrack]);
    }

}
