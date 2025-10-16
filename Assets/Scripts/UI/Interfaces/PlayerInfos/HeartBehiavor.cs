using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartBehiavor : MonoBehaviour
{
    public Sprite completeHeart;         // 4/4
    public Sprite thirdQuarterHeart;     // 3/4
    public Sprite halfHeart;             // 2/4
    public Sprite firstQuarterHeart;     // 1/4
    public Image heartImage;
    private Animator animator;
    public int previousValue = 4;        // De 0 à 4

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetHeartValue(int value) // value entre 0 et 4
    {
        value = Mathf.Clamp(value, 0, 4);

        if (value == previousValue) return;

        switch (value)
        {
            case 4:
                heartImage.sprite = completeHeart;
                heartImage.color = Color.red;
                break;
            case 3:
                heartImage.sprite = thirdQuarterHeart;
                heartImage.color = Color.red;
                break;
            case 2:
                heartImage.sprite = halfHeart;
                heartImage.color = Color.red;
                break;
            case 1:
                heartImage.sprite = firstQuarterHeart;
                heartImage.color = Color.red;
                break;
            case 0:
                heartImage.sprite = completeHeart;
                heartImage.color = Color.black;
                break;
        }

        animator.SetTrigger("Update");
        previousValue = value;
    }
}
