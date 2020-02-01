using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public GameManagerSingleton gameManager;

    // Audio players components.
    public AudioSource EffectsSource;
    public AudioSource MusicSource;
    public AudioClip[] bgm;
    public AudioClip[] fx;
    public GameManagerSingleton.GameState lastState;

    // Random pitch adjustment range.
    public float LowPitchRange = .95f;
    public float HighPitchRange = 1.05f;

    // Singleton instance.
    //  public static AudioManager Instance = null;
    /*
    private void Awake()
    {
        // If there is not already an instance of SoundManager, set it to this.
        if (Instance == null)
        {
            Instance = this;
        }
        //If an instance already exists, destroy whatever this object is to enforce the singleton.
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        //Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
        DontDestroyOnLoad(gameObject);
    }*/
    public void Play(AudioClip clip)
    {
        EffectsSource.clip = clip;
        EffectsSource.Play();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        switch (gameManager.state)
        {
            case GameManagerSingleton.GameState.Menu:
                MusicSource.clip = bgm[0];
                break;
            case GameManagerSingleton.GameState.InGame:
                MusicSource.clip = bgm[1];
                break;
            case GameManagerSingleton.GameState.GameOver:
                MusicSource.clip = bgm[2];
                //MusicSource.clip = Resources.Load<AudioClip>("Menu");
                break;
            case GameManagerSingleton.GameState.HighScore:
                MusicSource.clip = bgm[3];
                break;
            default:
                MusicSource.clip = bgm[0];
                break;
        }
        lastState = gameManager.state;
        if (!MusicSource.isPlaying)
        {
            MusicSource.Play();
        }
    }
}