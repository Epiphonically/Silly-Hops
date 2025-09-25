using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    public enum POSITION {
        HEAD,
        BODY,
        FEET
    }
    public GameObject him;
    public POSITION pos;
    void Damage(float dmg) {
        if (dmg < 0) {
            dmg = Mathf.Abs(dmg);
            him.GetComponent<Collider>().SendMessage("Damage", dmg);
        } else {
            switch (pos) {
                case POSITION.HEAD:
                    him.GetComponent<Collider>().SendMessage("Damage", 2*dmg);
              
                break;

                case POSITION.BODY:
                    him.GetComponent<Collider>().SendMessage("Damage", dmg);
              
                break;

                case POSITION.FEET:
                    him.GetComponent<Collider>().SendMessage("Damage", (0.5f)*dmg);
               
                break;
            }
        }
        
        
        
    }
}
