using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ObjectAnimation))]
[RequireComponent(typeof(SoundContainer))]
[RequireComponent(typeof(ObjectParticles))]
public class MyliaIceBlockVisual : MonoBehaviour
{
    [Header("Particle Settings")]
    public string snowParticleName = "Snow";
    public float snowSpawnInterval = 1f;

    [Header("Sound Settings")]
    public string freezeSoundName = "Freeze";
    public string meltingSoundName = "Melting";

    private ObjectAnimation objectAnimation;
    private SoundContainer soundContainer;
    private ObjectParticles objectParticles;

    private void Awake()
    {
        objectAnimation = GetComponent<ObjectAnimation>();
        soundContainer = GetComponent<SoundContainer>();
        objectParticles = GetComponent<ObjectParticles>();
    }

    public void Init()
    {
        objectAnimation.PlayAnimation("Growing", true);
        soundContainer.PlaySound(freezeSoundName, 1);
        objectParticles.SpawnParticle(snowParticleName, gameObject, snowSpawnInterval);
    }

    public void Remove()
    {
        StartCoroutine(RemoveRoutine());
    }

    private IEnumerator RemoveRoutine()
    {
        objectParticles.StopSpawningParticles();
        soundContainer.PlaySound(meltingSoundName, 1);

        // lastImageStay = true : garantit que la coroutine d'animation se termine réellement
        // (sinon AnimateSprite se relance en boucle et PlayAnimationCoroutine ne rend jamais la main).
        yield return StartCoroutine(objectAnimation.PlayAnimationCoroutine("Reducting", true));

        Destroy(gameObject);
    }
}
