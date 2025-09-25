using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RGBB : MonoBehaviour
{
    public Image sprite;
    private float maxTimer = 3;
    private float timer = 0;
    private int index = 0, lastIndex = 0;
    private float where = 0;
    private Color[] colors;

    void Start() {
        colors = new Color[6];
        colors[0] = new Color(255,0,0);
        colors[1] = new Color(255,0,255);
        colors[2] = new Color(0,0,255);
        colors[3] = new Color(0,255,255);
        colors[4] = new Color(0,255,0);
        colors[5] = new Color(255,255,0);
        sprite.color = colors[0];
    }
    void Update()
    {

        if (timer == 0) {
            timer = maxTimer;
            index++;
            index = index % colors.Length;
            where = 0;
            lastIndex = (index - 1 < 0 ? colors.Length - 1 : index - 1);
        }
    
        timer = Mathf.Clamp(timer - Time.deltaTime, 0, timer);
        where = Mathf.Clamp(where + (Time.deltaTime / maxTimer), 0, 1);
        
       
        sprite.color = Color.Lerp(colors[lastIndex], colors[index], where);
        Debug.Log(sprite.color);
 
    }
}
