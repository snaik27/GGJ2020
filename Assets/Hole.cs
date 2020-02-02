using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
    public HoleManager hm;
    public int hp = 5;
    public Transform correspondingTrack;
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
    
    private IEnumerator DestroySelf()
    {
        yield return null;
        Destroy(this.gameObject);
    }
    public void ReduceHealthByOne()
    {
        hp--;
        StartCoroutine(scaleDown());
        if(hp <= 0)
        {
            hm.RemoveHole(correspondingTrack);
            StartCoroutine(DestroySelf());
        }
    }

    public IEnumerator scaleDown()
    {
        Vector3 newLocalScale = transform.localScale - Vector3.one * 0.5f;
        while (transform.localScale != newLocalScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, newLocalScale, 3f * Time.deltaTime);
            yield return null;
        }
    }
}
