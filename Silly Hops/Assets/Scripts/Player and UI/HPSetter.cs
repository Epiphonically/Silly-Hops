using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPSetter : MonoBehaviour
{
    public Slider slider;
    public PlayerManager playerScript;
    public GameObject hurt, heal;
    public int lives = -1;
    public float hurtTimer = 0f;
    public float healTimer = 0f;
    public TextMeshProUGUI livesText;
    // Update is called once per frame

    void Start() {
        lives = -1;
        playerScript = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerManager>();
        slider.value = 0;
    }
    void Update()
    {   
        if (lives < 0) {
            livesText.text = "";
        } else {
            livesText.text = lives.ToString();
        }
        if (hurtTimer > 0) {
            hurt.SetActive(true);
        } else {
            hurt.SetActive(false);
        }
        if (healTimer > 0) {
            heal.SetActive(true);
        } else {
            heal.SetActive(false);
        }
        hurtTimer = Mathf.Clamp(hurtTimer - Time.deltaTime, 0, hurtTimer);
        healTimer = Mathf.Clamp(healTimer - Time.deltaTime, 0, healTimer);

        slider.value = Mathf.Lerp(slider.value, playerScript.hp/playerScript.maxHp, 20*Time.deltaTime);
        
    }
}
