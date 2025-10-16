using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MineralType
{
    IRON,
    SILVER,
    DIAMOND,
    ANTIMATTER,
    SQUAREBLOCK

}
public class MineralBehiavor : MonoBehaviour
{
    public int resistance;
    public GameObject item;
    public MineralType type;
    SpriteRenderer sprite;

    float elapsedTime;
    float cycleDuration = 5f;

    public bool isBig;
    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    public void HitMineral(int powerHit)
    {
        resistance -= powerHit;

        GetComponent<ObjectParticles>().SpawnParticle("Destroy1", transform.position);


        if (resistance <= 0 )
        {
            GetComponent<SoundContainer>().PlaySound("Destroy", 1);
            SpawnMinerals();
            GetComponent<ObjectParticles>().SpawnParticle("Destroy2", transform.position);
            Destroy(gameObject);
        }
    }

    void SpawnMinerals()
    {
        int nbMinerals = NbMinerals();

        if (Random.Range(0, 100) <= PlayerManager.instance.doubleMineralDropChance * 100)
            nbMinerals *= 2;

        for (int i = 0; i < nbMinerals; i++)
        {
            Vector2 randomPosition = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
            Instantiate(item, randomPosition, Quaternion.identity);
        }
    }

    void Update()
    {
        if (type == MineralType.SQUAREBLOCK)
        {
            elapsedTime += Time.deltaTime;
            float t = (elapsedTime % cycleDuration) / cycleDuration; // Valeur normalisée entre 0 et 1

            sprite.color = GetRainbowColor(t);
        }
    }

    int NbMinerals()
    {
        return 1 + Mathf.RoundToInt((Random.Range(0, PlayerLevels.instance.lvlLuck * 200) / 1000));
    }

    Color GetRainbowColor(float t)
    {
        // Interpolation linéaire entre les couleurs de l'arc-en-ciel
        t *= 6f; // Échelle pour passer par 6 couleurs
        int i = Mathf.FloorToInt(t);
        float f = t - i;

        switch (i)
        {
            case 0: return Color.Lerp(Color.red, Color.yellow, f);      // Red to Yellow
            case 1: return Color.Lerp(Color.yellow, Color.green, f);    // Yellow to Green
            case 2: return Color.Lerp(Color.green, Color.cyan, f);      // Green to Cyan
            case 3: return Color.Lerp(Color.cyan, Color.blue, f);       // Cyan to Blue
            case 4: return Color.Lerp(Color.blue, Color.magenta, f);    // Blue to Magenta
            case 5: return Color.Lerp(Color.magenta, Color.red, f);     // Magenta to Red
            default: return Color.red;
        }
    }
}
