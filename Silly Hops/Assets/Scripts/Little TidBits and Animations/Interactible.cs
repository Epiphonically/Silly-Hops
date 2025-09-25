using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible : MonoBehaviour
{

    public bool canInteract = false, timerStarted = false;
    public float timer = 0.1f;
    public int numInteractions = 0;
    public AudioSource mySource;

    void Start() {
     
        timerStarted = false;
        numInteractions = 0;

    }

    void Update() {
        if (timer == 0 && timerStarted) {
            canInteract = false;
            timerStarted = false;
        }
        if (timerStarted) {
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, timer);
        }
    }

    void Interact() {
        if (!timerStarted && canInteract) {
            timerStarted = true;
            mySource.Play(0);
            numInteractions++;
        }
        
    }
}
