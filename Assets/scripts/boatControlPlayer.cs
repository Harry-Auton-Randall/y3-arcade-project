using UnityEngine;
using UnityEngine.InputSystem;

//a
public class boatControlPlayer : MonoBehaviour
{
    //reticle stuff
    MeshCollider zeroPlane;
    Transform reticle;
    RaycastHit hit;
    public LayerMask oceanLayer;
    Ray ray;
    camControl cc;

    //Boat movement stuff
    //float movement;
    //float rotation;
    boatMove bm;

    //Input system stuff
    InputActionMap mainActions;
    InputAction movementA, rotationA, camRotCWA, camRotCCWA, shootA;

    boatCombat bc;

    void Awake()
    {
        bm = GetComponent<boatMove>();
        cc = GetComponent<camControl>();
        bc = GetComponent<boatCombat>();

        zeroPlane = GameObject.Find("ocean").GetComponent<MeshCollider>();
        reticle = transform.Find("reticle");

        mainActions = InputSystem.actions.FindActionMap("Main");
        movementA = mainActions.FindAction("Movement");
        rotationA = mainActions.FindAction("Rotation");
        camRotCWA = mainActions.FindAction("CamRotCW");
        camRotCCWA = mainActions.FindAction("CamRotCCW");
        shootA = mainActions.FindAction("Shoot");

        oceanLayer |= (1 << 6);

    }
    void Start()
    {
        
    }

    void OnEnable()
    {
        mainActions.Enable();
        camRotCWA.performed += OnCamRotCW;
        camRotCCWA.performed += OnCamRotCCW;
        shootA.performed += OnShoot;
    }
    void OnDisable()
    {
        mainActions.Enable();
        camRotCWA.performed -= OnCamRotCW;
        camRotCCWA.performed -= OnCamRotCCW;
        shootA.performed -= OnShoot;
    }

    void OnCamRotCW(InputAction.CallbackContext context)
    {
        cc.cameraRotY += 90;
    }
    void OnCamRotCCW(InputAction.CallbackContext context)
    {
        cc.cameraRotY -= 90;
    }
    void OnShoot(InputAction.CallbackContext context)
    {
        bc.Shoot();
    }

    void Update()
    {
        //Runs CamControl's main stuff
        cc.CamControlUpdate();

        //Movement stuff
        bm.SetMovementIn(movementA.ReadValue<float>());
        bm.SetRotationIn(rotationA.ReadValue<float>());

        //Gets where the mouse is on the ocean, and moves the reticle to it. Works for pers and orth
        ray = cc.cam.ScreenPointToRay(cc.mousePositionClamp);
        //Debug.DrawRay(ray.origin, ray.direction * 1000, Color.yellow);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, oceanLayer))
        {
            reticle.transform.position = hit.point;
            bc.aimPos.x = reticle.transform.localPosition.x;
            bc.aimPos.y = reticle.transform.localPosition.z;
        }
        reticle.transform.rotation = Quaternion.Euler(0,cc.cameraRotYGrad,0);
    }
}
