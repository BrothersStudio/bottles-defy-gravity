﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flash : MonoBehaviour
{
    Image image;

    public Transform player;

    bool flashing = false;
    int dist_from_center = 0;

    Vector2 center_pos = new Vector2(160 / 2, 90 / 2);

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void FlashStart()
    {
        flashing = true;
        dist_from_center = 0;

        center_pos = Camera.main.WorldToScreenPoint(player.position);
        center_pos.x = (center_pos.x / Screen.width) * 256;
        center_pos.y = (center_pos.y / Screen.height) * 144;

        gameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        if (flashing)
        {
            if (!image.enabled) image.enabled = true;

            Texture2D texture = new Texture2D(256, 144);
            texture.filterMode = FilterMode.Point;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 256, 144), Vector2.zero);
            image.sprite = sprite;

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center_pos);
                    if (dist < dist_from_center + 3)
                    {
                        texture.SetPixel(x, y, new Color(1, 1, 1, 0));
                    }
                    else if (dist < dist_from_center)
                    {
                        texture.SetPixel(x, y, new Color(1, 1, 1, 1));
                    }
                }
            }
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            dist_from_center += 13;
            if (dist_from_center >= 220)
            {
                flashing = false;
                gameObject.SetActive(false);
            }
        }
    }
}
