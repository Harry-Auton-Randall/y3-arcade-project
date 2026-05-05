using UnityEngine;
using UnityEngine.UI;

public class Scorecard : MonoBehaviour
{
    Text rankText, contentText, scoreText, classText;
    Color colour;

    void Awake()
    {
        rankText = transform.Find("RankText").GetComponent<Text>();
        contentText = transform.Find("ContentText").GetComponent<Text>();
        scoreText = transform.Find("ScoreText").GetComponent<Text>();
        classText = transform.Find("ClassText").GetComponent<Text>();
    }
    public void Init(int rankIn)
    {
        rankText.text = (rankIn + ".");
    }

    public void SetText(string nameIn, int scoreIn, int classIn)
    {
        contentText.text = nameIn;
        //if (extraIn == 1)
        //{
        //    contentText.text += (" (SUNK)");
        //}
        //else if (extraIn == 2)
        //{
        //    contentText.text += (" (OUT)");
        //}

        scoreText.text = scoreIn.ToString();

        switch(classIn)
        {
            case 0:
                classText.text = "Cutter";
                break;
            case 1:
                classText.text = "Brigantine";
                break;
            case 2:
                classText.text = "Frigate";
                break;
            case 3:
                classText.text = "Galleon";
                break;
        }
        SetColours("#ffffff");
    }
    public void SetSunk()
    {
        contentText.text += (" (SUNK)");
        SetColours("#b6b6b6");
    }
    public void SetOut()
    {
        contentText.text += (" (OUT)");
        SetColours("#999999");
    }
    void SetColours(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out colour);
        rankText.color = colour;
        contentText.color = colour;
        scoreText.color = colour;
        classText.color = colour;

    }
}
