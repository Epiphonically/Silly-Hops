using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossLevelManager : MonoBehaviour {
    

    public bool debug = false;
    private Animator anim;
    public TextMeshProUGUI objective;
    public GameObject objectiveGui;
    public Camera maincam;
    public HPSetter hpSetter;
    public GameObject fallLimit, player;
    public GameObject camobject; // for clearing guns and weapon

    public GameObject currCheckPoint, spawn, area1, area3, area4, area5;
    public PlayerManager playerScript;
    public Material enabledMaterial, disabledMaterial;

    public GameObject[] enemies;

    // Sounds
    public AudioClip[] earthquake;
    public AudioSource gameSource;

    // Cutscene 1 variables
    public GameObject cutscene1;
    public Camera cam1;
    public GameObject Door1;
    public GameObject rune;
    private bool Opened;

    // Area 1 variables
    public GameObject area1Obj, dashUnlock, Area2Collider;
    private bool area1Open = false;
    public GameObject lever1, lever2, indicator1, indicator2, area1Enemies;
    private float L1spinNum = 0, L2spinNum = 0;
    private float L1spinsPerSecond = 1, L2spinsPerSecond = 1;

    // Area 2 variables
    public GameObject area2Obj, raiseLavaObj;
    private bool area2Open = false;

    // Area 3 variables
    public GameObject area3Obj;
    private bool area3Open = false;
    public GameObject lever3, area3Enemies;
    private float L3spinNum = 0;
    private float L3spinsPerSecond = 1;

    // Area4 variables
    public GameObject area4Obj;
    private bool area4Open = false;
    public GameObject lever4, area4Enemies;
    private float L4spinNum = 0;
    private float L4spinsPerSecond = 1;

    // Area5 (Boss)
    public GameObject area5Obj, WinGate;
    private bool area5Open = false;
    public GameObject lever5, area5Enemies, theBigBoss;
    private float L5spinNum = 0;
    private float L5spinsPerSecond = 10;
    private int totalBigEnemies, currBigEnemies;
    private bool bossSpawned = false, GateOpened = false;

    private float timerBeforeBossSpawns = 8f;
    // Start is called before the first frame update
    void Start() {
        // Player Setup
        playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(spawn.transform.forward.x, spawn.transform.forward.z)));

        currCheckPoint = spawn;
        Opened = false;
        maincam.enabled = true;
        maincam.GetComponent<AudioListener>().enabled = true;
        cam1.enabled = false;
        cam1.GetComponent<AudioListener>().enabled = false;

        // setting up cutscene 1
        rune.SetActive(false);

        // Setting up area 1
        area1Obj.SetActive(false);
        area1Enemies.SetActive(false);
        if (!debug) {
            dashUnlock.SetActive(false);
            Area2Collider.SetActive(false);
        } else {
            //area2Open = true;
            dashUnlock.SetActive(true);
            Area2Collider.SetActive(true);

            area4Open = true;
            area5Open = true;
        }

        lever1.GetComponent<Renderer>().material = disabledMaterial; 
        lever2.GetComponent<Renderer>().material = disabledMaterial; 
        indicator1.GetComponent<Renderer>().material = disabledMaterial; 
        indicator2.GetComponent<Renderer>().material = disabledMaterial; 

        // Setting Up area 2
        area2Obj.SetActive(false);

        // Setting Up area 3
        area3Obj.SetActive(false);
        area3Enemies.SetActive(false);
        //enemies = GameObject.FindGameObjectsWithTag("Enemy");
        // totalBigEnemies = 0;
        // currBigEnemies = 0;

        // for (int i = 0; i < enemies.Length; i++) {
        //     switch (enemies[i].GetComponent<EnemyStats>().lv1role) {
        //         case Level1Manager.LEVELONEOBJECTIVES.FIRST:
        //             totalBigEnemies++;
        //         break;
        //     }
        // }

        // Setting Up area 4
        area4Obj.SetActive(false);
        area4Enemies.SetActive(false);

        // Setting Up area 5
        area5Obj.SetActive(false);
        area5Enemies.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        hpSetter.lives = ManageScenes.lives;
        if (playerScript.hp <= 0) {
            playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(spawn.transform.forward.x, spawn.transform.forward.z)));
            ManageScenes.lives = Mathf.Clamp(ManageScenes.lives - 1, 0, ManageScenes.lives);
            player.transform.position = currCheckPoint.transform.position + new Vector3(0, 5, 0);
            player.transform.localRotation = currCheckPoint.transform.localRotation;
            playerScript.cameraVerticalRotation = 0;
            playerScript.hp = playerScript.maxHp;
            player.GetComponent<Collider>().SendMessage("DeathSound");
        }
        if (ManageScenes.lives == 0) {
            ManageScenes.lost = true;
        }

        if (player.transform.position.y <= fallLimit.transform.position.y) {
            playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(spawn.transform.forward.x, spawn.transform.forward.z)));
            player.transform.position = currCheckPoint.transform.position + new Vector3(0, 5, 0);
            player.transform.localRotation = currCheckPoint.transform.localRotation;
            playerScript.cameraVerticalRotation = 0;
            player.GetComponent<Collider>().SendMessage("FallDamage", 25);
        }

        if (Opened == false && Door1.GetComponent<Interactible>().numInteractions > 0) {
            fallLimit.transform.position = new Vector3(fallLimit.transform.position.x, (float) 10.6, fallLimit.transform.position.z);

            Opened = true;
            anim = cutscene1.GetComponent<Animator>();
            anim.Play("OpeningDoor");
            rune.SetActive(true);
            maincam.enabled = false;
            maincam.GetComponent<AudioListener>().enabled = false;
            camobject.SetActive(false);
            cam1.enabled = true;
            cam1.GetComponent<AudioListener>().enabled = true;
            currCheckPoint = area1;

            // Reveal area1
            area1Obj.SetActive(true);
            area1Open = true;

            StartCoroutine(TurnOffPlayer(0.5f));
        }

        // Area 1 Code
        if (area1Open) {
            L1spinNum = L1spinNum + 360*Time.deltaTime/L1spinsPerSecond;
            if (L1spinNum >= 720) {
                L1spinNum = 0;
            }

            L2spinNum = L2spinNum + 360*Time.deltaTime/L2spinsPerSecond;
            if (L1spinNum >= 720) {
                L1spinNum = 0;
            }

            lever1.transform.localRotation = Quaternion.Euler(L1spinNum, L1spinNum, L1spinNum);
            lever2.transform.localRotation = Quaternion.Euler(L2spinNum, L2spinNum, L2spinNum);

            if (lever1.GetComponent<Interactible>().numInteractions > 0) {
                lever1.GetComponent<Renderer>().material = enabledMaterial;
                indicator1.GetComponent<Renderer>().material = enabledMaterial;
                L1spinsPerSecond = 10f;
            }

            if (lever2.GetComponent<Interactible>().numInteractions > 0) {
                lever2.GetComponent<Renderer>().material = enabledMaterial;
                indicator2.GetComponent<Renderer>().material = enabledMaterial;
                L2spinsPerSecond = 10f;
            }

            if (L1spinsPerSecond == 10f && L2spinsPerSecond == 10f && area2Open == false) {
                shownewObjective("Discover the ruins ahead", true);
                dashUnlock.SetActive(true);
                Area2Collider.SetActive(true);
                area2Open = true;
            }
        }

        // Area 2 Code
        if (area2Open) {
            L3spinNum = L3spinNum + 360*Time.deltaTime/L3spinsPerSecond;
            if (L3spinNum >= 720) {
                L3spinNum = 0;
            }

            lever3.transform.localRotation = Quaternion.Euler(L3spinNum, L3spinNum, L3spinNum);

            if (lever3.GetComponent<Interactible>().numInteractions > 0 && area3Open == false) {
                lever3.GetComponent<Renderer>().material = enabledMaterial;
                L3spinsPerSecond = 10f;
                area3Open = true;
                area1Open = false;
                currCheckPoint = area3;
                area3Obj.SetActive(true);
                RaiseLavaAnim(3);
            }
        }

        // Area3 Code
        if (area3Open) {

            // enemies = GameObject.FindGameObjectsWithTag("Enemy");
            // currBigEnemies = 0;
            // for (int i = 0; i < enemies.Length; i++) {
            //         if (enemies[i].GetComponent<EnemyStats>().lv1role == Level1Manager.LEVELONEOBJECTIVES.FIRST) {
            //             currBigEnemies++;
            //     }
            // }

            // if (currBigEnemies > 0) {
            //     shownewObjective("Defeat brutes to progress (" + currBigEnemies.ToString() + "/2)", true);
            // } else {
            //     shownewObjective("Find the way and continue upwards", true);
            // }
            L4spinNum = L4spinNum + 360*Time.deltaTime/L4spinsPerSecond;
            if (L4spinNum >= 720) {
                L4spinNum = 0;
            }

            lever4.transform.localRotation = Quaternion.Euler(L4spinNum, L4spinNum, L4spinNum);

            if (lever4.GetComponent<Interactible>().numInteractions > 0 && area4Open == false) {
                lever4.GetComponent<Renderer>().material = enabledMaterial;
                L4spinsPerSecond = 10f;
                area4Open = true;
                area2Open = false;
                currCheckPoint = area4;
                area4Obj.SetActive(true);
                RaiseLavaAnim(4);
            }
        }

        // Area4 Code
        if (area4Open) {
            L5spinNum = L5spinNum + 360*Time.deltaTime/L5spinsPerSecond;
            if (L5spinNum >= 720) {
                L5spinNum = 0;
            }

            lever5.transform.localRotation = Quaternion.Euler(L5spinNum, L5spinNum, L5spinNum);

            if (lever5.GetComponent<Interactible>().numInteractions > 0 && area5Open == false) {
                lever5.GetComponent<Renderer>().material = enabledMaterial;
                shownewObjective("", false);
                ManageScenes.lives = 5;
                playerScript.hp = playerScript.maxHp;
                L5spinsPerSecond = 15f;
                area5Open = true;
                area3Open = false;
                currCheckPoint = area5;
                area5Obj.SetActive(true);
                RaiseLavaAnim(5);
            }
        }

        // Area5 Code
        if (area5Open) {
            if (timerBeforeBossSpawns == 0 &&
                !bossSpawned) {
                /* Spawn the boss when the platform fully rises */
                bossSpawned = true;
                Instantiate(theBigBoss, area5Obj.transform.position + new Vector3(0, 4.5f, 0), Quaternion.identity);
            } else {
                timerBeforeBossSpawns = Mathf.Clamp(timerBeforeBossSpawns - Time.deltaTime, 0, timerBeforeBossSpawns);
            }

            GameObject[] bossAlive = GameObject.FindGameObjectsWithTag("Boss");
            if (bossSpawned && bossAlive.Length == 0 && !GateOpened) {
                GateOpened = true;
                WinGate.GetComponent<winGateMan>().on = true;
                area5Obj.GetComponent<Animator>().Play("BossWinGateReveal");
            }
        }
        
    }

    public void shownewObjective(string s, bool b) {
        if (b) {
            objectiveGui.SetActive(true);
            objective.text = s;
        } else {
            objectiveGui.SetActive(false);
        }
    }

    public void ResetCameras() {
        player.SetActive(true);
        camobject.SetActive(true);
        maincam.enabled = true;
        maincam.GetComponent<AudioListener>().enabled = true;
        cam1.enabled = false;
        cam1.GetComponent<AudioListener>().enabled = false;
    }

    public void RaiseLavaAnim(int level) {
        gameSource.GetComponent<AudioSource>().clip = earthquake[0];
        gameSource.Play(0);
        if (level == 2) {
            GetComponent<Animator>().Play("RaiseLava2");
            area2Obj.SetActive(true);
        } else if (level == 3) {
            GetComponent<Animator>().Play("RaiseLava3");
            area3Obj.SetActive(true);
        } else if (level == 4) {
            GetComponent<Animator>().Play("RaiseLava4");
            area4Obj.SetActive(true);
        } else if (level == 5) {
            GetComponent<Animator>().Play("RaiseLava5");
            area5Obj.SetActive(true);
        }
    }

    IEnumerator TurnOffPlayer(float seconds) {
        yield return new WaitForSeconds(seconds);
        player.SetActive(false);
    }



    
}
