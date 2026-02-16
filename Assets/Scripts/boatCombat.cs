using UnityEngine;

public class boatCombat : MonoBehaviour
{
    public enum Classes {Cutter, Brigantine, Frigate, Galleon};
    public Classes shipClass = Classes.Cutter;

    //team stuff
    public bool isPlayer = false;
    public int team = 0;

    //stats
    public float speed;
    public float rotate;
    public int maxHealth;
    public float maxReload;
    public float maxReloadSpecial;

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
    float reloadL, reloadR;
    float reloadSpecial;
    public Vector2 aimPos;

    //cannons
    GameObject[] cannonsL, cannonsR;
    Vector2 cannonCentreL = Vector2.zero;
    Vector2 cannonCentreR = Vector2.zero;

    Renderer cannonRangeL, cannonRangeR, cannonRangeL2, cannonRangeR2;//Cutter uses cannonRangeR, Brigantine uses all 4
    Transform cannonRangeTipL, cannonRangeTipR;//for Frigates and Galleons
    bool goodAimL, goodAimR;

    boatMove bm;

    void Awake()
    {
        bm = GetComponent<boatMove>();

        //Set stats and cannons based on class
        switch (shipClass)
        {
            case Classes.Cutter:
                speed = 8f;
                rotate = 90f;
                maxHealth = 20;
                maxReload = 3f;
                maxReloadSpecial = 15f;

                cannonsR = new GameObject[] { transform.Find("cannon").gameObject };
                cannonCentreR.x = cannonsR[0].transform.localPosition.x;
                cannonCentreR.y = cannonsR[0].transform.localPosition.z;
                cannonRangeR = cannonsR[0].transform.Find("range").GetComponent<Renderer>();

                break;

            case Classes.Brigantine:
                speed = 6f;
                rotate = 75f;
                maxHealth = 30;
                maxReload = 4f;
                maxReloadSpecial = 20f;

                //set cannons here
                cannonCentreL = Vector3.zero;
                cannonCentreR = Vector3.zero;
                break;

            case Classes.Frigate:
                speed = 4.5f;
                rotate = 50f;
                maxHealth = 40;
                maxReload = 5f;
                maxReloadSpecial = 25f;

                //set cannons here
                //set cannon centres here
                break;

            case Classes.Galleon:
                speed = 3.5f;
                rotate = 35f;
                maxHealth = 60;
                maxReload = 6f;
                maxReloadSpecial = 30f;

                //set cannons here
                //set cannon centres here
                break;
        }

        //Set stats based on class-specific stats
        health = maxHealth;
        maxWood = (int)(maxHealth * 0.4f);

        reloadL = maxReload;
        reloadR = maxReload;
        reloadSpecial = maxReloadSpecial;
    }

    //speed and rotate initialised in this script and written to boatMove, so each ship can use the same boatMove script
    void Start()
    {
        bm.SetStats(speed, rotate);
    }

    void Update()
    {
        if (shipClass == Classes.Cutter)
        {
            if ((aimPos.y - cannonCentreR.y < -1 * (aimPos.x - cannonCentreR.x)) && (aimPos.y - cannonCentreR.y < aimPos.x - cannonCentreR.x))
            {
                cannonRangeR.enabled = true;
                goodAimR = true;
                cannonsR[0].transform.localRotation = Quaternion.Euler(0, Mathf.Atan2(aimPos.x - cannonCentreR.x, aimPos.y - cannonCentreR.y) * Mathf.Rad2Deg, 0);
            }
            else
            {
                cannonRangeR.enabled = false;
                goodAimR = false;
                cannonsR[0].transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }
    public void Shoot()
    {
        if (shipClass == Classes.Cutter)
        {
            if (goodAimR)
            {
                //if(isPlayer || (team == playerTeam && team != 0))
                if (isPlayer)
                {
                    cannonsR[0].GetComponent<cannonShoot>().Shoot(40, 0.75f, true, 0);
                }
                else
                {
                    cannonsR[0].GetComponent<cannonShoot>().Shoot(40, 0.75f, false, 0);
                }
                
            }
        }
    }
}
