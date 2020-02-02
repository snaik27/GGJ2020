using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleManager : MonoBehaviour
{
    //public GameManagerSingleton mainGame;

    public GameObject[] spawnLocations;
    public List<GameObject> currentHoles;
    public GameObject holeSpawner;
    public GameManagerSingleton gm;
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
        if (currentHoles.Count >= spawnLocations.Length )
        {

        }

        else
        {
            while (currentHoles.Contains(spawnLocations[randomTrack]))
            {
                randomTrack = Random.Range(0, spawnLocations.Length);
            }
            //currentHoles.Add(spawnLocations[randomTrack]);
            holeSpawner.GetComponent<SpawnHole>().SpawnNewHole(spawnLocations[randomTrack]);
        }
    }

    public void removeDestroyedHole(Vector3 position)
    {
        Debug.Log(currentHoles.Count);
        for (int i = 0; i < currentHoles.Count; i++)
        {
            //Debug.Log(currentHoles[i].transform.position);
            //Debug.Log(position);
            if (currentHoles[i].transform.position == position)
            {
                currentHoles.Remove(currentHoles[i]);
            }
        }
    }
}
