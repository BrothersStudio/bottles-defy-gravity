﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    int player_room = 0;
    int new_room_height_factor = 0;
    List<GameObject> spawned_rooms = new List<GameObject>();

    public GameObject healing_item;

    public List<GameObject> level_1_rooms;
    public List<GameObject> level_2_rooms;
    public List<GameObject> level_3_rooms;
    public List<GameObject> boss_rooms;

    List<GameObject> current_rooms;
    public GameObject stair_room;

    List<float> x_vals = new List<float>();
    List<float> y_vals = new List<float>();
    public Grid pathfinding_grid;

    LevelController lev_con;
    FadeToBlack fade;

    void Awake()
    {
        lev_con = FindObjectOfType<LevelController>();
        fade = FindObjectOfType<FadeToBlack>();

        switch (lev_con.current_level)
        {
            case 1:
                current_rooms = level_1_rooms;
                break;
            case 2:
                current_rooms = level_2_rooms;
                break;
            case 3:
                current_rooms = level_3_rooms;
                break;
            case 4:
                FindObjectOfType<MusicController>().PlayBossLaugh();
                current_rooms = boss_rooms;
                break;
        }

        SpawnRooms();
    }

    void SpawnRooms()
    {
        for (int i = 0; i < current_rooms.Count; i++)
        {
            GameObject new_room = Instantiate(current_rooms[i]);
            Room room_comp = new_room.GetComponent<Room>();

            // Reposition
            Vector3 new_position = new_room.transform.position;
            new_position.y = new_room_height_factor;
            new_room.transform.position = new_position;
            new_room_height_factor += (int)(room_comp.max_and_min_cam_pos[0].y - room_comp.max_and_min_cam_pos[1].y);

            Vector3 max_camera_pos = new_room.transform.TransformPoint(room_comp.max_and_min_cam_pos[0]);
            Vector3 min_camera_pos = new_room.transform.TransformPoint(room_comp.max_and_min_cam_pos[1]);

            x_vals.Add(max_camera_pos.x);
            x_vals.Add(min_camera_pos.x);

            y_vals.Add(max_camera_pos.y);
            y_vals.Add(min_camera_pos.y);

            spawned_rooms.Add(new_room);
        }

        // Position stair room
        GameObject new_stair_room = Instantiate(stair_room);
        Vector3 stair_position = new_stair_room.transform.position;
        stair_position.y = new_room_height_factor;
        new_stair_room.transform.position = stair_position;
        spawned_rooms.Add(new_stair_room);

        player_room = 0;

        // Position pathfinding grid
        float max_x = Mathf.Max(x_vals.ToArray());
        float min_x = Mathf.Min(x_vals.ToArray());
        float max_y = Mathf.Max(y_vals.ToArray());
        float min_y = Mathf.Min(y_vals.ToArray());

        float x_dist = max_x - min_x;
        float y_dist = max_y - min_y;
        pathfinding_grid.gridWorldSize = new Vector2(x_dist, y_dist);

        pathfinding_grid.transform.position = new Vector3((max_x + min_x) / 2f, (max_y + min_y) / 2f, 0);
    }

    void Start()
    {
        spawned_rooms[0].GetComponent<Room>().Activate();
        fade.FadeIn();
    }

    public void MoveToNextRoom()
    {
        foreach (Enemy enemy in spawned_rooms[player_room].GetComponent<Room>().enemies)
        {
            if (enemy.is_hazard)
            {
                enemy.health = -10;
            }
        }

        player_room++;

        TimeChange.current.ForceRoomChangeDelay();

        spawned_rooms[player_room].GetComponent<Room>().Activate();

        Player player = FindObjectOfType<Player>();
        Vector3 player_pos = spawned_rooms[player_room].transform.position;
        player_pos.y -= 3f;
        player.transform.position = player_pos;
        player.ResetSpeed();
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        Camera.main.GetComponent<CameraFollow>().MoveToNextRoom(spawned_rooms[player_room].transform.position);
    }
}
