using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public GameObject cutterP, brigantineP, frigateP, galleonP;
    public GameObject cutterA, brigantineA, frigateA, galleonA;


    GameObject instance;

    DeathScreen ds;

    public float respawnTime = 10f;
    public bool lives = false;
    public int maxLives = 1;
    public int ships = 32;
    public ShipInfo[] shipStatuses;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        ds = GameObject.Find("/deathScreen").GetComponent<DeathScreen>();
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
        if (shipStatuses[id].isPlayer)
        {
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
