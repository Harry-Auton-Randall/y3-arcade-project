using UnityEngine;
using UnityEngine.InputSystem;

public class boatMove : MonoBehaviour
{
    public PlayerInput pi;
    Rigidbody rb;
    float speed;
    float rotate;
    float movementIn;
    float rotationIn;

    public float outp;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //pi = GetComponent<PlayerInput>();
    }

    void Start()
    {

    }

    public void SetStats(float speed1, float rotate1)
    {
        speed = speed1;
        rotate = rotate1;
    }
    void OnMovement(InputValue value)
    {
        movementIn = value.Get<float>();
        if (movementIn < 0)
        {
            movementIn = movementIn / 3.0f;
        }
    }
    void OnRotation(InputValue value)
    {
        rotationIn = value.Get<float>();
    }

    void FixedUpdate()
    {
        rb.AddRelativeForce(0, 0, rb.mass * speed * 0.5f * movementIn);
        rb.AddTorque(Vector3.up * rb.inertiaTensor.y * rotate * Mathf.Deg2Rad * 0.5f * rotationIn);
        //no need to worry about tweaking inertiaTensorRotation bc the boat's rotation is locked on the x and z axes
        //Solutions if i need to unlock rotation: 1. override inertiaTensor and inertiaTensorRotation in Start, 2. get chatgpt to make code that un-adjusts the tensor for the rotation

        outp = rb.angularVelocity.y * Mathf.Rad2Deg;
    }
}
