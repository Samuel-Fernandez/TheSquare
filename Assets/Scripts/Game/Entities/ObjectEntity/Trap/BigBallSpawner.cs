using System.Collections;
using UnityEngine;

public class BigBallSpawner : MonoBehaviour
{
    [Header("Références")]
    public GameObject ballPrefab;
    public Transform spawnPoint;

    [Header("Apparition")]
    public float spawnInterval = 5f;

    [Header("Paramètres de la balle")]
    public float ballSpeed = 3f;
    public Vector2 ballDirection = Vector2.right;
    public bool ballComesFromSky = false;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnBall();
        }
    }

    public void SpawnBall()
    {
        if (ballPrefab == null || spawnPoint == null) return;

        GameObject instance = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);

        BigBallRockBehiavor ball = instance.GetComponent<BigBallRockBehiavor>();
        if (ball != null)
            ball.Init(ballSpeed, ballDirection, ballComesFromSky);
    }
}
