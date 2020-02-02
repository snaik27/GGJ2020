    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shovel : MonoBehaviour
{
    [SerializeField] public MovingSphere Player;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hole"))
        {
            Player.ImTouchingThisHole = other.GetComponent<Hole>();
            Player.canShovel = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hole"))
        {
            Player.ImTouchingThisHole = null;
            Player.canShovel = false;
        }
    }
}
