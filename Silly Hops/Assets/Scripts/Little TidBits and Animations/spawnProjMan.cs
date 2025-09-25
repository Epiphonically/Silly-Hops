using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnProjMan : MonoBehaviour
{
    
    public Vector3 dest;
    public float spd;
    public float Timer = 1f;

    /* Spawner projectile */
    void LateUpdate()
    {
        Timer = Mathf.Clamp(Timer - Time.deltaTime, 0, Timer);
        if (Timer == 0) {
            Destroy(gameObject);
        }
        float delta = 0.1f;
        if ((transform.position - dest).magnitude > delta) {
            transform.position = transform.position + ((dest - transform.position).normalized * spd * Time.deltaTime);
        }
        
    }
}
