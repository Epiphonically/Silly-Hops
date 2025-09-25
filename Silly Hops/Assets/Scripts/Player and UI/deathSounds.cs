using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deathSounds : MonoBehaviour
{
    public AudioSource mySource;
    public AudioClip[] death;

    void Awake() {
        mySource.GetComponent<AudioSource>().clip = death[Random.Range(0, death.Length)];
        mySource.Play(0);
    }
}
