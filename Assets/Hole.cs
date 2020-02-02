using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
    public int hp = 8;
    // Start is called before the first frame update
    void Start()
    {
        //print("Spawned Hole at " + transform.position);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnAKey()
    {
        hp--;
    }
}
