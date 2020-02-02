using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHole : MonoBehaviour
{
    public Hole hole;
    public GameManagerSingleton gm;
    public HoleManager hm;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SpawnNewHole(GameObject go)
    {

        // Instantniate hole
        switch (FindObjectOfType<GameManagerSingleton>().level)
        {
            case 1:
                MakeHole(1, go);
                break;
            case 2:
                MakeHole(2, go);
                break;
            case 3:
                MakeHole(3, go);
                break;
        }
        
    }
    void MakeHole(int level, GameObject go)
    {
        //hole = new Hole(level, go);
        Hole holeGo = Instantiate(hole, go.transform.position, go.transform.rotation);
    }
}
