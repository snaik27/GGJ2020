using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HoleManager : MonoBehaviour
{
    //public GameManagerSingleton mainGame;

    public GameObject[] spawnLocations;
    public List<Transform> spawnedHoles;
    //public GameObject holeSpawner;
    public GameObject Hole;

    public float spawnTimer = 2f;
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
        //holeSpawner.GetComponent<SpawnHole>().SpawnNewHole(spawnLocations[randomTrack]);
        Transform newHole = Instantiate(Hole, spawnLocations[randomTrack].transform.position, spawnLocations[randomTrack].transform.rotation).transform;
        newHole.GetComponent<Hole>().hm = this;
        spawnedHoles.Add(newHole);

    }

    
}
