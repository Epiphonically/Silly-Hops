using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level3Man : MonoBehaviour
{
    public GameObject winGate, spawn, fallLimit, player;
    public PlayerManager playerScript;
    public HPSetter hpSetter;

    void Start() {
        spawn.GetComponent<Interactible>().canInteract = false;
        spawn.GetComponent<checkPointMan>().currMat = spawn.GetComponent<checkPointMan>().onMat;
        player.transform.position = spawn.transform.position + new Vector3(0, 5, 0);
        playerScript.cameraHorizontalRotation = Vector2.Angle(new Vector2(0, 1), (new Vector2(spawn.transform.forward.x, spawn.transform.forward.z)));
        playerScript.cameraVerticalRotation = 0;
    }

    void Update() {
        hpSetter.lives = ManageScenes.lives;
        winGate.GetComponent<winGateMan>().on = true;

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
    }   
}
