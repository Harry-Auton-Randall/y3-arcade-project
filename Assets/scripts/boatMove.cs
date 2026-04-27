using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class boatMove : MonoBehaviour
{
    //public PlayerInput pi;
    public Rigidbody rb;
    float speed;
    float rotate;
    float movementIn;
    float rotationIn;

    //For getting pushed by pushing zones
    Vector3 globalForce, totalPushForce;
    List<GameObject> pushingZones;
    float pushDirMag;

    //For brigantine's special attack
    public bool charge = false;
    public float chargeSpeedMult = 1f;

    public float outSpd, outGlbSpd, outRotSpd;
    public Vector3 globalMoveDir, localMoveDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //pi = GetComponent<PlayerInput>();
        pushingZones = new List<GameObject>();
    }

    void Start()
    {
        //overrides inertiaTensor and inertiaTensorRotation of the rigidbody, otherwise rotation slightly affects the x and z axes
        rb.inertiaTensor = new Vector3(rb.mass, rb.mass, rb.mass);
        rb.inertiaTensorRotation = Quaternion.Euler(Vector3.zero);
    }

    public void SetStats(float speed1, float rotate1)
    {
        speed = speed1;
        rotate = rotate1;
    }

    public void SetMovementIn(float value)
    {
        movementIn = value;
        if (movementIn < 0)
        {
            movementIn = movementIn / 3.0f;
        }
    }
    public void SetRotationIn(float value)
    {
        rotationIn = value;
    }

    void FixedUpdate()
    {
        if (!charge)
        {
            rb.AddRelativeForce(0, 0, rb.mass * speed * rb.linearDamping * movementIn);
            globalForce = this.transform.forward * (rb.mass * speed * rb.linearDamping * movementIn);
        }
        else
        {
            rb.AddRelativeForce(0, 0, rb.mass * speed * rb.linearDamping * chargeSpeedMult);
            globalForce = this.transform.forward * (rb.mass * speed * rb.linearDamping * chargeSpeedMult);
        }
        rb.AddTorque(Vector3.up * rb.inertiaTensor.y * rotate * Mathf.Deg2Rad * rb.angularDamping * rotationIn);

        //PushingZone stuff

        totalPushForce = Vector3.zero;
        for (int i = 0; i < pushingZones.Count; i++)
        {
            totalPushForce += (pushingZones[i].transform.forward);
        }
        if (totalPushForce != Vector3.zero)
        {
            totalPushForce.Normalize();
            //pushDirMag = the amount of force being applied to the rb, in totalPushForce's direction
            pushDirMag = Vector3.Dot(globalForce, totalPushForce);
            if (pushDirMag > 0)
            {
                rb.AddForce(totalPushForce * pushDirMag * -1);
            }
            rb.AddForce(totalPushForce * rb.mass * rb.linearDamping * -4);
        }



        outSpd = Vector3.Dot(transform.forward, rb.linearVelocity);
        outGlbSpd = rb.linearVelocity.magnitude;
        outRotSpd = rb.angularVelocity.y * Mathf.Rad2Deg;

        globalMoveDir = rb.linearVelocity;
        localMoveDir = transform.InverseTransformDirection(globalMoveDir);
        globalMoveDir.y = 0;
        localMoveDir.y = 0;

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
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
}
