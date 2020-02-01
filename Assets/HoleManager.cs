using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleManager : MonoBehaviour
{
    //public GameManagerSingleton mainGame;

    public GameObject[] spawnLocations;
    public GameObject holeSpawner;
    public List<Hole> holeStack;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnBKey()
    {
        int randomTrack = Random.Range(0, spawnLocations.Length);
        holeSpawner.GetComponent<SpawnHole>().SpawnNewHole(spawnLocations[randomTrack]);
    }


}
