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

    void Awake()
    {
        bm = GetComponent<boatMove>();
        cc = GetComponent<camControl>();

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
        //Gets where the mouse is on the ocean, and moves the reticle to it. Works for pers and orth
        ray = cc.cam.ScreenPointToRay(cc.mousePositionClamp);
        //Debug.DrawRay(ray.origin, ray.direction * 1000, Color.yellow);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, oceanLayer))
        {
            reticle.transform.position = hit.point;
        }
    }
}
