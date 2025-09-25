using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    public GameObject player;
    public UnityEngine.AI.NavMeshAgent agent;

    public Vector3 origPos;
    public EnemyManager him;
    public EnemyStats stats;

    public int unitCirclePartions = 10;

    public RaycastHit[] hitList;
    public RaycastHit hit;

    public Vector3 desired;
    public GameObject testBox;
    
    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        origPos = transform.position;
        stats = him.stats;
    }
    /* Our movement utilizes the Nav Mesh feature of Unity. This script goes under the algorithmic complexity category. */
    void Update() {
        
        if (!stats.idle && !(ManageScenes.win || ManageScenes.lost)) {
        if (stats.enemyType == EnemyStats.ENEMYTYPE.SENTRY) { /* The SENTRY enemy type does not move */
            stats.currState = EnemyStats.ENEMYSTATE.SHOOTING;
        } else {
            
        
        Vector3 toPlayerPos = player.GetComponent<PlayerManager>().head.transform.position - him.transform.position;
        Vector3 toPlayerShot = player.GetComponent<PlayerManager>().head.transform.position - him.shootPos.position;

        Vector3 toPlayerXZ = new Vector3(player.transform.position.x - him.transform.position.x, 0, player.transform.position.z - him.transform.position.z);
       
        float distFromPlayer = toPlayerXZ.magnitude;
         
        bool hasSight = false;

        hitList = Physics.RaycastAll(him.shootPos.position - toPlayerShot.normalized, toPlayerShot.normalized, him.stats.sight);
        System.Array.Sort(hitList, (x, y) => x.distance.CompareTo(y.distance));

        for (int j = 0; j < hitList.Length; j++) {
            hit = hitList[j];
            if (hitList[j].collider.tag == "Player") {
                hasSight = true;
                break;
            } else if (hitList[j].collider.tag == "Wall") {

                hasSight = false;
                break;
            }
        }

        if (hasSight) {
            desired = him.transform.position + toPlayerPos.normalized * (toPlayerPos.magnitude - him.stats.shootingMaxRange - 0.5f);
           
        }

        bool hasSightAtDesired = false;

        hitList = Physics.RaycastAll(desired, (player.transform.position - desired).normalized, him.stats.shootingMaxRange);
        Debug.DrawRay(player.transform.position, (desired - player.transform.position).normalized * 100, Color.green);
        System.Array.Sort(hitList, (x, y) => x.distance.CompareTo(y.distance));

        for (int j = 0; j < hitList.Length; j++)
        {
            hit = hitList[j];
            if (hitList[j].collider.tag == "Player")
            {
                hasSightAtDesired = true;
                break;
            }
            else if (hitList[j].collider.tag == "Wall")
            {

                hasSightAtDesired = false;
                break;
            }
        }

        /* Find the player around the wall */

        /* Divides a circle around player of radius shootingMaxRange into unitCirclePartions 
                       partitions and raycasts to check if there a wall between */


        float angle = 0;
        bool existsWall = false;
        /* We do this algorithm if smart is enabled and a wall is blocking us at our current desired position */
        if ((!hasSightAtDesired) && stats.smart) {
           
            for (int i = 0; i < unitCirclePartions; i++) {
                desired = player.transform.position + ((Quaternion.Euler(0, angle, 0) * new Vector3(1, 0, 0))) * (him.stats.shootingMaxRange/2);

                hitList = Physics.RaycastAll(player.transform.position, (Quaternion.Euler(0, angle, 0) * new Vector3(1, 0, 0)).normalized, him.stats.shootingMaxRange);
                System.Array.Sort(hitList, (x, y) => x.distance.CompareTo(y.distance));

                existsWall = false;
                for (int j = 0; j < hitList.Length; j++) {
                    hit = hitList[j];
                    if (hitList[j].collider.tag == "Wall") {

                        existsWall = true;
                        break;
                    }
                }

                if (!existsWall) {
                   
                    break;
                }
                
                angle = Mathf.Clamp(angle + 360 / unitCirclePartions, 0, 360);
            }

        }

        switch (stats.currState) {
            case EnemyStats.ENEMYSTATE.TURNEDOFF:

                break;

            case EnemyStats.ENEMYSTATE.IDLE:

                if (distFromPlayer <= him.stats.shootingMaxRange || him.stats.role == Tutorial.TUTORIALPHASES.BLOCKING) {
                    /* SHOOT HIM GRRR */
                    stats.currState = EnemyStats.ENEMYSTATE.SHOOTING;


                } else if (distFromPlayer <= him.stats.sight) {
                    stats.currState = EnemyStats.ENEMYSTATE.MOVING;


                } else {
                    agent.destination = origPos;

                }
                break;

            case EnemyStats.ENEMYSTATE.MOVING:


                if (distFromPlayer <= him.stats.sight) {

                    agent.destination = desired;   

                } else {
                    stats.currState = EnemyStats.ENEMYSTATE.IDLE;
                }


                if ((distFromPlayer <= him.stats.shootingMaxRange + 2 || him.stats.role == Tutorial.TUTORIALPHASES.BLOCKING) && hasSight) {
                    /* SHOOT HIM GRRR */
                    stats.currState = EnemyStats.ENEMYSTATE.SHOOTING;


                }

                break;

            case EnemyStats.ENEMYSTATE.SHOOTING:

                if ((distFromPlayer <= him.stats.shootingMaxRange + 2 || him.stats.role == Tutorial.TUTORIALPHASES.BLOCKING) && hasSight) {
                    agent.destination = transform.position;
                } else if (him.stats.role != Tutorial.TUTORIALPHASES.BLOCKING) {
                    stats.currState = EnemyStats.ENEMYSTATE.MOVING;
                }

                break;
        }
        him.navTarget = agent.destination;
        }
        }
    }
}



