using UnityEngine;

public class DeathScreen : MonoBehaviour
{
    Camera deathCam;
    RespawnManager rm;
    public float respawnTime;
    float zoomOutTime, zoomOut;
    Vector3 initialPos;
    int gameID;
    
    void Awake()
    {
        deathCam = transform.Find("deathCamera").GetComponent<Camera>();
        rm = GameObject.Find("/RoundManager").GetComponent<RespawnManager>();
    }

    public void Enable(int idIn, Transform camPosition)
    {
        gameID = idIn;
        deathCam.transform.position = camPosition.position;
        deathCam.transform.rotation = camPosition.rotation;
        deathCam.enabled = true;
        deathCam.GetComponent<AudioListener>().enabled = true;
        
        zoomOut = 0;
        zoomOutTime = 0;
        initialPos = camPosition.position;
    }
    public void Disable()
    {
        deathCam.enabled = false;
        deathCam.GetComponent<AudioListener>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        zoomOutTime += (Time.deltaTime * 1);
        zoomOut = (15 * zoomOutTime) / (zoomOutTime + 1);
        deathCam.transform.position = initialPos + deathCam.transform.forward * -1 * zoomOut;
        respawnTime = rm.shipStatuses[gameID].respawnProgress;
    }
}
