using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Powerup : MonoBehaviour
{
    public List<Transform> SpawnLocations;
    public Material myMaterial;
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            int randal = (int)Random.Range(0f, SpawnLocations.Count);
            transform.position = new Vector3(SpawnLocations[randal].position.x, 1f, SpawnLocations[randal].position.z);
        }
    }


    private void Update()
    {
        float emissionVal = Mathf.Lerp(0f, 0.5f, Mathf.Sin(10f * Time.time) * 0.5f + 0.5f);
        myMaterial.SetFloat("_EmissionValue", emissionVal);
    }
}
