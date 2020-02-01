using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
    [SerializeField] private Vector3 OriginalWorldPos;
    [SerializeField] public float trainSpeed;
    [SerializeField] public bool canMove;

    private void Awake()
    {
        OriginalWorldPos = transform.position;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            transform.Translate(transform.right * trainSpeed);
        }
    }

    public void ResetPosition()
    {
        transform.position = OriginalWorldPos;
    }
}
