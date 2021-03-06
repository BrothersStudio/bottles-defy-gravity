﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summoner : MonoBehaviour
{
    public float shift_attack_delay;
    public float speed;

    float last_movement_change = 0;
    float change_cooldown = 1f;

    bool death_playing = false;
    public AudioClip death_clip;

    bool summoning = false;
    bool summon_done = false;
    int summon_animation_frame = 0;
    float frame_hold_time = 0.10f;
    float current_frame_hold = 0;
    public List<Sprite> summon_animation_sprites;

    int walk_cycle = 0;
    public List<Sprite> walking_animation_sprites;

    float last_summon = -2f;
    float summon_cooldown = 3f;
    public GameObject enemy_to_summon;
    public int num_to_summon;
    public AudioClip summon_sound;

    Player player;
    Enemy enemy;
    SpriteRenderer sprite_renderer;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        enemy = GetComponent<Enemy>();
        sprite_renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(WalkCycle());
    }

    void Update()
    {
        if (Hitstop.current.Hitstopped)
        {
            return;
        }

        // Summon animation
        if (summoning &&
            enemy.health > 0 &&
            TimeChange.current.dimension == enemy.enemy_dimension)
        {
            ChooseSummonAnimationSprite();

            if (summon_done)
            {
                Vector3 summoner_pos = transform.position;
                for (int i = 0; i < num_to_summon; i++)
                {
                    Vector3 summoned_pos = summoner_pos;
                    summoned_pos.x += Random.Range(-1, 1);
                    summoned_pos.y += Random.Range(-1, 1);

                    if (Vector3.Distance(player.transform.position, summoned_pos) > 0.5)
                    {
                        GameObject new_skull = Instantiate(enemy_to_summon, summoned_pos, Quaternion.identity);
                        new_skull.GetComponent<Enemy>().SetRoomPosition(enemy.max_pos, enemy.min_pos);
                    }
                }

                summon_done = false;
                last_summon = Time.timeSinceLevelLoad;
            }
        }
        else if (summoning &&
             enemy.health > 0 &&
             TimeChange.current.dimension != enemy.enemy_dimension)
        {
            ResetSummon();
            ChooseWalkingSprite();
        }

        // Moving
        if (!summoning &&
            enemy.health > 0 && 
            Time.timeSinceLevelLoad > change_cooldown + last_movement_change)
        {
            last_movement_change = Time.timeSinceLevelLoad;
            GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)).normalized * speed * Time.deltaTime;
            CheckWall();
        }
        else if (!summoning &&
            enemy.health > 0)
        {
            CheckWall();
        }

        if (!summoning)
        {
            ChooseWalkingSprite();
        }

        // Do I want to start summoning?
        if (enemy.health > 0 &&
            !summoning &&
            Time.timeSinceLevelLoad > summon_cooldown + last_summon)
        {
            summoning = true;

            GetComponent<AudioSource>().clip = summon_sound;
            GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
            GetComponent<AudioSource>().Play();
        }

        if (enemy.health <= 0 &&
            !death_playing)
        {
            death_playing = true;
            GetComponent<AudioSource>().clip = death_clip;
            GetComponent<AudioSource>().Play();
        }

        if (TimeChange.current.dimension == enemy.enemy_dimension &&
            enemy.health > 0)
        {
            SetEnemyFacing();
        }
    }

    void CheckWall()
    {
        if (transform.position.x > enemy.max_pos.x ||
            transform.position.x < enemy.min_pos.x ||
            transform.position.y > enemy.max_pos.y ||
            transform.position.y < enemy.min_pos.y)
        {
            // Turn around if we hit a wall
            last_movement_change = Time.timeSinceLevelLoad;
            GetComponent<Rigidbody2D>().velocity = -GetComponent<Rigidbody2D>().velocity;
        }
    }

    void SetEnemyFacing()
    {
        Vector3 check = transform.position - player.transform.position;
        if (check.x > 0)
        {
            sprite_renderer.flipX = true;
        }
        else
        {
            sprite_renderer.flipX = false;
        }
    }

    void ChooseSummonAnimationSprite()
    {
        current_frame_hold += Time.deltaTime;
        if (current_frame_hold > frame_hold_time)
        {
            summon_animation_frame++;
            current_frame_hold = 0;
        }

        if (summon_animation_frame < summon_animation_sprites.Count)
        {
            sprite_renderer.sprite = summon_animation_sprites[summon_animation_frame];
        }
        else
        {
            summon_done = true;

            ResetSummon();
        }
        
        if (enemy.flashing)
        {
            sprite_renderer.sprite = enemy.flash_sprite;
        }
    }

    void ResetSummon()
    {
        summoning = false;
        summon_animation_frame = 0;
        current_frame_hold = 0;
    }

    IEnumerator WalkCycle()
    {
        while (enemy.health > 0)
        {
            walk_cycle = 0;
            yield return new WaitForSeconds(0.25f);
            walk_cycle = 1;
            yield return new WaitForSeconds(0.25f);
            walk_cycle = 2;
            yield return new WaitForSeconds(0.25f);
            walk_cycle = 3;
            yield return new WaitForSeconds(0.25f);
        }
    }

    void ChooseWalkingSprite()
    {
        if (!enemy.flashing)
        {
            sprite_renderer.sprite = walking_animation_sprites[walk_cycle];
        }
        else
        {
            sprite_renderer.sprite = enemy.flash_sprite;
        }
    }
}
