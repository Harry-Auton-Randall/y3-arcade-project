using UnityEngine;
using UnityEngine.InputSystem;

//Handles camera settings (tilt, y rotation, orth vs pers) and camera movement with the mouse
public class camControl : MonoBehaviour
{
    public Vector2 mousePosition, mousePositionClamp, mousePositionAdj;
    public Vector2 screenSize;
    public Vector3 camPosition;
    Vector3 camBaseRotation = new Vector3(0f, 0f, 0f);

    Transform camBase, camMove, camRot;
    public Camera cam, camSpy;
    public bool cameraSpy = false;
    public bool cameraOrth = false;
    public bool cameraTilt = true;
    public int cameraRotY = 0;
    public float cameraRotYGrad;
    float camRotVel;

    boatCombat bc;

    void Awake()
    {
        screenSize.x = Screen.width;
        screenSize.y = Screen.height;
        camBase = transform.Find("CameraBase");
        camMove = transform.Find("CameraBase/CameraMove");
        camRot = transform.Find("CameraBase/CameraMove/CameraRot");

        cam = transform.Find("CameraBase/CameraMove/CameraRot/PlayerCamera").GetComponent<Camera>();
        camSpy = transform.Find("CameraBase/CameraMove/CameraSpyglass").GetComponent<Camera>();

        camPosition = camMove.localPosition;

        bc = GetComponent<boatCombat>();
    }

    public void CamControlUpdate()
    {
        //TEMPORARY: adjusts camera based on settings
        cam.orthographic = cameraOrth;
        if (cameraTilt)
        {
            camRot.transform.localEulerAngles = new Vector3(-20, 0, 0);
        }
        else
        {
            camRot.transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        //Moves cameraRotYGrad towards cameraRotY
        cameraRotYGrad = Mathf.SmoothDamp(cameraRotYGrad, cameraRotY, ref camRotVel, 0.1f, 99999, Time.deltaTime);

        //rotates CamBase to be pointing in the right direction
        camBaseRotation.y = cameraRotYGrad;
        camBase.eulerAngles = camBaseRotation;

        //gets mousePosition and mousePositionAdj values
        mousePosition = Mouse.current.position.ReadValue();
        mousePositionClamp.x = Mathf.Clamp(mousePosition.x, 0, screenSize.x);
        mousePositionClamp.y = Mathf.Clamp(mousePosition.y, 0, screenSize.y);
        mousePositionAdj.x = 2 * (mousePositionClamp.x / screenSize.x) - 1;
        mousePositionAdj.y = 2 * (mousePositionClamp.y / screenSize.y) - 1;

        //moves the camera with mousePosition
        camPosition.x = mousePositionAdj.x * 13.5f * (screenSize.x / screenSize.y);
        camPosition.z = (mousePositionAdj.y * 13.5f);
        if (cameraSpy)
        {
            camPosition.x *= 2;
            camPosition.z *= 2;
            cam.enabled = false;
            camSpy.enabled = true;
        }
        else
        {
            cam.enabled = true;
            camSpy.enabled = false;
        }
        if (!cameraOrth && cameraTilt && !cameraSpy)
        {
            camPosition.z = camPosition.z - 3.25f;
        }
        //13.5 = extra view distance in vertical direction. total view area = 2*(22.5+13.5) = 72 vertical and (72 * 16/9 =) 128m horizontal
        camMove.localPosition = camPosition;

        bc.rMan.playerCamRotation = camRot.eulerAngles.y;
    }
}
