using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEditor;

[CanEditMultipleObjects]
public class Bone : MonoBehaviour
{
    #region BASE BONE PROPERTIES
    public float lerpSpeed;
    [SerializeField] public Vector3 originalLocalPosition;
    [SerializeField] public Quaternion originalLocalRotation;
    public Transform correspondingMesh;
    public Rigidbody m_Rigidbody;
    public Collider m_Collider;
    #endregion

    #region IK PROPERTIES
    public Transform player;
    public bool targetWithinRange = false;
    public float maxMoveDistance;
    public bool isAggressive;
    public bool isPassive;
    [SerializeField] public bool isSnakeLike; //Set from editor for now, automate later

    //Set these manually for now, later automate somehow
    public Bone prev;
    public Bone next;

    public float sinShift;
    #endregion
    private void Awake()
    {
        SetBoneProperties();
    }
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        ComputeMaxMoveDistance();
    }

    // Update is called once per frame
    void Update()
    {
    }

    #region ALL BONES CAN DO THESE THINGS
    public void SetBoneProperties()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        try
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Collider = GetComponent<Collider>();
            correspondingMesh = GetComponent<DampedTransform>().data.constrainedObject;
        }
        catch
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Collider = GetComponent<Collider>();
            correspondingMesh = GetComponent<MultiAimConstraint>().data.constrainedObject;
        }
    }

    public void MoveToMeshLocation()
    {
        transform.position = correspondingMesh.position;
        transform.rotation = correspondingMesh.rotation;
    }
    public IEnumerator ReturnToOriginalLocal()
    {
        MoveToMeshLocation();
        m_Rigidbody.isKinematic = true;
        m_Rigidbody.useGravity = false;
        while (transform.localPosition != originalLocalPosition)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, lerpSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalLocalRotation, lerpSpeed * Time.deltaTime);
            yield return null;
        }
        yield return null;
    }
    #endregion


    /// <summary>
    /// TODO:
    /// 1. Clamp all end-positions to be inside the chain-ik's actual movement range 
    /// </summary>
    /// <returns></returns>
     #region SNAKE SPECIFIC


    public void ComputeMaxMoveDistance()
    {
        try
        {
            maxMoveDistance = transform.root.GetComponent<SphereCollider>().radius;
        }
        catch
        {
            Debug.Log("root doesn't have a sphere collider");
        }
    }

    public IEnumerator LerptoAimTarget()
    {
        isPassive = false;
        isAggressive = true;

        if (Vector3.Distance(transform.root.position, player.position) <= maxMoveDistance)
        {
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;
            Transform target = GetComponent<MultiAimConstraint>().data.sourceObjects[0].transform;

            while (transform.position != target.position)
            {
                transform.position = Vector3.Lerp(transform.position, target.position, lerpSpeed * Time.deltaTime);
                yield return null;
            }
        }
        else
            yield return null;

    }
    public IEnumerator LerpDampedTransform(float constraintWeight)
    {

        float timeLeft = 3f;
        while (timeLeft > 0)
        {
            GetComponent<DampedTransform>().weight = Mathf.Lerp(1-constraintWeight, constraintWeight, (3 - timeLeft) / 3);
            timeLeft -= Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator HoverNearTarget()
    {
        isPassive = true;
        isAggressive = false;
        if (Vector3.Distance(transform.root.position, player.position) <= maxMoveDistance)
        {
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;

            Transform target = GetComponent<MultiAimConstraint>().data.sourceObjects[0].transform;
            
            while (transform.position != target.position)
            {
                
                float sinOffset = Mathf.Sin(Time.time) * sinShift; //Shift Range of sin to +-10
                float cosOffset = Mathf.Cos(Time.time) * sinShift;
                Vector3 direction = ((target.position + (2f * Vector3.up) + correspondingMesh.right * sinOffset) - correspondingMesh.position);
                Vector3 normalizedDir = direction / Vector3.Distance((target.position + correspondingMesh.right * sinOffset), correspondingMesh.position);
                Vector3 slitherToTarget = correspondingMesh.position + direction - 10f*normalizedDir; //Slither to 3 units away from target (so it's not annoying/touching player)
                
                transform.position = Vector3.Lerp(correspondingMesh.position, slitherToTarget, Time.deltaTime);
                yield return null;
            }
        }
        else
            yield return null;

    }
    #endregion
}

[CustomEditor(typeof(Bone))]
[CanEditMultipleObjects]
public class BoneEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Bone bone = (Bone)target;

        if(GUILayout.Button("Move to mesh location"))
        {
            bone.MoveToMeshLocation();
        }
        if (GUILayout.Button("Return"))
        {
            bone.StopAllCoroutines();
            bone.StartCoroutine(bone.ReturnToOriginalLocal());
        }
        if (GUILayout.Button("Attack"))
        {
            bone.StopAllCoroutines();
            bone.StartCoroutine(bone.LerptoAimTarget());
        }
        if (GUILayout.Button("Hover Near Target"))
        {
            bone.StopAllCoroutines();
            bone.StartCoroutine(bone.HoverNearTarget());
        }
    }
}
