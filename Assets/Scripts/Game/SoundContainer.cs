using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundGroup
{
    public string id;
    public List<AudioClip> clips;
}

public class SoundContainer : MonoBehaviour
{
    public List<SoundGroup> allSounds;

    public void PlayUISound(string id, int pitchPower)
    {
        // Trouver le groupe de sons correspondant à l'ID
        SoundGroup soundGroup = allSounds.Find(group => group.id.Contains(id));

        if (soundGroup == null)
        {
            Debug.LogError("Sound group not found: " + id);
            return;
        }

        if (soundGroup.clips == null || soundGroup.clips.Count == 0)
        {
            return;
        }

        // Sélectionner un clip aléatoire
        AudioClip clip = soundGroup.clips[Random.Range(0, soundGroup.clips.Count)];

        // Déterminer le pitch
        float pitch = 1f;
        switch (pitchPower)
        {
            case 1: pitch = 1f + Random.Range(-0.05f, 0.05f); break;
            case 2: pitch = 1f + Random.Range(-0.1f, 0.1f); break;
            case 3: pitch = 1f + Random.Range(-0.2f, 0.2f); break;
        }

        // Jouer le son en 2D via SoundManager
        SoundManager.instance.PlayUISound(clip, pitch);
    }

    public void PlaySound(string id, int pitchPower)
    {
        // Trouver le groupe de sons correspondant à l'ID
        SoundGroup soundGroup = allSounds.Find(group => group.id.Contains(id));

        if (soundGroup == null)
        {
            Debug.LogError("Sound group not found: " + id);
            return;
        }

        if (soundGroup.clips == null || soundGroup.clips.Count == 0)
        {
            return;
        }

        // Sélectionner un clip aléatoire
        AudioClip clip = soundGroup.clips[Random.Range(0, soundGroup.clips.Count)];

        // Déterminer le pitch en fonction du pitchPower
        float pitch = 1f; // Valeur par défaut
        switch (pitchPower)
        {
            case 1:
                pitch = 1f + Random.Range(-0.05f, 0.05f);
                break;
            case 2:
                pitch = 1f + Random.Range(-0.1f, 0.1f);
                break;
            case 3:
                pitch = 1f + Random.Range(-0.2f, 0.2f);
                break;
        }

        // Jouer le son avec le pitch déterminé
        SoundManager.instance.PlaySound(clip, pitch, transform.position);
    }
}
