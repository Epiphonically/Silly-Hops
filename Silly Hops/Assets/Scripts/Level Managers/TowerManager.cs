using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TowerManager : MonoBehaviour
{ 
    public enum FLOORS {
        NONE,
        ONE,
        TWO,
        THREE,
        FOUR,
        ROOF
    }
    
    public TextMeshProUGUI objective;
    public GameObject[] enemies;
    public GameObject[] spawners;
    public FLOORS currentFloor = FLOORS.ONE;
    public GameObject currCheck, nextCheck, floorOneCheck, floorTwoCheck, floorThreeCheck, floorFourCheck, winGate, player, fallLimit;
    public PlayerManager playerScript;
    public int totalFloorOneEnemies, totalFloorTwoEnemies, totalFloorThreeEnemies, totalFloorFourEnemies;
    public int totalFloorOneSpawners, totalFloorTwoSpawners, totalFloorThreeSpawners, totalFloorFourSpawners;
    
    public HPSetter hpSetter;
    void Start() {
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        playerScript = player.GetComponent<PlayerManager>();
        floorOneCheck.GetComponent<Interactible>().canInteract = false;
        floorTwoCheck.GetComponent<Interactible>().canInteract = false;
        floorThreeCheck.GetComponent<Interactible>().canInteract = false;
        floorFourCheck.GetComponent<Interactible>().canInteract = false;
        
        currentFloor = FLOORS.ONE;
        currCheck = floorOneCheck;

        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        spawners = GameObject.FindGameObjectsWithTag("Spawner");

        for (int i = 0; i < enemies.Length; i++) {
            switch (enemies[i].GetComponent<EnemyStats>().towerFloor) {
                case FLOORS.ONE:    
                    totalFloorOneEnemies++;
                break;

                case FLOORS.TWO:
                    totalFloorTwoEnemies++;
                break;

                case FLOORS.THREE:
                    totalFloorThreeEnemies++;
                break;

                case FLOORS.FOUR:
                    totalFloorFourEnemies++;
                break;
            }
        }

        for (int i = 0; i < spawners.Length; i++) {
            switch (spawners[i].GetComponent<EnemyStats>().towerFloor) {
                case FLOORS.ONE:    
                    totalFloorOneSpawners++;
                break;

                case FLOORS.TWO:
                    totalFloorTwoSpawners++;
                break;

                case FLOORS.THREE:
                    totalFloorThreeSpawners++;
                break;

                case FLOORS.FOUR:
                    totalFloorFourSpawners++;
                break;
            }
        }
        player.transform.position = currCheck.transform.position + new Vector3(0, 5, 0);
        playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(currCheck.transform.forward.x, currCheck.transform.forward.z)));
        playerScript.cameraVerticalRotation = 0;
    }

    void Update()
    {
        if (player.transform.position.y <= fallLimit.transform.position.y) {
            player.transform.position = currCheck.transform.position + new Vector3(0, 5, 0);
            playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(currCheck.transform.forward.x, currCheck.transform.forward.z)));
            playerScript.cameraVerticalRotation = 0;
            player.GetComponent<Collider>().SendMessage("FallDamage", 25);
        }
        hpSetter.lives = ManageScenes.lives;
        if (playerScript.hp <= 0) {
            ManageScenes.lives = Mathf.Clamp(ManageScenes.lives - 1, 0, ManageScenes.lives);
            player.transform.position = currCheck.transform.position + new Vector3(0, 5, 0);
            playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(currCheck.transform.forward.x, currCheck.transform.forward.z)));
            playerScript.cameraVerticalRotation = 0;
            playerScript.hp = playerScript.maxHp;
            player.GetComponent<Collider>().SendMessage("DeathSound");
        }
   
        if (ManageScenes.lives == 0) {
            ManageScenes.lost = true;
        }
        
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        spawners = GameObject.FindGameObjectsWithTag("Spawner");
        int currEnemies = 0;
        int currSpawners = 0;

        floorOneCheck.GetComponent<checkPointMan>().beacon = false;
        floorOneCheck.GetComponent<checkPointMan>().currMat = floorOneCheck.GetComponent<checkPointMan>().off;

        floorTwoCheck.GetComponent<checkPointMan>().beacon = false;
        floorTwoCheck.GetComponent<checkPointMan>().currMat = floorTwoCheck.GetComponent<checkPointMan>().off;

        floorThreeCheck.GetComponent<checkPointMan>().beacon = false;
        floorThreeCheck.GetComponent<checkPointMan>().currMat = floorThreeCheck.GetComponent<checkPointMan>().off;

        floorFourCheck.GetComponent<checkPointMan>().beacon = false;
        floorFourCheck.GetComponent<checkPointMan>().currMat = floorFourCheck.GetComponent<checkPointMan>().off;

        currCheck.GetComponent<checkPointMan>().currMat = floorOneCheck.GetComponent<checkPointMan>().onMat;

        for (int i = 0; i < enemies.Length; i++) {
            if (enemies[i].GetComponent<EnemyStats>().towerFloor != currentFloor && enemies[i].GetComponent<EnemyStats>().towerFloor != FLOORS.NONE) {
                enemies[i].GetComponent<EnemyStats>().invincible = true;
                enemies[i].GetComponent<EnemyStats>().idle = true;
            } else {
                if (enemies[i].GetComponent<EnemyStats>().towerFloor != FLOORS.NONE) {
                    currEnemies++;
                }
                enemies[i].GetComponent<EnemyStats>().invincible = false;
                enemies[i].GetComponent<EnemyStats>().idle = false;
            }
        }
        for (int i = 0; i < spawners.Length; i++) {
            if (spawners[i].GetComponent<EnemyStats>().towerFloor != currentFloor && spawners[i].GetComponent<EnemyStats>().towerFloor != FLOORS.NONE) {
                spawners[i].GetComponent<EnemyStats>().invincible = true;
                spawners[i].GetComponent<EnemyStats>().idle = true;
            } else {
                if (spawners[i].GetComponent<EnemyStats>().towerFloor != FLOORS.NONE) {
                    currSpawners++;
                }   
                spawners[i].GetComponent<EnemyStats>().invincible = false;
                spawners[i].GetComponent<EnemyStats>().idle = false;
            }
        }
        switch (currentFloor) {
            case FLOORS.ONE:
                if (currEnemies == 0 && currSpawners == 0) {
                    if (floorTwoCheck.GetComponent<Interactible>().numInteractions > 0) {
                        currCheck = floorTwoCheck;
                        currentFloor = FLOORS.TWO;
                    } else {
                        objective.text = "Get to the beacon on the next floor";
                        floorTwoCheck.GetComponent<Interactible>().canInteract = true;
                        floorTwoCheck.GetComponent<checkPointMan>().beacon = true;
                        floorTwoCheck.GetComponent<checkPointMan>().currMat = floorTwoCheck.GetComponent<checkPointMan>().comeToMe;
                    }
                    
                } else if (currEnemies > 0 && currSpawners > 0) {
                    objective.text = "Eliminate " + currSpawners.ToString() + "/" + totalFloorOneSpawners.ToString() + " spawners and " + currEnemies.ToString() + "/" + totalFloorOneEnemies.ToString() + " spawners";
                } else if (currEnemies > 0) {
                    objective.text = "Eliminate " + currEnemies.ToString() + "/" + totalFloorOneEnemies.ToString() + " enemies";
                } else if (currSpawners > 0) {
                    objective.text = "Eliminate " + currSpawners.ToString() + "/" + totalFloorOneSpawners.ToString() + " spawners";
                }
            break;

            case FLOORS.TWO:
                if (currEnemies == 0 && currSpawners == 0) {
                    if (floorThreeCheck.GetComponent<Interactible>().numInteractions > 0) {
                        currCheck = floorThreeCheck;
                        currentFloor = FLOORS.THREE;
                    } else {
                        objective.text = "Get to the beacon on the next floor";
                        floorThreeCheck.GetComponent<Interactible>().canInteract = true;
                        floorThreeCheck.GetComponent<checkPointMan>().beacon = true;
                        floorThreeCheck.GetComponent<checkPointMan>().currMat = floorThreeCheck.GetComponent<checkPointMan>().comeToMe;
                    }
                } else if (currEnemies > 0 && currSpawners > 0) {
                    objective.text = "Eliminate " + currSpawners.ToString() + "/" + totalFloorTwoSpawners.ToString() + " spawners and " + currEnemies.ToString() + "/" + totalFloorTwoEnemies.ToString() + " spawners";
                } else if (currEnemies > 0) {
                    objective.text = "Eliminate " + currEnemies.ToString() + "/" + totalFloorTwoEnemies.ToString() + " enemies";
                } else if (currSpawners > 0) {
                    objective.text = "Eliminate " + currSpawners.ToString() + "/" + totalFloorTwoSpawners.ToString() + " spawners";
                }
            break;

            case FLOORS.THREE:
                if (currEnemies == 0 && currSpawners == 0) {
                    if  (floorFourCheck.GetComponent<Interactible>().numInteractions > 0) {
                        currCheck = floorFourCheck;
                        currentFloor = FLOORS.FOUR;
                    } else {
                        objective.text = "Get to the beacon on the next floor";
                        floorFourCheck.GetComponent<Interactible>().canInteract = true;
                        floorFourCheck.GetComponent<checkPointMan>().beacon = true;
                        floorFourCheck.GetComponent<checkPointMan>().currMat = floorFourCheck.GetComponent<checkPointMan>().comeToMe;
                    }
                } else if (currEnemies > 0 && currSpawners > 0) {
                    objective.text = "Eliminate " + currSpawners.ToString() + "/" + totalFloorThreeSpawners.ToString() + " spawners and " + currEnemies.ToString() + "/" + totalFloorThreeEnemies.ToString() + " spawners";
                } else if (currEnemies > 0) {
                    objective.text = "Eliminate " + currEnemies.ToString() + "/" + totalFloorThreeEnemies.ToString() + " enemies";
                } else if (currSpawners > 0) {
                    objective.text = "Eliminate " + currSpawners.ToString() + "/" + totalFloorThreeSpawners.ToString() + " spawners";
                }
            break;  

            case FLOORS.FOUR:
                if (currEnemies == 0 && currSpawners == 0) {
                    
                    currentFloor = FLOORS.ROOF;

                } else if (currEnemies > 0 && currSpawners > 0) {
                    objective.text = "Eliminate " + currSpawners.ToString() + "/" + totalFloorFourSpawners.ToString() + " spawners and " + currEnemies.ToString() + "/" + totalFloorFourEnemies.ToString() + " spawners";
                } else if (currEnemies > 0) {
                    objective.text = "Eliminate " + currEnemies.ToString() + "/" + totalFloorFourEnemies.ToString() + " enemies";
                } else if (currSpawners > 0) {
                    objective.text = "Eliminate " + currSpawners.ToString() + "/" + totalFloorFourSpawners.ToString() + " spawners";
                }
            break;

            case FLOORS.ROOF:
                objective.text = "Well done, exit through the portal";
                if (!winGate.GetComponent<winGateMan>().on) {
                    
                    winGate.GetComponent<winGateMan>().on = true;  
                } 
            break;    
        }

        
    }
}
