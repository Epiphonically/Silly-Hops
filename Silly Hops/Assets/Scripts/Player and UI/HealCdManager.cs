using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealCdManager : MonoBehaviour
{
    public PlayerManager playerScript;
    public Slider healCdSlider;

    void Start() {
        playerScript = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerManager>();
        healCdSlider.value = 0;
    }

    void Update()
    {
        healCdSlider.value = (playerScript.maxHealCd - playerScript.healCd) / playerScript.maxHealCd;
    }
}
