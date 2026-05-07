using UnityEngine;

public class DataPasser : MonoBehaviour
{
    public int id;
    public string mapName;

    //Every possible thing one might want to change about the round (minus team/game mode stuff). Hardcoded for now
    public bool player = true;
    public int shipNo = 99;
    public int[] shipTypes = new int[] { 0, 1, 2, 3 };

    public bool scoreOrTime = true;
    public int score = 5;

    public bool lives = false;
    public int maxLives = 1;

    public float respawnTime = 10;
    public float spawnImmunityTime = 5;

    void Awake()
    {
        Object.DontDestroyOnLoad(this.gameObject);
    }
}
