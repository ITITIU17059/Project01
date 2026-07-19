using UnityEngine;

[System.Serializable]
public struct SoundEffect
{
    public string groupId;
    public AudioClip[] clips;
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
}
