using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeUI : MonoBehaviour
{
    public Stats stats;
    public LifeManager lifeManager;
    public GameObject heartPrefab;
    public GridLayoutGroup gridLayoutGroup;

    private List<GameObject> heartList = new List<GameObject>();
    private int previousHealth;

    private void Start()
    {
        AddingHearts();
        previousHealth = lifeManager.life;
    }

    void Update()
    {
        // Si le nombre de cœurs nécessaires a changé
        if (heartList.Count != Mathf.CeilToInt(stats.health / 4f))
        {
            AddingHearts();
        }

        // Si la vie actuelle a changé
        if (lifeManager.life != previousHealth)
        {
            UpdateHearts();
            previousHealth = lifeManager.life;
        }
    }

    public void UpdateHearts()
    {
        int currentHealth = lifeManager.life;

        for (int i = 0; i < heartList.Count; i++)
        {
            HeartBehiavor heart = heartList[i].GetComponent<HeartBehiavor>();
            int value = Mathf.Min(4, currentHealth);
            heart.SetHeartValue(value);
            currentHealth -= value;
        }
    }

    public void AddingHearts()
    {
        foreach (GameObject heart in heartList)
        {
            Destroy(heart);
        }
        heartList.Clear();

        int numberOfHearts = Mathf.CeilToInt(stats.health / 4f);
        for (int i = 0; i < numberOfHearts; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, gridLayoutGroup.transform);
            heartList.Add(newHeart);
        }

        UpdateHearts();
    }
}
