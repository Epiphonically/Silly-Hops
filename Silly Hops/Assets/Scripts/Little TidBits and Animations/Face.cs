using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Face : MonoBehaviour
{   
    public Camera cam;
    public EnemyStats stats;
    public Slider hpSlider;
    public Slider shieldSlider;

    void Start() {
        hpSlider.value = 0;
        cam = Camera.main;
    }

    void Update() {
        transform.LookAt(transform.position + cam.transform.forward);
        hpSlider.value = Mathf.Lerp(hpSlider.value, stats.hp/stats.maxHp, 30*Time.deltaTime);
        shieldSlider.value = Mathf.Lerp(shieldSlider.value, stats.shield/stats.maxShield, 30*Time.deltaTime);
        //slide.value = Mathf.Lerp(slide.value, stats.hp/stats.maxHp, 0.8f*Time.deltaTime);
    }   
}
