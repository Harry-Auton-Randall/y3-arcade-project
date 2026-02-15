using UnityEngine;

public class testCannon : MonoBehaviour
{
    GameObject cannon;
    cannonShoot cs;
    float t;
    void Awake()
    {
        cannon = transform.Find("cannon").gameObject;
        cs = cannon.GetComponent<cannonShoot>();
    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime;
        if (t > 2.5f)
        {
            cs.Shoot(40, 0.75f, false, 0);
            t = 0;
        }
        
    }
}
