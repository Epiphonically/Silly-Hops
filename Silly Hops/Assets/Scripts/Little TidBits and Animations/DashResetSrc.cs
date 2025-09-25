using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashResetSrc : MonoBehaviour
{
    public bool gone = false;
    public PlayerManager player;
    public float timer = 1f, soundCd, maxSoundCd = 0.1f;
    public AudioSource mySource;
    public Material on, off;
    public Material[] temp;
    void Update() {
        soundCd = Mathf.Clamp(soundCd - Time.deltaTime, 0, soundCd);
        if (timer == 0) {
            gone = false;
        } else {
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, timer);
        }
        temp = GetComponent<MeshRenderer>().materials;
        if (!gone) {
            
            temp[0] = on;
            
        } else {
            temp[0] = off;
        }
        GetComponent<MeshRenderer>().materials = temp;
    }

    void Blip() {
        if (soundCd == 0) {
            soundCd = maxSoundCd;
            mySource.Play(0);
        }
        
         
    }
  
}
