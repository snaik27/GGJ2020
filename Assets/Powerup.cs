using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Powerup : MonoBehaviour
{
    public List<Transform> SpawnLocations;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            int randal = (int)Random.Range(0f, SpawnLocations.Count);
            transform.position = new Vector3(SpawnLocations[randal].position.x, 1f, SpawnLocations[randal].position.z);
        }
    }
}
