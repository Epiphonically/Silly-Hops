using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputAnimator : MonoBehaviour
{
    public PlayerManager playerScript;

    public Image shift;
    public Image leftMb;
    public Image rightMb;

    public Image leftMbImage;
    public Image rightMbImage;

    public GameObject Eobj, empowerDashAni;
    public Image E;
    public Image C;


    public Sprite leftMbUp;
    public Sprite leftMbDown;

    public Sprite rightMbUp;
    public Sprite rightMbDown;

    public Sprite shiftUp;
    public Sprite shiftDown;

    public Sprite sword;
    public Sprite shield;
    public Sprite shoot;
    public Sprite swap;

    public Sprite Edown;
    public Sprite Eup;

    public Sprite Cdown;
    public Sprite Cup;

    public bool eActive = false;
    void Update()
    {
        if (playerScript.nextDashStrong) {
            empowerDashAni.SetActive(true);
        } else {
            empowerDashAni.SetActive(false);
        }
        
        if (playerScript.hand == PlayerManager.HAND.MEELEE) {
            leftMbImage.sprite = sword;
            rightMbImage.sprite = shield;
        } else {
            leftMbImage.sprite = shoot;
            rightMbImage.sprite = swap;
        }

        if (Input.GetMouseButton(0)) {
            leftMb.sprite = leftMbDown;
        } else {
            leftMb.sprite = leftMbUp;
        }

        if (Input.GetMouseButton(1)) {
            rightMb.sprite = rightMbDown;
        } else {
            rightMb.sprite = rightMbUp;
        }

        if (Input.GetKey(KeyCode.LeftShift)) {
            shift.sprite = shiftDown;
        } else {
            shift.sprite = shiftUp;
        }
        
        if (eActive) {
            Eobj.SetActive(true);
            if (Input.GetKey(KeyCode.E)) {
                E.sprite = Edown;
            } else {
                E.sprite = Eup;
            }
        } else {
            Eobj.SetActive(false);
        }

        if (Input.GetKey(KeyCode.C)) {
            C.sprite = Cdown;
        } else {
            C.sprite = Cup;
        }
        
    }
}
