using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    float closestPoi, closestShootablePoi;

    //for tracking the gameObject of a POI
    GameObject previousPoi;
    GameObject[] poiObjects;

    int targetPoi;
    Vector3 targetWaypoint, targetWaypointLocal;
    float targetWaypointAngle;
    float targetWaypointAngleAiming;
    float targetWaypointAngleCircleAdj;
    float targetWaypointAimCircleRadius;

    int shootablePoi;
    Vector3 shootableWaypoint, shootableWaypointVel;

    int cannonballSpeed = 40;
    bool anythingToShoot;
    Vector3 cannonCentreL, cannonCentreR;
    float shootableCannonCentreDist;

    float shootDelay = 1f;
    float timeAiming;
    bool isAiming;

    //used by RotateBoat
    float steerThreshold = 5;

    float moveIn, steerIn;

    bool stopAtTarget;
    bool aimingAtTargetPoi; //specifically for rotating ships to aim cannons at their targetPOI, if relevant

    //AStar pathfinding stuff
    Waypoint[] allWaypoints;
    Ray ray;
    RaycastHit rayHit;
    LayerMask terrainMask;
    List<WaypointRoutingInfo> wrf = new List<WaypointRoutingInfo>();
    List<Vector3> pathingWaypoints = new List<Vector3>();
    bool pathfinding;


    void Awake()
    {
        poiMask = (1 << LayerMask.NameToLayer("boat"));
        bm = GetComponent<boatMove>();
        bc = GetComponent<boatCombat>();

        reticle = transform.Find("reticle");
        reticleCircle = transform.Find("reticle/reticleCanvas/Slider").GetComponent<Slider>();
        reticleCircle.enabled = false;

        terrainMask = (1 << LayerMask.NameToLayer("terrain"));
    }
    void Start()
    {
        //sets cannonCentres based on class
        switch (bc.shipClass)
        {
            case boatCombat.Classes.Cutter:
                cannonCentreR = new Vector3(bc.cannonCentreR.x, 0, bc.cannonCentreR.y);
                break;
            case boatCombat.Classes.Brigantine:
                cannonCentreL = new Vector3(-0.75f, 0, 0);
                cannonCentreR = new Vector3(0.75f, 0, 0);
                break;
            default:
                cannonCentreL = new Vector3(bc.cannonCentreL2.x, 0, bc.cannonCentreL2.y);
                cannonCentreR = new Vector3(bc.cannonCentreR2.x, 0, bc.cannonCentreR2.y);
                break;
        }

        //sets objectiveWaypoints based on mode
        if (bc.rMan.mode == 0)
        {
            objectiveWaypoints = new Vector3[] { new Vector3(0, 0, 0) }; //TEMPORARY
        }
        if (bc.shipClass == boatCombat.Classes.Cutter)
        {
            targetWaypointAimCircleRadius = 15;
        }
        else
        {
            targetWaypointAimCircleRadius = 25;
        }

        //initialises nearbyPOIs and poiInfo

        //nearbyPois = new Collider[bc.rMan.totalShips];
        nearbyPois = new Collider[bc.rMan.totalShips + 10]; //TEMPORARY - while AIs aren't spawned by RoundManager and instead placed manually

        poiInfos = new PoiInfo[nearbyPois.Length + objectiveWaypoints.Length];
        for (int i = 0; i < poiInfos.Length; i++)
        {
            poiInfos[i] = new PoiInfo(poiRadius);
        }

        poiObjects = new GameObject[poiInfos.Length];


        //Waypoint Stuff
        //Duplicates every Waypoint object in RoundManager's allWaypoints

        this.allWaypoints = new Waypoint[bc.rMan.allWaypoints.Length];
        for (int i=0;i<allWaypoints.Length;i++)
        {
            if (bc.rMan.allWaypoints[i] != null)
            {
                this.allWaypoints[i] = new Waypoint(bc.rMan.allWaypoints[i].obj);
                this.allWaypoints[i].neighbourNo = bc.rMan.allWaypoints[i].neighbourNo;
            }
        }
        for (int i = 0; i < allWaypoints.Length; i++)
        {
            if (bc.rMan.allWaypoints[i] != null)
            {
                for (int j = 0; j < bc.rMan.allWaypoints[i].neighbours.Count; j++)
                {
                    this.allWaypoints[i].neighbours.Add(this.allWaypoints[bc.rMan.allWaypoints[i].neighbourAddresses[j]]);
                    this.allWaypoints[i].neighbourAddresses.Add(bc.rMan.allWaypoints[i].neighbourAddresses[j]);
                    this.allWaypoints[i].neighbourDists.Add(bc.rMan.allWaypoints[i].neighbourDists[j]);
                }
            }
        }

        //Debug.LogError(gameObject);
        //for (int i = 0; i < allWaypoints.Length; i++)
        //{
        //    if (allWaypoints[i] == null)
        //    {
        //        Debug.Log(null);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("Waypoint: " + allWaypoints[i].obj);
        //        for (int j = 0; j < allWaypoints[i].neighbours.Count; j++)
        //        {
        //            Debug.Log("Neighbour is " + allWaypoints[i].neighbours[j].obj);
        //            Debug.Log("Stored in address " + allWaypoints[i].neighbourAddresses[j]);
        //            Debug.Log("Distance is " + allWaypoints[i].neighbourDists[j]);
        //        }
        //        Debug.Log(allWaypoints[i].neighbourNo + " neighbours in total");
        //        Debug.Log("heuristic is " + allWaypoints[i].heuristic);
        //        Debug.Log("position is " + allWaypoints[i].globalPos);
        //    }
        //}

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
                poiInfos[colliderToPoi].priority =  10; //if deathmatch, sets objective priority extra low (should only go here if there are no other POIs)
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
        //Also finds the closest shootable POI - ignores priority, just uses distance
        highestPriorityPoi = -999;
        for (int i = 0; i < colliderToPoi; i++)
        {
            if (poiInfos[i].priority > highestPriorityPoi) { highestPriorityPoi = poiInfos[i].priority; }
        }

        closestPoi = Mathf.Infinity;
        closestShootablePoi = Mathf.Infinity;
        anythingToShoot = false;
        for (int i = 0; i < colliderToPoi; i++)
        {
            if ((poiInfos[i].dist < closestPoi) && (poiInfos[i].priority == highestPriorityPoi))
            {
                targetPoi = i;
                closestPoi = poiInfos[i].dist;
            }
            //shootable POI
            if (poiInfos[i].dist < closestShootablePoi &&
                (poiInfos[i].poiType == PoiInfo.Types.ObjectiveBoat ||
                 poiInfos[i].poiType == PoiInfo.Types.EnemyBoat ||
                 poiInfos[i].poiType == PoiInfo.Types.EnemyFort))
            {
                anythingToShoot = true;
                shootablePoi = i;
                closestShootablePoi = poiInfos[i].dist;
            }
        }

        //TEMPORARY - logs if the targetPoi changes - uses poiObject, because the order of poiInfos is volatile
        if (previousPoi != poiObjects[targetPoi])
        {
            //Debug.Log("Target changed from " + previousPoi + " to " + poiObjects[targetPoi]);
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
                if (steerThreshold == 0) { return 0; }
                else { return angle / steerThreshold; }
            }
        }
    }

    void GeneratePathingRoute()
    {
        //Initialise the Waypoint Array, 

        //Add this ship's position and target ship's position to allWaypoints
        int al = allWaypoints.Length;

        allWaypoints[al - 2] = new Waypoint(this.gameObject);
        allWaypoints[al - 1] = new Waypoint(poiObjects[targetPoi]);

        //Removes any neighbour info that would've been connected to the target of the last pathing route
        for (int i=0;i<al;i++)
        {
            while (allWaypoints[i].neighbours.Count > allWaypoints[i].neighbourNo)
            {
                allWaypoints[i].neighbours.RemoveAt(allWaypoints[i].neighbourNo);
                allWaypoints[i].neighbourAddresses.RemoveAt(allWaypoints[i].neighbourNo);
                allWaypoints[i].neighbourDists.RemoveAt(allWaypoints[i].neighbourNo);
            }
        }

        //For every waypoint, checks if either this ship or the target can reach them. If so, makes new neighbours
        for (int i=0;i<al - 2;i++)
        {
            //This ship's waypoint
            if (Physics.SphereCast(allWaypoints[i].globalPos, 2.5f, (allWaypoints[al - 2].globalPos - allWaypoints[i].globalPos),
                out rayHit, Vector3.Distance(allWaypoints[i].globalPos, allWaypoints[al - 2].globalPos), terrainMask))
            {
                allWaypoints[al - 2].neighbours.Add(allWaypoints[i]);
                allWaypoints[al - 2].neighbourAddresses.Add(i);
                allWaypoints[al - 2].neighbourDists.Add(Vector3.Distance(allWaypoints[i].globalPos, allWaypoints[al - 2].globalPos));
            }

            //target ship's waypoint
            if (Physics.SphereCast(allWaypoints[i].globalPos, 2.5f, (allWaypoints[al - 1].globalPos - allWaypoints[i].globalPos),
                out rayHit, Vector3.Distance(allWaypoints[i].globalPos, allWaypoints[al - 1].globalPos), terrainMask))
            {
                allWaypoints[i].neighbours.Add(allWaypoints[al - 1]);
                allWaypoints[i].neighbourAddresses.Add(al - 1);
                allWaypoints[i].neighbourDists.Add(Vector3.Distance(allWaypoints[i].globalPos, allWaypoints[al - 1].globalPos));
            }
        }

        //ACTUALLY DOING THE PATHFINDING


        wrf.Clear();
        wrf.Add(new WaypointRoutingInfo());
        wrf[0].cameFrom.Add(al - 2);
        wrf[0].costSoFar = 0;
        wrf[0].priority = 0;

        int hpi; //highest priority index - also the index of the wrf currently being looked at
        int mrcf; //most recent cameFrom - the index of the Waypoint in allWaypoints, connected to the wrf being looked at
        float newCost;

        while (wrf.Count > 0)
        {
            //Find whichever waypointRoutingInfo has the highest priority, because i don't know how to make a priority queue in Unity
            hpi = -1;
            for (int i=0;i<wrf.Count;i++)
            {
                if (hpi == -1 || wrf[hpi].priority > wrf[i].priority)
                {
                    hpi = i;
                }
            }

            mrcf = wrf[hpi].cameFrom[wrf[hpi].cameFrom.Count - 1];

            //check if the goal has been reached
            if (mrcf == al - 1)
            {
                //do stuff here
            }
            else
            {
                //Check out each neighbour of the mrcf
                for (int i = 0; i < allWaypoints[mrcf].neighbours.Count; i++)
                {
                    newCost = wrf[hpi].costSoFar + allWaypoints[mrcf].neighbourDists[i];
                    //if...
                }
            }

        }


    }

    void Update()
    {
        FindPois(); //keep at the top of Update


        // ------ MOVEMENT ------


        //Checks if there's any terrain between this ship and the targetPOI. If not, head straight for it. If so, generate pathfinding route (if not already present) and follow it
        ray = new Ray(this.transform.position, poiInfos[targetPoi].globalPos - this.transform.position);
        Debug.DrawRay(this.transform.position, ray.direction * Vector3.Distance(this.transform.position, poiInfos[targetPoi].globalPos), Color.yellow);

        if (Physics.Raycast(ray, out rayHit, Vector3.Distance(this.transform.position, poiInfos[targetPoi].globalPos), terrainMask))
        {
            if (!pathfinding)
            {
                //Call Generate-Route function here
            }
            else
            {
                //Use more rays to check if the route's starting position can still see this ship, and the ending position can still see the target
                //If not, regenerate the route
                //note: starting position and ending position should only be actual waypoints, not the positions of the ships themselves
            }

            //check all pathingWaypoints to see if the ship's within a certain zone
            //if so, remove it and everything before it

            if (pathingWaypoints.Count == 0)
            {
                //regenerate the route
            }

            targetWaypoint = pathingWaypoints[0];
            pathfinding = true;
        }
        else
        {
            targetWaypoint = poiInfos[targetPoi].globalPos;
            pathfinding = false;
        }
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
                //when targeting enemy ships, offset their rotation a little bit, to enter a better orbit around their target
                if ((poiInfos[targetPoi].poiType == PoiInfo.Types.ObjectiveBoat ||
                     poiInfos[targetPoi].poiType == PoiInfo.Types.EnemyBoat) &&
                     poiInfos[targetPoi].dist > targetWaypointAimCircleRadius && poiInfos[targetPoi].dist <= 100)
                {
                    if (targetWaypointAngle < 0)
                    {
                        targetWaypointAngleCircleAdj = Mathf.Rad2Deg * Mathf.Asin(targetWaypointAimCircleRadius / poiInfos[targetPoi].dist);
                    }
                    else
                    {
                        targetWaypointAngleCircleAdj = -1 * Mathf.Rad2Deg * Mathf.Asin(targetWaypointAimCircleRadius / poiInfos[targetPoi].dist);
                    }
                }
                else
                {
                    targetWaypointAngleCircleAdj = 0;
                }
                steerIn = RotateBoat(targetWaypointAngle + targetWaypointAngleCircleAdj);
            }
            else
            {
                targetWaypointAngleCircleAdj = 0;
                steerIn = 0;
            }

            //set moveIn
            //sets to 0 if the target is >45deg away OR
            //(stopAtTarget enabled AND EITHER the target is close enough, OR its moving fast enough to reach the target just by coasting)
            if (Mathf.Abs(targetWaypointAngle + targetWaypointAngleCircleAdj) > 45 ||
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

            //point guns towards target waypoint - uses targetWaypointAngleAiming to do so
            if (bc.shipClass == boatCombat.Classes.Cutter)
            {
                //this is to combat an issue where cutters will approach a target sideways, steer the wrong way and slam into their target

                targetWaypointAngleAiming = Vector3.SignedAngle(Vector3.forward * -1, targetWaypointLocal, Vector3.up);

                float moveDirToTarget = Vector3.SignedAngle(bm.localMoveDir * -1, targetWaypointLocal, Vector3.up);
                float angleToMoveDir = Vector3.SignedAngle(Vector3.forward * -1, bm.localMoveDir * -1, Vector3.up);

                if ((moveDirToTarget >= 90 || moveDirToTarget <= -90) &&
                    ((moveDirToTarget < 0 && targetWaypointAngleAiming > 0) || 
                    (moveDirToTarget > 0 && targetWaypointAngleAiming < 0)))
                {
                    steerIn = RotateBoat(angleToMoveDir);
                }
                else
                {
                    steerIn = RotateBoat(targetWaypointAngleAiming);
                }
            }
            else
            {
                //If target is dead ahead or too close, steer away
                if (Mathf.Abs(targetWaypointAngle) < 15 || poiInfos[targetPoi].dist < 10)
                {
                    targetWaypointAngleAiming = Vector3.SignedAngle(Vector3.forward * -1, targetWaypointLocal, Vector3.up);
                    steerIn = RotateBoat(targetWaypointAngleAiming);
                }
                //Else, only bother steering if you've passed the target
                else if (Mathf.Abs(targetWaypointAngle) <= 90)
                {
                    steerIn = 0;
                }
                else 
                {
                    //figures out if the left or right broadside is closer, then aims it towards the target
                    if (targetWaypointAngle < 0)
                    {
                        targetWaypointAngleAiming = Vector3.SignedAngle(Vector3.right * -1, targetWaypointLocal, Vector3.up);
                    }
                    else
                    {
                        targetWaypointAngleAiming = Vector3.SignedAngle(Vector3.right, targetWaypointLocal, Vector3.up);
                    }
                    steerIn = RotateBoat(targetWaypointAngleAiming);
                }
            }

            moveIn = 1;
        }

        bm.SetMovementIn(moveIn);
        bm.SetRotationIn(steerIn);


        // ------ SHOOTING ------


        // If not already aiming, Gets the global position and velocity of its target
        if (!isAiming)
        {
            if (anythingToShoot)
            {
                shootableWaypoint = poiInfos[shootablePoi].globalPos;

                if (poiInfos[shootablePoi].poiType == PoiInfo.Types.ObjectiveBoat ||
                    poiInfos[shootablePoi].poiType == PoiInfo.Types.EnemyBoat)
                {
                    shootableWaypointVel = poiObjects[shootablePoi].GetComponent<boatMove>().globalMoveDir;
                }
                else
                {
                    shootableWaypointVel = Vector3.zero;
                }
            }
            else
            {
                shootableWaypoint = transform.position;
                shootableWaypointVel = Vector3.zero;
            }

            reticle.transform.position = shootableWaypoint;
        }

        //If it is aiming, use previously found target position/velocity, and timeAiming, to estimate its current position
        else
        {
            reticle.transform.position = shootableWaypoint + (shootableWaypointVel * timeAiming);
        }

        //Gets the position it's currently aiming at, finds its distance, and uses it to lead its shot
        reticle.transform.position += shootableWaypointVel * (reticle.transform.localPosition.magnitude / cannonballSpeed);

        //Finds the distance between the reticle and the relevant cannons, to check if its in range
        if (reticle.transform.localPosition.x >= 0 || bc.shipClass == boatCombat.Classes.Cutter)
        {
            shootableCannonCentreDist = Vector3.Distance(cannonCentreR, reticle.transform.localPosition);
        }
        else
        {
            shootableCannonCentreDist = Vector3.Distance(cannonCentreL, reticle.transform.localPosition);
        }

        if(shootableCannonCentreDist > 30.5f)
        {
            reticle.transform.localPosition = Vector3.zero;
        }

        //updates aimPos in boatCombat
        bc.aimPos.x = reticle.transform.localPosition.x;
        bc.aimPos.y = reticle.transform.localPosition.z;

        //if aiming is valid, set isAiming to true. Also stays true, as long as volleying is > 0
        if ((shootableCannonCentreDist <= 30.5f && bc.reloadProgress == 100) || (bc.volleying > 0 && isAiming))
        {
            if (timeAiming >= shootDelay)
            {
                bc.Shoot();
            }

            timeAiming += Time.deltaTime;
            isAiming = true;
        }
        else
        {
            //when aiming stops being valid, if volleying == 0, fire off one last shot
            //if (bc.volleying == 0 && isAiming)
            //{
            //    bc.Shoot();
            //}

            timeAiming = 0;
            isAiming = false;
        }

        //reticleCircle.value = bc.reloadProgress;
        //reticle.transform.rotation = bc.rMan.playerReticleRotation;
    }

    public void RotateReticle(Quaternion input)
    {
        reticle.transform.rotation = input;
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

public class Waypoint
{
    public GameObject obj;
    public List<Waypoint> neighbours;
    public List<int> neighbourAddresses;
    public List<float> neighbourDists;
    public int neighbourNo;
    public float heuristic;
    public Vector3 globalPos;

    public Waypoint(GameObject objIn)
    {
        obj = objIn;
        neighbours = new List<Waypoint>();
        neighbourAddresses = new List<int>();
        neighbourDists = new List<float>();
        neighbourNo = 0;
        heuristic = 0;
        globalPos = obj.transform.position;
    }
}

public class WaypointRoutingInfo
{
    public float priority;
    public List<int> cameFrom; //This is a list of allWaypoints indexes, not wrf indexes
    public float costSoFar;
    public WaypointRoutingInfo()
    {
        cameFrom = new List<int>();
    }
}
