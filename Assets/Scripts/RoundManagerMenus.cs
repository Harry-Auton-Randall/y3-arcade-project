using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoundManagerMenus : MonoBehaviour
{
    RoundManager rMan;
    public GameObject scorecard;
    Transform scorecardBase;
    GameObject instance;
    Scorecard[] scorecards;
    GameObject scoreboardPanel, pausePanel;
    Text pauseTitleText, pauseExtraText;

    InputActionMap menuActions;
    InputAction showScoresA, pauseA, autoWinA;
    float autoWinTimer;

    int index;

    bool pauseOpen;
    public bool menusOpen;

    void Awake()
    {
        rMan = GetComponent<RoundManager>();
        scorecardBase = transform.Find("Canvas/ScoreboardPanel/ScorecardBase");
        scoreboardPanel = transform.Find("Canvas/ScoreboardPanel").gameObject;

        pausePanel = transform.Find("Canvas/PausePanel").gameObject;
        pauseTitleText = transform.Find("Canvas/PausePanel/TitleText").GetComponent<Text>();
        pauseExtraText = transform.Find("Canvas/PausePanel/ExtraText").GetComponent<Text>();
        pauseTitleText.text = "PAUSED";
        pauseExtraText.text = "";

        menuActions = InputSystem.actions.FindActionMap("Menus");
        showScoresA = menuActions.FindAction("ShowScores");
        pauseA = menuActions.FindAction("PauseUnpause");
        autoWinA = menuActions.FindAction("AutoWin");
        pauseOpen = false;
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
        pauseA.performed += OnPauseToggle;
    }
    void OnDisable()
    {
        menuActions.Disable();
        pauseA.performed -= OnPauseToggle;
    }

    void OnPauseToggle(InputAction.CallbackContext context)
    {
        if (pauseOpen)
        {
            Unpause();
        }
        else
        {
            PauseNormal();
        }
    }

    public void PauseNormal()
    {
        if (rMan.gameStarted)
        {
            pauseTitleText.text = "PAUSED";
            pauseExtraText.text = "";
            pauseOpen = true;
            Time.timeScale = 0;
        }
    }
    public void PauseGameWin(int mapId)
    {
        if (rMan.gameStarted)
        {
            pauseTitleText.text = "YOU WIN";
            if (mapId != -1 && mapId >= PlayerPrefs.GetInt("unlockedMaps"))
            {
                PlayerPrefs.SetInt("unlockedMaps", mapId + 1);
                PlayerPrefs.Save();
                pauseExtraText.text = ("Level " + (mapId + 2) + " unlocked");
            }
            else
            {
                pauseExtraText.text = "";
            }

            pauseOpen = true;
            Time.timeScale = 0;
        }
    }
    public void PauseGameLoss(int position, string winnerName)
    {
        if (rMan.gameStarted)
        {
            pauseTitleText.text = "You lose";
            pauseExtraText.text = (winnerName + " came 1st, you came " + NumberthFormat(position + 1));

            pauseOpen = true;
            Time.timeScale = 0;
        }
    }
    public void Unpause()
    {
        pauseOpen = false;
        Time.timeScale = 1;
    }

    public void RetryButtonPress()
    {
        rMan.RemakePasser();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void QuitButtonPress()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void Update()
    {
        //switch panel
        if(!rMan.gameStarted)
        {
            scoreboardPanel.SetActive(false);
            pausePanel.SetActive(false);
            menusOpen = false;
        }
        else if (pauseOpen)
        {
            scoreboardPanel.SetActive(false);
            pausePanel.SetActive(true);
            menusOpen = true;
        }
        else if (showScoresA.IsPressed())
        {
            scoreboardPanel.SetActive(true);
            pausePanel.SetActive(false);
            menusOpen = true;
        }
        else
        {
            scoreboardPanel.SetActive(false);
            pausePanel.SetActive(false);
            menusOpen = false;
        }

        //Check progress of autoWin button
        if (autoWinA.IsPressed())
        {
            autoWinTimer += Time.deltaTime;
        }
        else
        {
            autoWinTimer = 0;
        }
        if (autoWinTimer >= 5)
        {
            rMan.PlayerCheatButton = true;
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

    public static string NumberthFormat(int number)
    {
        switch (number)
        {
            case 1:
                return "1st";
                //break;
            case 2:
                return "2nd";
                //break;
            case 3:
                return "3rd";
                //break;
            default:
                if (number >= 21)
                {
                    if (number % 10 == 1)
                    {
                        return (number + "st");
                    }
                    else if (number % 10 == 2)
                    {
                        return (number + "nd");
                    }
                    else if (number % 10 == 3)
                    {
                        return (number + "rd");
                    }
                    else
                    {
                        return (number + "th");
                    }
                }
                else
                {
                    return (number + "th");
                }
                //break;

        }
    }
}
