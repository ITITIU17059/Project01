using UnityEngine;

[System.Serializable]
public struct SoundEffect
{
    public string groupId;
    public AudioClip[] clips;
    public float volume;
}

public class SoundLibrary : MonoBehaviour
{
    public SoundEffect[] soundEffects;

    public AudioClip GetClipFromName(string name)
    {
        foreach (var soundeffect in soundEffects)
        {
            if (soundeffect.groupId == name)
            {
                return soundeffect.clips[Random.Range(0, soundeffect.clips.Length)];
            }
        }
        return null;
    }

    public float GetVolumeFromName(string name)
    {
        foreach (var soundeffect in soundEffects)
        {
            if (soundeffect.groupId == name)
            {
                return soundeffect.volume;
            }
        }
        return 1;
    }
}
