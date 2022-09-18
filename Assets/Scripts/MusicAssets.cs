using UnityEngine.Audio;
using UnityEngine;
[System.Serializable]
public class MusicAssets // a storage for sound assets
{
    public string AudioName; // name of the audio
    public AudioClip Clip; // the clip of the audio
    public AudioMixerGroup output; // identify is sound effect or music
    [Range(.001f, 1f)]
    public float Volume = 1f; // the volume of the audio
    [Range(.1f, 3f)]
    public float Pitch = 1f; // the speed of the audio
    [Range(0.001f, 1f)]
    public float Change2DTo3D = 0.01f; // 2D or 3D
    public bool loop; // looping or not
    
    [HideInInspector]
    public AudioSource source;
}
