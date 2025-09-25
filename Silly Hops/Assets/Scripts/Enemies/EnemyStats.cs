using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    
    public enum ENEMYTYPE {
        DUMMY,
        SLOWSHOOTSPEED,
        MEDIUMSHOOTSPEED,
        FASTSHOOTSPEED,
        SHOTGUN,
        SENTRY
    }

    public enum ENEMYSTATE {
        TURNEDOFF,
        IDLE,
        MOVING,
        SHOOTING
    }

    public float hp = 100f;
    public float maxHp = 100f;
    public float shield = 100f;
    public float maxShield = 100f;
    public bool hasShield = false;

    public float regenShieldTimer = 0f;
    public float maxRegenShieldTimer = 1f;
    public float timeItTakesToRegenShield = 0.5f;

    public float shootingMaxRange = 10f;
    public float sight = 20f;
    public bool invincible = false;
    public Tutorial.TUTORIALPHASES role;
    public Level1Manager.LEVELONEOBJECTIVES lv1role;
    public TowerManager.FLOORS towerFloor;

    public bool smart = false;
    public bool idle = true;
    public ENEMYTYPE enemyType = ENEMYTYPE.DUMMY;
    
    public ENEMYSTATE currState = ENEMYSTATE.IDLE;


}
