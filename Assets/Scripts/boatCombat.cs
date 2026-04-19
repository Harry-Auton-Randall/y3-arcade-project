using UnityEngine;
using System.Collections;

public class boatCombat : MonoBehaviour
{
    public enum Classes {Cutter, Brigantine, Frigate, Galleon};
    public Classes shipClass = Classes.Cutter;

    //team/ID stuff
    public bool isPlayer = false;
    public int team = 0;
    public int gameID;
    public RoundManager rMan;

    //stats
    public float speed;
    public float rotate;
    public int maxHealth;
    float maxReload;
    float maxReloadSpecial;

    float reloadMult = 1f;

    //inventory
    public int maxWood;
    public int wood = 0;
    //public int ammoChain = 0;
    //public int ammoFire = 0;
    //public int ammoPierce = 0;
    public int[] ammoTypes = new int[] { 0, 0, 0, 0 }; //standard, chain, fire, piercing

    public float repairTime = 2.5f;
    public float unchainTime;

    public int selectedAmmo = 0;

    //current variables
    public int health;
    public float reloadL, reloadR;
    public float reloadSpecial;
    public Vector2 aimPos;

    public bool onFire = false;
    public bool chained = false;

    //reticle
    public float reloadProgress;

    //cannons
    GameObject[] cannonsL, cannonsR;
    public Vector2 cannonCentreL = Vector2.zero;
    public Vector2 cannonCentreR = Vector2.zero;

    public Vector2 cannonCentreL2 = Vector2.zero; //frigates and galleons
    public Vector2 cannonCentreR2 = Vector2.zero;
    float cannonXDist;

    Renderer cannonRangeL, cannonRangeR, cannonRangeL2, cannonRangeR2;//Cutter uses cannonRangeR, Brigantine uses all 4
    public Material cannonRangeMat, cannonRangeMatEmpty;
    Transform cannonRangeBoneL, cannonRangeBoneR, cannonRangeBoneL2, cannonRangeBoneR2;//for Frigates and Galleons
    public bool goodAimL, goodAimR;
    Vector3 cannonRangeDefaultRot; //for frigates and galleons, because bones imported from Blender have weird rotations

    public float volleyFireRate;
    int[] volleyFireOrderL, volleyFireOrderR;
    public int volleying = 0;

    //repairing
    public float repairProgress;
    public bool isRepairing;

    //fire
    float fireDuration;
    int fireDamage;
    public float fireDamageInterval = 5;
    public int fireDamageTotal = 3;

    //deathmatch scoring
    //these are the IDs of the most recent enemy to damage/ignite this ship
    int latestDamageID = -1;
    int latestFireID = -1;

    Collider hullCollider;

    boatMove bm;

    Rigidbody rb;

    //respawn-immunity stuff
    LayerMask boatMask;
    int boatCollisions;
    float respawnTime;
    public bool respawnImmunity;
    int fixSinceLastUp;

    void Awake()
    {
        volleying = 0;

        bm = GetComponent<boatMove>();
        rb = GetComponent<Rigidbody>();
        rMan = GameObject.Find("RoundManager").GetComponent<RoundManager>();

        boatMask = (1 << LayerMask.NameToLayer("boat")) | (1 << LayerMask.NameToLayer("boatRam"));

        //Set stats and cannons based on class
        switch (shipClass)
        {
            case Classes.Cutter:
                speed = 12f;
                rotate = 90f;
                maxHealth = 10;
                maxReload = 3f;
                maxReloadSpecial = 15f;
                unchainTime = repairTime;

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
                unchainTime = repairTime * (4f/3f);

                cannonsL = new GameObject[] { transform.Find("cannonL1").gameObject,
                                              transform.Find("cannonL2").gameObject };
                cannonsR = new GameObject[] { transform.Find("cannonR1").gameObject,
                                              transform.Find("cannonR2").gameObject };
                cannonCentreL = Vector2.zero;
                cannonCentreR = Vector2.zero;
                cannonRangeL = cannonsL[0].transform.Find("range").GetComponent<Renderer>();
                cannonRangeL2 = cannonsL[1].transform.Find("range").GetComponent<Renderer>();
                cannonRangeR = cannonsR[0].transform.Find("range").GetComponent<Renderer>();
                cannonRangeR2 = cannonsR[1].transform.Find("range").GetComponent<Renderer>();

                volleyFireRate = 0.125f;
                volleyFireOrderL = new int[] { 0, 1 };
                break;

            case Classes.Frigate:
                speed = 6.5f;
                rotate = 50f;
                maxHealth = 20;
                maxReload = 5f;
                maxReloadSpecial = 25f;
                unchainTime = repairTime * (5f/3f);

                cannonsL = new GameObject[] { transform.Find("cannonL1").gameObject,
                                              transform.Find("cannonL2").gameObject,
                                              transform.Find("cannonL3").gameObject,
                                              transform.Find("cannonL4").gameObject };
                cannonsR = new GameObject[] { transform.Find("cannonR1").gameObject,
                                              transform.Find("cannonR2").gameObject,
                                              transform.Find("cannonR3").gameObject,
                                              transform.Find("cannonR4").gameObject };
                cannonCentreL = new Vector2(0.5f, 0);
                cannonCentreR = new Vector2(-0.5f, 0);
                cannonCentreL2 = new Vector2(-1, 0);
                cannonCentreR2 = new Vector2(1, 0);

                cannonRangeL = transform.Find("cannonRangeL/mesh").GetComponent<Renderer>();
                cannonRangeR = transform.Find("cannonRangeR/mesh").GetComponent<Renderer>();

                cannonRangeBoneL = transform.Find("cannonRangeL/Armature/baseBone/rotateBone");
                cannonRangeBoneL2 = transform.Find("cannonRangeL/Armature/baseBone/rotateBone/tipBone");
                cannonRangeBoneR = transform.Find("cannonRangeR/Armature/baseBone/rotateBone");
                cannonRangeBoneR2 = transform.Find("cannonRangeR/Armature/baseBone/rotateBone/tipBone");

                volleyFireRate = 0.08f;
                volleyFireOrderL = new int[] { 0, 1, 2, 3 };
                break;

            case Classes.Galleon:
                speed = 5f;
                rotate = 35f;
                maxHealth = 30;
                maxReload = 6f;
                maxReloadSpecial = 30f;
                unchainTime = repairTime * 2;

                cannonsL = new GameObject[] { transform.Find("cannonL1").gameObject,
                                              transform.Find("cannonL2").gameObject,
                                              transform.Find("cannonL3").gameObject,
                                              transform.Find("cannonL4").gameObject,
                                              transform.Find("cannonL5").gameObject,
                                              transform.Find("cannonL6").gameObject };
                cannonsR = new GameObject[] { transform.Find("cannonR1").gameObject,
                                              transform.Find("cannonR2").gameObject,
                                              transform.Find("cannonR3").gameObject,
                                              transform.Find("cannonR4").gameObject,
                                              transform.Find("cannonR5").gameObject,
                                              transform.Find("cannonR6").gameObject };
                cannonCentreL = new Vector2(0.625f, 0);
                cannonCentreR = new Vector2(-0.625f, 0);
                cannonCentreL2 = new Vector2(-1.25f, 0);
                cannonCentreR2 = new Vector2(1.25f, 0);

                cannonRangeL = transform.Find("cannonRangeL/mesh").GetComponent<Renderer>();
                cannonRangeR = transform.Find("cannonRangeR/mesh").GetComponent<Renderer>();

                cannonRangeBoneL = transform.Find("cannonRangeL/Armature/baseBone/rotateBone");
                cannonRangeBoneL2 = transform.Find("cannonRangeL/Armature/baseBone/rotateBone/tipBone");
                cannonRangeBoneR = transform.Find("cannonRangeR/Armature/baseBone/rotateBone");
                cannonRangeBoneR2 = transform.Find("cannonRangeR/Armature/baseBone/rotateBone/tipBone");

                volleyFireRate = 0.06f;
                volleyFireOrderL = new int[] { 0, 1, 2, 3, 4, 5 };
                break;
        }
        if (shipClass != Classes.Cutter)
        {
            cannonXDist = cannonsR[0].transform.localPosition.x;
            volleyFireOrderR = new int[volleyFireOrderL.Length];
            for (int i=0;i<volleyFireOrderL.Length;i++)
            {
                volleyFireOrderR[i] = volleyFireOrderL[i];
            }
        }

        if (shipClass == Classes.Frigate || shipClass == Classes.Galleon)
        {
            cannonRangeDefaultRot = cannonRangeBoneL.localEulerAngles;
        }

        maxReload = maxReload / reloadMult;

        //Set stats based on class-specific stats
        health = maxHealth;
        maxWood = (int)(maxHealth * 0.4f);

        reloadL = maxReload;
        reloadR = maxReload;
        reloadSpecial = maxReloadSpecial;

        hullCollider = transform.Find("hull").GetComponent<Collider>();

        //SetTeamStuff(team); //TEMPORARY - base team stuff off of scene-specified settings, need to change eventually
    }

    public void SetTeamStuff(int teamIn, int gameIDIn, bool respawning)
    {
        gameID = gameIDIn;
        team = teamIn;
        if(isPlayer || (team == rMan.playerTeam && team != 0))
        {
            cannonRangeMat = Resources.Load("SolidMaterials/white30", typeof(Material)) as Material;
            cannonRangeMatEmpty = Resources.Load("SolidMaterials/white5", typeof(Material)) as Material;
        }
        else
        {
            cannonRangeMat = Resources.Load("SolidMaterials/red60", typeof(Material)) as Material;
            cannonRangeMatEmpty = Resources.Load("SolidMaterials/red10", typeof(Material)) as Material;
        }

        if(respawning)
        {
            respawnTime = 0;
            rb.excludeLayers = boatMask;
            respawnImmunity = true;
            transform.Find("hull").GetComponent<Renderer>().enabled = false;
        }
    }

    public void TakeDamage(int damage, bool fire, bool chain, int shooterIDIn)
    {
        health -= damage;

        //Updates latestDamageID and burnID if the damager isn't part of the environment (-1), self-harm (gameID) or a teammate (team stuff)
        if (shooterIDIn != -1 && shooterIDIn != gameID &&
            (rMan.shipStatuses[shooterIDIn].team != team || rMan.shipStatuses[shooterIDIn].team == 0))
        {
            latestDamageID = shooterIDIn;
            if (fire)
            {
                latestFireID = shooterIDIn;
            }
        }
        if (health <= 0)
        {
            health = 0;
            Sink();
        }
        if (fire)
        {
            onFire = true;
            fireDuration = 0;
            fireDamage = 0;
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
            rMan.ChangeClass(gameID, rMan.allowedShips[Random.Range(0, rMan.allowedShips.Count)]);
        }
        rMan.KillShip(gameID);
        rMan.Killfeed(latestDamageID, gameID);

        //If game mode is deathmatch, give a kill to whoever last damaged you
        if (rMan.mode == 0)
        {
            if (latestDamageID != -1) //if this is still -1, literally no enemy ever touched you
            {
                rMan.ScoreIncSolo(latestDamageID);
                if (rMan.teams)
                {
                    //increase correct team's score
                }
            }
        }
        Destroy(this.gameObject);
    }

    public void AttemptRepair(float n)
    {
        if (n > 0 && ((health < maxHealth && wood > 0) || onFire || chained))
        {
            //NOTE: order of priority is: dousing fires, unchaining, normal repairs
            isRepairing = true;
            if (onFire)
            {
                repairProgress += (100 * (Time.deltaTime / repairTime));
            }
            else if (chained)
            {
                repairProgress += (100 * (Time.deltaTime / unchainTime));
            }
            else
            {
                repairProgress += (100 * (Time.deltaTime / repairTime));
            }

            if (repairProgress >= 100)
            {
                repairProgress -= 100;
                if (onFire)
                {
                    onFire = false;
                }
                else if (chained)
                {
                    chained = false;
                }
                else
                {
                    health += 1;
                    wood -= 1;
                }
            }
        }
        else
        {
            isRepairing = false;
            repairProgress = 0;
        }
    }

    public void AddResource(int type, int amount)
    {
        int resourceAdded = amount;

        if (type == 0) //wood
        {
            if (amount + wood > maxWood)
            {
                resourceAdded = maxWood - wood;
            }
            wood += resourceAdded;
        }
        else //not wood
        {
            ammoTypes[type] += resourceAdded;
        }

        //(make a popup showing what was gained and how much here)
    }

    public void SwitchAmmo(int direction) //1 or -1
    {
        selectedAmmo += direction;

        //keep between 0 and 3
        if(selectedAmmo > 3)
        {
            selectedAmmo = 0;
        }
        else if(selectedAmmo < 0)
        {
            selectedAmmo = 3;
        }

        //Uses recursion to prevent switching to ammo types with 0 ammo
        if (selectedAmmo != 0)
        {
            if (ammoTypes[selectedAmmo] <= 0)
            {
                this.SwitchAmmo(direction);
            }
        }
    }

    public void SelectAmmo(int type)
    {
        if (type != 0)
        {
            if (ammoTypes[type] > 0)
            {
                selectedAmmo = type;
            }
        }
        else
        {
            selectedAmmo = type;
        }
    }

    void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.tag == "boatTrigger")
        {
            boatCollisions += 1;
        }
    }

    //Keeps track of how many FixedUpdates have run before Update
    //Also resets boatCollisions. Because FixedUpdate runs before OnTrigger stuff, boatCollisions only counts collisions on the final physics-frame before Update
    void FixedUpdate()
    {
        boatCollisions = 0;
        fixSinceLastUp++;
    }

    //speed and rotate initialised in this script and written to boatMove, so each ship can use the same boatMove script
    void Start()
    {
        bm.SetStats(speed, rotate);
    }

    void Update()
    {
        if (respawnImmunity && (respawnTime < rMan.spawnImmunityTime))
        {
            respawnTime += Time.deltaTime;
        }

        //checks if respawnImmunity is enabled, and if it needs disabling
        //Because this happens in Update, which occurs after OnTrigger stuff, collision info will be up-to-date, and not x frames behind
        if (fixSinceLastUp > 0)
        { 
            if (respawnImmunity && (respawnTime >= rMan.spawnImmunityTime) && (boatCollisions == 0))
            {
                rb.excludeLayers = 0;
                respawnImmunity = false;
                transform.Find("hull").GetComponent<Renderer>().enabled = true; //TEMPORARY
            }
        }
        fixSinceLastUp = 0;

        //on fire - deals 1 damage every (fireDamageInterval) seconds
        //checks that 1. enough time has passed and 2. it hasn't already dealt damage for that time period (fireDamage)
        if (onFire)
        {
            fireDuration += Time.deltaTime;
            for (int i=1; i<fireDamageTotal; i++)
            {
                if (fireDuration >= fireDamageInterval * i && fireDamage == i-1)
                {
                    TakeDamage(1, false, false, latestFireID);
                    fireDamage += 1;
                }
            }
            if (fireDuration >= fireDamageInterval * fireDamageTotal && fireDamage == fireDamageTotal-1)
            {
                TakeDamage(1, false, false, latestFireID);
                onFire = false;
            }
        }

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
            if ((aimPos.y - cannonCentreR.y < -1 * (aimPos.x - cannonCentreR.x))
             && (aimPos.y - cannonCentreR.y < aimPos.x - cannonCentreR.x))
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
        else
        {
            //Determines which broadside (if any) is valid, determines goodAim values + reloadProgress
            if ((aimPos.y - cannonCentreL.y < -1 * (aimPos.x - cannonCentreL.x))
             && (aimPos.y - cannonCentreL.y > aimPos.x - cannonCentreL.x)
             && (aimPos.x <= -1 * cannonXDist))
            {
                goodAimL = true;
                goodAimR = false;
                reloadProgress = 100 * (reloadL / maxReload);
            }
            else if ((aimPos.y - cannonCentreR.y > -1 * (aimPos.x - cannonCentreR.x))
                  && (aimPos.y - cannonCentreR.y < aimPos.x - cannonCentreR.x)
                  && (aimPos.x >= cannonXDist))
            {
                goodAimR = true;
                goodAimL = false;
                reloadProgress = 100 * (reloadR / maxReload);
            }
            else
            {
                goodAimR = false;
                goodAimL = false;
                reloadProgress = 0;
            }
            
            //Sorts out cannonRanges / rotations based on goodAim values
            //Left first
            if (goodAimL)
            {
                cannonRangeL.enabled = true;
                if (shipClass == Classes.Brigantine)
                {
                    cannonRangeL2.enabled = true;
                    for (int i = 0; i < cannonsL.Length; i++)
                    {
                        cannonsL[i].transform.localRotation = Quaternion.Euler(0,
                            Mathf.Clamp(Mathf.Atan2(aimPos.x - cannonsL[i].transform.localPosition.x, aimPos.y - cannonsL[i].transform.localPosition.z) * Mathf.Rad2Deg, -135f, -45f),
                            0);
                    }
                }
                else
                {
                    cannonRangeDefaultRot.y = Mathf.Clamp(Mathf.Atan2(aimPos.x - cannonCentreL2.x, aimPos.y - cannonCentreL2.y) * Mathf.Rad2Deg, -135f, -45f) - 90;
                    cannonRangeBoneL.localRotation = Quaternion.Euler(cannonRangeDefaultRot);
                    cannonRangeBoneL2.localRotation = Quaternion.Euler(0, 0, -1 * (cannonRangeDefaultRot.y + 180));
                    for (int i = 0; i < cannonsL.Length; i++)
                    {
                        cannonsL[i].transform.localRotation = Quaternion.Euler(0, cannonRangeDefaultRot.y + 90, 0);
                    }
                }
            }
            else
            {
                cannonRangeL.enabled = false;
                if (shipClass == Classes.Brigantine)
                {
                    cannonRangeL2.enabled = false;
                }
                for (int i = 0; i < cannonsL.Length; i++)
                {
                    cannonsL[i].transform.localRotation = Quaternion.Euler(0,270,0);
                }
            }

            //Right second
            if (goodAimR)
            {
                cannonRangeR.enabled = true;
                if (shipClass == Classes.Brigantine)
                {
                    cannonRangeR2.enabled = true;
                    for (int i = 0; i < cannonsR.Length; i++)
                    {
                        cannonsR[i].transform.localRotation = Quaternion.Euler(0,
                            Mathf.Clamp(Mathf.Atan2(aimPos.x - cannonsR[i].transform.localPosition.x, aimPos.y - cannonsR[i].transform.localPosition.z) * Mathf.Rad2Deg, 45f, 135f),
                            0);
                    }
                }
                else
                {
                    cannonRangeDefaultRot.y = Mathf.Clamp(Mathf.Atan2(aimPos.x - cannonCentreR2.x, aimPos.y - cannonCentreR2.y) * Mathf.Rad2Deg, 45f, 135f) + 90;
                    cannonRangeBoneR.localRotation = Quaternion.Euler(cannonRangeDefaultRot);
                    cannonRangeBoneR2.localRotation = Quaternion.Euler(0, 0, -1 * (cannonRangeDefaultRot.y + 180));
                    for (int i = 0; i < cannonsR.Length; i++)
                    {
                        cannonsR[i].transform.localRotation = Quaternion.Euler(0, cannonRangeDefaultRot.y - 90, 0);
                    }
                }
            }
            else
            {
                cannonRangeR.enabled = false;
                if (shipClass == Classes.Brigantine)
                {
                    cannonRangeR2.enabled = false;
                }
                for (int i = 0; i < cannonsR.Length; i++)
                {
                    cannonsR[i].transform.localRotation = Quaternion.Euler(0, 90, 0);
                }
            }
        }

        //sort out cannon range colours
        //cannonRangeL and L2
        if (shipClass != Classes.Cutter)
        {
            if (reloadL >= maxReload)
            {
                cannonRangeL.material = cannonRangeMat;
                if (shipClass == Classes.Brigantine)
                {
                    cannonRangeL2.material = cannonRangeMat;
                }
            }
            else
            {
                cannonRangeL.material = cannonRangeMatEmpty;
                if (shipClass == Classes.Brigantine)
                {
                    cannonRangeL2.material = cannonRangeMatEmpty;
                }
            }
        }
        //cannonRangeR and R2
        if (reloadR >= maxReload)
        {
            cannonRangeR.material = cannonRangeMat;
            if (shipClass == Classes.Brigantine)
            {
                cannonRangeR2.material = cannonRangeMat;
            }
        }
        else
        {
            cannonRangeR.material = cannonRangeMatEmpty;
            if (shipClass == Classes.Brigantine)
            {
                cannonRangeR2.material = cannonRangeMatEmpty;
            }
        }


    }
    public void Shoot()
    {
        if (shipClass == Classes.Cutter)
        {
            if (goodAimR && (reloadR >= maxReload))
            {
                if(isPlayer || (team == rMan.playerTeam && team != 0))
                {
                    cannonsR[0].GetComponent<cannonShoot>().Shoot(40, 0.75f, true, selectedAmmo, hullCollider, gameID);
                }
                else
                {
                    cannonsR[0].GetComponent<cannonShoot>().Shoot(40, 0.75f, false, selectedAmmo, hullCollider, gameID);
                }

                if (selectedAmmo != 0)
                {
                    ammoTypes[selectedAmmo] -= 1;
                    if (ammoTypes[selectedAmmo] <= 0)
                    {
                        ammoTypes[selectedAmmo] = 0;
                        selectedAmmo = 0;
                    }
                }

                reloadR = 0;
            }
        }
        else
        {
            if (goodAimL && (reloadL >= maxReload))
            {
                if(isPlayer || (team == rMan.playerTeam && team != 0))
                {
                    StartCoroutine(VolleyFire(volleyFireOrderL, cannonsL, true, selectedAmmo));
                    volleying++;
                }
                else
                {
                    StartCoroutine(VolleyFire(volleyFireOrderL, cannonsL, false, selectedAmmo));
                    volleying++;
                }

                if (selectedAmmo != 0)
                {
                    ammoTypes[selectedAmmo] -= 1;
                    if (ammoTypes[selectedAmmo] <= 0)
                    {
                        ammoTypes[selectedAmmo] = 0;
                        selectedAmmo = 0;
                    }
                }

                reloadL = 0;
            }
            else if (goodAimR && (reloadR >= maxReload))
            {
                if(isPlayer || (team == rMan.playerTeam && team != 0))
                {
                    StartCoroutine(VolleyFire(volleyFireOrderR, cannonsR, true, selectedAmmo));
                    volleying++;
                }
                else
                {
                    StartCoroutine(VolleyFire(volleyFireOrderR, cannonsR, false, selectedAmmo));
                    volleying++;
                }

                if (selectedAmmo != 0)
                {
                    ammoTypes[selectedAmmo] -= 1;
                    if (ammoTypes[selectedAmmo] <= 0)
                    {
                        ammoTypes[selectedAmmo] = 0;
                        selectedAmmo = 0;
                    }
                }

                reloadR = 0;
            }
        }
    }

    IEnumerator VolleyFire(int[] volleyOrder, GameObject[] broadside, bool isPlayer1, int selectedAmmo1)
    {
        //shuffle volleyFireOrder
        int temp;
        int position;
        for (int i=0;i< volleyOrder.Length;i++)
        {
            position = Random.Range(i, volleyOrder.Length);
            temp = volleyOrder[i];
            volleyOrder[i] = volleyOrder[position];
            volleyOrder[position] = temp;
        }

        //fire broadside in order of volleyFireOrder
        for (int i = 0; i < volleyOrder.Length; i++)
        {
            broadside[volleyOrder[i]].GetComponent<cannonShoot>().Shoot(40, 0.75f, isPlayer1, selectedAmmo1, hullCollider, gameID);
            yield return new WaitForSeconds(volleyFireRate);
        }
        volleying--;
    }
}
