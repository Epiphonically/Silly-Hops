using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Level1Manager : MonoBehaviour
{
    public enum LEVELONEOBJECTIVES {
        NONE,
        FIRST,
        SECOND,
        THIRD
    }

    public LEVELONEOBJECTIVES currObjective;
    public TextMeshProUGUI objective;

    public GameObject spawn, firstPoint, secondPoint, thirdPoint, firstDomain, secondDomain, thirdDomain, player, currDomain, currPoint, fallLimit, winGate;

    public bool firstPointDone = false, secondPointDone = false, thirdPointDone = false;

    public int totalFirstEnemies, totalSecondEnemies, totalThirdEnemies;
    public int totalFirstSpawners, totalSecondSpawners, totalThirdSpawners;
    
    public Material evilMat;
    public GameObject[] enemies, spawners;
    public PlayerManager playerScript;
    public string domName;
    public HPSetter hpSetter;
    void Start() {
        player.transform.position = spawn.transform.position + new Vector3(0, 5, 0);
        playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(spawn.transform.forward.x, spawn.transform.forward.z)));
        playerScript.cameraVerticalRotation = 0;
        spawn.GetComponent<Interactible>().canInteract = false;
        firstPoint.GetComponent<Interactible>().canInteract = false;
        secondPoint.GetComponent<Interactible>().canInteract = false;
        thirdPoint.GetComponent<Interactible>().canInteract = false;
        
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        spawners = GameObject.FindGameObjectsWithTag("Spawner");

        for (int i = 0; i < enemies.Length; i++) {
            switch (enemies[i].GetComponent<EnemyStats>().lv1role) {
                case LEVELONEOBJECTIVES.FIRST:
                    totalFirstEnemies++;

                break;

                case LEVELONEOBJECTIVES.SECOND:
                    totalSecondEnemies++;

                break;
                
                case LEVELONEOBJECTIVES.THIRD:
                    totalThirdEnemies++;

                break;
            }
        }

        for (int i = 0; i < spawners.Length; i++) {
            switch (spawners[i].GetComponent<EnemyStats>().lv1role) {
                case LEVELONEOBJECTIVES.FIRST:
                    totalFirstSpawners++;

                break;

                case LEVELONEOBJECTIVES.SECOND:
                    totalSecondSpawners++;

                break;
                
                case LEVELONEOBJECTIVES.THIRD:
                    totalThirdSpawners++;

                break;
            }
        }
    }

    void Update()
    {
        hpSetter.lives = ManageScenes.lives;
        if (playerScript.hp <= 0) {
            ManageScenes.lives = Mathf.Clamp(ManageScenes.lives - 1, 0, ManageScenes.lives);
            player.transform.position = spawn.transform.position + new Vector3(0, 5, 0);
            playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(spawn.transform.forward.x, spawn.transform.forward.z)));
            playerScript.cameraVerticalRotation = 0;
            playerScript.hp = playerScript.maxHp;
            player.GetComponent<Collider>().SendMessage("DeathSound");
        }
        if (ManageScenes.lives == 0) {
            ManageScenes.lost = true;
        }

        if (player.transform.position.y <= fallLimit.transform.position.y) {
            player.transform.position = spawn.transform.position + new Vector3(0, 5, 0);
            playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(spawn.transform.forward.x, spawn.transform.forward.z)));
            playerScript.cameraVerticalRotation = 0;
            player.GetComponent<Collider>().SendMessage("FallDamage", 25);
        }

        if (!firstPointDone) {
            firstPoint.GetComponent<checkPointMan>().beacon = true;
            firstPoint.GetComponent<checkPointMan>().currMat = evilMat;
        }

        if (!secondPointDone) {
            secondPoint.GetComponent<checkPointMan>().beacon = true;
            secondPoint.GetComponent<checkPointMan>().currMat = evilMat;
        }

        if (!thirdPointDone) {
            thirdPoint.GetComponent<checkPointMan>().beacon = true;
            thirdPoint.GetComponent<checkPointMan>().currMat = evilMat;
        }

        /* Find which domain you are closest to */
        currDomain = spawn;
        domName = "Spawn";
        if ((player.transform.position - firstDomain.transform.position).magnitude < (player.transform.position - currDomain.transform.position).magnitude) {
            currDomain = firstDomain;
            currPoint = firstPoint;
            domName = "First";
        }
        if ((player.transform.position - secondDomain.transform.position).magnitude < (player.transform.position - currDomain.transform.position).magnitude) {
            currDomain = secondDomain;
            currPoint = secondPoint;
            domName = "Second";
        }
        if ((player.transform.position - thirdDomain.transform.position).magnitude < (player.transform.position - currDomain.transform.position).magnitude) {
            currDomain = thirdDomain;
            currPoint = thirdPoint;
            domName = "Third";
        }
        
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        spawners = GameObject.FindGameObjectsWithTag("Spawner");

        int currSpawners = 0;
        int currEnemies = 0;
        int currTotalEnemies = 0;
        int currTotalSpawners = 0;
        bool set = false;

        firstPointDone = firstPoint.GetComponent<Interactible>().numInteractions > 0;
        secondPointDone = secondPoint.GetComponent<Interactible>().numInteractions > 0;
        thirdPointDone = thirdPoint.GetComponent<Interactible>().numInteractions > 0;

        if (firstPointDone) {
            firstPoint.GetComponent<checkPointMan>().currMat = firstPoint.GetComponent<checkPointMan>().onMat;
        }

        if (secondPointDone) {
            secondPoint.GetComponent<checkPointMan>().currMat = secondPoint.GetComponent<checkPointMan>().onMat;
        }

        if (thirdPointDone) {
            thirdPoint.GetComponent<checkPointMan>().currMat = thirdPoint.GetComponent<checkPointMan>().onMat;
        }   

        if (firstPointDone && secondPointDone && thirdPointDone) {
            objective.text = "Well done, go through the gate at spawn";
            if (!winGate.GetComponent<winGateMan>().on) {
                winGate.GetComponent<winGateMan>().on = true;
            }
            
            set = true;
        }
        switch (domName) {
            case "Spawn":
                if (!set) {
                    objective.text = "Clear out all three beacons";
                    set = true;
                }  
            break;

            case "First":
                
                currTotalEnemies = totalFirstEnemies;
                currTotalSpawners = totalFirstSpawners;
                for (int i = 0; i < enemies.Length; i++) {
                    if (enemies[i].GetComponent<EnemyStats>().lv1role == LEVELONEOBJECTIVES.FIRST) {
                        currEnemies++;
                    }
                }

                for (int i = 0; i < spawners.Length; i++) {
                    if (spawners[i].GetComponent<EnemyStats>().lv1role == LEVELONEOBJECTIVES.FIRST) {
                        currSpawners++;
                    }
                }
                if (!set && firstPointDone) {
                    objective.text = "Area One Cleared!";
                    set = true;
                }
            break;

            case "Second":
                currTotalEnemies = totalSecondEnemies;
                currTotalSpawners = totalSecondSpawners;
                for (int i = 0; i < enemies.Length; i++) {
                    if (enemies[i].GetComponent<EnemyStats>().lv1role == LEVELONEOBJECTIVES.SECOND) {
                        currEnemies++;
                    }
                }

                for (int i = 0; i < spawners.Length; i++) {
                    if (spawners[i].GetComponent<EnemyStats>().lv1role == LEVELONEOBJECTIVES.SECOND) {
                        currSpawners++;
                    }
                }
                if (!set && secondPointDone) {
                    objective.text = "Area Two Cleared!";
                    set = true;
                }
            break;

            case "Third":
                currTotalEnemies = totalThirdEnemies;
                currTotalSpawners = totalThirdSpawners;
                for (int i = 0; i < enemies.Length; i++) {
                    if (enemies[i].GetComponent<EnemyStats>().lv1role == LEVELONEOBJECTIVES.THIRD) {
                        currEnemies++;
                    }
                }

                for (int i = 0; i < spawners.Length; i++) {
                    if (spawners[i].GetComponent<EnemyStats>().lv1role == LEVELONEOBJECTIVES.THIRD) {
                        currSpawners++;
                    }
                }
                if (!set && thirdPointDone) {
                    objective.text = "Area Three Cleared!";
                    set = true;
                }
            break;

           
        }

        if (currSpawners > 0 && currEnemies > 0) {
            objective.text = "Eliminate " + currSpawners.ToString() + "/" + currTotalSpawners.ToString() + " spawners and " + currEnemies.ToString() + "/" + currTotalEnemies.ToString() + " enemies";
        } else if (currSpawners > 0) {
            objective.text = "Eliminate " + currSpawners.ToString() + "/" + currTotalSpawners.ToString() + " spawners";
        } else if (currEnemies > 0) {
            objective.text = "Eliminate " + currEnemies.ToString() + "/" + currTotalEnemies.ToString() + " enemies";
        } else if (!set) {
            objective.text = "Take control of beacon";
            currPoint.GetComponent<Interactible>().canInteract = true;
        }

        

    }
}
