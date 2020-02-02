using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerSingleton : MonoBehaviour
{
    public static GameManagerSingleton instance = null;                //Static instance of GameManager which allows it to be accessed by any other script.
    public int level = 1;
    public float time;
    public enum GameState
    {
        Menu,
        InGame,
        GameOver,
        HighScore
    }
    public GameState state;
    //private Level level;
    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        //Get a component reference to the attached BoardManager script
     //   boardScript = GetComponent<BoardManager>();

        //Call the InitGame function to initialize the first level 
        InitGame();
    }
    //Initializes the game for each level.
    void InitGame()
    {
        time = 0f;
        
    }
    private void Update()
    {
        time += Time.deltaTime;
        if(time < 60)
        {
            level = 1;
        } else if(time >60 && time < 120)
        {
            level = 2;
        } else if(time >120 && time< 180)
        {
            level = 3;
        }
    }
}


