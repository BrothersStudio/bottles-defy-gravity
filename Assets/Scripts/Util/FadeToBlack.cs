﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeToBlack : MonoBehaviour
{
    Image image;

    public bool done = false;
    bool from_boss = false;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void FadeIn()
    {
        done = false;
        image.color = new Color(0, 0, 0, 1);
        StartCoroutine(In());
    }

    public void FadeOut(bool from_boss = false)
    {
        done = false;
        this.from_boss = from_boss;
        if (from_boss)
        {
            FindObjectOfType<MusicController>().PlayBeach();
        }

        image.color = new Color(0, 0, 0, 0);
        StartCoroutine(Out());
    }

    IEnumerator In()
    {
        while (image.color.a > 0)
        {
            Color color = image.color;
            color.a -= 3f * Time.deltaTime;
            image.color = color;

            yield return null;
        }
        done = true;
    }

    IEnumerator Out()
    {
        while (image.color.a < 1)
        {
            Color color = image.color;
            color.a += 3f * Time.deltaTime;
            image.color = color;

            yield return null;
        }
        done = true;

        if (from_boss)
        {
            transform.parent.Find("Credits").GetComponent<AdvanceCredits>().Begin();
        }
    }
}
