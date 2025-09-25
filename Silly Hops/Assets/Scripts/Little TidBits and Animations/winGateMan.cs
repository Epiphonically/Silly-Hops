using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class winGateMan : MonoBehaviour
{
    public bool on;
    public GameObject beacon1, beacon2, portal;
    private bool done = false;
    void Start() {
        on = false;
    }

    void Update()
    {
        if (portal.GetComponent<Interactible>().numInteractions > 0 && !done) {
       
            ManageScenes.win = true;
            done = true;
        }

        if (on) {
            portal.GetComponent<Interactible>().canInteract = true;
            portal.SetActive(true);
            beacon1.SetActive(true);
            beacon2.SetActive(true);
        } else {
            portal.GetComponent<Interactible>().canInteract = false;
            portal.SetActive(false);
            beacon1.SetActive(false);
            beacon2.SetActive(false);
        }
    }
}
