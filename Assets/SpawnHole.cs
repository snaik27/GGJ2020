using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHole : MonoBehaviour
{
    public Hole hole;
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
        Instantiate(hole, go.transform.position, go.transform.rotation);
    }
}
