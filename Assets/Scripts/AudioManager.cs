using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour {

    public Sound[] sounds;
    public static AudioManager instance; //allows for the prefab to remain inbetween scenes
    List<Sound> currentSounds = new List<Sound>();
    bool muted;

	void Awake () {

        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>(); //great way to edit/add any sounds to the game
            s.source.clip = s.clip; //assigning variables
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
	}

    public void OnMuteButton(Text text)
    {
        muted = !muted;

        for (int i = 0; i < sounds.Length; i++)
            sounds[i].source.mute = muted;

        if (muted)
            text.text = "Unmute";
        else
            text.text = "Mute";        
    }

    

    private void Start()
    {
        Play("Background");
    }

    public void Play(string name)//function to play a sound by name
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        s.source.Play();
        if(!currentSounds.Contains(s))
            currentSounds.Add(s);
    }

    public void Stop(string name) //function for stopping a specifc sound
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        s.source.Stop();
    }

    public List<Sound> CurrentSounds()
    {
        return currentSounds;
    }

    public void PauseAll()
    {
        foreach (Sound s in currentSounds)
        {
            if (s.name == "Background")
                continue;
            if(s.source.isPlaying)
                s.source.Pause();
        }
    }

    public void ResumeAll() //for when the game is paused 
    {
        foreach (Sound s in currentSounds)
            s.source.UnPause();
    }
}
