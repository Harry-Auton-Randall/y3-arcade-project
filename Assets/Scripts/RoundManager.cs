using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    int mapId;
    string mapName;
    DataPasser dps;

    public GameObject dataPasser;

    public bool PlayerCheatButton; //Causes the player to instantly win, for demo-ing progression on victory because I suck at my own game
    GameObject startCanvas;
    Text startTitle, startDesc;

    //temporary
    //public int playerStartingClass;
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

    //Waypoints
    public Transform[] waypointTransforms;
    public WaypointInfo[] waypoints;

    public Waypoint[] allWaypoints;
    LayerMask terrainMask;
    RaycastHit rayHit;

    public ShipInfo[] shipStatuses;

    //GAME MODE STUFF
    public int mode = 0;
    //public int[] scoresSolo;
    public int[] scoresTeam;
    public int scoreTarget = 5;
    public float timeLeft = 300;

    public bool scoreOrTime = true; //true for score target, false for time limit

    public int[] scoresSorted;

    public bool gameStarted;
    bool gameEnded;


    //For rotating UI elements based on camera rotation
    public float playerCamRotation = 0;

    public void RemakePasser()
    {
        instance = Instantiate(dataPasser);
        DataPasser dps2 = instance.GetComponent<DataPasser>();

        dps2.id = mapId;
        dps2.mapName = this.mapName;

        dps2.player = this.player;
        dps2.shipNo = totalShips;
        dps2.shipTypes = new int[allowedShips.Count];
        for (int i=0;i<allowedShips.Count;i++)
        {
            dps2.shipTypes[i] = allowedShips[i];
        }

        dps2.scoreOrTime = this.scoreOrTime;
        dps2.score = this.scoreTarget;

        dps2.lives = this.lives;
        dps2.maxLives = this.maxLives;

        dps2.respawnTime = this.respawnTime;
        dps2.spawnImmunityTime = this.spawnImmunityTime;
    }

    void Awake()
    {
        Time.timeScale = 1;

        gameStarted = false;
        gameEnded = false;

        //Reading info from DataPasser, if present
        if (GameObject.Find("/DataPasser(Clone)") != null)
        {
            Debug.Log("Level selected from menu");
            dps = GameObject.Find("/DataPasser(Clone)").GetComponent<DataPasser>();

            mapId = dps.id;
            this.mapName = dps.mapName;

            this.player = dps.player;
            totalShips = dps.shipNo;
            allowedShips = new List<int>();
            for (int i=0;i<dps.shipTypes.Length;i++)
            {
                allowedShips.Add(dps.shipTypes[i]);
            }

            this.scoreOrTime = dps.scoreOrTime;
            scoreTarget = dps.score;

            this.lives = dps.lives;
            this.maxLives = dps.maxLives;

            this.respawnTime = dps.respawnTime;
            this.spawnImmunityTime = dps.spawnImmunityTime;

            Destroy(dps.gameObject);
        }
        else
        {
            Debug.Log("Level not selected from menu");
            mapId = -1;
            mapName = "N/A";
        }

        if (!scoreOrTime)
        {
            timeLeft = scoreTarget; //When time-based, scoreTarget doubles as the full time limit
        }

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

        //Waypoints
        SpawnLocationAssign(ref waypointTransforms, "/Waypoints");
        waypoints = new WaypointInfo[waypointTransforms.Length];
        for (int i = 0; i < waypointTransforms.Length; i++)
        {
            waypoints[i] = waypointTransforms[i].GetComponent<WaypointInfo>();
        }

        //Waypoint objects are initialised here, so respawning boats can just copy it, instead of having to re-initialise every time
        //The Waypoint class itself is stored in boatControlAI.cs

        allWaypoints = new Waypoint[waypoints.Length + 2];
        for (int i = 0; i < waypoints.Length; i++)
        {
            allWaypoints[i] = new Waypoint(waypoints[i].gameObject);
        }

        //For each element in allWaypoints, checks if any other elements match any of the neighbours in the relevant object
        //If so, copy the information into allWaypoints

        terrainMask = (1 << LayerMask.NameToLayer("terrain"));

        for (int i = 0; i < waypoints.Length; i++)
        {
            for (int j = 0; j < waypoints.Length; j++)
            {
                if (j != i)
                {
                    //for (int k = 0; k < waypoints[i].neighbours.Length; k++)
                    //{
                    //    if (waypoints[i].neighbours[k] == waypoints[j].gameObject)
                    //    {
                    //        allWaypoints[i].neighbours.Add(allWaypoints[j]);

                    //        allWaypoints[i].neighbourAddresses.Add(j);

                    //        allWaypoints[i].neighbourDists.Add
                    //            (Vector3.Distance(waypoints[i].transform.position, waypoints[j].transform.position));

                    //        allWaypoints[i].neighbourNo += 1;
                    //    }
                    //}

                    if (!Physics.SphereCast(allWaypoints[i].globalPos, 5, (allWaypoints[j].globalPos - allWaypoints[i].globalPos).normalized, out rayHit,
                         Vector3.Distance(allWaypoints[i].globalPos, allWaypoints[j].globalPos), terrainMask))
                    {
                        allWaypoints[i].neighbours.Add(allWaypoints[j]);

                        allWaypoints[i].neighbourAddresses.Add(j);

                        allWaypoints[i].neighbourDists.Add
                            (Vector3.Distance(allWaypoints[i].globalPos, allWaypoints[j].globalPos));

                        allWaypoints[i].neighbourNo += 1;

                        Debug.DrawLine(allWaypoints[i].globalPos, allWaypoints[j].globalPos, Color.yellow, Mathf.Infinity);
                    }
                }
            }
        }

        //Debug.Log The entire contents of every Waypoint object
        //for (int i=0;i<allWaypoints.Length;i++)
        //{
        //    if (allWaypoints[i] == null)
        //    {
        //        Debug.Log(null);
        //    }
        //    else
        //    {
        //        Debug.Log(allWaypoints[i].obj);
        //        for (int j = 0; j < allWaypoints[i].neighbours.Count; j++)
        //        {
        //            Debug.Log(allWaypoints[i].neighbours[j].obj);
        //            Debug.Log(allWaypoints[i].neighbourAddresses[j]);
        //            Debug.Log(allWaypoints[i].neighbourDists[j]);
        //        }
        //        Debug.Log(allWaypoints[i].neighbourNo);
        //        Debug.Log(allWaypoints[i].heuristic);
        //        Debug.Log(allWaypoints[i].globalPos);
        //    }
        //}

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
                shipStatuses[0] = new ShipInfo(true, playerTeam, 0, "You"); //class changed when starting buttons pressed, doesn't matter which one is picked here
            }
            else
            {
                shipStatuses[0] = new ShipInfo(false, playerTeam, 0, "CPU 0");
            }

            for (int i=1;i<totalShips;i++)
            {
                shipStatuses[i] = new ShipInfo(false, 0, allowedShips[Random.Range(0, allowedShips.Count)], "CPU " + i);
            }
        }

        //shipStatuses = new ShipInfo[] { new ShipInfo(true, 0, playerStartingClass) };
        //scoresSolo = new int[shipStatuses.Length];
        scoresSorted = new int[shipStatuses.Length];
        if (teams)
        {
            scoresTeam = new int[] { 0, 0 };
        }

        for (int i = 0; i < shipStatuses.Length; i++)
        {
            shipStatuses[i].SetLives(maxLives);
            shipStatuses[i].SetRespawn(0f);
            //scoresSolo[i] = 0;
            scoresSorted[i] = i;
        }

        startCanvas = transform.Find("RoundBeginCanvas").gameObject;
        startTitle = transform.Find("RoundBeginCanvas/sunkText").GetComponent<Text>();
        startDesc = transform.Find("RoundBeginCanvas/respawningText").GetComponent<Text>();

        if (teams) //when conquest/raid modes are added, they are always team-based so don't need this
        {
            startTitle.text = "TEAM ";
        }
        else
        {
            startTitle.text = "";
        }

        switch (mode)
        {
            case 0:
                startTitle.text += "DEATHMATCH";
                if (scoreOrTime)
                {
                    startDesc.text = ("Be the first to sink " + scoreTarget + " ships to win");
                }
                else
                {
                    startDesc.text = ("Sink the most ships within " + (int)timeLeft + " seconds to win");
                }
                break;
        }
    }
    //void Start()
    //{
    //    if (teams)
    //    {
    //    }
    //    else
    //    {
    //        for (int i = 0; i < shipStatuses.Length; i++)
    //        {
    //            SpawnShip(i, spawns0[i], false);
    //        }
    //    }
    //}

    public void StartGame(int startingClass)
    {
        //Debug.Log("Button " + startingClass + " pressed");
        ChangeClass(0, startingClass);

        startCanvas.SetActive(false);
        if (player)
        {
            transform.Find("RoundBeginCamera").GetComponent<Camera>().enabled = false;
        }

        if (teams)
        {
        }
        else
        {
            for (int i = 0; i < shipStatuses.Length; i++)
            {
                SpawnShip(i, spawns0[i], false);
            }
        }
        gameStarted = true;
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
        if (gameStarted)
        {
            if (!scoreOrTime)
            {
                timeLeft -= Time.deltaTime;
                if (timeLeft < 0)
                {
                    timeLeft = 0;
                }
            }

            for (int i = 0; i < shipStatuses.Length; i++)
            {
                if (shipStatuses[i].respawnProgress != 0f)
                {
                    shipStatuses[i].respawnProgress -= Time.deltaTime;
                    if (shipStatuses[i].respawnProgress <= 0f && !(lives && shipStatuses[i].lives == 0))
                    {
                        shipStatuses[i].respawnProgress = 0f;
                        shipStatuses[i].isAlive = true;
                        shipStatuses[i].hasLives = true;
                        switch (shipStatuses[i].team)
                        {
                            case 0:
                                SpawnShip(i, spawns0[Random.Range(0, spawns0.Length)], true);
                                break;
                            case 1:
                                SpawnShip(i, spawns1[Random.Range(0, spawns1.Length)], true);
                                break;
                            case 2:
                                SpawnShip(i, spawns2[Random.Range(0, spawns2.Length)], true);
                                break;
                        }
                    }
                }
            }

            if (PlayerCheatButton)
            {
                shipStatuses[0].score = 999;
                if (!scoreOrTime)
                {
                    timeLeft = 0;
                }
                PlayerCheatButton = false;
            }

            //sort scoresSorted by points of respective shipStatus
            //System not included at the top because that causes every Random to throw compile errors
            System.Array.Sort(scoresSorted, (a, b) => (shipStatuses[b].hasLives, shipStatuses[b].score).CompareTo((shipStatuses[a].hasLives, shipStatuses[a].score)));

            //Check if game needs to end
            if (!gameEnded)
            {
                if ((scoreOrTime && shipStatuses[scoresSorted[0]].score >= scoreTarget) ||
                    (!scoreOrTime && timeLeft <= 0))
                {
                    EndResult(scoresSorted[0]);
                }
            }
        }
    }

    void EndResult(int winner)
    {
        //PLAN: Repurpose pause menu

        if (winner == 0)
        {
            //Debug.Log("YOU WIN");
            GetComponent<RoundManagerMenus>().PauseGameWin(mapId);
        }
        else
        {
            //Debug.Log(shipStatuses[winner].name + " won, you did not.");
            GetComponent<RoundManagerMenus>().PauseGameLoss(PositionGetter(0), shipStatuses[scoresSorted[0]].name);
        }
        
        gameEnded = true;
    }
    int PositionGetter(int id)
    {
        for (int i=0;i<scoresSorted.Length;i++)
        {
            if (scoresSorted[i] == id) 
            { 
                return i;
                //break; 
            }
        }
        return -1;
    }

    void SpawnShip(int id, Transform spawnPos, bool respawning)
    {
        if (shipStatuses[id].isPlayer)
        {
            //temporary
            //ChangeClass(id, playerStartingClass);

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

        instance.GetComponent<boatCombat>().SetTeamStuff(shipStatuses[id].team, id, shipStatuses[id].name, respawning);
        instance.transform.position = spawnPos.position;
        instance.transform.rotation = spawnPos.rotation;
        instance.transform.position += spawnPos.forward * Random.Range(-1f, 1f);
        instance.transform.position += spawnPos.right * Random.Range(-1f, 1f);

    }

    public void KillShip(int id)
    {
        shipStatuses[id].RegisterKill(respawnTime, lives);
    }

    public void ScoreIncSolo(int id)
    {
        //scoresSolo[id] += 1;
        shipStatuses[id].score += 1;
        Debug.Log(shipStatuses[id].name + "'s score increases to " + shipStatuses[id].score);
    }

    public void Killfeed(int killerId, int victimId)
    {
        if (killerId == -1)
        {
            Debug.Log(shipStatuses[victimId].name + " sunk due to their own incompetence.");
        }
        else
        {
            Debug.Log(shipStatuses[killerId].name + " sunk " + shipStatuses[victimId].name);
        }
        
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
    public string name;

    public bool isAlive;
    public int lives;
    public bool hasLives; //specifically for scoreboarding

    public int score;

    public float respawnProgress;
    public ShipInfo(bool isPlayerIn, int teamIn, int shipClassIn, string nameIn)
    {
        isPlayer = isPlayerIn;
        team = teamIn;
        shipClass = shipClassIn;
        name = nameIn;
        isAlive = true;
        score = 0;
        hasLives = true;
    }
    public void SetLives(int livesIn)
    {
        lives = livesIn;
        if (lives > 0)
        {
            hasLives = true;
        }
        else
        {
            hasLives = false; 
        }
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
            if (lives <= 0)
            { 
                lives = 0; 
                hasLives = false;
            }
        }
        respawnProgress = respawnTimeIn;
    }
}
