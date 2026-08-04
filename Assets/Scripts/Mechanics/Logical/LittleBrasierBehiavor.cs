using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityEffects))]
public class LittleBrasierBehiavor : MonoBehaviour
{
    [Header("Logique")]
    public string id;
    public List<GameObject> logicalEntites;

    [Header("Visuel")]
    public Sprite offSprite;

    [Header("Sons & Particules")]
    public string onSoundName = "On";
    public string smokeParticleName = "Smoke";
    public float smokeInterval = 0.5f;

    [Header("Light Flicker Settings")]
    public Color lightColor = new Color(1f, 0.55f, 0.1f);
    public float lightIntensityMin = 0.5f;
    public float lightIntensityMax = 1.5f;
    public float lightRadiusMin = 1f;
    public float lightRadiusMax = 2f;
    public float flickerIntervalMin = 0.05f;
    public float flickerIntervalMax = 0.25f;
    public float flickerTransitionTime = 0.1f;

    [Header("Durée")]
    public bool isPerpetual = true;
    public float onDuration = 5f;

    public bool isOn = false;

    private EntityEffects entityEffects;
    private ObjectAnimation objectAnimation;
    private SpriteRenderer spriteRenderer;
    private SoundContainer soundContainer;
    private ObjectParticles objectParticles;
    private EntityLight entityLight;
    private Coroutine flickerCoroutine;
    private Coroutine autoOffCoroutine;

    private void Awake()
    {
        entityEffects = GetComponent<EntityEffects>();
        objectAnimation = GetComponent<ObjectAnimation>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        soundContainer = GetComponent<SoundContainer>();
        objectParticles = GetComponent<ObjectParticles>();
        entityLight = GetComponent<EntityLight>();
    }

    private void Start()
    {
        if (entityLight != null)
        {
            entityLight.SetLightColor(lightColor);
        }

        bool state;
        bool restoredOn = SaveManager.instance.twoStateContainer.TryGetState(id, out state) && state;

        if (restoredOn)
        {
            ActivateBrasier(true);
        }
        else if (spriteRenderer != null && offSprite != null)
        {
            spriteRenderer.sprite = offSprite;
        }

        StartCoroutine(MonitorFireState());
    }

    // On capture l'état de flamme et on l'annule immédiatement (comme IceBlockBehiavor) : le brasier ne
    // doit jamais brûler "normalement" (dégâts, timer aléatoire d'EntityEffects), seulement s'allumer.
    private IEnumerator MonitorFireState()
    {
        while (true)
        {
            if (entityEffects.isFire)
            {
                entityEffects.StopFire();
                ActivateBrasier(true);
            }

            yield return null;
        }
    }

    private void ActivateBrasier(bool toggleEntities)
    {
        if (autoOffCoroutine != null)
        {
            StopCoroutine(autoOffCoroutine);
            autoOffCoroutine = null;
        }

        if (!isOn)
        {
            ApplyState(true, toggleEntities);
        }

        if (!isPerpetual)
        {
            autoOffCoroutine = StartCoroutine(AutoOffRoutine());
        }
    }

    private IEnumerator AutoOffRoutine()
    {
        yield return new WaitForSeconds(onDuration);
        autoOffCoroutine = null;
        ApplyState(false, true);
    }

    private void ApplyState(bool activate, bool toggleEntities)
    {
        if (isOn == activate) return;

        isOn = activate;

        if (activate)
        {
            if (objectAnimation != null) objectAnimation.PlayAnimation("On");
            if (soundContainer != null) soundContainer.PlaySound(onSoundName, 1);
            if (objectParticles != null) objectParticles.SpawnParticle(smokeParticleName, gameObject, smokeInterval);

            if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
            flickerCoroutine = StartCoroutine(FlickerRoutine());
        }
        else
        {
            if (objectAnimation != null) objectAnimation.StopAnimation();
            if (spriteRenderer != null && offSprite != null) spriteRenderer.sprite = offSprite;
            if (objectParticles != null) objectParticles.StopSpawningParticles();

            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
            }
            if (entityLight != null) entityLight.TransitionLightIntensity(0f, 0f, flickerTransitionTime);
        }

        SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, isOn);

        if (toggleEntities)
        {
            StartCoroutine(ToggleLogicalEntities());
        }
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            float intensity = Random.Range(lightIntensityMin, lightIntensityMax);
            float radius = Random.Range(lightRadiusMin, lightRadiusMax);
            float interval = Random.Range(flickerIntervalMin, flickerIntervalMax);

            if (entityLight != null)
            {
                entityLight.TransitionLightIntensity(intensity, radius, flickerTransitionTime);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator ToggleLogicalEntities()
    {
        if (logicalEntites == null || logicalEntites.Count == 0) yield break;

        foreach (GameObject entity in logicalEntites)
        {
            if (entity == null) continue;

            if (entity.GetComponent<DoorBehiavor>() is DoorBehiavor door)
            {
                if (door.isOpen) door.CloseDoor();
                else door.OpenDoor();
            }
            else if (entity.GetComponent<Spades>() is Spades spades)
            {
                StartCoroutine(spades.RoutineSpades());
            }
            else if (entity.GetComponent<SkeletonBridgeBehiavor>() is SkeletonBridgeBehiavor bridge)
            {
                bridge.Activate();
            }
            else if (entity.GetComponent<LeverBehiavor>() is LeverBehiavor lever)
            {
                lever.ToggleLever(false, false);
            }
            else if (entity.GetComponent<RustCircleBehiavor>() is RustCircleBehiavor rustCircle)
            {
                rustCircle.Toggle();
            }
            else
            {
                entity.SetActive(!entity.activeSelf);
            }
        }

        yield return null;
    }
}
