using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
    public HoleManager hm;
    public int hp = 8;
    public enum Commands
    {
        ButtonA,
        ButtonB,
        ButtonX,
        ButtonY
    }
    public enum Difficulty
    {
        Easy,
        Intermediate,
        Hard
    }
    public Commands commands;
    public Difficulty difficulty;
    /*
    public Hole(int level, GameObject location)
    {
        switch (level)
        {
            case 1:
                difficulty = Difficulty.Easy;
                break;
            case 2:
                difficulty = Difficulty.Intermediate;
                break;
            case 3:
                difficulty = Difficulty.Hard;
                break;
        }
        transform.position = location.transform.position;
        hp = 8;
    }*/
    // Start is called before the first frame update
    void Start()
    {
        print("Spawned Hole at " + transform.position);
        InitHole();
    }
    void InitHole()
    {
        if(difficulty == Difficulty.Easy)
        {
            hp = Random.Range(5, 10);
        }
        else if (difficulty == Difficulty.Intermediate)
        {
            hp = Random.Range(11, 20);
        }
        else
        {
            hp = Random.Range(21, (int)(FindObjectOfType<GameManagerSingleton>().time/6));
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(hp <= 0)
        {
            //hm.removeDestroyedHole(transform.position);
            Debug.Log("Hi");
            hm.currentHoles.Remove(this.gameObject);
            Destroy(gameObject);
        }
    }
}
