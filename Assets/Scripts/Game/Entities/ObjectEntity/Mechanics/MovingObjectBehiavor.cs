using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MovingPosition
{
    public Vector2 position;
    public float duration;
}

public class MovingObjectBehiavor : MonoBehaviour
{
    public List<MovingPosition> positions;
    public bool reverseCycle = false;

    private Vector3 basePosition;
    private int currentIndex = 0;
    private bool isReversing = false;

    void Start()
    {
        basePosition = transform.position;
        if (positions.Count > 0)
            StartCoroutine(MoveLoop());
    }

    private IEnumerator MoveLoop()
    {
        while (true)
        {
            Vector3 targetPos = basePosition + (Vector3)positions[currentIndex].position;
            float duration = positions[currentIndex].duration;

            yield return StartCoroutine(MoveToPosition(targetPos, duration));

            if (!reverseCycle)
            {
                currentIndex = (currentIndex + 1) % positions.Count;
            }
            else
            {
                if (!isReversing)
                {
                    currentIndex++;
                    if (currentIndex >= positions.Count)
                    {
                        currentIndex = positions.Count - 2;
                        isReversing = true;
                    }
                }
                else
                {
                    currentIndex--;
                    if (currentIndex < 0)
                    {
                        currentIndex = 1;
                        isReversing = false;
                    }
                }
            }
        }
    }

    private IEnumerator MoveToPosition(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }
}
