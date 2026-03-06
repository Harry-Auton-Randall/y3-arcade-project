using UnityEngine;

public class boatCombat : MonoBehaviour
{
    public enum Classes {Cutter, Brigantine, Frigate, Galleon};
    public Classes shipClass = Classes.Cutter;

    //team/ID stuff
    public bool isPlayer = false;
    public int team = 0;
    public int gameID;
    RespawnManager rMan;

    //stats
    public float speed;
    public float rotate;
    public int maxHealth;
    float maxReload;
    float maxReloadSpecial;

    //inventory
    public int maxWood;
    public int wood = 0;
    public int ammoChain = 0;
    public int ammoFire = 0;
    public int ammoPierce = 0;

    public float repairTime = 2.5f;
    public float unchainTime;

    //current variables
    int health;
    public float reloadL, reloadR;
    public float reloadSpecial;
    public Vector2 aimPos;

    bool onFire = false;
    bool chained = false;

    //reticle
    public float reloadProgress;

    //cannons
    GameObject[] cannonsL, cannonsR;
    Vector2 cannonCentreL = Vector2.zero;
    Vector2 cannonCentreR = Vector2.zero;

    Renderer cannonRangeL, cannonRangeR, cannonRangeL2, cannonRangeR2;//Cutter uses cannonRangeR, Brigantine uses all 4
    public Material cannonRangeMat, cannonRangeMatEmpty;
    Transform cannonRangeTipL, cannonRangeTipR;//for Frigates and Galleons
    bool goodAimL, goodAimR;

    Collider hullCollider;

    boatMove bm;

    void Awake()
    {
        bm = GetComponent<boatMove>();
        rMan = GameObject.Find("RoundManager").GetComponent<RespawnManager>();

        //Set stats and cannons based on class
        switch (shipClass)
        {
            case Classes.Cutter:
                speed = 12f;
                rotate = 90f;
                maxHealth = 10;
                maxReload = 3f;
                maxReloadSpecial = 15f;

                cannonsR = new GameObject[] { transform.Find("cannon").gameObject };
                cannonCentreR.x = cannonsR[0].transform.localPosition.x;
                cannonCentreR.y = cannonsR[0].transform.localPosition.z;
                cannonRangeR = cannonsR[0].transform.Find("range").GetComponent<Renderer>();

                break;

            case Classes.Brigantine:
                speed = 9f;
                rotate = 75f;
                maxHealth = 15;
                maxReload = 4f;
                maxReloadSpecial = 20f;

                //set cannons here
                cannonCentreL = Vector3.zero;
                cannonCentreR = Vector3.zero;
                //set cannon ranges here
                break;

            case Classes.Frigate:
                speed = 6.5f;
                rotate = 50f;
                maxHealth = 20;
                maxReload = 5f;
                maxReloadSpecial = 25f;

                //set cannons here
                //set cannon centres here
                //set cannon ranges here
                break;

            case Classes.Galleon:
                speed = 5f;
                rotate = 35f;
                maxHealth = 30;
                maxReload = 6f;
                maxReloadSpecial = 30f;

                //set cannons here
                //set cannon centres here
                //set cannon ranges here
                break;
        }

        //Set stats based on class-specific stats
        health = maxHealth;
        maxWood = (int)(maxHealth * 0.4f);

        reloadL = maxReload;
        reloadR = maxReload;
        reloadSpecial = maxReloadSpecial;

        hullCollider = transform.Find("hull").GetComponent<Collider>();

        //SetTeamStuff(team); //TEMPORARY - base team stuff off of scene-specified settings, need to change eventually
    }

    public void SetTeamStuff(int teamIn, int gameIDIn)
    {
        gameID = gameIDIn;
        team = teamIn;
        //if(isPlayer || (team == playerTeam && team != 0))
        if (isPlayer)
        {
            cannonRangeMat = Resources.Load("SolidMaterials/white30", typeof(Material)) as Material;
            cannonRangeMatEmpty = Resources.Load("SolidMaterials/white5", typeof(Material)) as Material;
        }
        else
        {
            cannonRangeMat = Resources.Load("SolidMaterials/red60", typeof(Material)) as Material;
            cannonRangeMatEmpty = Resources.Load("SolidMaterials/red10", typeof(Material)) as Material;
        }
    }

    public void TakeDamage(int damage, bool fire, bool chain)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            Sink();
        }
        if (fire)
        {
            onFire = true;
        }
        if (chain)
        {
            chained = true;
        }
    }

    void Sink()
    {
        if (isPlayer)
        {
            GameObject.Find("/deathScreen").GetComponent<DeathScreen>().Enable(gameID, transform.Find("CameraBase/CameraMove/CameraRot/PlayerCamera"));
        }
        else
        {
            GameObject.Find("/RoundManager").GetComponent<RespawnManager>().ChangeClass(gameID, Random.Range(0, 4));
        }
        GameObject.Find("RoundManager").GetComponent<RespawnManager>().KillShip(gameID);
        Destroy(this.gameObject);
    }

    //speed and rotate initialised in this script and written to boatMove, so each ship can use the same boatMove script
    void Start()
    {
        bm.SetStats(speed, rotate);
    }

    void Update()
    {
        //Increase reload values
        if (shipClass != Classes.Cutter)
        {
            if (reloadL < maxReload)
            {
                reloadL += Time.deltaTime;
            }
            if (reloadL > maxReload)
            {
                reloadL = maxReload;
            }
        }

        if (reloadR < maxReload)
        {
            reloadR += Time.deltaTime;
        }
        if (reloadR > maxReload)
        {
            reloadR = maxReload;
        }

        if (reloadSpecial < maxReloadSpecial)
        {
            reloadSpecial += Time.deltaTime;
        }
        if (reloadSpecial > maxReloadSpecial)
        {
            reloadSpecial = Time.deltaTime;
        }

        //Sort out valid cannon aiming
        if (shipClass == Classes.Cutter)
        {
            if ((aimPos.y - cannonCentreR.y < -1 * (aimPos.x - cannonCentreR.x)) && (aimPos.y - cannonCentreR.y < aimPos.x - cannonCentreR.x))
            {
                cannonRangeR.enabled = true;
                goodAimR = true;
                cannonsR[0].transform.localRotation = Quaternion.Euler(0, Mathf.Atan2(aimPos.x - cannonCentreR.x, aimPos.y - cannonCentreR.y) * Mathf.Rad2Deg, 0);
                reloadProgress = 100 * (reloadR / maxReload);
            }
            else
            {
                cannonRangeR.enabled = false;
                goodAimR = false;
                cannonsR[0].transform.localRotation = Quaternion.Euler(0, 180, 0);
                reloadProgress = 0;
            }
        }

        //sort out cannon range colours
        if (shipClass != Classes.Cutter)
        {
            //cannonRangeL
        }
        if (shipClass == Classes.Brigantine)
        {
            //cannonRangeL2 and R2
        }
        //cannonRangeR
        if (reloadR >= maxReload)
        {
            cannonRangeR.material = cannonRangeMat;
        }
        else
        {
            cannonRangeR.material = cannonRangeMatEmpty;
        }


    }
    public void Shoot()
    {
        if (shipClass == Classes.Cutter)
        {
            if (goodAimR && (reloadR >= maxReload))
            {
                //if(isPlayer || (team == playerTeam && team != 0))
                if (isPlayer)
                {
                    cannonsR[0].GetComponent<cannonShoot>().Shoot(40, 0.75f, true, 0, hullCollider);
                }
                else
                {
                    cannonsR[0].GetComponent<cannonShoot>().Shoot(40, 0.75f, false, 0, hullCollider);
                }
                reloadR = 0;
            }
        }
    }
}
