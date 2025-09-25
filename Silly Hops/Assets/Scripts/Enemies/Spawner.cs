using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    
    public EnemyStats stats;
    public GameObject[] spawnPositions;
    public GameObject enemy, spawnProj, deathSpark;
    public float revolutionsPerSecond = 1;
    public float currAngle;

    public float currBob;
    public float bobAmmount;
    public Vector3 origPos;

    public AudioSource mySource;
    public AudioClip[] ouch, death, spawn;

    public Material myMat;

    void Start() {
        currAngle = Random.Range(0, 360);
        currBob = Random.Range(0, 360);
        bobAmmount = Random.Range(1, 3);
        origPos = transform.position;
    }

    void Update()
    {

        if (stats.hasShield) {
            stats.regenShieldTimer = Mathf.Clamp(stats.regenShieldTimer - Time.deltaTime, 0, stats.regenShieldTimer);
            if (stats.regenShieldTimer == 0) {
                stats.shield = Mathf.Clamp(stats.shield + stats.maxShield * (Time.deltaTime / stats.timeItTakesToRegenShield), 0, stats.maxShield);
            }
        } else {
            stats.shield = 0;
        }
        
        currAngle = currAngle + 360*Time.deltaTime/revolutionsPerSecond;
        if (currAngle >= 360) {
            currAngle = 0;
        }

        transform.localRotation = Quaternion.Euler(0, currAngle, 0);

        if (currBob >= 360) {
            currBob = 0;
        }
        transform.position = origPos + new Vector3(0, bobAmmount*Mathf.Sin(currBob), 0);

        

        if (!stats.idle) { 
            for (int i = 0; i < spawnPositions.Length; i++) {
                spawnerPosMan curr = spawnPositions[i].GetComponent<spawnerPosMan>();
                if (curr.timer == 0 && curr.myEnemy == null) {

                    GameObject zap = Instantiate(spawnProj, transform.position, Quaternion.identity);
                    zap.GetComponent<spawnProjMan>().dest = spawnPositions[i].transform.position; 
                    zap.GetComponent<spawnProjMan>().spd = 50;
                    curr.myEnemy = Instantiate(enemy, spawnPositions[i].transform.position, Quaternion.identity);
                    mySource.GetComponent<AudioSource>().clip = spawn[Random.Range(0, spawn.Length)];
                    mySource.Play(0);



                    curr.myEnemy.GetComponent<EnemyManager>().origPos = spawnPositions[i].transform.position;
                    curr.myEnemy.GetComponent<EnemyStats>().enemyType = curr.spawnType;
                    curr.myEnemy.GetComponent<EnemyStats>().smart = curr.smart;
                }
            }
        } 
        

        
    }


    void Damage(float dmg) {
        dmg = Mathf.Abs(dmg);
        if (!stats.invincible) {
            stats.regenShieldTimer = stats.maxRegenShieldTimer;
            
            if (stats.shield > 0) {
                
                stats.shield = Mathf.Clamp(stats.shield - dmg, 0, stats.maxShield);
            } else {
                stats.hp = Mathf.Clamp(stats.hp - dmg, 0, stats.maxHp);
            }
            if (stats.hp <= 0) {

                GameObject spark = Instantiate(deathSpark, transform.position, Quaternion.identity);
                spark.GetComponent<ManageSparks>().mat = myMat;

                for (int i = 0; i < spawnPositions.Length; i++) {
                    Destroy(spawnPositions[i]);
                }

                Destroy(gameObject);
            } else {
                mySource.GetComponent<AudioSource>().clip = ouch[Random.Range(0, ouch.Length)];
                mySource.Play(0);
            }
        }
        
    }
}
