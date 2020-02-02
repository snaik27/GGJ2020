using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerSingleton : MonoBehaviour
{
    public static GameManagerSingleton instance = null;                //Static instance of GameManager which allows it to be accessed by any other script.
    public int level = 1;
    public float time;

    // UI Canvas
    public GameObject inGameCanvas;
    public GameObject bg;
    public GameObject ready;
    public GameObject go;
    public GameObject pause;

    public enum GameState
    {
        Menu,
        Ready,
        Go,
        InGame,
        GameOver,
        Pause,
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
        //Ready Go!
        InitCanvas();

    }
    void InitCanvas()
    {
        if (!inGameCanvas.activeSelf)
        {
            inGameCanvas.SetActive(true);
        }
        bg.SetActive(true);
        ready.SetActive(true);
        pause.SetActive(false);
        go.SetActive(false);

        d = ready.GetComponent<Image>().color;
        e = go.GetComponent<Image>().color;
        Debug.Log("hi");
        //StartCoroutine(StartTimerCoroutine());
    }
    public Color c;
    public Color d;
    public Color e;

    public float timeLeft = 3f;

    IEnumerator GoCoroutine()
    {
        while (timeLeft >= 0.0f)
        {
            c.a = Mathf.Lerp(go.GetComponent<Image>().color.a, 0, Time.deltaTime);
            go.GetComponent<Image>().color = c;

            timeLeft -= Time.deltaTime;
        }
        yield return null;
        Debug.Log("Waited 3 seconds");
        go.SetActive(false);
    }
    public float readyTimer = 3.0f;
    public float goTimer = 3.0f;
    private void Update()
    {
        switch (state)
        {
            case GameState.Ready:
                readyTimer -= Time.deltaTime;
                c.a = Mathf.Lerp(bg.GetComponent<Image>().color.a, 0, Time.deltaTime);
                d.a = Mathf.Lerp(ready.GetComponent<Image>().color.a, 0, Time.deltaTime);
                bg.GetComponent<Image>().color = c;
                ready.GetComponent<Image>().color = d;
                if (readyTimer <= 0)
                {
                    go.SetActive(true);
                    ready.SetActive(false);
                    bg.SetActive(false);
                    state = GameState.Go;
                }
                break;

            case GameState.Go:
                goTimer -= Time.deltaTime;
                e.a = Mathf.Lerp(go.GetComponent<Image>().color.a, 0, Time.deltaTime);
                go.GetComponent<Image>().color = e;
                if (goTimer <= 0)
                {
                    go.SetActive(false);
                    state = GameState.InGame;
                }
                break;

            case GameState.InGame:
                Time.timeScale = 1;
                bg.SetActive(false);
                pause.SetActive(false);
                break;

            case GameState.GameOver:
                // EndGame 
                break;

            case GameState.Pause:
                Time.timeScale = 0;
                pause.SetActive(true);
                bg.SetActive(true);
                break;

        }

        time += Time.deltaTime;

        if (time < 60)
        {
            level = 1;
        }
        else if (time >= 60 && time < 120)
        {
            level = 2;
        }
        else if (time >= 120)
        {
            level = 3;
        }



    }
}


