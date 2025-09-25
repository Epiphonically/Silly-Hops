using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageSparks : MonoBehaviour
{
    public float timer = 1f;
    public Material mat;
    public GameObject particle;

    /* A simple cleanup class for particles */
    void Update()
    {
        if (mat != null) {
            Material[] temp = particle.GetComponent<ParticleSystemRenderer>().materials;
            temp[0] = mat;
            particle.GetComponent<ParticleSystemRenderer>().materials = temp;
            particle.GetComponent<Renderer>().materials = temp;
        }
        
       
                                     
                                        
        timer = Mathf.Clamp(timer - Time.deltaTime, 0, timer);
        if (timer <= 0) {
            Destroy(gameObject);
        }
    }
}
