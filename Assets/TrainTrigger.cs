using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainTrigger : MonoBehaviour
{
    [SerializeField] public float TrainDelay;

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(SetNextTrain(other));
      
    }

    private IEnumerator SetNextTrain(Collider other)
    {
        yield return new WaitForSeconds(TrainDelay);
        if (other.CompareTag("Train"))
        {
            other.GetComponent<Train>().ResetPosition();
            if (other.GetComponent<Train>().trainSpeed < 0)
            {
                other.GetComponent<Train>().trainSpeed = (int)Random.Range(-5.0f, -1.0f);
            }
            else
            {
                other.GetComponent<Train>().trainSpeed = (int)Random.Range(1.0f, 5.0f);

            }

        }
    }
}
