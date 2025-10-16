using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObjectPerspective : MonoBehaviour
{
    int firstSorting = 0;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer shadowSpriteRenderer;
    public int sortingOrderBase = 1000;
    public int bonusSortingOrder = 0;
    public int level = 0;

    private void Start()
    {
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (spriteRenderer)
        {
            if (firstSorting == 0)
                firstSorting = spriteRenderer.sortingOrder;

            float adjustedPositionY = transform.position.y / 0.25f;

            int sortingOrder = sortingOrderBase + bonusSortingOrder + (level * 1000) - Mathf.RoundToInt(adjustedPositionY) + firstSorting;

            spriteRenderer.sortingOrder = sortingOrder;

            if(shadowSpriteRenderer != null)
                shadowSpriteRenderer.sortingOrder = sortingOrder - 100;
        }
    }

    public int SortingOrder()
    {
        return sortingOrderBase + bonusSortingOrder + (level * 1000) - Mathf.RoundToInt(transform.position.y / 0.25f) + firstSorting;
    }
}
