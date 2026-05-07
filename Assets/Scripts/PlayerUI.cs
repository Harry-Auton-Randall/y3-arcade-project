using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    boatCombat bc;
    Vector3 rotationVec;

    Slider healthSlider, specialSlider;
    Image healthBar, specialBar;
    Color colour;

    Text scoreboardText1, scoreboardText2, scoreboardTextTime;

    Transform compassNorth;

    Transform compassObjective;
    Transform objectiveLocation;
    float objectiveAngle;

    void Awake()
    {
        bc = transform.parent.GetComponent<boatCombat>();

        healthSlider = transform.Find("UICanvas/HealthSlider").GetComponent<Slider>();
        healthBar = transform.Find("UICanvas/HealthSlider/Fill Area/Fill").GetComponent<Image>();

        specialSlider = transform.Find("UICanvas/SpecialSlider").GetComponent<Slider>();
        specialBar = transform.Find("UICanvas/SpecialSlider/Fill Area/Fill").GetComponent<Image>();

        scoreboardText1 = transform.Find("UICanvas/ScoreboardText1").GetComponent<Text>();
        scoreboardText2 = transform.Find("UICanvas/ScoreboardText2").GetComponent<Text>();
        scoreboardTextTime = transform.Find("UICanvas/ScoreboardTextTime").GetComponent<Text>();

        compassNorth = transform.Find("UICanvas/CompassCentre/NorthHead/NorthIcon");

        compassObjective = transform.Find("UICanvas/CompassCentre/ObjectiveHead/ObjectiveIcon");

        rotationVec = Vector3.zero;
    }
    void Start()
    {
        if (bc.rMan.mode == 0)
        {
            objectiveLocation = GameObject.Find("/DeathmatchCentre").transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = ((float)bc.health / bc.maxHealth);
        healthBar.color = new Color((1 - healthSlider.value) * 2, healthSlider.value * 2, 0f, 1f);

        specialSlider.value = (bc.specialReloadFloat);
        if (specialSlider.value == 1)
        {
            ColorUtility.TryParseHtmlString("#FFFFFF", out colour);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#b6b6b6", out colour);
        }
        specialBar.color = colour;
    }
    void LateUpdate()
    {
        rotationVec.z = bc.rMan.playerCamRotation;
        compassNorth.transform.parent.eulerAngles = rotationVec;
        compassNorth.transform.eulerAngles = Vector3.zero;

        rotationVec.z -= Vector3.SignedAngle(Vector3.forward, (objectiveLocation.position - transform.parent.position), Vector3.up);
        compassObjective.transform.parent.eulerAngles = rotationVec;
        compassObjective.transform.eulerAngles = Vector3.zero;

        scoreboardText1.text = ScoreboardFormat(0);
        if (bc.rMan.scoresSorted[0] == 0)
        {
            //Player's in first
            scoreboardText2.text = ScoreboardFormat(1);
        }
        else
        {
            for (int i = 0; i < bc.rMan.scoresSorted.Length; i++)
            {
                if (bc.rMan.scoresSorted[i] == 0)
                {
                    scoreboardText2.text = ScoreboardFormat(i);
                    break;
                }
            }
        }

        if (!bc.rMan.scoreOrTime)
        {
            scoreboardTextTime.text = FormatTime(bc.rMan.timeLeft);
        }
        else
        {
            scoreboardTextTime.text = "";
        }
    }
    string ScoreboardFormat(int id)
    {
        string str;

        int idP = id+1;
        switch (idP)
        {
            case 1:
                str = "1st: ";
                break;
            case 2:
                str = "2nd: ";
                break;
            case 3:
                str = "3rd: ";
                break;
            default:
                if (idP >= 21)
                {
                    if (idP % 10 == 1)
                    {
                        str = (idP + "st: ");
                    }
                    else if (idP % 10 == 2)
                    {
                        str = (idP + "nd: ");
                    }
                    else if (idP % 10 == 3)
                    {
                        str = (idP + "rd: ");
                    }
                    else
                    {
                        str = (idP + "th: ");
                    }
                }
                else
                {
                    str = (idP + "th: ");
                }
                break;

        }

        str += bc.rMan.shipStatuses[bc.rMan.scoresSorted[id]].name;

        str += (" (" + bc.rMan.shipStatuses[bc.rMan.scoresSorted[id]].score);
        if (bc.rMan.scoreOrTime)
        {
            str += ("/" + bc.rMan.scoreTarget);
        }
        str += ")";
        return str;

    }

    public static string FormatTime(float input)
    {
        int minute, second;

        minute = Mathf.FloorToInt((input+1) / 60);

        second = Mathf.CeilToInt(input) % 60;

        return (minute + ":" + second.ToString("D2"));
    }
}
