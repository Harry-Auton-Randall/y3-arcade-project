using UnityEngine;

public class boatRamTip : MonoBehaviour
{
    boatCombat bc;
    int damage = 10;
    void Awake()
    {
        bc = transform.parent.GetComponent<boatCombat>();
    }

    void OnTriggerEnter(Collider collision)
    {
        if (bc.bm.charge && !collision.isTrigger)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("boat"))
            {
                //figure out how much damage to do based on z-axis speed
                float speed = Vector3.Dot(bc.transform.forward, bc.rb.linearVelocity);
                if (speed >= bc.speed)
                {
                    collision.transform.parent.GetComponent<boatCombat>().TakeDamage(damage, false, false, bc.gameID);
                }
                else if (speed > 0)
                {
                    int damageOut = (int)(damage * (speed / bc.speed));
                    collision.transform.parent.GetComponent<boatCombat>().TakeDamage(damageOut, false, false, bc.gameID);
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
