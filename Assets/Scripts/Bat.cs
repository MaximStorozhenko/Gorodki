using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private const float MIN_FORCE = 1200f;
    private const float MAX_FORCE = 3000f;
    private const float MIN_TORQUE = 50f;
    private const float MAX_TORQUE = 90000f;

    private Collider gorod;  // Коллайдер объекта "город"

    private int game_number;
    private int count_removed;
    private int count_left;
    private int count_turn;

    private Text game_text;
    private Text count_removed_text;
    private Text count_left_text;
    private Text count_turn_text;

    private Vector3 figure_position;

    private GameObject main_camera;
    private Vector3 camera_start_pos;
    private Vector3 camera_second_pos;

    void Start()
    {
        game_number = 1;
        count_removed = 0;
        count_left = 5;
        count_turn = 0;

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

        figure_position = GameObject.Find("FigurePosition").transform.position;

        main_camera = GameObject.Find("Main Camera");
        camera_start_pos = main_camera.transform.position;
        camera_second_pos = new Vector3(stand_pos_2.x + 2.7f, camera_start_pos.y, stand_pos_2.z - 2.7f);
    }

    void Update()
    {
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
                    game_number++;
                    count_turn = 0;
                    count_removed = 0;
                    count_left = 5;

                    stand.transform.position = new Vector3(
                        stand_pos_1.x,
                        stand_start_pos.y,
                        stand_pos_1.z);
                    this.transform.position = start_pos;
                    this.transform.rotation = start_rotation;

                    main_camera.transform.position = camera_start_pos;

                    GameObject figure_old = GameObject.FindGameObjectWithTag("figure" + (game_number - 1).ToString());
                    figure_old.SetActive(false);

                    if (game_number <= 2)  // Изменить по количеству фигур 
                    {
                        GameObject figure_new = GameObject.FindGameObjectWithTag("figure" + game_number.ToString());
                        figure_new.transform.position = figure_position;
                    }
                }

                count_left = 5 - count_removed;

                game_text.text = "Игра №" + game_number;
                count_removed_text.text = "Выбито: " + count_removed;
                count_left_text.text = "Осталось: " + count_left;
                count_turn_text.text = "Ударов: " + count_turn;
            }
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
}
