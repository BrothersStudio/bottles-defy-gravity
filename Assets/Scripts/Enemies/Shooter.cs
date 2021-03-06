﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public float speed;

    float last_shot = 0;
    public float shot_cooldown = 1f;
    float shift_attack_delay = 0.5f;

    public AudioClip enemy_shot;

    Vector3[] path = new Vector3[] { };
    int targetIndex;

    int walk_cycle_int = 0;
    public List<Sprite> walk_animation;

    bool death_playing = false;
    public AudioClip death_clip;

    Enemy enemy;
    Player player;
    SpriteRenderer sprite_renderer;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        player = FindObjectOfType<Player>();
        sprite_renderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        StartCoroutine(RefreshPath());
        StartCoroutine(WalkCycle());
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;

            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    void Update()
    {
        if (Hitstop.current.Hitstopped)
        {
            return;
        }

        // Shooting
        if (TimeChange.current.dimension == enemy.enemy_dimension && 
            (Time.timeSinceLevelLoad > last_shot + shot_cooldown) && 
            enemy.health > 0 &&
            Time.timeSinceLevelLoad > shift_attack_delay + TimeChange.current.last_change_time)
        {
            last_shot = Time.timeSinceLevelLoad;

            GameObject bullet = PlayerBulletPool.current.GetPooledBullet();
            bullet.SetActive(true);
            bullet.transform.position = transform.position;

            bullet.GetComponent<Bullet>().side = BulletSide.Enemy;
            bullet.GetComponent<Bullet>().SetSpriteAndSpeed();

            Vector3 direction = FindObjectOfType<Player>().transform.position - transform.position;
            bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(direction.x, direction.y).normalized * bullet.GetComponent<Bullet>().speed;

            GetComponent<AudioSource>().clip = enemy_shot;
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

        SelectSprite();
    }

    void SelectSprite()
    {
        SetEnemyFacing();

        if (enemy.flashing)
        {
            sprite_renderer.sprite = enemy.flash_sprite;
        }
        else
        {
            if (walk_animation.Count > 0)
            {
                sprite_renderer.sprite = walk_animation[walk_cycle_int];
            }
            else if (walk_animation.Count == 0)
            {
                sprite_renderer.sprite = enemy.normal_sprite;
            }
        }
    }

    void SetEnemyFacing()
    {
        if (enemy.health > 0 &&
            TimeChange.current.dimension == enemy.enemy_dimension)
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
    }

    IEnumerator WalkCycle()
    {
        while (true)
        {
            walk_cycle_int = 0;
            yield return new WaitForSeconds(0.2f);
            walk_cycle_int = 1;
            yield return new WaitForSeconds(0.2f);
            walk_cycle_int = 2;
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator RefreshPath()
    {
        while (enemy.health > 0)
        {
            PathRequestManager.RequestPath(transform.position, player.transform.position, OnPathFound);

            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length > 0)
        {
            Vector3 currentWaypoint = path[0];
            while (TimeChange.current.dimension == enemy.enemy_dimension && 
                enemy.health > 0 &&
                !enemy.CanSeePlayer())
            {
                if (Hitstop.current.Hitstopped)
                {
                    yield return null;
                    continue;
                }

                if (transform.position == currentWaypoint)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        yield break;
                    }
                    currentWaypoint = path[targetIndex];
                }

                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
                yield return null;
            }
        }
    }
}

