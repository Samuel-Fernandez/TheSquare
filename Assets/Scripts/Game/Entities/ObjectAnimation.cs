using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteAnimation
{
    public List<Sprite> sprites;
    public float duration;
    public string animationName;
}

public class ObjectAnimation : MonoBehaviour
{
    public List<SpriteAnimation> animations;

    private SpriteRenderer spriteRenderer;
    private Coroutine currentAnimationCoroutine;
    private string currentAnimationName;
    private Sprite initialSprite;

    void Start()
    {
        // Initialiser le SpriteRenderer et stocker le sprite initial
        StartCoroutine(WaitForSpriteRenderer());
    }

    public bool CheckAnimation(string animationID)
    {
        foreach (var animation in animations)
        {
            if (animation.animationName == animationID)
                return true;
        }

        return false;
    }

    private IEnumerator WaitForSpriteRenderer()
    {
        while (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            yield return null; // Attendre une frame
        }

        // Stocker le sprite initial
        if (spriteRenderer != null)
        {
            initialSprite = spriteRenderer.sprite;
        }

        PlayAnimation("Start");
    }

    public void StopAnimation()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        currentAnimationName = null;
    }

    // Méthode principale pour jouer l'animation
    public void PlayAnimation(string animationName, bool lastImageStay = false, bool playInReverse = false, float animationSpeed = 1f)
    {
        if (!gameObject.activeSelf) return;

        SpriteAnimation foundAnimation = animations.Find(a => a.animationName == animationName);

        if (foundAnimation != null)
        {


            if (spriteRenderer == null)
            {
                StartCoroutine(WaitForSpriteRenderer());
                StartCoroutine(PlayAnimationAfterSpriteRendererIsReady(animationName, lastImageStay, playInReverse, animationSpeed));
                return;
            }

            if (currentAnimationCoroutine != null && animationName == currentAnimationName)
            {
                return;
            }

            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
            }
        
            currentAnimationCoroutine = StartCoroutine(AnimateSprite(foundAnimation, lastImageStay, playInReverse, animationSpeed));
            currentAnimationName = animationName;
        }
    }

    // Méthode IEnumerator pour jouer l'animation
    public IEnumerator PlayAnimationCoroutine(string animationName, bool lastImageStay = false, bool playInReverse = false, float animationSpeed = 1f)
    {
        if (!gameObject.activeSelf) yield break;

        SpriteAnimation foundAnimation = animations.Find(a => a.animationName == animationName);

        if (foundAnimation != null)
        {
            if (spriteRenderer == null)
            {
                yield return StartCoroutine(WaitForSpriteRenderer());
                yield return StartCoroutine(PlayAnimationCoroutine(animationName, lastImageStay, playInReverse, animationSpeed));
                yield break;
            }

            if (currentAnimationCoroutine != null && animationName == currentAnimationName)
            {
                yield break;
            }

            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
            }

            currentAnimationCoroutine = StartCoroutine(AnimateSprite(foundAnimation, lastImageStay, playInReverse, animationSpeed));
            currentAnimationName = animationName;

            // attendre la fin de l’animation
            yield return currentAnimationCoroutine;
        }
    }


    // Coroutine pour attendre le SpriteRenderer avant de jouer l'animation
    private IEnumerator PlayAnimationAfterSpriteRendererIsReady(string animationName, bool lastImageStay, bool playInReverse, float animationSpeed)
    {
        while (spriteRenderer == null)
        {
            yield return null; // Attendre une frame
        }
        PlayAnimation(animationName, lastImageStay, playInReverse, animationSpeed);
    }

    // Coroutine pour animer les sprites
    private IEnumerator AnimateSprite(SpriteAnimation animation, bool lastImageStay, bool playInReverse, float animationSpeed)
    {
        int spriteCount = animation.sprites.Count;

        if (spriteCount == 0) yield break; // S'assurer qu'il y a des sprites à animer

        float frameDuration = (animation.duration / spriteCount) / animationSpeed;

        if (playInReverse)
        {
            // Animation en sens inverse
            for (int i = spriteCount - 1; i >= 0; i--)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = animation.sprites[i];
                }
                yield return new WaitForSeconds(frameDuration);
            }
        }
        else
        {
            // Animation en sens normal
            for (int i = 0; i < spriteCount; i++)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = animation.sprites[i];
                }
                yield return new WaitForSeconds(frameDuration);
            }
        }

        if (lastImageStay && spriteCount > 0)
        {
            spriteRenderer.sprite = animation.sprites[playInReverse ? 0 : spriteCount - 1];
        }
        else
        {
            // Redémarrer l'animation si elle ne doit pas rester sur la dernière image
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
            }

            currentAnimationCoroutine = StartCoroutine(AnimateSprite(animation, lastImageStay, playInReverse, animationSpeed));
        }
    }




    public void StopAllAnimations()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }

        // Remettre le sprite initial stocké
        if (spriteRenderer != null && initialSprite != null)
        {
            spriteRenderer.sprite = initialSprite;
        }

        currentAnimationName = null;
    }

    private void OnEnable()
    {
        // Réinitialiser le sprite et stopper les animations lors de l'activation
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = initialSprite;
        }
        StopAllAnimations();
    }
}
