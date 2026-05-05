using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    Camera deathCam;
    Canvas deathCan;
    Text sunkT, respawningT, respawningClassT;
    //Button cutterB, brigantineB, frigateB, galleonB;
    GameObject buttonBase;
    RoundManager rMan;
    RoundManagerMenus rManMen;
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
        respawningClassT = transform.Find("deathCanvas/respawningClassText").GetComponent<Text>();

        //cutterB = transform.Find("deathCanvas/Buttons/Cutter").GetComponent<Button>();
        //brigantineB = transform.Find("deathCanvas/Buttons/Brigantine").GetComponent<Button>();
        //frigateB = transform.Find("deathCanvas/Buttons/Frigate").GetComponent<Button>();
        //galleonB = transform.Find("deathCanvas/Buttons/Galleon").GetComponent<Button>();
        buttonBase = transform.Find("deathCanvas/Buttons").gameObject;

        rMan = GameObject.Find("/RoundManager").GetComponent<RoundManager>();
        rManMen = rMan.GetComponent<RoundManagerMenus>();
    }

    public void SwitchPlayerClass(int classIn)
    {
        rMan.ChangeClass(gameID, classIn);
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
        //sunkT.enabled = true;
        //respawningT.enabled = true;

        zoomOut = 0;
        zoomOutTime = 0;
        initialPos = camPosition.position;
    }
    public void Disable()
    {
        deathCam.enabled = false;
        deathCam.GetComponent<AudioListener>().enabled = false;
        //deathCan.enabled = false;
        //sunkT.enabled = false;
        //respawningT.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (deathCam.enabled && !rManMen.menusOpen)
        {
            sunkT.enabled = true;
            respawningT.enabled = true;
            respawningClassT.enabled = true;

            //cutterB.enabled = true;
            //brigantineB.enabled = true;
            //frigateB.enabled = true;
            //galleonB.enabled = true;
            buttonBase.SetActive(true);
        }
        else
        {
            sunkT.enabled = false;
            respawningT.enabled = false;
            respawningClassT.enabled = false;

            //cutterB.enabled = false;
            //brigantineB.enabled = false;
            //frigateB.enabled = false;
            //galleonB.enabled = false;
            buttonBase.SetActive(false);
        }

        zoomOutTime += (Time.deltaTime * 1);
        zoomOut = (15 * zoomOutTime) / (zoomOutTime + 1);
        deathCam.transform.position = initialPos + deathCam.transform.forward * -1 * zoomOut;
        respawnTime = (int)rMan.shipStatuses[gameID].respawnProgress;
        respawnTime += 1;

        respawningT.text = ("Respawning in " + respawnTime);
        respawningClassT.text = "Respawning as: ";
        switch (rMan.shipStatuses[gameID].shipClass)
        {
            case 0:
                respawningClassT.text += "Cutter";
                break;
            case 1:
                respawningClassT.text += "Brigantine";
                break;
            case 2:
                respawningClassT.text += "Frigate";
                break;
            case 3:
                respawningClassT.text += "Galleon";
                break;
        }
    }
}
