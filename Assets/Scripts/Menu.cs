using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    private GameObject ReturnButton;
    private GameObject View;

    private Text startButtonText;

    public void StartButtonClick()
    {
        SceneManager.LoadScene("SampleScene");
        Pause.is_paused = false;
        Pause.pause_mode = PauseMode.Game;
        Pause.is_playing = true;
    }

    public void RulesButtonClick()
    {
        View.SetActive(true);
        ReturnButton.SetActive(true);
    }

    public void ExitButtonClick()
    {
        Application.Quit();
    }

    public void ReturnButtonClick()
    {
        View.SetActive(false);
        ReturnButton.SetActive(false);
    }

    void Start()
    {
        View = GameObject.Find("Scroll View");
        ReturnButton = GameObject.Find("ReturnButton");

        startButtonText = GameObject.Find("StartButtonText").GetComponent<Text>();

        View.SetActive(false);
        ReturnButton.SetActive(false);

        Pause.is_paused = true;

        if (Pause.is_playing == true)
            startButtonText.text = "Продолжить";
        else
            startButtonText.text = "Играть";
    }

    void Update()
    {
        Debug.Log("is_playing" + Pause.is_playing.ToString());
    }
}