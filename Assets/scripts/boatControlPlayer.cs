using UnityEngine;
using UnityEngine.InputSystem;

public class boatControlPlayer : MonoBehaviour
{
    Vector2 mousePosition;
    Vector2 screenSize;
    float movement;
    float rotation;

    boatMove bm;
    Transform camBase;
    Transform camMove;

    Vector3 camPosition;
    Vector3 camBaseRotation;

    void Awake()
    {
        bm = GetComponent<boatMove>();
        screenSize.x = Screen.width;
        screenSize.y = Screen.height;
        camBase = transform.Find("CameraBase");
        camMove = transform.Find("CameraBase/CameraMove");
    }
    void Start()
    {
        
    }
    void OnMovement(InputValue value)
    {
        movement = value.Get<float>();
        bm.SetMovementIn(movement);

    }
    void OnRotation(InputValue value)
    {
        rotation = value.Get<float>();
        bm.SetRotationIn(rotation);
    }

    void Update()
    {
        camBaseRotation = transform.localEulerAngles;
        camBaseRotation.y = camBaseRotation.y * -1;
        camBase.localEulerAngles = camBaseRotation;

        mousePosition = Mouse.current.position.ReadValue();
        mousePosition.x = Mathf.Clamp(((mousePosition.x * 2) / screenSize.x) - 1, -1, 1);
        mousePosition.y = Mathf.Clamp(((mousePosition.y * 2) / screenSize.y) - 1, -1, 1);

        camPosition = camMove.localPosition;
        camPosition.x = mousePosition.x * 13.5f * (screenSize.x / screenSize.y);
        camPosition.z = (mousePosition.y * 13.5f) - 3.25f;
        //13.5 = extra view distance in vertical direction. total view area = 2*(22.5+13.5) = 72 vertical and (72 * 16/9 =) 128m horizontal
        camMove.localPosition = camPosition;



    }
}
