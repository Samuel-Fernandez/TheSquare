using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Particles
{
    public GameObject particleSystem;
    public string id;
    public bool makeChild; // Nouveau champ ajouté
}


public class ObjectParticles : MonoBehaviour
{
    public List<Particles> particles = new List<Particles>();

    private Coroutine particleCoroutine;
    private PlayerController controller;

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    public void ChangeDuration(float duration)
    {
        foreach (var particle in particles)
        {
            var mainModule = particle.particleSystem.GetComponent<ParticleSystem>().main;
            mainModule.duration = duration - 2;
            mainModule.startLifetime = 1;
            particle.particleSystem.GetComponent<DestroyingTime>().destroyTime = duration;
        }
    }


    public void SpawnParticle(string id, Vector3 position, Quaternion rotation = default)
    {
        Particles particle = particles.Find(p => p.id == id);
        if (particle.particleSystem != null)
        {
            if (GetComponent<ObjectPerspective>() != null)
                particle.particleSystem.GetComponent<ParticleSystemRenderer>().sortingOrder = GetComponent<ObjectPerspective>().SortingOrder() + 3;
            else
                particle.particleSystem.GetComponent<ParticleSystemRenderer>().sortingOrder = 0;

            GameObject temp = Instantiate(particle.particleSystem, position, rotation == default ? Quaternion.identity : rotation);

            // Ajout de la parentalité si makeChild est vrai
            if (particle.makeChild)
            {
                temp.transform.SetParent(transform);
            }


            if (temp.GetComponent<EntityLight>())
            {
                temp.GetComponent<EntityLight>().TransitionLightIntensity(0, 0, temp.GetComponent<DestroyingTime>().destroyTime);
            }
        }
        else
        {
            Debug.LogWarning($"Particle with ID '{id}' not found or particleSystem not assigned.");
        }
    }


    public void SpawnParticle(string id, GameObject gameObject, float interval, Quaternion rotation = default, float x = 0, float y = 0)
    {
        if (particleCoroutine != null)
        {
            StopCoroutine(particleCoroutine);
        }
        particleCoroutine = StartCoroutine(SpawnParticleContinuously(id, gameObject, interval, rotation, x, y));
    }


    private IEnumerator SpawnParticleContinuously(string id, GameObject gameObject, float interval, Quaternion rotation, float x, float y)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            Vector3 offsetPosition = gameObject.transform.position + new Vector3(x, y, 0);
            SpawnParticle(id, offsetPosition, rotation);
        }
    }


    public void SpawnParticle(string id, Vector3 position, float interval, int iterations, Quaternion rotation = default)
    {
        StartCoroutine(SpawnParticleForIterations(id, position, interval, iterations, rotation));
    }

    private IEnumerator SpawnParticleForIterations(string id, Vector3 position, float interval, int iterations, Quaternion rotation)
    {
        for (int i = 0; i < iterations; i++)
        {
            yield return new WaitForSeconds(interval);
            SpawnParticle(id, position, rotation);
        }
    }

    public void StopSpawningParticles()
    {
        if (particleCoroutine != null)
        {
            StopCoroutine(particleCoroutine);
            particleCoroutine = null;
        }
    }

    private void Update()
    {
        if (GetComponent<Stats>() != null)
        {
            if (controller != null && controller.isMoving)
            {
                if (particleCoroutine == null)
                {
                    float delay = UnityEngine.Random.Range(0.5f * (1 / GetComponent<Stats>().speed), 3f * (1 / GetComponent<Stats>().speed));
                    particleCoroutine = StartCoroutine(SpawnRandomParticle(delay));
                }
            }
            else
            {
                if (particleCoroutine != null)
                {
                    StopCoroutine(particleCoroutine);
                    particleCoroutine = null;
                }
            }
        }
    }

    private IEnumerator SpawnRandomParticle(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);

            if (controller.isMoving)
            {
                SpawnParticle("LittleCloud", new Vector2(transform.position.x, transform.position.y - 0.3f));
            }
            else
            {
                particleCoroutine = null; // Réinitialise la coroutine
                yield break;
            }

            // Reset the delay for the next particle
            delay = UnityEngine.Random.Range(0.5f, 2f);
        }
    }
}
