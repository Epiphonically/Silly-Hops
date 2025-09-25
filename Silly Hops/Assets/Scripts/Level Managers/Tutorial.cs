using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Tutorial : MonoBehaviour
{
    public enum TUTORIALPHASES {
        NONE,
        DASHING,
        TAKEOUTSPRAY,
        RELOADSPRAY,
        SPRAYING,
        WALLDASHING,
        TAKEOUTSHOTGUN,
        RELOADSHOTGUN,
        SHOTGUNNING,
        TAKEOUTBOUNCE,
        RELOADBOUNCE,
        BOUNCING,
        BLOCKING,
        KILLBLOCKER,
        MINIBATTLE
    }
    public TUTORIALPHASES currPhase;
    public TextMeshProUGUI objective;
    public GameObject[] enemies;
    public GameObject[] spawners;
    public GameObject player;
    public int totalDashingEnemies = 0;
    public int totalSprayEnemies = 0;
    public int totalShotGunEnemies = 0;
    public int totalBlockers = 0;
    public int miniBattlers = 0;
    public PlayerManager playerScript;
    public GameObject currCheckPoint, spawn, dashCheckPoint, sprayCheckPoint, wallDashCheckPoint, shotGunCheckPoint, bouncingCheckPoint, finalCheckPoint, winGate;
    public GameObject currBeacon;
    public GameObject fallLimit;
    public platformLife firstGate, secondGate, thirdGate;
    public int blocked = 0, needToBlock = 5;

    public float height2 = 23; /* Fall limit goes up as you ascend the level */
    public float height3 = 60;

    void Start() {
        
        currCheckPoint = spawn;
        playerScript = player.GetComponent<PlayerManager>();
        playerScript.currShotGunAmmo = 0;
        playerScript.currSprayAmmo = 0;
        playerScript.currBounceAmmo = 0;
        playerScript.canReloadSpray = false;
        playerScript.canReloadShotGun = false;
        playerScript.canSwapToGun = false;
        currPhase = TUTORIALPHASES.DASHING;
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++) {
            switch (enemies[i].GetComponent<EnemyStats>().role) {
                case TUTORIALPHASES.DASHING:
                    totalDashingEnemies++;
                    enemies[i].GetComponent<EnemyStats>().currState = EnemyStats.ENEMYSTATE.TURNEDOFF;
                break;

                case TUTORIALPHASES.SPRAYING:
                    totalSprayEnemies++;
                    enemies[i].GetComponent<EnemyStats>().currState = EnemyStats.ENEMYSTATE.TURNEDOFF;
                break;
                
                case TUTORIALPHASES.SHOTGUNNING:
                    totalShotGunEnemies++;
                    enemies[i].GetComponent<EnemyStats>().currState = EnemyStats.ENEMYSTATE.TURNEDOFF;
                break;

                case TUTORIALPHASES.BLOCKING:
                    totalBlockers++;
                    enemies[i].GetComponent<EnemyStats>().currState = EnemyStats.ENEMYSTATE.IDLE;
                break;

                case TUTORIALPHASES.MINIBATTLE:
                    
                    miniBattlers++;
                break;
            }
        }
       
        spawn.GetComponent<Interactible>().canInteract = false;
        dashCheckPoint.GetComponent<Interactible>().canInteract = false;
        sprayCheckPoint.GetComponent<Interactible>().canInteract = false;
        wallDashCheckPoint.GetComponent<Interactible>().canInteract = false;
        shotGunCheckPoint.GetComponent<Interactible>().canInteract = false;
        bouncingCheckPoint.GetComponent<Interactible>().canInteract = false;
        finalCheckPoint.GetComponent<Interactible>().canInteract = false;

        player.transform.position = currCheckPoint.transform.position + new Vector3(0, 5, 0);
        playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(currCheckPoint.transform.forward.x, currCheckPoint.transform.forward.z)));
        playerScript.cameraVerticalRotation = 0;
    }

    /* We create your standard tutorial experince with handholding, objectives, infinite lives and a rundown of all basic features in the game 
       this goes towards the completeness category of the game */
    void Update() {
    
        if (player.transform.position.y <= fallLimit.transform.position.y) {
            player.transform.position = currCheckPoint.transform.position + new Vector3(0, 5, 0);
            playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(currCheckPoint.transform.forward.x, currCheckPoint.transform.forward.z)));
            playerScript.cameraVerticalRotation = 0;
            player.GetComponent<Collider>().SendMessage("FallDamage", 25);
        }

        if (playerScript.hp <= 0) {
            playerScript.hp = playerScript.maxHp;
            player.transform.position = currCheckPoint.transform.position + new Vector3(0, 5, 0);
            playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(currCheckPoint.transform.forward.x, currCheckPoint.transform.forward.z)));
            playerScript.cameraVerticalRotation = 0;
            player.GetComponent<Collider>().SendMessage("DeathSound");
        }

        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        spawners = GameObject.FindGameObjectsWithTag("Spawner");
        int enemiesLeft = 0;
        
        switch (currPhase) {
            case TUTORIALPHASES.DASHING: /* Disable all player weapons except for the sword and dash */
                
                for (int i = 0; i < enemies.Length; i++) {
                    if (enemies[i].GetComponent<EnemyStats>().role == TUTORIALPHASES.DASHING) {
                        enemies[i].GetComponent<EnemyStats>().invincible = false;
                        enemies[i].GetComponent<EnemyStats>().idle = false;
                        enemiesLeft++;
                    } else {
                        enemies[i].GetComponent<EnemyStats>().invincible = true;
                        enemies[i].GetComponent<EnemyStats>().idle = true;
                    }
                }
                if (enemiesLeft == 0) {
                    objective.text = "Press E on beacon for checkpoint";
                    currBeacon = dashCheckPoint;
                } else {
                    objective.text = "Eliminate " + enemiesLeft.ToString() + "/" + totalDashingEnemies.ToString() + " enemies left\nPress shift to dash";
                }
                
                if (enemiesLeft == 0 && dashCheckPoint.GetComponent<Interactible>().numInteractions > 0) {
                    currCheckPoint = dashCheckPoint;
                    currPhase = TUTORIALPHASES.TAKEOUTSPRAY;
                }
            break;

            case TUTORIALPHASES.TAKEOUTSPRAY:
                currBeacon = null;
                objective.text = "Press F then 1 to switch to blaster";
                playerScript.canSwapToGun = true;
                if (playerScript.hand == PlayerManager.HAND.GUN && playerScript.ammoType == PlayerManager.SHOTS.SPRAY) {
                    currPhase = TUTORIALPHASES.RELOADSPRAY;
                }
            break;

            case TUTORIALPHASES.RELOADSPRAY:
                objective.text = "Press R to recharge blaster";
                playerScript.canReloadSpray = true;
                if (playerScript.currSprayAmmo >= 1) {
                    currPhase = TUTORIALPHASES.SPRAYING;
                }
            break;

            case TUTORIALPHASES.SPRAYING:
                if (firstGate != null) {
                    firstGate.fall = true;
                }
                for (int i = 0; i < enemies.Length; i++) {
                    
                    if (enemies[i].GetComponent<EnemyStats>().role == TUTORIALPHASES.SPRAYING) {
                        enemies[i].GetComponent<EnemyStats>().invincible = false;
                        enemies[i].GetComponent<EnemyStats>().idle = false;
                        enemiesLeft++;
                    } else {
                        enemies[i].GetComponent<EnemyStats>().invincible = true;
                        enemies[i].GetComponent<EnemyStats>().idle = true;
                    }
                }
                if (enemiesLeft == 0) {
                    objective.text = "Go to the next beacon";
                    currBeacon = sprayCheckPoint;
                } else {
                    objective.text = "Eliminate " + enemiesLeft.ToString() + "/" + totalSprayEnemies.ToString() + " enemies left";
                }
                
                if (enemiesLeft == 0 && sprayCheckPoint.GetComponent<Interactible>().numInteractions > 0) {
                    currCheckPoint = sprayCheckPoint;
                    currPhase = TUTORIALPHASES.WALLDASHING;
                }
                
            break;

            case TUTORIALPHASES.WALLDASHING:
                currBeacon = wallDashCheckPoint;
                objective.text = "The blue walls reset your dash";
                if (enemiesLeft == 0 && wallDashCheckPoint.GetComponent<Interactible>().numInteractions > 0) {
                    currCheckPoint = wallDashCheckPoint;
                    currPhase = TUTORIALPHASES.TAKEOUTSHOTGUN;
                }
            break;

            case TUTORIALPHASES.TAKEOUTSHOTGUN:
                currBeacon = null;
                playerScript.currSprayAmmo = 0;
                playerScript.canReloadSpray = false;
                objective.text = "Press 2 with blaster out to switch to shotgun";
                if (playerScript.hand == PlayerManager.HAND.GUN && playerScript.ammoType == PlayerManager.SHOTS.SHOTGUN) {
                    currPhase = TUTORIALPHASES.RELOADSHOTGUN;
                }
            break;

            case TUTORIALPHASES.RELOADSHOTGUN:
                playerScript.canReloadShotGun = true;
                objective.text = "Press R to recharge shotgun";
                if (playerScript.currShotGunAmmo >= 1) {
                    currPhase = TUTORIALPHASES.SHOTGUNNING;
                }
            break;

            case TUTORIALPHASES.SHOTGUNNING:
                if (secondGate != null) {
                    secondGate.fall = true;
                }
                for (int i = 0; i < enemies.Length; i++) {
                    
                    if (enemies[i].GetComponent<EnemyStats>().role == TUTORIALPHASES.SHOTGUNNING) {
                        enemies[i].GetComponent<EnemyStats>().invincible = false;
                        enemies[i].GetComponent<EnemyStats>().idle = false;
                        enemiesLeft++;
                    } else {
                        enemies[i].GetComponent<EnemyStats>().invincible = true;
                        enemies[i].GetComponent<EnemyStats>().idle = true;
                    }
                }
                if (enemiesLeft == 0) {
                    objective.text = "Go to the next beacon";
                    currBeacon = shotGunCheckPoint;
                } else {
                    objective.text = "Eliminate " + enemiesLeft.ToString() + "/" + totalShotGunEnemies.ToString() + " enemies left";
                }
                
                if (enemiesLeft == 0 && shotGunCheckPoint.GetComponent<Interactible>().numInteractions > 0) {
                    currCheckPoint = shotGunCheckPoint;
                    currPhase = TUTORIALPHASES.TAKEOUTBOUNCE;
                }
            break;

            case TUTORIALPHASES.TAKEOUTBOUNCE:
                currBeacon = null;
                objective.text = "Press 3 with blaster out to switch to bouncy ammo";
                playerScript.canReloadSpray = true;

                if (playerScript.hand == PlayerManager.HAND.GUN && playerScript.ammoType == PlayerManager.SHOTS.BOUNCE) {
                    currPhase = TUTORIALPHASES.RELOADBOUNCE;
                }
            break;

            case TUTORIALPHASES.RELOADBOUNCE:
                objective.text = "Stand on the pink charging station";

                if (playerScript.ammoType == PlayerManager.SHOTS.BOUNCE && playerScript.currBounceAmmo >= 1) {
                    currPhase = TUTORIALPHASES.BOUNCING;


                }
            break;

            case TUTORIALPHASES.BOUNCING:
                objective.text = "Go to the next beacon";
                currBeacon = bouncingCheckPoint;
                if (bouncingCheckPoint.GetComponent<Interactible>().numInteractions > 0) {
                    currCheckPoint = bouncingCheckPoint;
                    currPhase = TUTORIALPHASES.BLOCKING;
                }
            break;

            case TUTORIALPHASES.BLOCKING:
                fallLimit.transform.position = Vector3.Lerp(fallLimit.transform.position, new Vector3(fallLimit.transform.position.x, height2, fallLimit.transform.position.z), 8*Time.deltaTime);
                currBeacon = null;
                for (int i = 0; i < enemies.Length; i++) {
                    enemies[i].GetComponent<EnemyStats>().invincible = true;
                    if (enemies[i].GetComponent<EnemyStats>().role == TUTORIALPHASES.BLOCKING) {
                        
                        enemies[i].GetComponent<EnemyStats>().idle = false;
                       
                    } 
                }
                objective.text = "Hold right click with sword to block\n" + blocked.ToString() + "/" + needToBlock.ToString();

                if (blocked == needToBlock) {
                    
                    currPhase = TUTORIALPHASES.KILLBLOCKER;
                }
            break;  

            case TUTORIALPHASES.KILLBLOCKER:
                for (int i = 0; i < enemies.Length; i++) {
                    
                    if (enemies[i].GetComponent<EnemyStats>().role == TUTORIALPHASES.BLOCKING) {
                        enemies[i].GetComponent<EnemyStats>().invincible = false;
                        enemies[i].GetComponent<EnemyStats>().idle = false;
                        enemiesLeft++;
                    } else {
                        enemies[i].GetComponent<EnemyStats>().invincible = true;
                        enemies[i].GetComponent<EnemyStats>().idle = true;
                    }
                }
                if (enemiesLeft == 0) {
                    objective.text = "Go to the final beacon";
                    currBeacon = finalCheckPoint;
                    if (thirdGate != null) {
                        thirdGate.fall = true;
                    }
                } else {
                    objective.text = "Eliminate " + enemiesLeft.ToString() + "/" + totalBlockers.ToString() + " enemies left";

                }
                
                if (finalCheckPoint.GetComponent<Interactible>().numInteractions > 0) {
                    currCheckPoint = finalCheckPoint;
                    currPhase = TUTORIALPHASES.MINIBATTLE;
                }
                
            break;

            case TUTORIALPHASES.MINIBATTLE:
                currBeacon = null;
                
                fallLimit.transform.position = Vector3.Lerp(fallLimit.transform.position, new Vector3(fallLimit.transform.position.x, height3, fallLimit.transform.position.z), 8*Time.deltaTime);
                for (int i = 0; i < spawners.Length; i++) {
                    
                    if (spawners[i].GetComponent<EnemyStats>().role == TUTORIALPHASES.MINIBATTLE) {
                        spawners[i].GetComponent<EnemyStats>().invincible = false;
                        spawners[i].GetComponent<EnemyStats>().idle = false;
                        enemiesLeft++;
                    }
                }

                

                if (enemiesLeft == 0) {
                    objective.text = "Go through the gate";
                    if (!winGate.GetComponent<winGateMan>().on) {
                        winGate.GetComponent<winGateMan>().on = true;
                    }
                    
                } else {
                    objective.text = "Eliminate Spawner";
                }
            break;

        }

        spawn.GetComponent<checkPointMan>().beacon = false;
        spawn.GetComponent<checkPointMan>().currMat = spawn.GetComponent<checkPointMan>().off;
        dashCheckPoint.GetComponent<checkPointMan>().beacon = false;
        dashCheckPoint.GetComponent<checkPointMan>().currMat = dashCheckPoint.GetComponent<checkPointMan>().off;
        sprayCheckPoint.GetComponent<checkPointMan>().beacon = false;
        sprayCheckPoint.GetComponent<checkPointMan>().currMat = sprayCheckPoint.GetComponent<checkPointMan>().off;
        wallDashCheckPoint.GetComponent<checkPointMan>().beacon = false;
        wallDashCheckPoint.GetComponent<checkPointMan>().currMat = wallDashCheckPoint.GetComponent<checkPointMan>().off;
        shotGunCheckPoint.GetComponent<checkPointMan>().beacon = false;
        shotGunCheckPoint.GetComponent<checkPointMan>().currMat = shotGunCheckPoint.GetComponent<checkPointMan>().off;
        bouncingCheckPoint.GetComponent<checkPointMan>().beacon = false;
        bouncingCheckPoint.GetComponent<checkPointMan>().currMat = bouncingCheckPoint.GetComponent<checkPointMan>().off;
        finalCheckPoint.GetComponent<checkPointMan>().beacon = false;
        finalCheckPoint.GetComponent<checkPointMan>().currMat = finalCheckPoint.GetComponent<checkPointMan>().off;
        if (currBeacon != null) {
            currBeacon.GetComponent<checkPointMan>().beacon = true;
            currBeacon.GetComponent<checkPointMan>().currMat = currBeacon.GetComponent<checkPointMan>().comeToMe;
            if (currBeacon.GetComponent<Interactible>().numInteractions == 0) {
                currBeacon.GetComponent<Interactible>().canInteract = true;
            }
            
        }
        currCheckPoint.GetComponent<checkPointMan>().currMat = currCheckPoint.GetComponent<checkPointMan>().onMat;
    }
}
