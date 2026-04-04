using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    Camera deathCam;
    Canvas deathCan;
    Text sunkT, respawningT;
    RoundManager rm;
    public int respawnTime;
    float zoomOutTime, zoomOut;
    Vector3 initialPos;
    int gameID;
    
    void Awake()
    {
        deathCam = transform.Find("deathCamera").GetComponent<Camera>();
        deathCan = transform.Find("deathCanvas").GetComponent<Canvas>();
        sunkT = transform.Find("deathCanvas/sunkText").GetComponent<Text>();
        respawningT = transform.Find("deathCanvas/respawningText").GetComponent<Text>();
        rm = GameObject.Find("/RoundManager").GetComponent<RoundManager>();
    }

    public void Enable(int idIn, Transform camPosition)
    {
        gameID = idIn;
        deathCam.transform.position = camPosition.position;
        deathCam.transform.rotation = camPosition.rotation;
        deathCam.enabled = true;
        deathCam.GetComponent<AudioListener>().enabled = true;
        //deathCan.enabled = true;
        //sunkT.fontSize = 1;
        //respawningT.fontSize = 1;
        //sunkT.fontSize = 76;
        //respawningT.fontSize = 38;
        sunkT.enabled = true;
        respawningT.enabled = true;

        zoomOut = 0;
        zoomOutTime = 0;
        initialPos = camPosition.position;
    }
    public void Disable()
    {
        deathCam.enabled = false;
        deathCam.GetComponent<AudioListener>().enabled = false;
        //deathCan.enabled = false;
        sunkT.enabled = false;
        respawningT.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        zoomOutTime += (Time.deltaTime * 1);
        zoomOut = (15 * zoomOutTime) / (zoomOutTime + 1);
        deathCam.transform.position = initialPos + deathCam.transform.forward * -1 * zoomOut;
        respawnTime = (int)rm.shipStatuses[gameID].respawnProgress;
        respawnTime += 1;

        respawningT.text = ("Respawning in " + respawnTime);
    }
}
