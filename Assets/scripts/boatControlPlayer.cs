using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    Slider reticleCircle;

    //Boat movement stuff
    //float movement;
    //float rotation;
    boatMove bm;

    //Input system stuff
    InputActionMap mainActions;
    InputAction movementA, rotationA, camRotCWA, camRotCCWA, shootA, spyglassA, repairA;

    bool usingSpyglass;
    RectTransform spyglassOutline;
    Image spyglassOutlineImage;
    Vector2 spyglassScale;
    Vector3 spyglassPos;
    float spyglassScaleFac;
    public float shipLength;

    boatCombat bc;

    void Awake()
    {
        bm = GetComponent<boatMove>();
        cc = GetComponent<camControl>();
        bc = GetComponent<boatCombat>();

        zeroPlane = GameObject.Find("ocean").GetComponent<MeshCollider>();
        reticle = transform.Find("reticle");
        reticleCircle = transform.Find("reticle/reticleCanvas/Slider").GetComponent<Slider>();

        mainActions = InputSystem.actions.FindActionMap("Main");
        movementA = mainActions.FindAction("Movement");
        rotationA = mainActions.FindAction("Rotation");
        camRotCWA = mainActions.FindAction("CamRotCW");
        camRotCCWA = mainActions.FindAction("CamRotCCW");
        shootA = mainActions.FindAction("Shoot");
        spyglassA = mainActions.FindAction("Spyglass");
        repairA = mainActions.FindAction("Repair");

        spyglassOutline = transform.Find("spyglassCanvas/Image").GetComponent<RectTransform>();
        spyglassOutlineImage = transform.Find("spyglassCanvas/Image").GetComponent<Image>();

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
        if (!usingSpyglass)
        {
            bc.Shoot();
        }
    }

    void Update()
    {
        //spyglass
        if (spyglassA.ReadValue<float>() > 0f)
        {
            usingSpyglass = true;
        }
        else
        {
            usingSpyglass = false;
        }
        cc.cameraSpy = usingSpyglass;

        //Runs CamControl's main stuff
        cc.CamControlUpdate();

        //repairing
        bc.AttemptRepair(repairA.ReadValue<float>());

        //Movement stuff
        if (!usingSpyglass && !bc.isRepairing && !bc.chained)
        {
            bm.SetMovementIn(movementA.ReadValue<float>());
            bm.SetRotationIn(rotationA.ReadValue<float>());
        }
        else
        {
            bm.SetMovementIn(0);
            bm.SetRotationIn(0);
        }

        //Gets where the mouse is on the ocean, and moves the reticle to it. Works for pers and orth
        if (!usingSpyglass)
        {
            ray = cc.cam.ScreenPointToRay(cc.mousePositionClamp);
        }
        else
        {
            ray = cc.camSpy.ScreenPointToRay(cc.mousePositionClamp);
        }
        //Debug.DrawRay(ray.origin, ray.direction * 1000, Color.yellow);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, oceanLayer))
        {
            reticle.transform.position = hit.point;
            if (!usingSpyglass && !bc.isRepairing)
            {
                bc.aimPos.x = reticle.transform.localPosition.x;
                bc.aimPos.y = reticle.transform.localPosition.z;
            }
            else
            {
                bc.aimPos.x = 0;
                bc.aimPos.y = 0;
            }
        }
        reticle.transform.rotation = Quaternion.Euler(0,cc.cameraRotYGrad,0);
        reticleCircle.value = bc.reloadProgress;

        //Spyglass canvas stuff
        if (usingSpyglass)
        {
            spyglassOutlineImage.enabled = true;
            spyglassScaleFac = (cc.screenSize.y / 90);
            spyglassScale.y = spyglassScaleFac * (shipLength + 6) * (4096 / 80);
            spyglassScale.x = spyglassScale.y;
            spyglassOutline.sizeDelta = spyglassScale;

            spyglassPos.x = -(spyglassScaleFac * cc.camPosition.x) + (cc.screenSize.x / 2);
            spyglassPos.y = -(spyglassScaleFac * cc.camPosition.z) + (cc.screenSize.y / 2);
            spyglassOutline.position = spyglassPos;

            spyglassOutline.rotation = Quaternion.Euler(0, 0, 
                ((Mathf.Rad2Deg * Mathf.Atan2(reticle.transform.localPosition.z, reticle.transform.localPosition.x)) - transform.eulerAngles.y) + cc.cameraRotYGrad);
        }
        else
        {
            spyglassOutlineImage.enabled = false;
        }


    }
}
