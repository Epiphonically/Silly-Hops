using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class AmmoBarManager : MonoBehaviour
{
    public Slider slider;
    public PlayerManager playerScript;
    public TextMeshProUGUI theText;
    public Image ammoBar;
    public Color meeleeColor, sprayColor, shotGunColor, bounceColor;
    void Start()
    {   
        playerScript = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerManager>();
        slider.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        theText.text = Mathf.Floor(playerScript.displayAmmo).ToString() + "/" + Mathf.Floor(playerScript.displayMaxAmmo).ToString();
        slider.value = playerScript.displayAmmo/playerScript.displayMaxAmmo;
        switch (playerScript.hand) {
            case PlayerManager.HAND.MEELEE:
                ammoBar.color = meeleeColor;
                
            break;

            case PlayerManager.HAND.GUN:
                switch (playerScript.ammoType) {
                    case PlayerManager.SHOTS.SPRAY:
                        ammoBar.color = sprayColor;
                    break;

                    case PlayerManager.SHOTS.SHOTGUN:
                        ammoBar.color = shotGunColor;
                    break;

                    case PlayerManager.SHOTS.BOUNCE:
                        ammoBar.color = bounceColor;
                    break;

                }

                
            break;
        }
    }
}
