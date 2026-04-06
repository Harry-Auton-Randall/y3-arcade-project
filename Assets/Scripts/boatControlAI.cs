using UnityEngine;
using UnityEngine.UI;

public class boatControlAI : MonoBehaviour
{
    Transform reticle;
    Slider reticleCircle;

    boatMove bm;
    boatCombat bc;

    //POI-finding
    LayerMask poiMask;

    Vector3[] objectiveWaypoints;
    Collider[] nearbyPois;
    PoiInfo[] poiInfos;
    float poiRadius = 150f;

    int nearbyPoiCount;
    int colliderToPoi;

    GameObject poiBoat;
    boatCombat poiBC;

    int highestPriorityPoi;
    float closestPoi;

    //for tracking the gameObject of a POI (for debugging)
    GameObject previousPoi;
    GameObject[] poiObjects;

    int targetPoi;
    Vector3 targetWaypoint, targetWaypointLocal;
    float targetWaypointAngle;

    //used by RotateBoat
    float steerThreshold = 10;

    float moveIn, steerIn;

    bool stopAtTarget;
    bool aimingAtTargetPoi; //specifically for rotating ships to aim cannons at their targetPOI, if relevant

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
            objectiveWaypoints = new Vector3[] { new Vector3(0, 0, 0) }; //TEMPORARY
        }

        //initialises nearbyPOIs and poiInfo

        //nearbyPois = new Collider[bc.rMan.totalShips];
        nearbyPois = new Collider[bc.rMan.totalShips + 10]; //TEMPORARY - while AIs aren't spawned by RoundManager and instead placed manually

        poiInfos = new PoiInfo[nearbyPois.Length + objectiveWaypoints.Length];
        for (int i=0;i<poiInfos.Length;i++)
        {
            poiInfos[i] = new PoiInfo(poiRadius);
        }

        poiObjects = new GameObject[poiInfos.Length];
    }


    //Finds all POIs within poiRadius (should be 150m), pick one based on priority + distance
    void FindPois()
    {
        //finds every boat within poiRadius
        for (int i = 0; i < nearbyPois.Length; i++)
        {
            nearbyPois[i] = default;
        }
        nearbyPoiCount = Physics.OverlapSphereNonAlloc(this.transform.position, poiRadius, nearbyPois, poiMask);

        //determines if each boat found is a valid POI, adds to poiInfos if so
        colliderToPoi = 0;
        for (int i = 0; i < nearbyPoiCount; i++)
        {
            poiBoat = nearbyPois[i].transform.parent.gameObject;

            //check if it's detecting its own collider
            if (poiBoat != this.gameObject)
            {
                poiBC = poiBoat.GetComponent<boatCombat>();

                //Currently only supports enemyShip POIs
                if ((bc.team != poiBC.team) || (bc.team == 0))
                {
                    poiInfos[colliderToPoi].Set(PoiInfo.Types.EnemyBoat, poiBoat.transform.position, this.transform.position);

                    poiObjects[colliderToPoi] = poiBoat;

                    colliderToPoi++;
                }
            }
        }
        //adds objective POIs to poiInfos
        for (int i = 0; i < objectiveWaypoints.Length; i++)
        {
            poiInfos[colliderToPoi].Set(PoiInfo.Types.ObjectivePoint, objectiveWaypoints[i], this.transform.position);
            if (bc.rMan.mode == 0)
            {
                poiInfos[colliderToPoi].priority = -10; //if deathmatch, sets objective priority extra low (should only go here if there are no other POIs)
            }

            poiObjects[colliderToPoi] = GameObject.Find("/sun");

            colliderToPoi++;
        }

        //disable all remaining PoiInfos
        for (int i = colliderToPoi; i < poiInfos.Length; i++)
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
        for (int i = 0; i < colliderToPoi; i++)
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

        //TEMPORARY - logs if the targetPoi changes - uses poiObject, because the order of poiInfos is volatile
        if (previousPoi != poiObjects[targetPoi])
        {
            Debug.Log("Target changed from " + previousPoi + " to " + poiObjects[targetPoi]);
            previousPoi = poiObjects[targetPoi];
        }
    }

    //Handles rotating the boat
    float RotateBoat(float angle)
    {
        angle = angle * -1;

        //Apply full steering if angle is too much (uses steerThreshold)
        if (angle < steerThreshold * -1)
        {
            return 1;
        }
        else if (angle > steerThreshold)
        {
            return -1;
        }
        //when angle is lower
        else
        {
            //if angle is getting further away from 0, apply full steering to counter
            if (angle <= 0 && bm.outRotSpd < 0)
            {
                return 1;
            }
            else if (angle >= 0 && bm.outRotSpd > 0)
            {
                return -1;
            }
            //softer steering to avoid overshooting
            else
            {
                return angle / steerThreshold;
            }
        }
    }

    void Update()
    {
        FindPois(); //keep at the top of Update

        //MOVEMENT

        //gets global/local positions + angle of targetPOI's location
        targetWaypoint = poiInfos[targetPoi].globalPos;
        targetWaypointLocal = transform.InverseTransformPoint(targetWaypoint);
        targetWaypointLocal.y = 0;
        targetWaypointAngle = Vector3.SignedAngle(Vector3.forward, targetWaypointLocal, Vector3.up);

        //set stopAtTarget, depending on waypoint type
        if (poiInfos[targetPoi].poiType == PoiInfo.Types.ObjectivePoint ||
            poiInfos[targetPoi].poiType == PoiInfo.Types.DyingAlly ||
            poiInfos[targetPoi].poiType == PoiInfo.Types.DyingEnemy)
        {
            stopAtTarget = true;
        }
        else
        {
            stopAtTarget = false;
        }

        //sets aimingAtTargetPOI, depending on targetPOI type + distance, + this ship class
        //aimingAtTargetPOI means the ship is positioning itself to fire upon its targetPOI
        if (poiInfos[targetPoi].poiType == PoiInfo.Types.ObjectiveBoat ||
            poiInfos[targetPoi].poiType == PoiInfo.Types.EnemyBoat ||
            poiInfos[targetPoi].poiType == PoiInfo.Types.EnemyFort)
        {
            //Most ships aim their guns if within 30m, cutters get in further, then turn around

            if (bc.shipClass == boatCombat.Classes.Cutter)
            {
                if (poiInfos[targetPoi].dist <= 20 || (Mathf.Abs(targetWaypointAngle) >= 90 && poiInfos[targetPoi].dist <= 30))
                {
                    aimingAtTargetPoi = true;
                }
                else { aimingAtTargetPoi = false; }
            }
            else
            {
                if (poiInfos[targetPoi].dist <= 30)
                {
                    aimingAtTargetPoi = true;
                }
                else { aimingAtTargetPoi = false; }
            }
        }
        else
        {
            aimingAtTargetPoi = false;
        }

        //Sorts out movement and steering - acts differently depending on aimingAtTargetPOI

        if (!aimingAtTargetPoi)
        {
            //NOT aimingAtTargetPOI - paths towards its target waypoint

            //point towards target waypoint
            //Doesnt bother if stopAtTarget is enabled AND the target is close enough
            if (!(stopAtTarget && poiInfos[targetPoi].dist < 5))
            {
                steerIn = RotateBoat(targetWaypointAngle);
            }
            else
            {
                steerIn = 0;
            }

            //set moveIn
            //sets to 0 if the target is >45deg away OR
            //(stopAtTarget enabled AND EITHER the target is close enough, OR its moving fast enough to reach the target just by coasting)
            if (Mathf.Abs(targetWaypointAngle) > 45 ||
                (stopAtTarget && (poiInfos[targetPoi].dist < 5 || (bm.outSpd / bm.rb.linearDamping >= poiInfos[targetPoi].dist))))
            {
                moveIn = 0;
            }
            else
            {
                moveIn = 1;
            }
        }
        else
        {
            //aimingAtTargetPOI - max moveIn, rotates its closest cannons towards the target waypoint

            //point guns towards target waypoint - recalculates targetWaypointAngle to do so
            if (bc.shipClass == boatCombat.Classes.Cutter)
            {
                targetWaypointAngle = Vector3.SignedAngle(Vector3.forward * -1, targetWaypointLocal, Vector3.up);
            }
            else
            {
                //figures out if the left or right broadside is closer
                if (targetWaypointAngle < 0)
                {
                    targetWaypointAngle = Vector3.SignedAngle(Vector3.right * -1, targetWaypointLocal, Vector3.up);
                }
                else
                {
                    targetWaypointAngle = Vector3.SignedAngle(Vector3.right, targetWaypointLocal, Vector3.up);
                }
            }
            steerIn = RotateBoat(targetWaypointAngle);

            moveIn = 1;
        }


        bm.SetMovementIn(moveIn);
        bm.SetRotationIn(steerIn);
    }
}





public class PoiInfo
{
    public bool inUse;
    public int priority;
    public Vector3 globalPos;
    public float dist;
    public enum Types {ObjectivePoint, ObjectiveBoat, EnemyBoat, EnemyFort, DyingAlly, DyingEnemy, ResourcePickup};
    public Types poiType;

    public float poiMaxDist;

    public PoiInfo(float poiMaxDistIn)
    {
        Reset();
        priority = 0;
        globalPos = Vector3.zero;
        dist = 0;
        poiType = Types.ObjectivePoint;

        poiMaxDist = poiMaxDistIn;
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

        //handles different priorities for different types/distances
        switch (poiType)
        {
            case Types.ObjectivePoint:
                if (dist <= poiMaxDist) { priority = 1; }
                else { priority = 0; }
                break;
            case Types.ObjectiveBoat:
                if (dist <= poiMaxDist) { priority = 1; }
                else { priority = 0; }
                break;

            case Types.EnemyBoat:
                priority = 0;
                break;

            default:
                priority = 0;
                break;
        }
    }
}
