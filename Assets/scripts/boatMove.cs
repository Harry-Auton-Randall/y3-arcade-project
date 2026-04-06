using UnityEngine;
using UnityEngine.InputSystem;

public class boatMove : MonoBehaviour
{
    //public PlayerInput pi;
    public Rigidbody rb;
    float speed;
    float rotate;
    float movementIn;
    float rotationIn;

    public float outSpd, outGlbSpd, outRotSpd;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //pi = GetComponent<PlayerInput>();
    }

    void Start()
    {
        //overrides inertiaTensor and inertiaTensorRotation of the rigidbody, otherwise rotation slightly affects the x and z axes
        rb.inertiaTensor = new Vector3(1.0f, 1.0f, 1.0f);
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
        rb.AddRelativeForce(0, 0, rb.mass * speed * rb.linearDamping * movementIn);
        rb.AddTorque(Vector3.up * rb.inertiaTensor.y * rotate * Mathf.Deg2Rad * rb.angularDamping * rotationIn);

        outSpd = Vector3.Dot(transform.forward, rb.linearVelocity);
        outGlbSpd = rb.linearVelocity.magnitude;
        outRotSpd = rb.angularVelocity.y * Mathf.Rad2Deg;
    }
}
