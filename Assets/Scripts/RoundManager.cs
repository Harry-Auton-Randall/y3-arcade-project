using UnityEngine;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    //temporary
    public int playerStartingClass;
    public bool player; //purely for debugging - if false, spawns an AI in the player's place

    //SPAWNING STUFF
    public GameObject cutterP, brigantineP, frigateP, galleonP;
    public GameObject cutterA, brigantineA, frigateA, galleonA;

    GameObject instance;

    DeathScreen ds;

    public float respawnTime = 10f;
    public bool lives = false;
    public int maxLives = 1;
    public int totalShips = 32;
    public List<int> allowedShips;
    
    public bool teams = false;
    public int playerTeam;

    public float spawnImmunityTime = 5f;

    public Transform[] spawnsTemp;
    public Transform spawnParent;
    int arrayMover;

    public Transform[] spawns0, spawns1, spawns2;
    Transform temp;
    int position;

    public ShipInfo[] shipStatuses;

    //GAME MODE STUFF
    public int mode = 0;
    public int[] scoresSolo;
    public int[] scoresTeam;
    public int scoreTarget = 50;


    //For rotating Ai's reticles
    public Quaternion playerReticleRotation;

    void Awake()
    {
        //TEMPORARY
        teams = false;
        mode = 0;

        ds = GameObject.Find("/deathScreen").GetComponent<DeathScreen>();

        //Fills spawn arrays - GetComponentsInChildren includes the parent, so the extra stuff is needed to remove it
        SpawnLocationAssign(ref spawns0, "/Spawns0");

        //SHUFFLE SPAWN LOCATION ARRAYS, FOR RANDOMISED SPAWN LOCATIONS
        SpawnShuffle(spawns0);
        SpawnShuffle(spawns1);
        SpawnShuffle(spawns2);

        if (teams)
        {
        }
        else
        {
            playerTeam = 0;
            totalShips = Mathf.Clamp(totalShips, 2, spawns0.Length);

            shipStatuses = new ShipInfo[totalShips];
            if (player)
            {
                shipStatuses[0] = new ShipInfo(true, playerTeam, playerStartingClass);
            }
            else
            {
                shipStatuses[0] = new ShipInfo(false, playerTeam, playerStartingClass);
            }

            for (int i=1;i<totalShips;i++)
            {
                shipStatuses[i] = new ShipInfo(false, 0, allowedShips[Random.Range(0, allowedShips.Count)]);
            }
        }

        //shipStatuses = new ShipInfo[] { new ShipInfo(true, 0, playerStartingClass) };
        scoresSolo = new int[shipStatuses.Length];
        if (teams)
        {
            scoresTeam = new int[] { 0, 0 };
        }
        for (int i = 0; i < shipStatuses.Length; i++)
        {
            shipStatuses[i].SetLives(maxLives);
            shipStatuses[i].SetRespawn(0f);
            scoresSolo[i] = 0;
        }
    }
    void Start()
    {
        if (teams)
        {
        }
        else
        {
            for (int i = 0; i < shipStatuses.Length; i++)
            {
                SpawnShip(i, spawns0[i]);
            }
        }
    }

    void SpawnShuffle(Transform[] spawnsIn)
    {
        for (int i = 0; i < spawnsIn.Length; i++)
        {
            position = Random.Range(i, spawnsIn.Length);
            temp = spawnsIn[i];
            spawnsIn[i] = spawnsIn[position];
            spawnsIn[position] = temp;
        }
    }
    void SpawnLocationAssign(ref Transform[] spawnsIn, string searchIn)
    {
        spawnsTemp = GameObject.Find(searchIn).GetComponentsInChildren<Transform>();
        spawnParent = GameObject.Find(searchIn).transform;
        arrayMover = 0;
        spawnsIn = new Transform[spawnsTemp.Length - 1];

        for (int i = 0; i < spawnsTemp.Length; i++)
        {
            if (spawnsTemp[i] != spawnParent)
            {
                spawnsIn[arrayMover] = spawnsTemp[i];
                arrayMover++;
            }
        }
    }

    void Update()
    {
        for (int i=0;i<shipStatuses.Length;i++)
        {
            if (shipStatuses[i].respawnProgress != 0f)
            {
                shipStatuses[i].respawnProgress -= Time.deltaTime;
                if (shipStatuses[i].respawnProgress <= 0f)
                {
                    shipStatuses[i].respawnProgress = 0f;
                    shipStatuses[i].isAlive = true;
                    switch (shipStatuses[i].team)
                    {
                        case 0:
                            SpawnShip(i, spawns0[Random.Range(0, spawns0.Length)]);
                            break;
                        case 1:
                            SpawnShip(i, spawns1[Random.Range(0, spawns1.Length)]);
                            break;
                        case 2:
                            SpawnShip(i, spawns2[Random.Range(0, spawns2.Length)]);
                            break;
                    }
                }
            }
        }
    }

    void SpawnShip(int id, Transform spawnPos)
    {
        if (shipStatuses[id].isPlayer)
        {
            //temporary
            ChangeClass(id, playerStartingClass);

            ds.Disable();
            switch (shipStatuses[id].shipClass)
            {
                case 0:
                    instance = Instantiate(cutterP);
                    break;
                case 1:
                    instance = Instantiate(brigantineP);
                    break;
                case 2:
                    instance = Instantiate(frigateP);
                    break;
                case 3:
                    instance = Instantiate(galleonP);
                    break;
            }
        }
        else
        {
            switch (shipStatuses[id].shipClass)
            {
                case 0:
                    instance = Instantiate(cutterA);
                    break;
                case 1:
                    instance = Instantiate(brigantineA);
                    break;
                case 2:
                    instance = Instantiate(frigateA);
                    break;
                case 3:
                    instance = Instantiate(galleonA);
                    break;
            }
        }

        instance.GetComponent<boatCombat>().SetTeamStuff(shipStatuses[id].team, id);
        instance.transform.position = spawnPos.position;
        instance.transform.rotation = spawnPos.rotation;

    }

    public void KillShip(int id)
    {
        shipStatuses[id].RegisterKill(respawnTime, lives);
    }

    public void ScoreIncSolo(int id)
    {
        scoresSolo[id] += 1;
        Debug.Log(id + "'s score increases to " + scoresSolo[id]);
    }

    public void Killfeed(int killerId, int victimId)
    {
        Debug.Log(killerId + " sunk " + victimId);
    }

    public void ChangeClass(int id, int classIn)
    {
        shipStatuses[id].SetClass(classIn);
    }
}

public class ShipInfo
{
    public bool isPlayer;
    public int team;
    public int shipClass;

    public bool isAlive;
    public int lives;

    public float respawnProgress;
    public ShipInfo(bool isPlayerIn, int teamIn, int shipClassIn)
    {
        isPlayer = isPlayerIn;
        team = teamIn;
        shipClass = shipClassIn;
        isAlive = true;
    }
    public void SetLives(int livesIn)
    {
        lives = livesIn;
    }
    public void SetRespawn(float respawnIn)
    {
        respawnProgress = respawnIn;
    }
    public void SetClass(int classIn)
    {
        shipClass = classIn;
    }
    public void RegisterKill(float respawnTimeIn, bool livesOn)
    {
        isAlive = false;
        if (livesOn)
        {
            lives -= 1;
            if (lives < 0) { lives = 0; }
        }
        respawnProgress = respawnTimeIn;
    }
}
