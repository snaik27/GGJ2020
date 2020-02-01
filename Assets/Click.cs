using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Click : MonoBehaviour
{
    public TextMeshProUGUI text;
    private int score = 0;
    // Start is called before the first frame update
    void Start()
    {
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        /*
            Debug.Log("Pressed");
        */
    }

    public void OnAKey()
    {
        Debug.Log("Jumped");
        score++;
        string txt = score.ToString();
        text.text = txt;
    }
}
