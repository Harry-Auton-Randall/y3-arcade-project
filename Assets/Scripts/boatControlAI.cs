using UnityEngine;
using UnityEngine.UI;

public class boatControlAI : MonoBehaviour
{
    Transform reticle;
    Slider reticleCircle;

    boatMove bm;
    boatCombat bc;

    Vector3[] objectiveWaypoints;
    Collider[] nearbyPois;
    PoiInfo[] poiInfos;
    int colliderToPoi;
    float poiRadius = 150f;
    int nearbyPoiCount;

    int highestPriorityPoi;
    float closestPoi;

    int targetPoi;
    Vector3 targetWaypoint, targetWaypointLocal;
    float targetWaypointAngle;

    GameObject poiBoat;
    boatCombat poiBC;

    LayerMask poiMask;

    float moveIn, steerIn;

    void Awake()
    {
        poiMask = (1 << LayerMask.NameToLayer("boat"));
        bm = GetComponent<boatMove>();
        bc = GetComponent<boatCombat>();
    }
    void Start()
    {
        //sets objectiveWaypoints based on mode
        if (bc.rMan.mode == 0)
        {
            objectiveWaypoints = new Vector3[] { new Vector3(200, 0, 0) }; //TEMPORARY
        }

        //initialises nearbyPOIs and poiInfo
        nearbyPois = new Collider[bc.rMan.totalShips];
        poiInfos = new PoiInfo[nearbyPois.Length + objectiveWaypoints.Length];
        for (int i=0;i<poiInfos.Length;i++)
        {
            poiInfos[i] = new PoiInfo(poiRadius);
        }
    }

    void Update()
    {
        //FINDING NEARBY POIS AND SORTING THEIR INFO OUT
        
        //finds every boat within poiRadius
        for (int i=0;i<nearbyPois.Length;i++)
        {
            nearbyPois[i] = default;
        }
        nearbyPoiCount = Physics.OverlapSphereNonAlloc(this.transform.position, poiRadius, nearbyPois, poiMask);

        //determines if each boat found is a valid POI, adds to poiInfos if so
        colliderToPoi = 0;
        for (int i=0;i<nearbyPoiCount;i++)
        {
            poiBoat = nearbyPois[i].transform.parent.gameObject;

            //check if it's detecting its own collider
            if (poiBoat != this.gameObject)
            {
                poiBC = poiBoat.GetComponent<boatCombat>();

                //Currently only supports enemyShip POIs
                if ((bc.team != poiBC.team) || (bc.team == 0))
                {
                    poiInfos[colliderToPoi].Set(PoiInfo.Types.EnemyShip, poiBoat.transform.position, this.transform.position);
                    colliderToPoi++;
                }
            }
        }
        //adds objective POIs to poiInfos
        for (int i=0;i<objectiveWaypoints.Length;i++)
        {
            poiInfos[colliderToPoi].Set(PoiInfo.Types.Objective, objectiveWaypoints[i], this.transform.position);
            colliderToPoi++;
        }

        //disable all remaining PoiInfos
        for (int i = colliderToPoi; i<poiInfos.Length;i++)
        {
            poiInfos[i].Reset();
        }

        //NOTE: colliderToPoi now stores the number of poiInfos slots in use

        //TEMPORARY: log everything
        //for(int i=0;i<poiInfos.Length;i++)
        //{
        //    Debug.Log("POI " + i + " - " + poiInfos[i].inUse);
        //    if(poiInfos[i].inUse)
        //    {
        //        Debug.Log(poiInfos[i].poiType);
        //        Debug.Log(poiInfos[i].priority);
        //        Debug.Log(poiInfos[i].globalPos);
        //        Debug.Log(poiInfos[i].dist);
        //    }
        //}

        //Finds highest-priority POIs, then finds the closest one
        highestPriorityPoi = -999;
        for (int i=0;i<colliderToPoi;i++)
        {
            if (poiInfos[i].priority > highestPriorityPoi) { highestPriorityPoi = poiInfos[i].priority; }
        }

        closestPoi = Mathf.Infinity;
        for (int i = 0; i < colliderToPoi; i++)
        {
            if ((poiInfos[i].dist < closestPoi) && (poiInfos[i].priority == highestPriorityPoi))
            {
                targetPoi = i;
                closestPoi = poiInfos[i].dist;
            }
        }

        targetWaypoint = poiInfos[targetPoi].globalPos;
        targetWaypointLocal = transform.InverseTransformPoint(targetWaypoint);
    }
}

public class PoiInfo
{
    public bool inUse;
    public int priority;
    public Vector3 globalPos;
    public float dist;
    public enum Types {Objective, EnemyShip, EnemyFort, DyingAlly, DyingEnemy};
    public Types poiType;

    public float poiDist;

    public PoiInfo(float poiDistIn)
    {
        Reset();
        priority = 0;
        globalPos = Vector3.zero;
        dist = 0;
        poiType = Types.Objective;

        poiDist = poiDistIn;
    }
    public void Reset()
    {
        inUse = false;
    }
    public void Set(Types poiTypeIn, Vector3 globalPosIn, Vector3 thisPosIn)
    {
        inUse = true;

        poiType = poiTypeIn;
        globalPos = globalPosIn;
        dist = Vector3.Distance(thisPosIn, globalPos);

        //handles different 
        switch (poiType)
        {
            case Types.Objective:
                if (dist <= poiDist) { priority = 1; }
                else { priority = 0; }
                break;

            case Types.EnemyShip:
                priority = 0;
                break;

            default:
                priority = 0;
                break;
        }
    }
}
