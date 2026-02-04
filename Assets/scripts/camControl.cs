using UnityEngine;
using UnityEngine.InputSystem;

//Handles camera settings (tilt, y rotation, orth vs pers) and camera movement with the mouse
public class camControl : MonoBehaviour
{
    public Vector2 mousePosition, mousePositionClamp, mousePositionAdj;
    Vector2 screenSize;
    Vector3 camPosition;
    Vector3 camBaseRotation;

    Transform camBase, camMove, camRot;
    public Camera cam;
    public bool cameraOrth = false;
    public bool cameraTilt = true;
    public int cameraRotY = 0;
    float cameraRotYGrad;
    float camRotVel;

    void Awake()
    {
        screenSize.x = Screen.width;
        screenSize.y = Screen.height;
        camBase = transform.Find("CameraBase");
        camMove = transform.Find("CameraBase/CameraMove");
        camRot = transform.Find("CameraBase/CameraMove/CameraRot");

        cam = transform.Find("CameraBase/CameraMove/CameraRot/PlayerCamera").GetComponent<Camera>();

        camPosition = camMove.localPosition;
    }

    void OnCamRotCW()
    {
        cameraRotY = cameraRotY + 90;
    }
    void OnCamRotCCW()
    {
        cameraRotY = cameraRotY - 90;
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
        camBaseRotation = transform.localEulerAngles;
        camBaseRotation.y = (camBaseRotation.y * -1) + cameraRotYGrad;
        camBase.localEulerAngles = camBaseRotation;

        //gets mousePosition and mousePositionAdj values
        mousePosition = Mouse.current.position.ReadValue();
        //mousePositionAdj.x = Mathf.Clamp(((mousePosition.x * 2) / screenSize.x) - 1, -1, 1);
        //mousePositionAdj.y = Mathf.Clamp(((mousePosition.y * 2) / screenSize.y) - 1, -1, 1);
        mousePositionClamp.x = Mathf.Clamp(mousePosition.x, 0, screenSize.x);
        mousePositionClamp.y = Mathf.Clamp(mousePosition.y, 0, screenSize.y);
        mousePositionAdj.x = 2 * (mousePositionClamp.x / screenSize.x) - 1;
        mousePositionAdj.y = 2 * (mousePositionClamp.y / screenSize.y) - 1;

        //moves the camera with mousePosition
        //camPosition = camMove.localPosition;
        camPosition.x = mousePositionAdj.x * 13.5f * (screenSize.x / screenSize.y);
        camPosition.z = (mousePositionAdj.y * 13.5f);
        if (!cameraOrth && cameraTilt)
        {
            camPosition.z = camPosition.z - 3.25f;
        }
        //13.5 = extra view distance in vertical direction. total view area = 2*(22.5+13.5) = 72 vertical and (72 * 16/9 =) 128m horizontal
        camMove.localPosition = camPosition;
    }
}
