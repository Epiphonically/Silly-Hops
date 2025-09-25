using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseLava : MonoBehaviour
{
    public int raiseLevel;
    private bool raised;
    public BossLevelManager manager;
    // Update is called once per frame

    void Raise() {
        if (raised == false) {
            raised = true;
            manager.RaiseLavaAnim(raiseLevel);
        }
    }
}
