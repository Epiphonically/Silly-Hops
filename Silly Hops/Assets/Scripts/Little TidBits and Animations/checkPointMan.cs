using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkPointMan : MonoBehaviour
{
    public Material onMat;
    public Material comeToMe;
    public Material off;
    public Material currMat;
    
    public bool beacon;
    Material[] temp;
    public GameObject beaconPart, beaconMats;
    void Start() {
        currMat = off;
        
    }

    void Update()
    {
        if (beacon) {
            beaconPart.SetActive(true);
        } else {
            beaconPart.SetActive(false);
        }

        temp = GetComponent<MeshRenderer>().materials;
        temp[0] = currMat;
        GetComponent<MeshRenderer>().materials = temp;


        temp = beaconMats.GetComponent<ParticleSystemRenderer>().materials;
        temp[0] = currMat;
        temp[1] = currMat;
        beaconMats.GetComponent<ParticleSystemRenderer>().materials = temp;
        beaconMats.GetComponent<Renderer>().materials = temp;

    }
}
