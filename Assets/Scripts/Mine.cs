using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mine : MonoBehaviour
{
    int spawnerID = -1;
    float radius = 5;
    Collider[] nearbyDamagers;
    LayerMask damagerMask;
    int nearbyDamagerCount;
    Rigidbody rb;

    Collider[] startDam = new Collider[1];
    int startDamCount = 0;
    LayerMask startDamMask;

    float duration = -1;
    float time = 0;

    //pushingZone stuff
    Vector3 totalPushForce;
    List<GameObject> pushingZones;

    public void SetStuff(int idIn, Vector3 parentVel)
    {
        spawnerID = idIn;
        rb.linearVelocity = parentVel + (transform.forward * -3);
        duration = 60;

        //Check if it's been spawned inside an object. if so, blow up immediately
        startDamMask = ~(1 << LayerMask.NameToLayer("ocean"));

        startDamCount = Physics.OverlapSphereNonAlloc(this.transform.position, 0.5f, startDam, startDamMask, QueryTriggerInteraction.Ignore);

        if (startDamCount > 0)
        {
            Debug.Log("aaaaa");
            Detonate();
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        damagerMask = ((1 << LayerMask.NameToLayer("boat")) | (1 << LayerMask.NameToLayer("mine")));
        pushingZones = new List<GameObject>();
    }
    void Update()
    {
        if (duration != -1)
        {
            time += Time.deltaTime;
            if (time > duration)
            {
                Destroy(this.gameObject);
            }
        }

        //copied from boatMove
        totalPushForce = Vector3.zero;
        for (int i = 0; i < pushingZones.Count; i++)
        {
            totalPushForce += (pushingZones[i].transform.forward);
        }
        if (totalPushForce != Vector3.zero)
        {
            totalPushForce.Normalize();
            rb.AddForce(totalPushForce * rb.mass * rb.linearDamping * -4);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("boat"))
        {
            if (collision.gameObject.GetComponent<boatCombat>().respawnImmunity == false)
            {
                Detonate();
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("mine"))
        {
            Detonate();
        }
    }
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("PushingZone")
            && !(pushingZones.Contains(collision.gameObject)))
        {
            pushingZones.Add(collision.gameObject);
        }
    }
    void OnTriggerExit(Collider collision)
    {
        if (pushingZones.Contains(collision.gameObject))
        {
            pushingZones.Remove(collision.gameObject);
        }
    }

    public void DetonateDelay()
    {
        //Debug.Log("I was detonated by another mine");
        StartCoroutine(DetonateDelay2());
    }
    IEnumerator DetonateDelay2()
    {
        yield return null;
        Detonate();
    }

    public void Detonate()
    {
        nearbyDamagers = new Collider[GameObject.Find("/RoundManager").GetComponent<RoundManager>().totalShips + 10]; //+10 temporary

        nearbyDamagerCount = Physics.OverlapSphereNonAlloc(this.transform.position, radius, nearbyDamagers, damagerMask);
        
        for (int i = 0; i < nearbyDamagerCount; i++)
        {
            if (nearbyDamagers[i].transform.parent.gameObject != this.gameObject)
            { 
                //This doesn't work for some reason, so the if-else is used instead
                //switch(nearbyDamagers[i].transform.parent.gameObject.layer)
                //{
                //    case (LayerMask.NameToLayer("boat")):
                //        nearbyDamagers[i].transform.parent.GetComponent<boatCombat>().TakeDamage(16, false, false, -1);
                //        break;
                //    case (LayerMask.NameToLayer("mine")):
                //        nearbyDamagers[i].transform.parent.GetComponent<Mine>().Detonate();
                //        break;
                //}

                if (nearbyDamagers[i].transform.parent.gameObject.layer == LayerMask.NameToLayer("boat"))
                {
                    nearbyDamagers[i].transform.parent.GetComponent<boatCombat>().TakeDamage(16, false, false, spawnerID);
                }
                else if (nearbyDamagers[i].transform.parent.gameObject.layer == LayerMask.NameToLayer("mine"))
                {
                    nearbyDamagers[i].transform.parent.GetComponent<Mine>().DetonateDelay();
                }
            }
        }
        Destroy(this.gameObject);
    }
}
