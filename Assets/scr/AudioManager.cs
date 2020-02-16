using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public GridSound[] sounds;
    
    private void Awake()
    {
        foreach(GridSound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name)
    {
        GridSound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null)
        {
            s.source.Play();
        }
        else Debug.LogWarning("No sfx found for name: " + name);
    }

    public void Pause(string name)
    {
        GridSound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null)
        {
            s.source.Pause();
        }
        else Debug.LogWarning("No sfx found for name: " + name);
    }
}
