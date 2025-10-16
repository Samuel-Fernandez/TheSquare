using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimator : MonoBehaviour
{
    public static UIAnimator instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        Application.targetFrameRate = 60;
    }


    public void ActivateObjectWithTransition(GameObject obj, float duration)
    {
        StartCoroutine(ActivateObjectCoroutine(obj, duration));
    }

    public void DeactivateObjectWithTransition(GameObject obj, float duration)
    {
        StartCoroutine(DeactivateObjectCoroutine(obj, duration));
    }

    public IEnumerator ActivateObjectCoroutine(GameObject obj, float duration)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = obj.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        obj.SetActive(true);

        float elapsedTime = 0f;
        while (elapsedTime < duration && canvasGroup)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if(canvasGroup)
            canvasGroup.alpha = 1f;
    }

    public IEnumerator DeactivateObjectCoroutine(GameObject obj, float duration)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = obj.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            if(canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        
        if(obj != null)
            obj.SetActive(false);
    }

}
