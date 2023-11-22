using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] bool _AutoPlay = false;
    [Space]
    [SerializeField] AudioSource _Source;
    [SerializeField] List<AudioClip> _Clips = new List<AudioClip>();



    private void Start()
    {
        if (_AutoPlay) PlaySound();
    }

    public void PlaySound()
    {
        if (_Clips.Count == 0) throw new System.Exception("There is no any clip in audio manager!");
        int randomIndex = Random.Range(0, _Clips.Count);
        _Source.clip = _Clips[randomIndex];
        _Source.Play();
    }

}
