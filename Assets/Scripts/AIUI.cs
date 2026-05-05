using UnityEngine;
using UnityEngine.UI;

public class AIUI : MonoBehaviour
{
    boatCombat bc;
    Vector3 rotationVec;

    Slider healthSlider;
    Text nameText;

    void Awake()
    {
        bc = transform.parent.GetComponent<boatCombat>();
        healthSlider = transform.Find("UICanvas/HealthSlider").GetComponent<Slider>();
        nameText = transform.Find("UICanvas/NameText").GetComponent<Text>();
        rotationVec = Vector3.zero;
    }
    void Start()
    {
        nameText.text = bc.playerName;
    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = ((float)bc.health / bc.maxHealth);
    }
    void LateUpdate()
    {
        //Check if player camera is angled or not (doesnt happen yet)
        rotationVec.x = 70;
        rotationVec.y = bc.rMan.playerCamRotation;
        transform.eulerAngles = rotationVec;
    }
}
