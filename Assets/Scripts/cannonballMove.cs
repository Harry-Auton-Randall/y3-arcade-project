using UnityEngine;
using System;

public class cannonballMove : MonoBehaviour
{
    public float speed = 40f;
    public float duration = 0.75f;
    public float startHeight = 0.73f;
    
    bool friendly = false;
    bool chain, fire, pierce;

    Collider ignore;
    int shooterID;

    float tss;
    Vector3 ballHeight = new Vector3(0f,0f,0f);
    Vector3 outlineSize = new Vector3(0f, 0f, 0f);

    public Material redSolid, whiteSolid;
    Transform ball, outline;

    void Awake()
    {
        tss = 0f;
        ball = transform.Find("ball");
        outline = transform.Find("ball/outline");
    }

    //With ammo type
    public void Init(float speedIn, float durationIn, float startHeightIn, bool friendlyIn, int typeIn, Collider ignoreIn, int shooterIDIn)
    {
        speed = speedIn;
        duration = durationIn;
        startHeight = startHeightIn;
        friendly = friendlyIn;
        ignore = ignoreIn;
        shooterID = shooterIDIn;
        if (friendly)
        {
            outline.GetComponent<Renderer>().material = whiteSolid;
        }
        else
        {
            outline.GetComponent<Renderer>().material = redSolid;
        }

        chain = false;
        fire = false;
        pierce = false;
        switch (typeIn)
        {
            case 1:
                chain = true;
                break;
            case 2:
                fire = true;
                break;
            case 3:
                pierce = true;
                break;
        }
        outline.localScale = Vector3.zero;
        ballHeight.y = startHeightIn;
        ball.localPosition = ballHeight;
    }

    void OnTriggerEnter(Collider collision)
    {
        //Ignores the boat that shot it, boat rams, and triggers
        if (collision != ignore && !collision.isTrigger && collision.gameObject.layer != LayerMask.NameToLayer("boatRam"))
        {
            //boat hit
            if (collision.gameObject.layer == LayerMask.NameToLayer("boat"))
            {
                if (collision.transform.parent.GetComponent<boatCombat>().respawnImmunity == false)
                {
                    //Debug.Log("Hit a boat");
                    //Debug.Log(DateTime.Now + "    " + collision.transform.parent.gameObject.name);
                    collision.transform.parent.GetComponent<boatCombat>().TakeDamage(1, fire, chain, shooterID);
                    if (!pierce)
                    {
                        Destroy(this.gameObject);
                    }
                }
                else
                {
                    //Debug.Log("Hit an immune boat");
                }
            }
            //mine hit
            else if (collision.gameObject.layer == LayerMask.NameToLayer("mine"))
            {
                collision.transform.parent.GetComponent<Mine>().DetonateShot(shooterID, false);
                Destroy(this.gameObject);
            }
            //terrain hit
            else
            {
                //Debug.Log("Hit Terrain");
                Destroy(this.gameObject);
            }
        }
    }

    void FixedUpdate()
    {
        tss += Time.fixedDeltaTime;
        if (tss > duration)
        {
            Destroy(this.gameObject);
        }

        transform.position += transform.forward * speed * Time.fixedDeltaTime;

        ballHeight.y = (0.02f + (startHeight - 0.02f) * (1 - Mathf.Pow(tss / duration, 2)));
        ball.localPosition = ballHeight;

        if (tss * speed >= 1.5f)
        {
            outlineSize.x = (10f / 3f);
        }
        else
        {
            outlineSize.x = ((tss * speed) / 1.5f) * (10f / 3f);
        }
        outlineSize.y = outlineSize.x;
        outlineSize.z = outlineSize.x;
        outline.localScale = outlineSize;
    }
}
