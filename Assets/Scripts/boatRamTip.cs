using UnityEngine;

public class boatRamTip : MonoBehaviour
{
    boatCombat bc;
    int damage = 10;
    float speed, speedPrior;

    void Awake()
    {
        bc = transform.parent.GetComponent<boatCombat>();
    }

    void FixedUpdate()
    {
        speedPrior = speed;
        speed = Vector3.Dot(bc.transform.forward, bc.rb.linearVelocity);
    }

    void OnTriggerEnter(Collider collision)
    {
        if (bc.bm.charge && !collision.isTrigger)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("boat"))
            {
                //figure out how much damage to do based on z-axis speed
                //Debug.Log("current speed: " + speed);
                //Debug.Log("speed last phys update: " + speedPrior);
                //Debug.Log("speed stat: " + bc.speed);
                if (speedPrior >= bc.speed)
                {
                    collision.transform.parent.GetComponent<boatCombat>().TakeDamage(damage, false, false, bc.gameID);
                    //Debug.Log(damage);
                }
                else if (speedPrior > 0)
                {
                    int damageOut = (int)(damage * (speedPrior / bc.speed));
                    collision.transform.parent.GetComponent<boatCombat>().TakeDamage(damageOut, false, false, bc.gameID);
                    //Debug.Log(damageOut);
                }

                bc.EndCharge();
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("mine"))
            {
                collision.transform.parent.GetComponent<Mine>().Detonate();
            }
            else
            {
                bc.EndCharge();
            }
        }
    }
}
