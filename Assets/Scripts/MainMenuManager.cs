using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MainMenuManager : MonoBehaviour
{
    GameObject[] panels;
    int panel;
    Transform levelButtonBase;

    public GameObject levelButton, dataPasser;
    GameObject instance;
    Button[] levelButtons;
    LevelInfo[] levels;

    void Awake()
    {
        Time.timeScale = 1;

        levelButtonBase = transform.Find("SelectPanel/LevelButtons");

        panels = new GameObject[]
        {
            transform.Find("MainPanel").gameObject,
            transform.Find("SelectPanel").gameObject
        };

        SwitchPanel(0);

        levels = new LevelInfo[] { 
            new LevelInfo("Level 1", "Level1"),
            new LevelInfo("Level 2", "Level2"),
            new LevelInfo("Level 3", "Level3"),
            new LevelInfo("Level 4", "Level4")
        };

        levelButtons = new Button[levels.Length];

        for (int i=0;i<levels.Length;i++)
        {
            instance = Instantiate(levelButton, levelButtonBase);
            instance.transform.localPosition = new Vector3(13.125f * (i % 4), -4 * Mathf.Floor(i / 4f), 0);
            levelButtons[i] = instance.GetComponent<Button>();
            levelButtons[i].transform.Find("Text").GetComponent<Text>().text = levels[i].name;
            int ii = i;
            levelButtons[i].onClick.AddListener(delegate { PickTrack(ii); });
        }

        EnableUnlocked();
    }

    public void SwitchPanel(int input)
    {
        panel = input;
        for (int i = 0; i < panels.Length; i++)
        {
            if (panel == i) { panels[i].SetActive(true); }
            else { panels[i].SetActive(false); }
        }
    }
    public void Quit()
    {
        Debug.Log("quitting");
        Application.Quit();
    }

    public void PickTrack(int id)
    {
        Debug.Log(id);

        if (Application.CanStreamedLevelBeLoaded(levels[id].scene))
        {
            instance = Instantiate(dataPasser);
            DataPasser dps = instance.GetComponent<DataPasser>();

            dps.id = id;
            dps.mapName = levels[id].name;

            SceneManager.LoadScene(levels[id].scene);
        }
        else
        {
            Debug.Log("Level doesn't actually exist yet");
        }
    }

    void EnableUnlocked()
    {
        for (int i=0;i<levels.Length;i++)
        {
            if (i <= PlayerPrefs.GetInt("unlockedMaps"))
            {
                levelButtons[i].interactable = true;
            }
            else
            {
                levelButtons[i].interactable = false;
            }
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt("unlockedMaps", 0);
        PlayerPrefs.Save();
        EnableUnlocked();
    }
}

public class LevelInfo
{
    public string name;
    public string scene;
    public LevelInfo(string nameIn, string sceneIn)
    {
        name = nameIn;
        scene = sceneIn;
    }
}
