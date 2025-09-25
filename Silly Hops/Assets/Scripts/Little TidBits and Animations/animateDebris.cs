using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animateDebris : MonoBehaviour
{
    public float currBob;
    public float bobAmmount;
    public Vector3 origPos;
    public float maxScale;

    void Start() {
        currBob = Random.Range(0, 360);
        bobAmmount = Random.Range(1, 3);
        maxScale = Random.Range(1, 10);
        transform.localRotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        origPos = transform.position;
        transform.localScale = new Vector3(Random.Range(1, maxScale), Random.Range(1, maxScale), Random.Range(1, maxScale));
     
    }

    void Update()
    {   
        currBob = currBob + Time.deltaTime;
        if (currBob >= 360) {
            currBob = 0;
        }
        transform.position = origPos + new Vector3(0, bobAmmount*Mathf.Sin(currBob), 0);
    }
}
