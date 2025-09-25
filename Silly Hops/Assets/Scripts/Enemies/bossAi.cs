using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bossAi : MonoBehaviour {

    public Animator myAnime;

    public GameObject player, blast, firePart, slashTrail, sickle, bossBoom, deathExplosion;
    public Transform handPos;
    public PlayerManager playerScript;
    public Material bossGlow;
    public float fireRate = 0.2f, hp = 2000, maxHp = 2000, groundY, sickleDmg = 3;
    private float dashSpd = 100, blastDmg = 1, slashDmg = 1, teleportSlashDmg = 75, swipeRange = 4, teleportTime = 0.2f;
    public bool faceFixed = false, playedBoom = false, started = false;
    public Collider[] colliders;
    private float sickleFireRate = 0.5f;
    public AudioSource mySource;
    public AudioClip[] shootBlastSound, swordDashSound, chargeSlashSound, BossOw;
    public AudioClip bossEntrance, swingHard, spinningDeath;
    public float currBob = 0;
    private float bobAmmount = 0.5f;
    
    public Vector3 origPos, face;
    public Slider healthBar;
    public Coroutine lastCO;

    private float initCutScene = 5, coward = 20;

    /* Short delay between n consecutive attacks then one long delay before the next batch */
    private int minNumConsec = 2;
    private int maxNumConsec = 4;
    private int currConsecs = 0;
    private int currMaxConsecs = 0;
    private float maxConsecCd = 1, consecCd = 0, maxBatchCd = 3, batchCd = 0;
    private bool currentlyInAttack = false, canBob = true;
    private int minFireNum = 6, maxFireNum = 9;

    private float deathTimer = 5.5f;
    void Start() {
        groundY = transform.position.y + 1;
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        playerScript = player.GetComponent<PlayerManager>();
        slashTrail.SetActive(false); 
        origPos = transform.position;
        face = transform.forward;
        currMaxConsecs = Random.Range(minNumConsec, maxNumConsec + 1); 
        healthBar.value = 0;
        
    }

    void Update() {
        if (!ManageScenes.paused) {
            healthBar.value = Mathf.Lerp(healthBar.value, hp/maxHp, 20*Time.deltaTime);
            if (hp > 0) {
                if (hp <= maxHp/4) { /* Phase 2 stats when they are below a quarter of health */
                    minNumConsec = 4;
                    maxNumConsec = 8;
                    sickleFireRate = 0.2f;
                    fireRate = 0.2f;
                    maxBatchCd = 2;
                    minFireNum = 9;
                    maxFireNum = 14;
                    maxConsecCd = 0.7f;
                    teleportSlashDmg = 75;
                    slashDmg = 50;
                    blastDmg = 35;
                    sickleDmg = 30;
                } else { /* Phase 1 stats */
                    minNumConsec = 2;
                    maxNumConsec = 4;
                    sickleFireRate = 0.6f;
                    fireRate = 0.4f;
                    maxBatchCd = 4;
                    minFireNum = 6;
                    maxFireNum = 9;
                    maxConsecCd = 1.3f;
                    teleportSlashDmg = 25;
                    slashDmg = 25;
                    blastDmg = 25;
                    sickleDmg = 25;
                }   
                if (initCutScene == 0) {
                    if (!currentlyInAttack) {
                        origPos = Vector3.Lerp(origPos, new Vector3(origPos.x, groundY, origPos.z), 20*Time.deltaTime);
                    }

                    if (currConsecs == currMaxConsecs && !currentlyInAttack) {
                        batchCd = Mathf.Clamp(batchCd - Time.deltaTime, 0, batchCd);
                        if (batchCd == 0) {
                            currConsecs = 0;
                            currMaxConsecs = Random.Range(minNumConsec, maxNumConsec + 1); 
                        }
                    }

                    if (consecCd == 0 && !currentlyInAttack && currConsecs < currMaxConsecs) { /* Every consecutive cooldown we call a new attack coroutine! */
                        currConsecs++;
                        int roll = Random.Range(1, 4);
                        int numToFire = Random.Range(minFireNum, maxFireNum) + 1;

                        /* The Teleportation slash has priority when the player is being a "coward" and is far from the boss*/
                        if ((transform.position - player.transform.position).magnitude > coward) { 
                            roll = 0;
                        } 
                    
                        switch (roll) {
                            case 0:
                                lastCO = StartCoroutine(ShortTeleportSlash());
                            break;

                            case 1:
                                
                                lastCO = StartCoroutine(RapidFire(numToFire));
                            break;

                            case 2:
                                lastCO = StartCoroutine(SlashAndDash());
                            break;

                            case 3:
                            
                                lastCO = StartCoroutine(SickleBarrage(numToFire));
                            break;
                        }
                        if (currConsecs == currMaxConsecs) {
                            batchCd = maxBatchCd;
                        }
                    }
                    currBob = currBob + Time.deltaTime;
                    if (currBob >= 180) {
                        currBob = 0;
                    }
                    if (canBob) {
                        transform.localPosition = origPos + new Vector3(0, bobAmmount*Mathf.Sin(currBob), 0);
                    } else {
                        transform.localPosition = origPos;
                    }   
                    if (!faceFixed) {
                        Vector3 target = player.transform.position - transform.position;
                        target = new Vector3(target.x, 0, target.z);
                        face = Vector3.Lerp(face, target, 10*Time.deltaTime);
                        transform.LookAt(transform.position + face);
                    } 
            
                    if (!currentlyInAttack) {
                        consecCd = Mathf.Clamp(consecCd - Time.deltaTime, 0, consecCd);
                    }

                } else {
                    initCutScene = Mathf.Clamp(initCutScene - Time.deltaTime, 0, initCutScene);
                    if (initCutScene < 3.3 && !playedBoom) {
                        Instantiate(bossBoom, transform.position, Quaternion.Euler(-90, 0, 0));
                        mySource.GetComponent<AudioSource>().clip = bossEntrance;
                        mySource.Play(0);
                        playedBoom = true;
                    }
                }   
            } else {
                /* Win! */
                /* COOL we end the game on a coroutine! */
                if (!started) {  
                    started = true;
                    StartCoroutine(Death());
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            hp = 0;
        }
    }

    public IEnumerator Death() {
        mySource.GetComponent<AudioSource>().clip = spinningDeath;
        mySource.Play(0);
        if (lastCO != null) {
            StopCoroutine(lastCO);
        }
        myAnime.SetTrigger("deathTrig");
        yield return new WaitForSeconds(deathTimer);
  

        Instantiate(deathExplosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    /* For our boss EVERY ATTACK is a coroutine this systems is as simple as picking a coroutine to run on a set time interval */

    public IEnumerator SickleBarrage(int numShots) {
        canBob = true;
        currentlyInAttack = true;
        int shotsFired = 0;
        int alternator = 1;
        myAnime.SetTrigger("sickleUpTrig");
        yield return new WaitForSeconds(0.5f);
        while (shotsFired < numShots) {
            shotsFired++;
            if (alternator == 1) { /* Swings right */
                myAnime.SetTrigger("sickleRightTrig");
               
            } else { /* Swings left */
            
                myAnime.SetTrigger("sickleLeftTrig");
            }

            mySource.GetComponent<AudioSource>().clip = shootBlastSound[Random.Range(0, shootBlastSound.Length)];
            mySource.Play(0);
          
            GameObject theSickle = Instantiate(sickle, slashTrail.transform.position + transform.forward, Quaternion.identity);
            theSickle.transform.forward = transform.forward;
            SickleMan sickMan = theSickle.GetComponent<SickleMan>();
            sickMan.direction = (player.transform.position - slashTrail.transform.position).normalized;
            sickMan.alternate = alternator;
            sickMan.spd = 30;
            sickMan.dmg = sickleDmg;
            alternator *= -1; /* The alternator dictates which direction the sickle spins in */
            yield return new WaitForSeconds(sickleFireRate);
        }
        myAnime.SetTrigger("sickleDownTrig");
        yield return new WaitForSeconds(0.5f);
        currentlyInAttack = false;
        consecCd = maxConsecCd;
        myAnime.SetTrigger("idleTrig");
    }

    public IEnumerator ShortTeleportSlash() {
        canBob = false;
        currentlyInAttack = true;
        Vector3 init = origPos;
        Vector3 targetPos = player.transform.position + 3*player.transform.forward - new Vector3(0, 3, 0);
        targetPos.y = Mathf.Max(targetPos.y, groundY - 1.5f);
       
        float timer = teleportTime;

        
        while (timer > 0) { /* Initial teleport up to player */


            origPos = init * (timer/teleportTime) + targetPos * (1 - (timer/teleportTime));


            colliders = Physics.OverlapBox(transform.position, 2*(new Vector3(swipeRange, swipeRange, swipeRange)), transform.localRotation);
           
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, timer);


            yield return null;
        }   
        
        timer = 1f;
        slashTrail.SetActive(true); 
        myAnime.SetTrigger("shortSlashTrig");
        mySource.GetComponent<AudioSource>().clip = chargeSlashSound[Random.Range(0, chargeSlashSound.Length)];
        mySource.Play(0);
        bool hitHim = false; 
        while (timer > 0) { /* Follow player */
            targetPos = player.transform.position + 3*player.transform.forward - new Vector3(0, 3, 0);
            targetPos.y = Mathf.Max(targetPos.y, groundY - 1.5f);
            origPos =  targetPos;

            
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, timer);
            yield return null;
        }
        mySource.GetComponent<AudioSource>().clip = swingHard;
        mySource.Play(0);
        colliders = Physics.OverlapBox(transform.position, new Vector3(swipeRange, swipeRange, swipeRange), transform.localRotation);
            
        for (int i = 0; i < colliders.Length; i++) {
            if (colliders[i].gameObject.layer == 6 && !hitHim) { /* hit player */
                hitHim = true;
                colliders[i].SendMessage("Damage", teleportSlashDmg);
            }
        }
        slashTrail.SetActive(false); 
        currentlyInAttack = false;
        consecCd = maxConsecCd;
        canBob = true;
        myAnime.SetTrigger("idleTrig");
    }

    public IEnumerator SlashAndDash() {
        canBob = false;
        currentlyInAttack = true;
        slashTrail.SetActive(true); 
        Vector3 start = origPos;
        
        myAnime.SetTrigger("slashTrig");
        
        /* 2 seconds to hold up the sword before swining and dashing */
        mySource.GetComponent<AudioSource>().clip = chargeSlashSound[Random.Range(0, chargeSlashSound.Length)];
        mySource.Play(0);
        yield return new WaitForSeconds(2f);

        /* Lets Dash and Slash */
        mySource.GetComponent<AudioSource>().clip = swordDashSound[Random.Range(0, swordDashSound.Length)];
        mySource.Play(0);
        Vector3 dashDir = transform.forward;
        faceFixed = true;
        float dashTimer = 0.3f;
        bool hitPlayer = false;
        while (dashTimer > 0) {
            colliders = Physics.OverlapBox(transform.position, new Vector3(swipeRange, swipeRange, swipeRange), transform.localRotation);
            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i].gameObject.layer == 6 && !hitPlayer) { /* hit player */
                    hitPlayer = true; /* Dont want to be continuously hitting the player */
                    colliders[i].SendMessage("CripplingDamage", slashDmg);
                }
            }
            origPos += (dashDir * dashSpd * Time.deltaTime);
            dashTimer = Mathf.Clamp(dashTimer - Time.deltaTime, 0, dashTimer);
    
            yield return null;
        }

        slashTrail.SetActive(false); 
        yield return new WaitForSeconds(2);
        faceFixed = false;
        currentlyInAttack = false;
        consecCd = maxConsecCd;
        canBob = true;
        myAnime.SetTrigger("idleTrig");
    }

    /* This coroutine follows that of similar logic to rapid fire in the enemy */
    public IEnumerator RapidFire(int numShots) {
        canBob = true;
        currentlyInAttack = true;
        int shotsFired = 0;
        myAnime.SetTrigger("beginShootTrig"); /* Take 1 second to move our arm up */
        yield return new WaitForSeconds(1);
        while (shotsFired < numShots) {
            shotsFired++;
            myAnime.SetTrigger("shootTrig"); /* An intermediary shooting animation */
            mySource.GetComponent<AudioSource>().clip = shootBlastSound[Random.Range(0, shootBlastSound.Length)];
            mySource.Play(0);
            Instantiate(firePart, handPos.position, transform.localRotation);
            GameObject theBlast = Instantiate(blast, handPos.position, transform.localRotation);
            theBlast.transform.localScale *= 8;
            BulletManager bullMan = theBlast.GetComponent<BulletManager>();
            bullMan.direction = ((player.transform.position - new Vector3(0, 1, 0)) - handPos.position).normalized;
            bullMan.team = 1;
            bullMan.spd = 50;
            bullMan.myType = BulletManager.TYPE.BOOM;
            bullMan.dmg = blastDmg;
            bullMan.hasTrail = false;
            Material[] temp = {bossGlow};
            bullMan.GetComponent<MeshRenderer>().materials = temp;
            bullMan.GetComponent<TrailRenderer>().materials = temp;
            yield return new WaitForSeconds(fireRate);
        }
        myAnime.SetTrigger("endShootTrig"); /* Move our arm back down */
        currentlyInAttack = false;
        consecCd = maxConsecCd;
        myAnime.SetTrigger("idleTrig");
    }

    public void Damage(float dmg) {
        mySource.GetComponent<AudioSource>().clip = BossOw[Random.Range(0, BossOw.Length)];
        mySource.Play(0);
        hp = Mathf.Clamp(hp - dmg, 0, hp);
    }
}
