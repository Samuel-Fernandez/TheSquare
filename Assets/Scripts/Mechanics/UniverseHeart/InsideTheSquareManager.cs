using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheSquare.Mechanics.UniverseHeart
{
    public class InsideTheSquareManager : MonoBehaviour
    {
        public static InsideTheSquareManager instance;

        [Header("Effets visuels")]
        public GameObject playerParticlesPrefab;
        private GameObject instantiatedParticles;

        [Header("Jauges de progression")]
        public Image leftEdge;
        public Image topEdge;
        public Image rightEdge;
        public Image bottomEdge;

        [Header("Musiques")]
        public string stealthMusic;
        public string alertMusic;

        [Header("UI Coeurs")]
        public Image[] heartImages;
        public int heartsCollected = 0;

        [Header("Victoire")]
        public GameObject victoryObject;

        [Header("Timer Paramètres")]
        public float timeToFill = 10f; // Temps total pour faire le tour
        
        public static bool player_is_revealed = false;
        public static bool is_in_safezone = true; // Vrai par défaut vu qu'on commence sur le coeur
        private float defaultSunIntensity = 0f;

        private AudioReverbFilter reverbFilter;
        private AudioEchoFilter echoFilter;

        public bool timerStarted = false;
        public float currentTimer = 0f;
        
        private bool hasPlayedBell1_0 = false;
        private bool hasPlayedBell1_25 = false;
        private bool hasPlayedBell2 = false;
        private bool hasPlayedBell3 = false;
        private bool hasPlayedReveal = false;

        private SoundContainer soundContainer;

        [HideInInspector] public List<SquareKillerBehiavor> squareKillers = new List<SquareKillerBehiavor>();
        [HideInInspector] public List<SquareEyeBehavior> squareEyes = new List<SquareEyeBehavior>();
        [HideInInspector] public List<SquareFollowersBehavior> squareFollowers = new List<SquareFollowersBehavior>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            soundContainer = GetComponent<SoundContainer>();

            // S'assurer que les jauges sont vides au départ
            if (leftEdge != null) leftEdge.fillAmount = 0f;
            if (topEdge != null) topEdge.fillAmount = 0f;
            if (rightEdge != null) rightEdge.fillAmount = 0f;
            if (bottomEdge != null) bottomEdge.fillAmount = 0f;

            player_is_revealed = false;
            is_in_safezone = true;
        }

        private IEnumerator Start()
        {
            if (ScenesManager.instance != null && ScenesManager.instance.transitionPanel != null && ScenesManager.instance.transitionPanel.activeInHierarchy)
            {
                yield return new WaitUntil(() => ScenesManager.instance.isSceneLoaded);
            }

            if (LightManager.instance != null && LightManager.instance.sunLight != null)
            {
                defaultSunIntensity = LightManager.instance.sunLight.intensity;
            }

            AudioListener listener = null;
            if (CameraManager.instance != null && CameraManager.instance.defaultCamera != null)
            {
                listener = CameraManager.instance.defaultCamera.GetComponent<AudioListener>();
                if (listener == null) listener = CameraManager.instance.defaultCamera.GetComponentInChildren<AudioListener>();
            }
            if (listener == null) listener = FindObjectOfType<AudioListener>();

            if (listener != null)
            {
                reverbFilter = listener.gameObject.GetComponent<AudioReverbFilter>();
                if (reverbFilter == null)
                {
                    reverbFilter = listener.gameObject.AddComponent<AudioReverbFilter>();
                }
                if (reverbFilter != null)
                {
                    reverbFilter.enabled = true;
                    reverbFilter.reverbPreset = AudioReverbPreset.Cave;
                }

                echoFilter = listener.gameObject.GetComponent<AudioEchoFilter>();
                if (echoFilter == null)
                {
                    echoFilter = listener.gameObject.AddComponent<AudioEchoFilter>();
                }
                if (echoFilter != null)
                {
                    echoFilter.enabled = true;
                    echoFilter.delay = 400; // Délai de l'écho en ms
                    echoFilter.decayRatio = 0.4f;
                    echoFilter.wetMix = 0.5f;
                    echoFilter.dryMix = 1.0f;
                }
            }

            // Exclure la musique des effets globaux
            if (SoundManager.instance != null && SoundManager.instance.audioSourceMusic != null)
            {
                SoundManager.instance.audioSourceMusic.bypassListenerEffects = true;
            }

            // Forcer la mise à jour des UI (pour cacher les postures et runes)
            if (StanceAndRunicManager.instance != null)
            {
                StanceAndRunicManager.instance.UpdateStanceUI();
                StanceAndRunicManager.instance.UpdateRuneUI();
            }

            // Forcer la mise à jour des stats du joueur pour retirer les bonus d'équipement
            if (PlayerManager.instance != null && PlayerManager.instance.player != null)
            {
                Stats playerStats = PlayerManager.instance.player.GetComponent<Stats>();
                if (playerStats != null)
                {
                    playerStats.SetVulnerability(false); // Rendre le joueur invulnérable
                    playerStats.UpdateStats();
                }

                PlayerAnimation pAnim = PlayerManager.instance.player.GetComponent<PlayerAnimation>();
                if (pAnim != null) pAnim.off = false;

                ObjectAnimation playerAnim = PlayerManager.instance.player.GetComponent<ObjectAnimation>();
                if (playerAnim != null)
                {
                    playerAnim.StopAllAnimations();
                }

                // Instanciation des particules sur la position du joueur puis parenter
                if (playerParticlesPrefab != null)
                {
                    instantiatedParticles = Instantiate(playerParticlesPrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
                    instantiatedParticles.transform.SetParent(PlayerManager.instance.player.transform);
                }
            }

            if (SoundManager.instance != null && !string.IsNullOrEmpty(stealthMusic))
            {
                SoundManager.instance.PlayMusic(stealthMusic);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;

                if (player_is_revealed)
                {
                    player_is_revealed = false;
                    if (LightManager.instance != null)
                    {
                        LightManager.instance.SetSunIntensity(defaultSunIntensity);
                    }
                }

                // Désactiver les filtres audio globaux
                if (reverbFilter != null) reverbFilter.enabled = false;
                if (echoFilter != null) echoFilter.enabled = false;

                // Remettre la musique à la normale
                if (SoundManager.instance != null && SoundManager.instance.audioSourceMusic != null)
                {
                    SoundManager.instance.audioSourceMusic.bypassListenerEffects = false;
                }
                
                if (SoundManager.instance != null && MeteoManager.instance != null && MeteoManager.instance.actualScene != null)
                {
                    SoundManager.instance.PlayMusic(MeteoManager.instance.actualScene.music);
                }

                // Détruire les particules à la fin du mode
                if (instantiatedParticles != null)
                {
                    Destroy(instantiatedParticles);
                }

                // Forcer le retour à la normale des UI
                if (StanceAndRunicManager.instance != null)
                {
                    StanceAndRunicManager.instance.UpdateStanceUI();
                    StanceAndRunicManager.instance.UpdateRuneUI();
                }

                // Forcer le retour des bonus d'équipement et des runes passives
                if (PlayerManager.instance != null && PlayerManager.instance.player != null)
                {
                    Stats playerStats = PlayerManager.instance.player.GetComponent<Stats>();
                    if (playerStats != null)
                    {
                        playerStats.SetVulnerability(true); // Rendre la vulnérabilité au joueur
                        playerStats.UpdateStats();
                    }
                }
            }
        }

        private void Update()
        {
            if (timerStarted && currentTimer < timeToFill)
            {
                currentTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(currentTimer / timeToFill);

                UpdateTimerVisual(progress);
                CheckTimerSounds(progress);
            }
        }
        
        private void UpdateTimerVisual(float progress)
        {
            // 1. On commence par le bas (0% à 25%)
            if (bottomEdge != null)
                bottomEdge.fillAmount = Mathf.Clamp01(progress / 0.25f);

            // 2. Puis à droite (25% à 50%)
            if (rightEdge != null)
                rightEdge.fillAmount = Mathf.Clamp01((progress - 0.25f) / 0.25f);

            // 3. Ensuite en haut (50% à 75%)
            if (topEdge != null)
                topEdge.fillAmount = Mathf.Clamp01((progress - 0.50f) / 0.25f);

            // 4. Et enfin à gauche (75% à 100%)
            if (leftEdge != null)
                leftEdge.fillAmount = Mathf.Clamp01((progress - 0.75f) / 0.25f);
        }

        private void CheckTimerSounds(float progress)
        {
            if (soundContainer == null) return;

            // Début (0%)
            if (!hasPlayedBell1_0)
            {
                soundContainer.PlayUISound("Bell1", 1);
                hasPlayedBell1_0 = true;
            }

            // Premier côté rempli (25%)
            if (progress >= 0.25f && !hasPlayedBell1_25)
            {
                soundContainer.PlayUISound("Bell1", 1);
                hasPlayedBell1_25 = true;
            }

            // Deuxième côté rempli (50%)
            if (progress >= 0.50f && !hasPlayedBell2)
            {
                soundContainer.PlayUISound("Bell2", 1);
                hasPlayedBell2 = true;
            }

            // Troisième côté rempli (75%)
            if (progress >= 0.75f && !hasPlayedBell3)
            {
                soundContainer.PlayUISound("Bell3", 1);
                hasPlayedBell3 = true;
            }

            // Timer terminé (100%)
            if (progress >= 1.0f && !hasPlayedReveal)
            {
                TriggerReveal();
            }
        }

        public static void TriggerReveal()
        {
            if (instance == null || player_is_revealed) return;

            if (instance.heartImages != null && instance.heartsCollected >= instance.heartImages.Length) return;
            
            // La safezone ne protège QUE si le timer n'est pas échu
            if (is_in_safezone && instance.currentTimer < instance.timeToFill) return;

            player_is_revealed = true;
            instance.hasPlayedReveal = true;

            if (instance.soundContainer != null)
            {
                instance.soundContainer.PlayUISound("Reveal", 1);
            }

            if (SoundManager.instance != null && !string.IsNullOrEmpty(instance.alertMusic))
            {
                SoundManager.instance.PlayMusic(instance.alertMusic);
            }

            if (CameraManager.instance != null)
            {
                CameraManager.instance.SetFilter(CameraFilter.SQUARE_EMERGENCY);
            }

            if (LightManager.instance != null)
            {
                LightManager.instance.SetSunIntensity(1f);
            }

            // Cacher tous les followers
            foreach (var follower in instance.squareFollowers)
            {
                if (follower != null) follower.HideFollower();
            }

            SquareDetector.DisappearAllDetectors();
        }

        public void ResetZone()
        {
            if (!player_is_revealed) return;

            player_is_revealed = false;
            hasPlayedReveal = false;

            if (CameraManager.instance != null)
            {
                CameraManager.instance.SetFilter(CameraFilter.NONE);
            }

            if (LightManager.instance != null)
            {
                float targetSun = defaultSunIntensity;
                if (MeteoManager.instance != null && MeteoManager.instance.actualScene != null)
                {
                    targetSun = MeteoManager.instance.actualScene.sunIntensity;
                    if (targetSun <= 0) targetSun = MeteoManager.squareWorldIntensity;
                }
                LightManager.instance.SetSunIntensity(targetSun);
            }

            if (SoundManager.instance != null && !string.IsNullOrEmpty(stealthMusic))
            {
                SoundManager.instance.PlayMusic(stealthMusic);
            }

            // Reset monsters
            foreach (var killer in squareKillers)
            {
                if (killer != null) killer.ResetToSleep();
            }
            foreach (var eye in squareEyes)
            {
                if (eye != null) eye.ResetToPhase1();
            }
            foreach (var follower in squareFollowers)
            {
                if (follower != null) follower.ResetFollower();
            }

            // Réactiver et réinitialiser la position de tous les détecteurs
            SquareDetector.ResetAllDetectors();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Stats stats = collision.GetComponent<Stats>();
            if (stats != null && stats.entityType == EntityType.Player)
            {
                if (heartImages != null && heartsCollected >= heartImages.Length)
                {
                    // 4 coeurs ramassés, on ne lance plus le timer
                }
                else if (!timerStarted)
                {
                    timerStarted = true;
                }
                is_in_safezone = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Stats stats = collision.GetComponent<Stats>();
            if (stats != null && stats.entityType == EntityType.Player)
            {
                is_in_safezone = true;
                if (player_is_revealed && currentTimer < timeToFill)
                {
                    ResetZone();
                }
            }

            GuardianHeartBehiavor heart = collision.GetComponent<GuardianHeartBehiavor>();
            if (heart != null && !heart.isAbsorbing)
            {
                heart.isAbsorbing = true;
                if (heart.isCarried)
                {
                    heart.ForceDrop();
                }

                StartCoroutine(AbsorbHeartRoutine(heart));
            }
        }

        private IEnumerator AbsorbHeartRoutine(GuardianHeartBehiavor heart)
        {
            // Si c'est le dernier coeur, le joueur doit être immobilisé immédiatement,
            // dès l'ajout du coeur, y compris pendant l'animation de fade qui suit.
            bool isLastHeart = heartImages != null && heartsCollected >= heartImages.Length - 1;
            if (isLastHeart && PlayerManager.instance != null && PlayerManager.instance.player != null)
            {
                Stats victoryStats = PlayerManager.instance.player.GetComponent<Stats>();
                if (victoryStats != null) victoryStats.canMove = false;
            }

            // S'assurer que le rigidbody est kineamatic pour qu'il ne tombe pas pendant l'animation
            Rigidbody2D rb = heart.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            SpriteRenderer heartSr = heart.GetComponentInChildren<SpriteRenderer>();
            if (heartSr != null)
            {
                Color originalColor = heartSr.color;
                float elapsed = 0f;

                // 1. S'illuminer en blanc pendant 0.5s
                while (elapsed < 0.5f)
                {
                    elapsed += Time.deltaTime;
                    heartSr.color = Color.Lerp(originalColor, Color.white, elapsed / 0.5f);
                    yield return null;
                }
                heartSr.color = Color.white;

                // 2. Disparaître (alpha 0) en 0.5s
                elapsed = 0f;
                while (elapsed < 0.5f)
                {
                    elapsed += Time.deltaTime;
                    Color c = heartSr.color;
                    c.a = Mathf.Lerp(1f, 0f, elapsed / 0.5f);
                    heartSr.color = c;
                    yield return null;
                }

                Color finalC = heartSr.color;
                finalC.a = 0f;
                heartSr.color = finalC;
            }

            // 3. Jouer le son et mettre à jour l'UI
            if (soundContainer != null)
            {
                soundContainer.PlayUISound("HeartGot", 1);
            }

            if (heartImages != null && heartsCollected < heartImages.Length)
            {
                if (heartImages[heartsCollected] != null)
                {
                    heartImages[heartsCollected].color = Color.white;
                }
                heartsCollected++;

                if (heartsCollected >= heartImages.Length)
                {
                    timerStarted = false;

                    if (player_is_revealed)
                    {
                        ResetZone();
                    }
                    else
                    {
                        SquareDetector.DisappearAllDetectors();
                    }

                    StartCoroutine(FadeOutTimerUIRoutine());

                    if (victoryObject != null)
                    {
                        victoryObject.SetActive(true);
                    }

                    if (soundContainer != null)
                    {
                        soundContainer.PlayUISound("Victory", 1);
                    }

                    if (SoundManager.instance != null)
                    {
                        SoundManager.instance.StopMusic();
                    }
                }
            }

            if (PlayerManager.instance != null && PlayerManager.instance.player != null)
            {
                Stats playerStats = PlayerManager.instance.player.GetComponent<Stats>();
                if (playerStats != null)
                {
                    // On ne remet PLUS la vulnérabilité ici, pour que le joueur reste toujours invulnérable dans le Square
                    // playerStats.SetVulnerability(true);
                }
            }

            // 4. Détruire l'entité
            Destroy(heart.gameObject);
        }

        private IEnumerator FadeOutTimerUIRoutine()
        {
            float duration = 1.0f;
            float elapsed = 0f;

            Color leftColor = leftEdge != null ? leftEdge.color : Color.white;
            Color topColor = topEdge != null ? topEdge.color : Color.white;
            Color rightColor = rightEdge != null ? rightEdge.color : Color.white;
            Color bottomColor = bottomEdge != null ? bottomEdge.color : Color.white;

            Color[] heartColors = null;
            if (heartImages != null)
            {
                heartColors = new Color[heartImages.Length];
                for (int i = 0; i < heartImages.Length; i++)
                {
                    heartColors[i] = heartImages[i] != null ? heartImages[i].color : Color.white;
                }
            }

            while (elapsed < duration)
            {
                // Time.deltaTime : si Time.timeScale est à 0 au moment de la victoire (pause, freeze, etc.),
                // elapsed n'avance jamais et le fade reste bloqué à alpha = 1 indéfiniment.
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

                if (leftEdge != null) leftEdge.color = new Color(leftColor.r, leftColor.g, leftColor.b, alpha);
                if (topEdge != null) topEdge.color = new Color(topColor.r, topColor.g, topColor.b, alpha);
                if (rightEdge != null) rightEdge.color = new Color(rightColor.r, rightColor.g, rightColor.b, alpha);
                if (bottomEdge != null) bottomEdge.color = new Color(bottomColor.r, bottomColor.g, bottomColor.b, alpha);

                if (heartImages != null)
                {
                    for (int i = 0; i < heartImages.Length; i++)
                    {
                        if (heartImages[i] != null)
                        {
                            Color hc = heartColors[i];
                            heartImages[i].color = new Color(hc.r, hc.g, hc.b, alpha);
                        }
                    }
                }

                yield return null;
            }

            // Un alpha à 0 laisse les Image actives (toujours raycast-blocking), mais surtout,
            // les jauges/coeurs sont généralement rangées sous un même panneau UI parent qui,
            // lui, reste actif et visible (fond/cadre du panneau). On remonte donc jusqu'à ce
            // parent via les références existantes (jauges puis coeurs) pour le désactiver.
            Transform gaugeParent = null;
            if (leftEdge != null) gaugeParent = leftEdge.transform.parent;
            else if (topEdge != null) gaugeParent = topEdge.transform.parent;
            else if (rightEdge != null) gaugeParent = rightEdge.transform.parent;
            else if (bottomEdge != null) gaugeParent = bottomEdge.transform.parent;

            if (gaugeParent != null) gaugeParent.gameObject.SetActive(false);

            Transform heartsParent = null;
            if (heartImages != null)
            {
                foreach (Image heartImage in heartImages)
                {
                    if (heartImage != null)
                    {
                        heartsParent = heartImage.transform.parent;
                        break;
                    }
                }
            }

            if (heartsParent != null && heartsParent != gaugeParent) heartsParent.gameObject.SetActive(false);
        }
    }
}
