using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private GameObject ReturnButton;
    private GameObject View;

    public void StartButtonClick()
    {
        SceneManager.LoadScene("SampleScene");
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

        View.SetActive(false);
        ReturnButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
