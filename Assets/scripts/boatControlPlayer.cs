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
    LayerMask oceanLayer;
    Ray ray;
    camControl cc;
    Slider reticleCircle;

    //Boat movement stuff
    //float movement;
    //float rotation;
    boatMove bm;

    //Input system stuff
    InputActionMap mainActions;
    InputAction movementA, rotationA, camRotCWA, camRotCCWA, shootA, spyglassA, repairA, specialA;
    InputAction ammoForwardA, ammoBackwardA, numBar1A, numBar2A, numBar3A, numBar4A;

    bool usingSpyglass;
    RectTransform spyglassOutline;
    Image spyglassOutlineImage;
    Vector2 spyglassScale;
    Vector3 spyglassPos;
    float spyglassScaleFac;

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
        specialA = mainActions.FindAction("Special");

        ammoForwardA = mainActions.FindAction("AmmoForward");
        ammoBackwardA = mainActions.FindAction("AmmoBackward");
        numBar1A = mainActions.FindAction("NumBar1");
        numBar2A = mainActions.FindAction("NumBar2");
        numBar3A = mainActions.FindAction("NumBar3");
        numBar4A = mainActions.FindAction("NumBar4");

        spyglassOutline = transform.Find("spyglassCanvas/Image").GetComponent<RectTransform>();
        spyglassOutlineImage = transform.Find("spyglassCanvas/Image").GetComponent<Image>();

        oceanLayer = (1 << LayerMask.NameToLayer("ocean"));

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
        specialA.performed += OnSpecial;

        ammoForwardA.performed += OnAmmoForward;
        ammoBackwardA.performed += OnAmmoBackward;
        numBar1A.performed += OnNumBar1;
        numBar2A.performed += OnNumBar2;
        numBar3A.performed += OnNumBar3;
        numBar4A.performed += OnNumBar4;
    }
    void OnDisable()
    {
        mainActions.Enable();
        camRotCWA.performed -= OnCamRotCW;
        camRotCCWA.performed -= OnCamRotCCW;
        shootA.performed -= OnShoot;
        specialA.performed -= OnSpecial;

        ammoForwardA.performed -= OnAmmoForward;
        ammoBackwardA.performed -= OnAmmoBackward;
        numBar1A.performed -= OnNumBar1;
        numBar2A.performed -= OnNumBar2;
        numBar3A.performed -= OnNumBar3;
        numBar4A.performed -= OnNumBar4;
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
    void OnSpecial(InputAction.CallbackContext context)
    {
        if (!(bc.shipClass == boatCombat.Classes.Frigate && !bc.aimingMortar) && !bc.isRepairing)
        {
            bc.UseSpecial();
        }
    }

    void OnAmmoForward(InputAction.CallbackContext context)
    {
        bc.SwitchAmmo(1);
    }
    void OnAmmoBackward(InputAction.CallbackContext context)
    {
        bc.SwitchAmmo(-1);
    }
    void OnNumBar1(InputAction.CallbackContext context)
    {
        bc.SelectAmmo(0);
    }
    void OnNumBar2(InputAction.CallbackContext context)
    {
        bc.SelectAmmo(1);
    }
    void OnNumBar3(InputAction.CallbackContext context)
    {
        bc.SelectAmmo(2);
    }
    void OnNumBar4(InputAction.CallbackContext context)
    {
        bc.SelectAmmo(3);
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

            if (bc.shipClass == boatCombat.Classes.Frigate)
            {
                if (usingSpyglass && !bc.isRepairing)
                {
                    bc.mortarAimPos.x = reticle.transform.localPosition.x;
                    bc.mortarAimPos.y = reticle.transform.localPosition.z;
                    bc.aimingMortar = true;
                }
                else
                {
                    bc.mortarAimPos.x = 0;
                    bc.mortarAimPos.y = 0;
                    bc.aimingMortar = false;
                }
                bc.MoveMortarOutline();
            }
        }
        reticle.transform.rotation = Quaternion.Euler(0,cc.cameraRotYGrad,0);
        reticleCircle.value = bc.reloadProgress;

        //Spyglass canvas stuff
        if (usingSpyglass)
        {
            spyglassOutlineImage.enabled = true;
            spyglassScaleFac = (cc.screenSize.y / 90);
            spyglassScale.y = spyglassScaleFac * (bc.shipLength + 6) * (4096 / 80);
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
