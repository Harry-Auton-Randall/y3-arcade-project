using UnityEngine;
using System.Collections;

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

    public void SetStuff(int idIn, Vector3 parentVel)
    {
        rb = GetComponent<Rigidbody>();
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
        damagerMask = ((1 << LayerMask.NameToLayer("boat")) | (1 << LayerMask.NameToLayer("mine")));
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
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("boat")
            || collision.gameObject.layer == LayerMask.NameToLayer("mine"))
        {
            Detonate();
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
                    nearbyDamagers[i].transform.parent.GetComponent<boatCombat>().TakeDamage(16, false, false, -1);
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
