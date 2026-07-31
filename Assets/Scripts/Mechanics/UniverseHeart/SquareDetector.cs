using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheSquare.Mechanics.UniverseHeart
{
    [RequireComponent(typeof(Collider2D))]
    [ExecuteAlways]
    public class SquareDetector : MonoBehaviour
    {
        [Header("Movement (Optionnel)")]
        [Tooltip("Position de départ pour le mouvement de va-et-vient")]
        public Transform startPos;
        [Tooltip("Position d'arrivée pour le mouvement de va-et-vient")]
        public Transform endPos;
        [Tooltip("Vitesse de déplacement")]
        public float speed = 1f;

        [Header("Light Settings")]
        public Color lightColor = Color.red;
        [Tooltip("Intensité maximum lors de la pulsation")]
        public float maxIntensity = 1f;
        [Tooltip("Intensité minimum lors de la pulsation")]
        public float minIntensity = 0.2f;
        [Tooltip("Vitesse de la pulsation")]
        public float pulseSpeed = 2f;
        [Tooltip("Atténuation (falloff) de la lumière")]
        [Range(0f, 1f)]
        public float lightFalloff = 0.5f;

        private EntityLight entityLight;
        private float baseLightRadius = 1.5f;

        private float journeyTime = 0f;

        // Liste statique pour garder la trace de tous les détecteurs de la scène
        public static List<SquareDetector> allDetectors = new List<SquareDetector>();

        private void Awake()
        {
            UpdateColliderAndLightSize();

            if (!Application.isPlaying) return;

            // Ajouter ce détecteur à la liste
            if (!allDetectors.Contains(this))
            {
                allDetectors.Add(this);
            }
            
            // S'assurer que le collider est bien en mode trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnValidate()
        {
            // Appelé automatiquement par Unity quand une valeur change dans l'inspecteur
            UpdateColliderAndLightSize();
        }

        private void UpdateColliderAndLightSize()
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            Collider2D col = GetComponent<Collider2D>();
            BoxCollider2D boxCol = col as BoxCollider2D;

            if (sr != null)
            {
                // Adapter la taille du BoxCollider2D (utile pour le 9-Slicing)
                if (boxCol != null)
                {
                    boxCol.size = sr.size;
                }

                // Adapter le rayon (pour point light) à la plus grande dimension du sprite
                baseLightRadius = Mathf.Max(sr.size.x, sr.size.y) * 1.5f; 

                // Adapter la forme (pour Freeform light) à la taille exacte du sprite
                EntityLight el = GetComponentInChildren<EntityLight>();
                if (el != null)
                {
                    el.AdaptShapeToSprite(sr.size);
                }
            }
        }

        private void Start()
        {
            if (!Application.isPlaying) return;

            // On configure la lumière dans Start() au lieu de Awake()
            // pour être sûr que le script EntityLight a bien fini son propre Awake().
            entityLight = GetComponentInChildren<EntityLight>();
            if (entityLight != null)
            {
                entityLight.SetLightColor(lightColor);
                entityLight.SetLightFalloff(lightFalloff);
            }
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying) return;

            // Retirer ce détecteur de la liste s'il est détruit
            if (allDetectors.Contains(this))
            {
                allDetectors.Remove(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Vérifier si c'est le joueur qui entre dans le trigger
            Stats stats = collision.GetComponent<Stats>();
            if (stats != null && stats.entityType == EntityType.Player)
            {
                // Si on était dans la safezone, le manager bloquait l'alerte !
                // On force donc la sortie de la safezone :
                InsideTheSquareManager.is_in_safezone = false;

                // Déclencher l'alerte
                InsideTheSquareManager.TriggerReveal();

                // Faire disparaître tous les détecteurs
                DisappearAllDetectors();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            // --- Mouvement ---
            if (startPos != null && endPos != null)
            {
                journeyTime += Time.deltaTime * speed;
                
                // PingPong fait osciller la valeur de t entre 0 et 1 (aller-retour constant)
                float t = Mathf.PingPong(journeyTime, 1f);
                
                // SmoothStep adoucit la transition (accélération au départ, décélération à l'arrivée)
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                
                // Déplace le détecteur
                transform.position = Vector3.Lerp(startPos.position, endPos.position, smoothT);
            }

            // --- Pulsation de la lumière ---
            if (entityLight != null)
            {
                // PingPong crée une oscillation continue pour l'intensité
                float pulseT = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, pulseT);
                
                entityLight.SetLightIntensity(currentIntensity, baseLightRadius);
            }
        }

        public static void DisappearAllDetectors()
        {
            // On parcourt la liste et on désactive les détecteurs au lieu de les détruire
            foreach (SquareDetector detector in allDetectors)
            {
                if (detector != null && detector.gameObject != null)
                {
                    detector.gameObject.SetActive(false);
                }
            }
            // On ne vide pas la liste pour pouvoir les réactiver plus tard
        }

        public void ResetPosition()
        {
            journeyTime = 0f; // On réinitialise le timer du mouvement
            if (startPos != null)
            {
                transform.position = startPos.position;
            }
        }

        public static void ResetAllDetectors()
        {
            if (InsideTheSquareManager.instance != null && 
                InsideTheSquareManager.instance.heartImages != null && 
                InsideTheSquareManager.instance.heartsCollected >= InsideTheSquareManager.instance.heartImages.Length)
            {
                return;
            }

            foreach (SquareDetector detector in allDetectors)
            {
                if (detector != null && detector.gameObject != null)
                {
                    detector.gameObject.SetActive(true);
                    detector.ResetPosition();
                }
            }
        }
    }
}
