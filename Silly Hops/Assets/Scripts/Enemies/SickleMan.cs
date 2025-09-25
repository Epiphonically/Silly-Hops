using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SickleMan : MonoBehaviour
{
    public Vector3 direction;
    public float spd;
    public float rotDegree = 0;
    private float timePerRevolution = 0.2f;
    public float needToDestroyTimer = 5f;
    public float dmg = 0;
    public int alternate = 1;
    
    void Update() {
        if (!ManageScenes.paused) {
            /* We alter the transform rotation to give the illusion of a spinning sword
               this follows the visual grading category */
            transform.position += direction.normalized * Time.deltaTime * spd;
            transform.localRotation = Quaternion.AngleAxis(rotDegree, transform.up);
            if (alternate < 0) { /* Rotate "positively" */
                rotDegree = Mathf.Clamp(rotDegree - 360*Time.deltaTime/timePerRevolution, 0, 360);
                if (rotDegree <= 0) {
                    rotDegree = 360;
                }
            } else { /* Rotate negatively */
                rotDegree = Mathf.Clamp(rotDegree + 360*Time.deltaTime/timePerRevolution, 0, 360);
                if (rotDegree >= 360) {
                    rotDegree = 0;
                }
            }
            
            
            needToDestroyTimer = Mathf.Clamp(needToDestroyTimer - Time.deltaTime, 0, needToDestroyTimer);
            if (needToDestroyTimer <= 0) {
                Destroy(gameObject);
            }
        }
        
    }

    void OnCollisionEnter(Collision c) {
        if (c.collider.tag == "Player") {
            c.collider.SendMessage("CripplingDamage", dmg);
            Destroy(gameObject);
        }
    }
}
