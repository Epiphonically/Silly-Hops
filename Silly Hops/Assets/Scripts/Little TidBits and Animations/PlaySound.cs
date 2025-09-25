using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public AudioSource Source;
    public AudioClip selectedSound;
    // Start is called before the first frame update
    public void playSound() {
        Source.GetComponent<AudioSource>().clip = selectedSound;
        Source.Play(0);
    }
}
