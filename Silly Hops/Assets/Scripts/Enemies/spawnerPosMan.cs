using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnerPosMan : MonoBehaviour
{
    public GameObject myEnemy = null;
    public float timer = 0;
    public float maxTimer = 8;
    public float delta = 0.1f;
    public float currAngle;
    public Vector3 origScale;
    public EnemyStats.ENEMYTYPE spawnType;
    public float bobAmmount = 0.2f;
    public float currBob = 0f;
    public Vector3 origPos;
    public bool smart = false;
    
    void Start() {
        currBob = Random.Range(0, 360);
        origPos = transform.position;
        currAngle = Random.Range(0, 360);
        origScale = transform.localScale;
    }
    void Update()
    {

        currBob = currBob + Time.deltaTime;
        if (currBob >= 360) {
            currBob = 0;
        }
        transform.position = origPos + new Vector3(0, bobAmmount*Mathf.Sin(currBob), 0);

        currAngle += 4*Time.deltaTime;
        if (currAngle >= 360) {
            currAngle = 0;
        }
        float lambda = delta*Mathf.Sin(currAngle);
        transform.localScale = origScale + new Vector3(lambda, lambda, lambda);

        if (myEnemy == null) {
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, timer);
        } else {
            timer = maxTimer;
        }

        
    }
}
