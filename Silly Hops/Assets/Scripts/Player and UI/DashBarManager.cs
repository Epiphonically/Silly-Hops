using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashBarManager : MonoBehaviour
{
    public Slider cdSlider;
    public Slider numDashesSlider;

    public PlayerManager playerScript;
    void Start()
    {
        playerScript = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerManager>();
        cdSlider.value = 0;
        numDashesSlider.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        cdSlider.value = (playerScript.maxDashCd - playerScript.dashCd)/playerScript.maxDashCd;
        numDashesSlider.value = Mathf.Lerp(numDashesSlider.value, ((float) playerScript.dashesLeft)/((float) playerScript.maxDashesLeft), 20*Time.deltaTime);
    }
}
