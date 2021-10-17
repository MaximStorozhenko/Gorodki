using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public static bool is_paused;
    public static PauseMode pause_mode;

    public void MenuButtonClick()
    {
        SceneManager.LoadScene("FirstScene");
    }

    public void ScoreButtonClick()
    {

    }

    private void Return()
    {
        is_paused = false;

        if(pause_mode == PauseMode.Pause)
            this.gameObject.SetActive(false);

        if(pause_mode == PauseMode.ScorePause)
            GameObject.Find("Score").SetActive(false);

        pause_mode = PauseMode.Game;
    }

    void Start()
    {
    }

    void Update()
    {
        if((Input.GetKeyDown(KeyCode.Escape) && pause_mode == PauseMode.Pause) ||
            (Input.GetKeyDown(KeyCode.H) && pause_mode == PauseMode.ScorePause))
            Return();
    }
}

public enum PauseMode
{
    Game,
    Pause,
    ScorePause,
    GameOver
}