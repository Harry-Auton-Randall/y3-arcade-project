using UnityEngine;
using UnityEngine.InputSystem;

public class boatControlPlayer : MonoBehaviour
{
    //Camera stuff
    Vector2 mousePosition, mousePositionAdj;
    Vector2 screenSize;
    Vector3 camPosition;
    Vector3 camBaseRotation;

    Transform camBase;
    Transform camMove;
    Camera persCam;
    Camera orthCam;
    public bool cameraPers = true;
    public bool cameraTilt = true;
    public int cameraRotY = 0;
    float cameraRotYGrad;

    //reticle stuff
    MeshCollider zeroPlane;
    Transform reticle;
    RaycastHit hit;
    LayerMask oceanLayer;
    Ray ray;

    //Boat movement stuff
    float movement;
    float rotation;
    boatMove bm;

    void Awake()
    {
        bm = GetComponent<boatMove>();
        screenSize.x = Screen.width;
        screenSize.y = Screen.height;
        camBase = transform.Find("CameraBase");
        camMove = transform.Find("CameraBase/CameraMove");
        persCam = transform.Find("CameraBase/CameraMove/CameraRot/PersCamera").GetComponent<Camera>();

        
        camPosition = camMove.localPosition;

        zeroPlane = GameObject.Find("ocean").GetComponent<MeshCollider>();
        reticle = transform.Find("reticle");

        oceanLayer |= (1 << 6);

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
        //rotates CamBase to be pointing up
        camBaseRotation = transform.localEulerAngles;
        camBaseRotation.y = (camBaseRotation.y * -1) + cameraRotY;
        camBase.localEulerAngles = camBaseRotation;

        //gets mousePosition and mousePositionAdj values
        mousePosition = Mouse.current.position.ReadValue();
        mousePositionAdj.x = Mathf.Clamp(((mousePosition.x * 2) / screenSize.x) - 1, -1, 1);
        mousePositionAdj.y = Mathf.Clamp(((mousePosition.y * 2) / screenSize.y) - 1, -1, 1);

        //moves the camera with mousePosition
        //camPosition = camMove.localPosition;
        camPosition.x = mousePositionAdj.x * 13.5f * (screenSize.x / screenSize.y);
        camPosition.z = (mousePositionAdj.y * 13.5f);
        if (cameraPers && cameraTilt)
        {
            camPosition.z = camPosition.z - 3.25f;
        }
        //13.5 = extra view distance in vertical direction. total view area = 2*(22.5+13.5) = 72 vertical and (72 * 16/9 =) 128m horizontal
        camMove.localPosition = camPosition;

        ray = persCam.ScreenPointToRay(mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.yellow);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, oceanLayer))
        {
            reticle.transform.position = hit.point;
        }
    }
}
