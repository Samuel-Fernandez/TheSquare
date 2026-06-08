using UnityEngine;
using System.Collections;

public class IceWizardSpikeBehavior : MonoBehaviour
{
    private int _damage;
    private bool _hasTouched = false;
    private bool _canDamage = false;
    private SpriteRenderer _spriteRenderer;
    private EntityLight _entityLight;

    [Header("Behavior Settings")]
    public float activationDelay = 0.5f;
    public float lifetime = 1.5f;
    public float fadeDuration = 0.5f;

    [Header("Light Effect")]
    public float lightIntensity = 1f;
    public float lightRadius = 3f;

    public void Init(int damage)
    {
        _damage = damage;
    }

    private void Start()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _entityLight = GetComponentInChildren<EntityLight>();

        // Jouer un son
        SoundContainer sc = GetComponent<SoundContainer>();
        if (sc != null)
        {
            sc.PlaySound("Appear", 2);
        }

        // Animation "Appear"
        ObjectAnimation anim = GetComponent<ObjectAnimation>();
        if (anim != null)
        {
            // Lance l'animation
            anim.PlayAnimation("Appear", true);
        }

        // Apparition de la lumière
        if (_entityLight != null)
        {
            _entityLight.TransitionLightIntensity(lightIntensity, lightRadius, 0.2f);
        }

        // Lance le compteur de disparition
        StartCoroutine(LifetimeRoutine());
    }

    private IEnumerator LifetimeRoutine()
    {
        // Attend le délai d'activation
        yield return new WaitForSeconds(activationDelay);
        _canDamage = true;

        // Attend le temps restant avant le fade
        // lifetime (1.5s par défaut) - activationDelay (0.5s) = 1.0s d'activité
        float activeTime = lifetime > activationDelay ? lifetime - activationDelay : 1f;
        yield return new WaitForSeconds(activeTime);

        // Si aucun contact n'a été fait, ça s'estompe
        if (!_hasTouched)
        {
            StartCoroutine(FadeAndDestroy());
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!_canDamage || _hasTouched) return;

        Stats targetStats = collision.GetComponent<Stats>();

        // On vérifie que c'est bien le joueur (et vulnérable)
        if (targetStats != null && targetStats.entityType == EntityType.Player && targetStats.isVulnerable)
        {
            LifeManager lifeManager = collision.GetComponent<LifeManager>();
            if (lifeManager != null && lifeManager.life > 0)
            {
                _hasTouched = true;

                // Application des dégâts reçus lors du Init()
                lifeManager.TakeDamage(_damage, this.gameObject, false, 1f);

                // 25% chance de générer le statut Glace
                if (Random.value <= 0.25f)
                {
                    EntityEffects effects = collision.GetComponent<EntityEffects>();
                    if (effects != null)
                    {
                        effects.SetState(0, false, true, false, false);
                    }
                }

                // Jouer un son
                SoundContainer sc = GetComponent<SoundContainer>();
                if (sc != null)
                {
                    sc.PlaySound("Hit", 2);
                }

                // Détruire instantanément l'objet
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        _hasTouched = true;

        // Désactive la collision pour éviter tout dommage pendant le fondu
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Éteint la lumière progressivement
        if (_entityLight != null)
            _entityLight.TransitionLightIntensity(0f, 0f, fadeDuration);

        // Fade du sprite
        if (_spriteRenderer != null)
        {
            float elapsedTime = 0f;
            Color startColor = _spriteRenderer.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float a = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeDuration);
                _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, a);
                yield return null;
            }
        }

        // Détruit le gameobject à la fin
        Destroy(gameObject);
    }
}
