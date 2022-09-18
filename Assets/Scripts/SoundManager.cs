using UnityEngine;
using UnityEngine.Audio;
using System;

public class SoundManager : MonoBehaviour {
    public MusicAssets[] Sounds; // create a list of audio
    void Awake(){ //start create 
        foreach (MusicAssets s in Sounds){
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.Clip;
            s.source.outputAudioMixerGroup = s.output;
            s.source.volume = s.Volume;
            s.source.pitch = s.Pitch;
            s.source.loop = s.loop;
            s.source.spatialBlend = s.Change2DTo3D; // 2D - 3D
        }
    }
    public void Play(string name){ // play the sound
        MusicAssets s = Array.Find(Sounds, sound => sound.AudioName == name);
        if(s == null){
            Debug.Log("missing sound " + name);
            return; //check if the sound is there or not
        }
        s.source.Play();      
    }
}