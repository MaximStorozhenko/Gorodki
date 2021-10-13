using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    private float time;
    private Text Timer;

    void Start()
    {
        time = 0;
        Timer = GetComponent<Text>();
    }

    void Update()
    {
        time += Time.deltaTime;
        int t = (int)time; ;

        int sec = t % 60;
        int min = t / 60;

        Timer.text = (min < 10 ? "0" + min : min.ToString()) + ":" + (sec < 10 ? "0" + sec : sec.ToString());
    }
}
