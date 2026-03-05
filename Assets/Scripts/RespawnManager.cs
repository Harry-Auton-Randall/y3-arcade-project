using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public GameObject cutterP;
    GameObject instance;

    public float respawnTime = 5f;
    public bool lives = false;
    public int maxLives = 1;
    public int ships = 32;
    public ShipInfo[] shipStatuses;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        shipStatuses = new ShipInfo[] { new ShipInfo(true, 0, 1) };
        for (int i = 0; i < shipStatuses.Length; i++)
        {
            shipStatuses[i].SetLives(maxLives);
            shipStatuses[i].SetRespawn(0f);
            SpawnShip(i);
        }
    }

    // Update is called once per frame
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
                    SpawnShip(i);
                }
            }
        }
    }

    void SpawnShip(int id)
    {
        //add ifs/switches to spawn player/AI ships of all classes
        instance = Instantiate(cutterP);
        instance.GetComponent<boatCombat>().SetTeamStuff(shipStatuses[id].team, id);
    }

    public void KillShip(int id)
    {
        shipStatuses[id].RegisterKill(respawnTime, lives);
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
