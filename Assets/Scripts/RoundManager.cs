using UnityEngine;

public class RoundManager : MonoBehaviour
{
    //temporary
    public int playerStartingClass;

    //SPAWNING STUFF
    public GameObject cutterP, brigantineP, frigateP, galleonP;
    public GameObject cutterA, brigantineA, frigateA, galleonA;

    GameObject instance;

    DeathScreen ds;

    public float respawnTime = 10f;
    public bool lives = false;
    public int maxLives = 1;
    public int totalShips = 32;
    
    public bool teams = false;
    public int playerTeam;

    public float spawnImmunityTime = 5f;

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
            shipStatuses[0] = new ShipInfo(true, playerTeam, playerStartingClass);
            for (int i=1;i<totalShips;i++)
            {
                shipStatuses[i] = new ShipInfo(false, 0, Random.Range(0, 4));
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

    void SpawnShuffle(Transform[] spawns)
    {
        for (int i = 0; i < spawns.Length; i++)
        {
            position = Random.Range(i, spawns.Length);
            temp = spawns[i];
            spawns[i] = spawns[position];
            spawns[position] = temp;
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
