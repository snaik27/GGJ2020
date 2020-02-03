using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainTrigger : MonoBehaviour
{
    [SerializeField] public float TrainDelay;
    [SerializeField] public GameObject warning;
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(SetNextTrain(other));
    }

    private IEnumerator SetNextTrain(Collider other)
    {
        //float delay = (int)Random.Range(.5f, 2.0f);
        //delay += TrainDelay;
        yield return new WaitForSeconds(TrainDelay);
        if (other.CompareTag("Train"))
        {
            other.GetComponent<Train>().ResetPosition();
            StartCoroutine(BlinkIndicator());
            
            yield return new WaitForSeconds(2f);
            StopAllCoroutines();

        }
    }

    private IEnumerator BlinkIndicator()
    {
        float duration = 1.5f;
        while (duration > 0f)
        {
            warning.SetActive(!warning.activeSelf);
            yield return new WaitForSeconds(0.25f);
            duration -= Time.deltaTime;
        }

        warning.SetActive(false);
    }
}
