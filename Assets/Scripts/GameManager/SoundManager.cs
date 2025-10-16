using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource audioSourceMusic;
    public int audioSourcePoolSize = 10; // Nombre d'AudioSources dans le pool
    private List<AudioSource> audioSourcePool;
    private int currentSourceIndex = 0;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SOUND_VOLUME_KEY = "SoundVolume";

    [System.Serializable]
    public class MusicTrack
    {
        public string id;
        public AudioClip clip;
    }

    public List<MusicTrack> musicTracks;

    public string currentMusicId = "";
    private AudioClip currentMusicClip = null;
    private Coroutine musicTransitionCoroutine;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSourcePool();
        }
        else
            Destroy(gameObject);

        if (!PlayerPrefs.HasKey(MUSIC_VOLUME_KEY))
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, 0.5f);

        if (!PlayerPrefs.HasKey(SOUND_VOLUME_KEY))
            PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, 0.5f);

        StartCoroutine(AmbientSounds());
    }

    private void Update()
    {
        if (audioSourceMusic)
            audioSourceMusic.volume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY);
    }

    // Méthode pour changer le volume de la musique
    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    // Méthode pour changer le volume du son
    public void SetSoundVolume(float volume)
    {
        PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    public void PlayUISound(AudioClip sound, float pitch = 1f)
    {
        if (sound == null) return;

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.pitch = pitch;
        audioSource.volume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY);
        audioSource.spatialBlend = 0f; // 2D
        audioSource.PlayOneShot(sound);
        Destroy(audioSource, sound.length);
    }


    // Initialiser le pool d'AudioSource
    private void InitializeAudioSourcePool()
    {
        audioSourcePool = new List<AudioSource>();
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            GameObject audioSourceObj = new GameObject("AudioSource_" + i);
            audioSourceObj.transform.SetParent(transform);
            AudioSource audioSource = audioSourceObj.AddComponent<AudioSource>();
            audioSource.volume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY);
            audioSource.spatialBlend = 1.0f; // Full 3D sound
            audioSourcePool.Add(audioSource);
        }
    }

    // Obtenir un AudioSource disponible dans le pool
    private AudioSource GetAvailableAudioSource()
    {
        AudioSource audioSource = audioSourcePool[currentSourceIndex];
        currentSourceIndex = (currentSourceIndex + 1) % audioSourcePoolSize;
        return audioSource;
    }

    // Joue un AudioClip unique avec position
    public void PlaySound(AudioClip sound, float pitch, Vector3? sourcePosition = null)
    {
        if (sound == null)
            return;

        AudioSource audioSource = gameObject.AddComponent<AudioSource>(); // Crée une nouvelle instance d'AudioSource
        audioSource.pitch = pitch;

        if (sourcePosition.HasValue)
        {
            audioSource.volume = CalculateVolumeByDistance(sourcePosition.Value);
            audioSource.panStereo = CalculateStereoPanByPosition(sourcePosition.Value);
        }
        else
        {
            audioSource.volume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY);
            audioSource.panStereo = 0; // Center
        }

        audioSource.PlayOneShot(sound);
        Destroy(audioSource, sound.length); // Détruit l'AudioSource après la fin du son
    }

    // Joue un AudioClip aléatoire à partir d'une liste avec position
    public void PlaySound(List<AudioClip> sounds, float pitch, Vector3? sourcePosition = null)
    {
        if (sounds == null || sounds.Count == 0) return; // Vérifie que la liste n'est pas vide

        int index = Random.Range(0, sounds.Count); // Sélectionne un index aléatoire
        PlaySound(sounds[index], pitch, sourcePosition); // Utilise la première méthode PlaySound
    }

    private float CalculateVolumeByDistance(Vector3 sourcePosition)
    {
        float maxVolume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY);
        float maxDistance = 15f; // Distance maximale pour entendre le son à son volume maximal
        float distance = Vector3.Distance(PlayerManager.instance.player.transform.position, sourcePosition);
        float volume = Mathf.Clamp(1 - (distance / maxDistance), 0, maxVolume);
        return volume;
    }

    private float CalculateStereoPanByPosition(Vector3 sourcePosition)
    {
        Vector3 playerPosition = PlayerManager.instance.player.transform.position;
        float pan = (sourcePosition.x - playerPosition.x) / 5f; // 5f corresponds to the max distance for full stereo effect
        return Mathf.Clamp(pan, -1f, 1f); // -1 = left, 1 = right
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Clip is null.");
            return;
        }

        // Si déjà en train de jouer ce clip, on ne fait rien
        if (currentMusicClip == clip && audioSourceMusic.isPlaying && !isTransitioning)
        {
            return;
        }

        // Trouver l'ID de la musique si possible
        string clipId = GetIdForClip(clip);

        // Si on n'a pas trouvé d'ID, utiliser le nom du clip
        if (string.IsNullOrEmpty(clipId))
        {
            clipId = clip.name;
        }


        // Arrêter la transition en cours si nécessaire
        StopMusicTransition();

        // Démarrer une nouvelle transition
        musicTransitionCoroutine = StartCoroutine(TransitionToNewMusic(clip));
        isTransitioning = true;

        // Mettre à jour les références actuelles
        currentMusicClip = clip;
        currentMusicId = clipId;
    }

    public void PlayMusic(string id)
    {
        // Si l'ID est vide, ne rien faire
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("Music ID is null or empty.");
            return;
        }

        // Vérifier si la musique actuelle correspond à l'ID
        if (currentMusicId == id && audioSourceMusic.isPlaying && !isTransitioning)
        {
            return;
        }

        AudioClip newClip = GetMusicClipById(id);

        if (newClip == null)
        {
            Debug.LogWarning($"Music track not found for ID: {id}");
            return;
        }


        // Arrêter la transition en cours si nécessaire
        StopMusicTransition();

        // Démarrer une nouvelle transition
        musicTransitionCoroutine = StartCoroutine(TransitionToNewMusic(newClip));
        isTransitioning = true;

        // Mettre à jour les références actuelles
        currentMusicClip = newClip;
        currentMusicId = id;
    }

    private string GetIdForClip(AudioClip clip)
    {
        foreach (MusicTrack track in musicTracks)
        {
            if (track.clip == clip)
            {
                return track.id;
            }
        }
        return null;
    }

    private AudioClip GetMusicClipById(string id)
    {
        foreach (MusicTrack track in musicTracks)
        {
            if (track.id == id)
            {
                return track.clip;
            }
        }
        return null;
    }

    private void StopMusicTransition()
    {
        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(musicTransitionCoroutine);
            musicTransitionCoroutine = null;
        }
    }

    private IEnumerator TransitionToNewMusic(AudioClip newClip)
    {
        // Charger le clip en mémoire avant de le jouer
        if (!newClip.preloadAudioData)
        {
            newClip.LoadAudioData();
            while (newClip.loadState == AudioDataLoadState.Loading)
                yield return null; // Attend le chargement complet
        }

        // Fade out current music if playing
        float initialVolume = audioSourceMusic.volume;
        float fadeOutDuration = 1f;

        if (audioSourceMusic.isPlaying && audioSourceMusic.clip != null)
        {
            for (float t = 0; t < fadeOutDuration; t += Time.unscaledDeltaTime)
            {
                audioSourceMusic.volume = Mathf.Lerp(initialVolume, 0, t / fadeOutDuration);
                yield return null;
            }
            audioSourceMusic.Stop();
        }

        // Play new music
        audioSourceMusic.clip = newClip;
        audioSourceMusic.volume = 0;
        audioSourceMusic.Play();


        // Fade in new music
        float fadeInDuration = 1f;
        for (float t = 0; t < fadeInDuration; t += Time.unscaledDeltaTime)
        {
            audioSourceMusic.volume = Mathf.Lerp(0, initialVolume, t / fadeInDuration);
            yield return null;
        }
        audioSourceMusic.volume = initialVolume;

        // Transition terminée
        isTransitioning = false;
    }

    public void StopMusic(float fadeOutDuration = 1f)
    {
        StopMusicTransition();

        if (fadeOutDuration <= 0)
        {
            audioSourceMusic.Stop();
            // Réinitialiser les références
            currentMusicId = "";
            currentMusicClip = null;
        }
        else
        {
            StartCoroutine(FadeOutSound(fadeOutDuration));
        }
    }

    private IEnumerator FadeOutSound(float fadeOutDuration)
    {
        float initialVolume = audioSourceMusic.volume;
        for (float t = 0; t < fadeOutDuration; t += Time.unscaledDeltaTime)
        {
            audioSourceMusic.volume = Mathf.Lerp(initialVolume, 0, t / fadeOutDuration);
            yield return null;
        }
        audioSourceMusic.Stop();
        audioSourceMusic.volume = initialVolume; // Reset the volume for future use

        // Réinitialiser les références
        currentMusicId = "";
        currentMusicClip = null;
    }

    public void StopSound(float fadeOutDuration)
    {
        if (fadeOutDuration <= 0)
        {
            audioSourcePool[currentSourceIndex].Stop();
        }
        else
        {
            StartCoroutine(FadeOutSoundSound(fadeOutDuration));
        }
    }

    private IEnumerator FadeOutSoundSound(float fadeOutDuration)
    {
        AudioSource audioSource = audioSourcePool[currentSourceIndex];
        float initialVolume = audioSource.volume;
        for (float t = 0; t < fadeOutDuration; t += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(initialVolume, 0, t / fadeOutDuration);
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = initialVolume; // Reset the volume for future use
    }

    public IEnumerator AmbientSounds()
    {
        while(true)
        {
            yield return new WaitForSecondsRealtime(0.5f);

            if (Random.Range(0, 100) >= 75 && MeteoManager.instance.actualScene.ambientSound != null && MeteoManager.instance.actualScene.ambientSound.Count > 0)
            {
                PlaySound(MeteoManager.instance.actualScene.ambientSound[Random.Range(0, MeteoManager.instance.actualScene.ambientSound.Count)], 3);
            }
        }
       
    }
}