using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IK_Follow : MonoBehaviour
{
    public Vector3 localPos;
    public Quaternion localRot;
    public Vector3 currentWorldPos;
    public Transform target = null;

    private void Awake()
    {
        localPos = transform.localPosition;
        localRot = transform.localRotation;
    }

    private void Update()
    {
        if (target != null)
        {
            try
            {

                transform.position = Vector3.Lerp(transform.position, target.transform.position, 100f * Time.deltaTime);
            }
            catch
            {
                transform.position = Vector3.Lerp(transform.position, target.GetComponent<Rigidbody>().worldCenterOfMass, 100f * Time.deltaTime);
            }
            //transform.position = Vector3.Lerp(transform.position, target.transform.position + Vector3.up * 0.5f, 100f * Time.deltaTime);

        }

        currentWorldPos = transform.position;
    }
}
