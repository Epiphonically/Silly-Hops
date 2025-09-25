using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{

    public EnemyStats stats;
    public Rigidbody rb;
    public GameObject player, canvas, body;
    public float currAttackCd = 0f;
    public float maxAttackCd = 5f;
    public float maxShotCd = 0.3f;
    public float bulletSpd = 50f;
    public GameObject bulletObj, deathSpark;

    public Vector3 aim;
    public Vector3 origPos;

    public bool canDie = true;
    public Animator myAnime;
    public GameObject nav, moverObj;
    public Transform shootPos;

    public Vector3 navTarget;
    public Vector3 elevation = new Vector3(0, 0, 0);
    public int numBullets = 0;
    public Material[] temp;
    public Material currMat, dummyMat, slowMat, medMat, fastMat, shotgunMat, sentryMat;
    public Color currColor, dummyColor, slowColor, medColor, fastColor, shotGunColor, sentryColor;
    public int numShotsPerShell = 7;
    float epsBound = 0.1f;
    public IEnumerator lastShoot;
    public bool shooting = false;
    public AudioClip[] ouch, death, fire;
    public AudioSource mySource, gunSource;

    public float dmgPerBull;
    public Transform head;

    public MeshRenderer gunModel;
    public SkinnedMeshRenderer myModel;


    public RaycastHit[] hitList;
    public RaycastHit hit;

    void Awake()
    {

        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
        nav = Instantiate(moverObj, transform.position, Quaternion.identity);
        nav.GetComponent<EnemyMover>().him = gameObject.GetComponent<EnemyManager>();
        rb.freezeRotation = true;
        origPos = transform.position;

    }

    void Update()
    {
        if (!(ManageScenes.win || ManageScenes.lost)) {
        if (stats.hasShield)
        {
            stats.regenShieldTimer = Mathf.Clamp(stats.regenShieldTimer - Time.deltaTime, 0, stats.regenShieldTimer);
            if (stats.regenShieldTimer == 0)
            {
                stats.shield = Mathf.Clamp(stats.shield + stats.maxShield * (Time.deltaTime / stats.timeItTakesToRegenShield), 0, stats.maxShield);
            }
        }
        else
        {
            stats.shield = 0;
        }


        Vector3 toPlayerPos = player.GetComponent<PlayerManager>().head.transform.position - transform.position;
        Vector3 toPlayerShot = player.GetComponent<PlayerManager>().head.transform.position - shootPos.position;

        Vector3 toPlayerXZ = new Vector3(player.transform.position.x - transform.position.x, 0, player.transform.position.z - transform.position.z);

        Vector3 toNav = navTarget + elevation - transform.position;
        float distFromPlayer = toPlayerXZ.magnitude;
        float distToNav = toNav.magnitude;


        switch (stats.enemyType)
        {
            case EnemyStats.ENEMYTYPE.DUMMY:
                currMat = dummyMat;
                break;

            case EnemyStats.ENEMYTYPE.SLOWSHOOTSPEED:
                currColor = slowColor;
                currMat = slowMat;
                dmgPerBull = 5;
                maxAttackCd = 2f;
                maxShotCd = 0.3f;
                numBullets = 3;
                epsBound = 0.1f;
                numShotsPerShell = 1;
                break;

            case EnemyStats.ENEMYTYPE.MEDIUMSHOOTSPEED:
                currColor = medColor;
                currMat = medMat;
                dmgPerBull = 5;
                maxAttackCd = 2f;
                maxShotCd = 0.2f;
                numBullets = 5;
                epsBound = 0.1f;
                numShotsPerShell = 1;
                break;

            case EnemyStats.ENEMYTYPE.FASTSHOOTSPEED:
                currColor = fastColor;
                currMat = fastMat;
                dmgPerBull = 5;
                maxAttackCd = 2f;
                maxShotCd = 0.1f;
                numBullets = 10;
                epsBound = 0.1f;
                numShotsPerShell = 1;
                break;

            case EnemyStats.ENEMYTYPE.SHOTGUN:
                currColor = shotGunColor;
                currMat = shotgunMat;
                dmgPerBull = 5;
                maxAttackCd = 1f;
                maxShotCd = 1;
                numBullets = 1;
                epsBound = 2;
                numShotsPerShell = 7;
                break;

            case EnemyStats.ENEMYTYPE.SENTRY:
                currColor = sentryColor;
                currMat = sentryMat;
                stats.shootingMaxRange = 30;
                dmgPerBull = 5;
                maxAttackCd = 0.1f;
                maxShotCd = 0.2f;
                numBullets = 1;
                epsBound = 0.1f;
                numShotsPerShell = 1;
                break;
        }

        temp = gunModel.materials;
        temp[2] = currMat;
        gunModel.materials = temp;
        transform.position = (new Vector3(nav.transform.position.x, rb.position.y, nav.transform.position.z));

        temp = myModel.materials;
        temp[5] = currMat;
        myModel.materials = temp;

        float tolerance = 0.01f;

        if (!stats.idle)
        {


            if (stats.currState != EnemyStats.ENEMYSTATE.TURNEDOFF)
            {
                if (nav.GetComponent<NavMeshAgent>().velocity.magnitude > tolerance)
                {
                    if (!myAnime.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                    {

                        myAnime.SetTrigger("walkTrig");

                    }
                }
                else if (!shooting &&
                           !myAnime.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {

                    myAnime.SetTrigger("idleTrig");

                }
            }

            bool hasSight = true;

            hitList = Physics.RaycastAll(shootPos.position - toPlayerShot.normalized, toPlayerShot.normalized, stats.shootingMaxRange);
            System.Array.Sort(hitList, (x, y) => x.distance.CompareTo(y.distance));

            for (int j = 0; j < hitList.Length; j++)
            {
                hit = hitList[j];
                if (hitList[j].collider.tag == "Player")
                {
                    hasSight = true;
                    break;
                }
                else if (hitList[j].collider.tag == "Wall")
                {
                    hasSight = false;
                    break;
                }
            }
            switch (stats.currState)
            {

                case EnemyStats.ENEMYSTATE.TURNEDOFF:
                    if (!myAnime.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    {
                        myAnime.SetTrigger("idleTrig");

                    }
                    break;

                case EnemyStats.ENEMYSTATE.IDLE:

                    if (distToNav > 1f)
                    {

                        aim = Vector3.Lerp(aim, toNav, 10 * Time.deltaTime);
                        //Debug.DrawRay(transform.position, aim * 10f, Color.green);
                        transform.LookAt(new Vector3(transform.position.x + aim.x,
                                                transform.position.y,
                                                transform.position.z + aim.z));

                    }

                    break;

                case EnemyStats.ENEMYSTATE.MOVING:

                    aim = Vector3.Lerp(aim, toPlayerShot, 10 * Time.deltaTime);
                    //Debug.DrawRay(transform.position, aim * 10f, Color.green);
                    transform.LookAt(new Vector3(transform.position.x + aim.x,
                                                transform.position.y,
                                                transform.position.z + aim.z));

                    break;

                case EnemyStats.ENEMYSTATE.SHOOTING:

                    aim = Vector3.Lerp(aim, toPlayerShot, 10 * Time.deltaTime);
                    //Debug.DrawRay(transform.position, aim * 10f, Color.green);
                    transform.LookAt(new Vector3(transform.position.x + aim.x,
                                                transform.position.y,
                                                transform.position.z + aim.z));



                    if (distFromPlayer <= stats.shootingMaxRange + 2 && hasSight)
                    {
                        /* SHOOT HIM GRRR */

                        if (currAttackCd == 0)
                        {
                            if (lastShoot != null)
                            {
                                StopCoroutine(lastShoot);
                            }
                            lastShoot = RapidFire(numBullets);
                            StartCoroutine(lastShoot);
                            currAttackCd = maxAttackCd;
                        }
                    }
                    break;
            }



            if (currAttackCd > 0)
            {
                currAttackCd = Mathf.Clamp(currAttackCd - Time.deltaTime, 0, currAttackCd);
            }

        }
        }
    }


    void Damage(float dmg)
    {

        dmg = Mathf.Abs(dmg);
        if (!stats.invincible)
        {
            stats.regenShieldTimer = stats.maxRegenShieldTimer;
            if (stats.shield > 0)
            {
                stats.shield = Mathf.Clamp(stats.shield - dmg, 0, stats.maxShield);
            }
            else
            {
                stats.hp = Mathf.Clamp(stats.hp - dmg, 0, stats.maxHp);
            }

        }


        if (stats.hp <= 0)
        {
            if (canDie)
            {

                if (lastShoot != null)
                {
                    StopCoroutine(lastShoot);
                }

                GameObject spark = Instantiate(deathSpark, transform.position, Quaternion.identity);
                spark.GetComponent<ManageSparks>().mat = currMat;

                Destroy(nav);

                Destroy(gameObject);
            }
            else
            {
                stats.hp = stats.maxHp;
            }
        }
        else
        {
            mySource.GetComponent<AudioSource>().clip = ouch[Random.Range(0, ouch.Length)];
            mySource.Play(0);
        }
    }

    /* This coroutine uses topics covered in class such as using WaitForSeconds between each bullet shot */
    IEnumerator RapidFire(int times)
    {
        int shotsFired = 0;
        GameObject tinker;
        Vector3 eps;


        BulletManager bullMan;

        while (shotsFired < times)
        {
            /* Make new bullets */
            shooting = true;

            gunSource.GetComponent<AudioSource>().clip = fire[Random.Range(0, fire.Length)];
            gunSource.Play(0);



            myAnime.SetTrigger("shootTrig");


            for (int i = 0; i < numShotsPerShell; i++)
            {

                tinker = Instantiate(bulletObj, shootPos.position, Quaternion.identity);




                bullMan = tinker.GetComponent<BulletManager>();
                bullMan.team = 1;
                bullMan.spd = bulletSpd;
                bullMan.dmg = dmgPerBull;
                bullMan = tinker.GetComponent<BulletManager>();
                temp = bullMan.GetComponent<MeshRenderer>().materials;

                temp[0] = currMat;
                bullMan.GetComponent<MeshRenderer>().materials = temp;
                bullMan.GetComponent<TrailRenderer>().materials = temp;

                eps = new Vector3(
                Random.Range(-epsBound, epsBound),
                Random.Range(-epsBound, epsBound),
                0
                );

                bullMan.myColor = currColor;
                bullMan.direction = aim + eps;
                shotsFired++;

            }
            yield return new WaitForSeconds(maxShotCd);
        }
        shooting = false;

    }
}
