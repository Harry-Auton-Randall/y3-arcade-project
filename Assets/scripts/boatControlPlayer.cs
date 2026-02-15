using UnityEngine;
using UnityEngine.InputSystem;

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

    //Boat movement stuff
    float movement;
    float rotation;
    boatMove bm;

    boatCombat bc;

    void Awake()
    {
        bm = GetComponent<boatMove>();
        cc = GetComponent<camControl>();
        bc = GetComponent<boatCombat>();

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
        //Runs CamControl's main stuff
        cc.CamControlUpdate();

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
