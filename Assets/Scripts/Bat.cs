using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Bat : MonoBehaviour
{
    private Rigidbody bat_rb;
    private Vector3 start_pos;
    private Quaternion start_rotation;
    private bool is_bat_moving;

    private GameObject stand;
    private Vector2 stand_start_pos;
    private Vector3 stand_pos_1;
    private Vector3 stand_pos_2;

    private Slider ForceSlider;
    private Vector3 force_direction;

    private Slider RotationSlider;
    private Vector3 rotation_direction;

    private const float MIN_FORCE  = 1200f;  // Сила броска
    private const float MAX_FORCE  = 3000f;  // биты
    private const float MIN_TORQUE = -1e3f;  // Вращательный
    private const float MAX_TORQUE =  1e3f;  // момент биты

    private const string GAMES_HISTORY_FILE = "history.xml";

    private Collider gorod;  // Коллайдер объекта "город"

    private GameObject[] figures;

    private int game_number;    // номер игры
    private int count_removed;  // кол-во выбитых городков
    private int count_left;     // кол-во оставшихся
    private int count_turn;     // кол-во бросков в каждой игре
    private int throws;         // кол-во бросков (за всю игру)

    private Text game_text;
    private Text count_removed_text;
    private Text count_left_text;
    private Text count_turn_text;

    private Vector3 figure_position;

    private GameObject main_camera;
    private Vector3 camera_start_pos;
    private Vector3 camera_second_pos;

    private List<GameResult> game_history;

    private List<ScoreResult> score_results;

    private Clock Clock;

    private GameObject pause_canvas;
    private GameObject score_canvas;
    private GameObject liders_canvas;
    private Text liders_score_list;

    private Text score_list;

    void Start()
    {
        score_results = new List<ScoreResult>();
        score_list = GameObject.Find("Scores").GetComponent<Text>();

        Clock = GameObject.Find("Clock").GetComponent<Clock>();

        pause_canvas = GameObject.Find("Pause");
        pause_canvas.SetActive(false);

        score_canvas = GameObject.Find("Score");
        score_canvas.SetActive(false);

        liders_canvas = GameObject.Find("Liders");
        Pause.liders_canvas = liders_canvas;
        liders_score_list = GameObject.Find("LidersScores").GetComponent<Text>();
        LoadHistory();

        if (File.Exists(GAMES_HISTORY_FILE))
        {
            var list1 = from h in game_history
                        orderby h.Throws
                        orderby h.Time
                        select h;

            game_history = list1.ToList();

            GameResult.Print(game_history, liders_score_list);
        }
        liders_canvas.SetActive(false);

        game_number = 1;
        count_removed = 0;
        count_left = 5;
        count_turn = 0;
        throws = 0;

        game_text = GameObject.Find("GameText").GetComponent<Text>();
        count_removed_text = GameObject.Find("CountRemovedText").GetComponent<Text>();
        count_left_text = GameObject.Find("CountLeftText").GetComponent<Text>();
        count_turn_text = GameObject.Find("CountTurnText").GetComponent<Text>();

        stand = GameObject.Find("Stand");
        stand_pos_1 = GameObject.Find("FirstStandPosition").transform.position;
        stand_pos_2 = GameObject.Find("SecondStandPosition").transform.position;

        stand_start_pos = stand.transform.position;

        bat_rb = this.GetComponent<Rigidbody>();
        start_pos = this.transform.position;
        start_rotation = this.transform.rotation;

        is_bat_moving = false;

        ForceSlider = GameObject.Find("ForceSlider").GetComponent<Slider>();
        force_direction = Vector3.left + Vector3.forward + 0.5f * Vector3.up;

        RotationSlider = GameObject.Find("RotationSlider").GetComponent<Slider>();
        rotation_direction = Vector3.up;

        gorod = GameObject.Find("Pole").GetComponent<Collider>();

        figures = GameObject.FindGameObjectsWithTag("figure");

        figure_position = GameObject.Find("FigurePosition").transform.position;

        main_camera = GameObject.Find("Main Camera");
        camera_start_pos = main_camera.transform.position;
        camera_second_pos = new Vector3(stand_pos_2.x + 2.7f, camera_start_pos.y, stand_pos_2.z - 2.7f);
    }

    void Update()
    {
        #region управление паузой

        if (Pause.is_paused)
        {
            ForceSlider.enabled = false;
            RotationSlider.enabled = false;
            return;
        }
        else
        {
            ForceSlider.enabled = true;
            RotationSlider.enabled = true;
        }

        if(Pause.pause_mode == PauseMode.Game)
        {
            if(Input.GetKeyDown(KeyCode.Escape) && !is_bat_moving)
            {
                pause_canvas.SetActive(true);
                Pause.is_paused = true;
                Pause.pause_mode = PauseMode.Pause;
            }

            if(Input.GetKeyDown(KeyCode.H) && !is_bat_moving)
            {
                score_canvas.SetActive(true);
                Pause.is_paused = true;
                Pause.pause_mode = PauseMode.ScorePause;
            }
        }

        #endregion

        #region Полет и остановка биты
        if (is_bat_moving)
        {
            if (bat_rb.velocity.magnitude < 0.2f)
            {
                bat_rb.velocity = Vector3.zero;
                bat_rb.angularVelocity = Vector3.zero;

                is_bat_moving = false;

                RemoveBars();

                if (count_removed > 0)
                {
                    stand.transform.position = new Vector3(
                        stand_pos_2.x,
                        stand.transform.position.y,
                        stand_pos_2.z);

                    this.transform.position = new Vector3(
                        stand_pos_2.x,
                        start_pos.y,
                        stand_pos_2.z);
                    this.transform.rotation = start_rotation;
                    main_camera.transform.position = camera_second_pos;
                }
                else
                {
                    stand.transform.position = new Vector3(
                        stand_pos_1.x,
                        stand_start_pos.y,
                        stand_pos_1.z);
                    this.transform.position = start_pos;
                    this.transform.rotation = start_rotation;
                    main_camera.transform.position = camera_start_pos;
                }

                if(count_removed == 5) 
                {
                    ScoreHistory();

                    game_number++;
                    count_turn = 0;
                    count_removed = 0;
                    count_left = 5;
                    Clock.ClockReset();

                    stand.transform.position = new Vector3(
                        stand_pos_1.x,
                        stand_start_pos.y,
                        stand_pos_1.z);
                    this.transform.position = start_pos;
                    this.transform.rotation = start_rotation;

                    main_camera.transform.position = camera_start_pos;

                    foreach(GameObject fig in figures)
                    {
                        if (fig.name == "Figure" + (game_number - 1).ToString())
                            fig.SetActive(false);

                        if (game_number <= figures.Length && fig.name == "Figure" + game_number.ToString())
                        {
                            fig.transform.position = figure_position;
                        }
                    }
                }

                count_left = 5 - count_removed;
                if(game_number <= figures.Length)
                    game_text.text = "Игра №" + game_number;
                count_removed_text.text = "Выбито: " + count_removed;
                count_left_text.text = "Осталось: " + count_left;
                count_turn_text.text = "Ударов: " + count_turn;
            }
        }
        #endregion

        ScoreResult.Print(score_results, score_list);

        #region GameOver

        if (game_number > figures.Length)
        {
            Debug.Log("GameOver");
            score_canvas.SetActive(true);
            Pause.is_paused = true;
            Pause.is_playing = false;
            Pause.pause_mode = PauseMode.ScorePause;

            LoadHistory();

            SaveHistory();
        }

        #endregion

        #region Бросок биты
        if (Input.GetKeyDown(KeyCode.Space) && !is_bat_moving)
        {
            bat_rb.velocity = force_direction * 0.2f;
            float force_factor = MIN_FORCE + ForceSlider.value * (MAX_FORCE - MIN_FORCE);
            bat_rb.AddForce(force_factor * force_direction);

            float torque_factor = MIN_TORQUE + RotationSlider.value * (MAX_TORQUE - MIN_TORQUE);
            bat_rb.AddTorque(torque_factor * rotation_direction);

            is_bat_moving = true;
            count_turn++;
            throws++;
        }
        #endregion

        #region Features

        if(Input.GetKeyDown(KeyCode.S))
        {
            //SaveHistory();
            //Debug.Log("Saved");
        }

        #endregion
    }

    private void RemoveBars()
    {
        // Задание вывести координаты центров всех брусков
        // 1. Добавили ко всем бруска тег "bar"
        // 2. Находим их по тегу
        GameObject[] bars = GameObject.FindGameObjectsWithTag("bar" + game_number.ToString());
        int count = 0;

        // 3. Обходим циклом и выводим
        foreach(GameObject bar in bars)
        {
            if(gorod.bounds.Contains(bar.transform.TransformPoint(Vector3.zero)))
            {
                // Брусок не вышел за пределы "города"
                Rigidbody rb = bar.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                // Брусок вышел за пределы "города"
                bar.SetActive(false);
                count++;
            }
        }

        count_removed += count;
    }

    // Загрузка файла с результатами
    private void LoadHistory()
    {
        if(File.Exists(GAMES_HISTORY_FILE))
        {
            using(StreamReader reader = new StreamReader(GAMES_HISTORY_FILE))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<GameResult>));
                game_history = (List<GameResult>)serializer.Deserialize(reader);
                Debug.Log("Desialized: " + game_history.Count);
            }
        }
    }

    // Сохранение всей мгры в файл с результатами
    private void SaveHistory()
    {
        if(game_history == null)
            game_history = new List<GameResult>();

        float time = 1f;

        foreach (ScoreResult res in score_results)
        {
            time += res.Time;
        }

        game_history.Add(new GameResult
        {
            WinGames = game_number - 1,
            Throws = throws,
            Time = time
        });



        using(StreamWriter writer = new StreamWriter(GAMES_HISTORY_FILE))
        {
            XmlSerializer serializer = new XmlSerializer(game_history.GetType());
            serializer.Serialize(writer, game_history);
        }
    }

    private void ScoreHistory()
    {
        score_results.Add(new ScoreResult
        {
            Game = game_number,
            Throws = count_turn,
            Time = Clock.CountTime
        });
    }
}

// Общая игра из 15-ти фигур
[Serializable]
public class GameResult
{
    public string Name { get; set; }   // Имя играющего

    public int WinGames { get; set; }  // сбито фигур

    public int Throws { get; set; }    // совершенно бросков за всю игру

    public float Time { get; set; }    // затраченно времени

    public static void Print(List<GameResult> list, Text score_list)
    {
        score_list.text = "";
        int num = 1;

        foreach(GameResult res in list)
        {
            score_list.text += (num < 10 ? "  " + num : num.ToString()) + ".   " + res.Name +
                "       " + res.WinGames + "       " + res.Throws + "       " + ScoreResult.GetTime(res.Time) + "\n\n";
            num++;
        }
    }
}

// Очки по каждой фигуре
public class ScoreResult
{
    public int Game { get; set; }    // номер игры

    public int Throws { get; set; }  // совершенно бросков на фигуру

    public float Time { get; set; }  // затраченно времени

    // Вывод результатов на экран при нажатии на кнопку "H"
    public static void Print(List<ScoreResult> list, Text score_list)
    {
        score_list.text = "";
        int num = 1;

        foreach (ScoreResult res in list)
        {
            score_list.text += (num < 10 ? "  " + num : num.ToString()) +
                ".                    " + res.Throws + "                    " + GetTime(res.Time) + "\n\n";
            num++;
        }

        for(int i = num; i <= 15; i++)
        {
            score_list.text += (i < 10 ? "  " + i : i.ToString()) +
                ".                    -                    - \n\n";
        }
    }

    public static string GetTime(float game_time)
    {
        int t = (int)game_time;

        int sec = t % 60;
        int min = t / 60;

        return (min < 10 ? "0" + min : min.ToString()) + ":" + (sec < 10 ? "0" + sec : sec.ToString());
    }
}