using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public enum TYPE
    {
        NORMAL,
        BOOM
    }
    public Vector3 direction;
    public float spd = 0f;
    public float destroyTimer = 0f;
    public bool destroyTimerStart = true, startGrowing = false, hasTrail = true;
    public float rayCastLen = 1f;
    public float dmg = 5f;
    public RaycastHit[] hitList;
    public RaycastHit hit;
    public int team;
    public TYPE myType = TYPE.NORMAL;

    public float needToDestroyTimer = 5f;
    public Vector3 big = new Vector3(5, 5, 5);
    public AudioSource xploSound;
    public AudioClip[] normalXplo, evilXplo;
    public GameObject bulletSpark;
    public TrailRenderer trail;
    public bool sparked = false;
    public Color myColor;
   
    public GameObject boss;

    

    void Update() {
        trail.enabled = hasTrail;
        GetComponent<Light>().color = myColor;
        switch (myType) {
            case TYPE.NORMAL: /* A normal bullet */
                hitList = Physics.RaycastAll(transform.position, direction, rayCastLen);
                System.Array.Sort(hitList, (x, y) => x.distance.CompareTo(y.distance));
                for (int i = 0; i < hitList.Length; i++) {
                    
                    hit = hitList[i];
                    if (team != 0) { /* Shot by enemy */
                        if (hit.collider.gameObject.layer != 7){ /* If shot by enemy bullet should go through other enemies */
                            spd = 0;
                            if (!destroyTimerStart) {
                                destroyTimer = 0.1f;
                                destroyTimerStart = true;
                            }
                            if (hit.collider != null && hit.collider.gameObject.layer == 6) {
                                hit.collider.SendMessage("Damage", dmg);
                                Destroy(gameObject);
                            }
                            if (!sparked) {
                                GameObject theSpark = Instantiate(bulletSpark, transform.position, Quaternion.identity);
                                Material[] temp = GetComponent<MeshRenderer>().materials;
                                
                                theSpark.GetComponent<ParticleSystemRenderer>().materials = temp;
                                theSpark.GetComponent<Renderer>().materials = temp;
                                sparked = true;
                            }
                        }
                    } else if (!destroyTimerStart) {
                        spd = 0;
                        destroyTimer = 0.1f;
                        destroyTimerStart = true;
                    }
                    

                }


                break;

            case TYPE.BOOM: /* A blast that will expand! */
                if (Physics.Raycast(transform.position, direction, out hit, rayCastLen)) {
                    if (team != 0) { /* This means enemy shot */
                   
                        if (hit.collider.gameObject.layer != 7) {
                            spd = 0;
                            if (!destroyTimerStart) {
                                destroyTimer = 1;
                                destroyTimerStart = true;
                            }
                            if (!startGrowing) {
                                Collider[] colliders = Physics.OverlapSphere(transform.position, big.x);
                                for (int i = 0; i < colliders.Length; i++) {

                                    if (colliders[i] != null && colliders[i].gameObject.layer == 6) { /* We hit the player */
                                  
    
                                        colliders[i].SendMessage("CripplingDamage", dmg);
                                        colliders[i].SendMessage("Bounce", (colliders[i].transform.position - hit.point).normalized);
                                    }
                                    
                                }
                                xploSound.GetComponent<AudioSource>().clip = evilXplo[Random.Range(0, evilXplo.Length)];
                                xploSound.Play();
                                startGrowing = true;
                            }
                        }
                        
                    } else if (hit.collider.gameObject != null && hit.collider.gameObject.GetComponent<platformLife>() != null && hit.collider.gameObject.GetComponent<platformLife>().bouncy) {
                        spd = 0;
                        if (!destroyTimerStart) {
                            destroyTimer = 1;
                            destroyTimerStart = true;
                        }
                        if (!startGrowing) {
                            Collider[] colliders = Physics.OverlapSphere(transform.position, big.x);
                            for (int i = 0; i < colliders.Length; i++) {

                                if (colliders[i] != null && colliders[i].gameObject.layer == 6)
                                {
                                    colliders[i].SendMessage("Bounce", (colliders[i].transform.position - hit.point).normalized);
                                }
                                
                            }
                            xploSound.GetComponent<AudioSource>().clip = normalXplo[Random.Range(0, normalXplo.Length)];
                            xploSound.Play();
                            startGrowing = true;
                        }
                    }
                }



                break;
        }
        if (startGrowing)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, big, 10 * Time.deltaTime);
        }
        needToDestroyTimer -= Time.deltaTime;
        if (needToDestroyTimer <= 0)
        {
            
            Destroy(gameObject);
        }
        if (destroyTimerStart)
        {
            destroyTimer -= Time.deltaTime;
            if (destroyTimer <= 0)
            {
                Destroy(gameObject);
            }
        }
        transform.position += direction.normalized * spd * Time.deltaTime;
    }
}
