using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 1. Hole spawning (not in the same space)          xxxxx
/// 2. Hole removal                                   xxxxx
///     2a. Button prompts for hole removal           -Andrew-
/// 3. Keep count of score (last person to hit the hole "wins" the point)    xxxxxxxxx
/// 4. Music
/// 5. SFX
/// 6. Decals
/// 7. Scale up train width a bit
/// 8. Let keyboard/mouse actually turn xxxxxxxxxxxxxx
/// 9. Make sure holes can't spawn out of camera view 
/// 10. Add railings to platforms
/// 11. add timeclock
/// 12. Train warning
/// powerup: Change model to shovel model, randomly spawn around map every X seconds
/// </summary>
public class HoleManager : MonoBehaviour
{
    //public GameManagerSingleton mainGame;

    public List<Transform> spawnLocations;
    public List<Transform> usedLocations;
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
        if (spawnLocations.Count > 0)
        {
            int randomTrack = Random.Range(0, spawnLocations.Count);
            Transform Track = spawnLocations[randomTrack];
            //holeSpawner.GetComponent<SpawnHole>().SpawnNewHole(spawnLocations[randomTrack]);
            Transform newHole = Instantiate(Hole, Track.transform.position, Track.transform.rotation).transform;
            newHole.GetComponent<Hole>().hm = this;
            newHole.GetComponent<Hole>().correspondingTrack = Track;
            newHole.gameObject.tag = "Hole";
            spawnedHoles.Add(newHole);
            usedLocations.Add(Track);
            spawnLocations.Remove(Track);
        }
    }

    public void RemoveHole(Transform track)
    {
        spawnLocations.Add(track);
        usedLocations.Remove(track);
    }

    
}
