using UnityEngine;
using UnityEngine.InputSystem;

public class RoundManagerMenus : MonoBehaviour
{
    RoundManager rMan;
    public GameObject scorecard;
    Transform scorecardBase;
    GameObject instance;
    Scorecard[] scorecards;
    GameObject scoreboardPanel;

    InputActionMap menuActions;
    InputAction showScoresA;

    int index;

    public bool menusOpen;

    void Awake()
    {
        rMan = GetComponent<RoundManager>();
        scorecardBase = transform.Find("Canvas/ScoreboardPanel/ScorecardBase");
        scoreboardPanel = transform.Find("Canvas/ScoreboardPanel").gameObject;

        menuActions = InputSystem.actions.FindActionMap("Menus");
        showScoresA = menuActions.FindAction("ShowScores");
    }
    void Start()
    {
        scorecards = new Scorecard[rMan.shipStatuses.Length];
        for (int i = 0; i < scorecards.Length; i++)
        {
            instance = Instantiate(scorecard, scorecardBase);
            instance.transform.localPosition = new Vector3(0, -22 * i, 0);
            scorecards[i] = instance.GetComponent<Scorecard>();
            scorecards[i].Init(i + 1);
        }

        scoreboardPanel.SetActive(false);
    }
    void OnEnable()
    {
        menuActions.Enable();
    }
    void OnDisable()
    {
        menuActions.Disable();
    }

    void Update()
    {
        if(showScoresA.IsPressed() && rMan.gameStarted)
        {
            scoreboardPanel.SetActive(true);
            menusOpen = true;
        }
        else
        {
            scoreboardPanel.SetActive(false);
            menusOpen = false;
        }
    }

    void LateUpdate()
    {
        for (int i = 0; i < scorecards.Length; i++)
        {
            index = rMan.scoresSorted[i];
            if (!rMan.shipStatuses[index].hasLives && !rMan.shipStatuses[index].isAlive && rMan.lives)
            {
                
            }
            scorecards[i].SetText(rMan.shipStatuses[index].name, rMan.shipStatuses[index].score, rMan.shipStatuses[index].shipClass);
            if (!rMan.shipStatuses[index].isAlive)
            {
                if (!rMan.shipStatuses[index].hasLives && rMan.lives)
                {
                    scorecards[i].SetOut();
                }
                else
                {
                    scorecards[i].SetSunk();
                }
            }
        }
    }
}
