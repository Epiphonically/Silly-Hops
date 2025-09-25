using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyDoor : MonoBehaviour
{

    public GameObject cutscene1;
    public GameObject Door1;
    public AudioSource ExplodeAudio;
    public BossLevelManager Manager;
    
    public void DestroyEntranceDoor() {
        Destroy(cutscene1);
        Manager.ResetCameras();
        Manager.shownewObjective("Explore further", true);
    }

    public void RuneExplosionSound() {
        ExplodeAudio.Play();
    }
}
