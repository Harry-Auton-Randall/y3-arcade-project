using UnityEngine;

public class testCannon : MonoBehaviour
{
    GameObject cannon;
    cannonShoot cs;
    float t;
    Collider ignore;

    void Awake()
    {
        cannon = transform.Find("cannon").gameObject;
        cs = cannon.GetComponent<cannonShoot>();
        ignore = GetComponent<Collider>();
    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime;
        if (t > 2.5f)
        {
            cs.Shoot(40, 0.75f, false, 0, ignore);
            t = 0;
        }
        
    }
}
