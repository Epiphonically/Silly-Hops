using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerManager : MonoBehaviour
{
    public enum STATES
    {
        MOVE,
        DASH,
        BOUNCE
    };

    public enum HAND
    {
        MEELEE,
        GUN
    };

    public enum SHOTS
    {
        SPRAY,
        SHOTGUN,
        BOUNCE
    };

    public new Camera camera;
    public HPSetter hpSetter;
    public Transform cameraEncapsulator;
    public RaycastHit[] hitList;
    public RaycastHit hit;
    public float cameraVerticalRotation = 0, cameraHorizontalRotation = 0;
    public float timeInDash = 0, maxTimeInDash = 0.2f,
    currJumpBoom = 0f, maxJumpBoom = 20f,
    shootCd = 0f, currMaxShootCd = 1f, shootRange = 100f, bloom = 0f, maxBloom = 0.3f,
    vertRecoil = 0f, maxVertRecoil = 4f, horRecoil = 0f, maxHorRecoil = 4f,
    alternator = 1f, maxShake = 0.1f, maxWeaponRecoil = 0.1f,
    walkBob = 0.1f, fastSpd = 30f, slowSpd = 20f,
    xaxis = 0f, zaxis = 0f,
    jumpBuffer = 0f, maxJumpBuffer = 0.1f, meeleeRange = 5f,
    meeleeDmg = 25f, dashDmg = 25f, meeleeCd = 1f, dashBuffer = 0f, maxDashBuffer = 0.2f, hp = 100f, maxHp = 100f,
    displayAmmo = 100f, displayMaxAmmo = 100f, spd = 0f, dashSpd, dashCd = 0f, maxDashCd = 1f, sensitivity = 4f,
    currSprayAmmo = 100f, maxSprayAmmo = 100f, currShotGunAmmo = 20f, maxShotGunAmmo = 20f, currBounceAmmo = 1f, maxBounceAmmo = 1f,
    sprayDmg = 10f, shotGunDmg = 10f, maxBounce = 20f, maxLittleBounce = 10f, currReloadTime, maxSway = 2,
    currBlock = 5, maxBlock = 5, blockRefillTime = 4, healCd = 0, maxHealCd = 60, jumpCd = 0, maxJumpCd = 0.3f, footStepTimer = 0, maxFootStepTimer;
    public AudioClip[] swipeAudio, dashAudio, sprayAudio, shotGunAudio, bounceAudio, hurt, block, jump, healSound, deathSound, footSteps, reload;
    public AudioSource bladeSource, gunSource, mySource, headSource;

    public Vector3 eps, cameraInitPos, dashDir;
    public Rigidbody rb;
    public STATES state = STATES.MOVE;
    public List<int> hitAlready;
    public bool isGrounded = true, blooming = false, recoiling = false, reloading = false, blocking = false,
    canReloadSpray, canReloadShotGun, canSwapToGun = true, nextDashStrong = false, onPrevDash = false;
    public GameObject bulletObj, shootingEpoch, bulletSparks, firingSparks, head;
    public HAND hand = HAND.MEELEE;
    public SHOTS ammoType = SHOTS.SPRAY;
    public WeaponManager weaponMan;

    public Tutorial tut;

    public MeshRenderer gunGlow;
    public Material sprayGlow, shotGunGlow, bounceGlow, noMat;
    public Material[] temp;
    public Collider[] colliders;
    public InputAnimator inputAni;
    public AmmoBarManager ammoBarMan;

    public Coroutine lastJump;

    public int dashesLeft = 3;
    public int maxDashesLeft = 3;
    void Start()
    {

        Time.fixedDeltaTime = 0.002f;
        hitAlready = new List<int>();

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        // Ensure the camera is a child of the player and reset its local position
        camera = Camera.main;

        camera.transform.localPosition = Vector3.zero;
        camera.transform.localRotation = Quaternion.identity;
        cameraInitPos = camera.transform.localPosition;


        gunGlow = weaponMan.gunModel.GetComponent<MeshRenderer>();

    }

    void LateUpdate() {
        /* In here we handle all our instantiation of bullets, and their respective raycasting
           this is after all needed variables are calculated in normal Update() */
        if (!ManageScenes.paused && !(ManageScenes.win || ManageScenes.lost)) {
            if (state != STATES.DASH) {
                switch (hand) {
                    case HAND.MEELEE:
                        if (Input.GetMouseButton(0)) {
                            if (shootCd == 0f && weaponMan.dashAniTimer <= 0) {

                                /* Notice that we will frequently be using an array of Audio Clips to have variety in our sounds
                                   this technique would go under the "Artistic" scoring category */
                                
                                bladeSource.GetComponent<AudioSource>().clip = swipeAudio[Random.Range(0, swipeAudio.Length)];
                                bladeSource.Play(0);


                                /* hitbox for sword */
                                colliders = Physics.OverlapBox((transform.position + cameraEncapsulator.transform.forward.normalized), new Vector3(0.5f, 1, meeleeRange), cameraEncapsulator.transform.localRotation);
                                for (int i = 0; i < colliders.Length; i++) {
                                    /* This collision check is a bit more complex because we dont want 
                                       one slash to hit multiple parts of the enemy or hit the same enemy twice... */
                                    if (colliders[i].tag == "Spawner")
                                    {
                                        if (!hitAlready.Contains(colliders[i].gameObject.GetInstanceID())) {

                                            colliders[i].SendMessage("Damage", meeleeDmg);
                                            hitAlready.Add(colliders[i].gameObject.GetInstanceID());
                                        }
                                    } else if (
                                        colliders[i] != null &&
                                        colliders[i].tag != "Enemy" && colliders[i].tag != "Boss" &&
                                        colliders[i].gameObject.layer == 7 &&
                                        !hitAlready.Contains(colliders[i].gameObject.GetComponent<EnemyHitBox>().him.GetInstanceID())) {

                                        colliders[i].SendMessage("Damage", -meeleeDmg);
                                        hitAlready.Add(colliders[i].gameObject.GetComponent<EnemyHitBox>().him.GetInstanceID());
                                        /* Enemy hit boxes are a bit more complex they reference the original enemy object */
                                    }
                                }

                                /* Begin animating slash here */
                                /* In theory slash is just forward movement followed by angular swing! */

                                weaponMan.aniNumber = 0;
                                weaponMan.currAniIndex = 0;
                                weaponMan.currBladeAnimationPos = new Vector3(0, 0, 0);
                                weaponMan.currBladeAnimationRot = Quaternion.identity;
                                weaponMan.timeTaken = 0f;

                                shootCd = currMaxShootCd;
                            }
                        }

                        break;

                    case HAND.GUN:

                        if (Input.GetMouseButton(0)) {
                            /* Handle shooting, and raycasting for whichever gun we are using */ 
                            switch (ammoType) { 
                                case SHOTS.SPRAY:
                                    if (shootCd == 0f && displayAmmo >= 1) {

                                        gunSource.GetComponent<AudioSource>().clip = sprayAudio[Random.Range(0, sprayAudio.Length)];
                                        gunSource.Play(0);
                                        GameObject theParticle = Instantiate(firingSparks, shootingEpoch.transform.position, cameraEncapsulator.transform.localRotation);
                                        temp = theParticle.GetComponent<ParticleSystemRenderer>().materials;
                                        temp[0] = gunGlow.materials[2];
                                        theParticle.GetComponent<ParticleSystemRenderer>().materials = temp;
                                        theParticle.GetComponent<Renderer>().materials = temp;

                                        eps = new Vector3(
                                        Random.Range(0, bloom),
                                        Random.Range(0, bloom),
                                        0
                                        );

                                        eps = cameraEncapsulator.transform.localRotation * eps;
                                        weaponMan.recoil = maxWeaponRecoil;
                                        weaponMan.recoilDir = new Vector3(0, 0, -1);

                                        Vector3 initPos = shootingEpoch.transform.position;
                                        GameObject tinker = Instantiate(bulletObj, initPos, Quaternion.identity);

                                        BulletManager bullMan = tinker.GetComponent<BulletManager>();
                                        temp = bullMan.GetComponent<MeshRenderer>().materials;

                                        temp[0] = gunGlow.materials[2];
                                        bullMan.GetComponent<MeshRenderer>().materials = temp;
                                        bullMan.GetComponent<TrailRenderer>().materials = temp;
                                        bullMan.destroyTimer = 1f;
                                        bullMan.myType = BulletManager.TYPE.NORMAL;
                                        bullMan.destroyTimerStart = false;
                                        bullMan.team = 0;
                                        bullMan.spd = 100f;

                                        bullMan.myColor = ammoBarMan.sprayColor;

                                        Vector3 Triangle = ((cameraEncapsulator.transform.forward).normalized * shootRange) + eps - (shootingEpoch.transform.localPosition);
                                        bullMan.direction = Triangle;
                                        hitList = Physics.RaycastAll(cameraEncapsulator.transform.position, cameraEncapsulator.transform.forward, shootRange);
                                        if (hitList.Length > 0) {
                                            bullMan.destroyTimerStart = true;
                                        }
                                        System.Array.Sort(hitList, (x, y) => x.distance.CompareTo(y.distance)); /* Sort to find the first thing we hit! */
                                        for (int i = 0; i < hitList.Length; i++) {
                                            hit = hitList[i];
                                            bullMan.destroyTimer = 0.1f;
                                            theParticle = Instantiate(bulletSparks, hit.point, Quaternion.identity);
                                            temp = theParticle.GetComponent<ParticleSystemRenderer>().materials;
                                            temp[0] = gunGlow.materials[2];
                                            theParticle.GetComponent<ParticleSystemRenderer>().materials = temp;
                                            theParticle.GetComponent<Renderer>().materials = temp;



                                            if (hit.collider != null) {
                                                if (hit.collider.gameObject.layer == 7) { /* This means we hit enemy */
                                                    hit.collider.SendMessage("Damage", sprayDmg);
                                                }


                                                break;
                                            }

                                        }

                                        shootCd = currMaxShootCd;


                                        currSprayAmmo--;
                                        displayAmmo = currSprayAmmo;

                                    }
                                    break;

                                case SHOTS.SHOTGUN:
                                    if (shootCd == 0f && displayAmmo >= 1) {
                                        gunSource.GetComponent<AudioSource>().clip = shotGunAudio[Random.Range(0, shotGunAudio.Length)];
                                        gunSource.Play(0);
                                        GameObject theParticle = Instantiate(firingSparks, shootingEpoch.transform.position, cameraEncapsulator.transform.localRotation);
                                        temp = theParticle.GetComponent<ParticleSystemRenderer>().materials;
                                        temp[0] = gunGlow.materials[2];
                                        theParticle.GetComponent<ParticleSystemRenderer>().materials = temp;
                                        theParticle.GetComponent<Renderer>().materials = temp;

                                        int shotsPerShotGunShot = 7; /* The shotgun has 7 bullets per round */
                                        for (int i = 0; i < shotsPerShotGunShot; i++) {

                                            eps = new Vector3(
                                            Random.Range(-maxBloom, maxBloom),
                                            Random.Range(-maxBloom, maxBloom),
                                            0
                                            );

                                            eps = cameraEncapsulator.transform.localRotation * eps;

                                            Vector3 initPos = shootingEpoch.transform.position;
                                            GameObject tinker = Instantiate(bulletObj, initPos, Quaternion.identity);


                                            BulletManager bullMan = tinker.GetComponent<BulletManager>();
                                            temp = bullMan.GetComponent<MeshRenderer>().materials;

                                            temp[0] = gunGlow.materials[2];
                                            bullMan.GetComponent<MeshRenderer>().materials = temp;
                                            bullMan.GetComponent<TrailRenderer>().materials = temp;
                                            bullMan.destroyTimer = 1f;
                                            bullMan.myType = BulletManager.TYPE.NORMAL;
                                            bullMan.destroyTimerStart = false;
                                            bullMan.team = 0;
                                            bullMan.spd = 100f;
                                            bullMan.myColor = ammoBarMan.shotGunColor;
                                            Vector3 Triangle = (((cameraEncapsulator.transform.forward).normalized * shootRange) + eps) - (shootingEpoch.transform.localPosition);
                                            bullMan.direction = Triangle;

                                            hitList = Physics.RaycastAll(cameraEncapsulator.transform.position, (cameraEncapsulator.transform.forward * shootRange + eps).normalized, shootRange);

                                            if (hitList.Length > 0) {
                                                bullMan.destroyTimerStart = true;
                                            }
                                            System.Array.Sort(hitList, (x, y) => x.distance.CompareTo(y.distance)); /* Sort to find the first thing we hit! */
                                            for (int j = 0; j < hitList.Length; j++) {
                                                hit = hitList[j];
                                                theParticle = Instantiate(bulletSparks, hit.point, Quaternion.identity);
                                                temp = theParticle.GetComponent<ParticleSystemRenderer>().materials;
                                                temp[0] = gunGlow.materials[2];
                                                theParticle.GetComponent<ParticleSystemRenderer>().materials = temp;
                                                theParticle.GetComponent<Renderer>().materials = temp;
                                                bullMan.destroyTimer = 0.1f;

                                                if (hit.collider != null) {
                                                    if (hit.collider.gameObject != null && hit.collider.gameObject.layer == 7) { /* This means we hit enemy */
                                                        hit.collider.SendMessage("Damage", sprayDmg);
                                                    }


                                                    break;
                                                }

                                            }

                                        }
                                        weaponMan.recoil = maxWeaponRecoil;
                                        weaponMan.recoilDir = new Vector3(0, 0, -1);
                                        shootCd = currMaxShootCd;
                                        currShotGunAmmo--;
                                        displayAmmo = currShotGunAmmo;


                                    }
                                    break;

                                case SHOTS.BOUNCE:
                                    if (shootCd == 0f && displayAmmo >= 1) {
                                        gunSource.GetComponent<AudioSource>().clip = bounceAudio[Random.Range(0, bounceAudio.Length)];
                                        gunSource.Play(0);
                                        GameObject theParticle = Instantiate(firingSparks, shootingEpoch.transform.position, cameraEncapsulator.transform.localRotation);

                                        temp = theParticle.GetComponent<ParticleSystemRenderer>().materials;
                                        temp[0] = gunGlow.materials[2];
                                        theParticle.GetComponent<ParticleSystemRenderer>().materials = temp;
                                        theParticle.GetComponent<Renderer>().materials = temp;

                                        weaponMan.recoil = maxWeaponRecoil;
                                        weaponMan.recoilDir = new Vector3(0, 0, -1);

                                        Vector3 initPos = shootingEpoch.transform.position - shootingEpoch.transform.forward;
                                        GameObject tinker = Instantiate(bulletObj, initPos, Quaternion.identity);



                                        BulletManager bullMan = tinker.GetComponent<BulletManager>();
                                        temp = bullMan.GetComponent<MeshRenderer>().materials;

                                        temp[0] = gunGlow.materials[2];
                                        bullMan.GetComponent<MeshRenderer>().materials = temp;
                                        bullMan.GetComponent<TrailRenderer>().materials = temp;
                                        bullMan.myType = BulletManager.TYPE.BOOM;
                                        bullMan.destroyTimerStart = false;
                                        bullMan.team = 0;
                                        bullMan.spd = 50f;
                                        bullMan.myColor = ammoBarMan.bounceColor;
                                        Vector3 Triangle = (camera.transform.position + camera.transform.forward * shootRange) - (shootingEpoch.transform.position);
                                        bullMan.direction = Triangle;

                                        shootCd = currMaxShootCd;


                                        currBounceAmmo--;
                                        displayAmmo = currBounceAmmo;

                                    }
                                    break;
                            }


                        }
                        break;

                }
                hitAlready.Clear();
            }


        }

    }

    void Update() {
        

        camera.fieldOfView = ManageScenes.currFov;

        if (!ManageScenes.paused) {
            if (footStepTimer == 0 && isGrounded && (new Vector2(rb.velocity.x, rb.velocity.z)).magnitude > 0.7f) {
                mySource.GetComponent<AudioSource>().clip = footSteps[Random.Range(0, footSteps.Length)];
                mySource.Play(0);
                footStepTimer = maxFootStepTimer;
            }
            footStepTimer = Mathf.Clamp(footStepTimer - Time.deltaTime, 0, footStepTimer);
            jumpCd = Mathf.Clamp(jumpCd - Time.deltaTime, 0, jumpCd);
            if (healCd == 0 && Input.GetKeyDown(KeyCode.C)) {
                headSource.GetComponent<AudioSource>().clip = healSound[Random.Range(0, healSound.Length)];
                headSource.Play(0);
                hpSetter.healTimer = 0.5f;
                healCd = maxHealCd;
                hp = Mathf.Clamp(hp + 50, 0, maxHp);
            }

            healCd = Mathf.Clamp(healCd - Time.deltaTime, 0, healCd);



            colliders = Physics.OverlapBox(rb.position + cameraEncapsulator.transform.forward, new Vector3(0.5f, 1, 0.5f), cameraEncapsulator.transform.localRotation, 7);
            inputAni.eActive = false;
            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i].gameObject != null && 
                    colliders[i].gameObject.GetComponent<Interactible>() != null && 
                    colliders[i].gameObject.GetComponent<Interactible>().canInteract) { /* This means the object is interactible */
                    inputAni.eActive = true;
                    if (Input.GetKeyDown(KeyCode.E)) { /* Display the E key on our gui then interact */
                        colliders[i].SendMessage("Interact");
                    }
                    break;
                }
            }

            MouseLook();


            xaxis = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
            zaxis = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);


            if (Input.GetKeyDown(KeyCode.F)) {
                if (hand == HAND.MEELEE && canSwapToGun) {
                    hand = HAND.GUN;
                } else {
                    hand = HAND.MEELEE;
                }
            }
            dashCd = Mathf.Clamp(dashCd - Time.deltaTime, 0, dashCd);
            shootCd = Mathf.Clamp(shootCd - Time.deltaTime, 0, shootCd);



            switch (hand)
            {
                
                case HAND.MEELEE:
                    /* Meelee has no bloom no recoil no shake */
                    maxFootStepTimer = 0.2f;
                    displayAmmo = currBlock;
                    displayMaxAmmo = maxBlock;

                    if (currBlock >= 1 && weaponMan.aniNumber == -1 && Input.GetMouseButton(1)) {
                        blocking = true;
                        weaponMan.aniNumber = 2;
                        weaponMan.currAniIndex = 0;
                    }

                    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.F) || !Input.GetMouseButton(1) || currBlock < 1) {
                        blocking = false;
                        if (weaponMan.aniNumber == 2) {

                            weaponMan.currAniIndex = 2;
                        }
                    }

                    if (!blocking && currBlock < maxBlock) {
                        currBlock = Mathf.Clamp(currBlock + blockRefillTime * Time.deltaTime, 0, maxBlock);
                    }

                    reloading = false;
                    spd = fastSpd;
                    camera.transform.localPosition = cameraInitPos;
                    currMaxShootCd = meeleeCd;
                    blooming = false;
                    recoiling = false;
                    bloom = Mathf.Clamp(bloom - maxBloom * Time.deltaTime, 0, maxBloom);
                    vertRecoil = Mathf.Clamp(vertRecoil - maxVertRecoil * Time.deltaTime, 0, maxVertRecoil);
                    if (horRecoil < 0) {
                        horRecoil = Mathf.Clamp(horRecoil + maxHorRecoil * Time.deltaTime, -maxHorRecoil, 0);
                    } else {
                        horRecoil = Mathf.Clamp(horRecoil - maxHorRecoil * Time.deltaTime, 0, maxHorRecoil);
                    }
                    break;

                case HAND.GUN:
                    maxFootStepTimer = 0.5f;
                    /* While holding gun right click to cycle through the gun modes */
                    if (Input.GetMouseButtonDown(1)) {
                        /* Swaps gun ammo type */
                        if (ammoType == SHOTS.BOUNCE) {
                            ammoType = SHOTS.SPRAY;
                        } else {
                            ammoType++;
                        }
                    }

                    /* While holding gun press 1 for spray, 2 for shotgun, 3 for bounce */
                    if (Input.GetKeyDown(KeyCode.Alpha1)) {
                        ammoType = SHOTS.SPRAY;
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha2)) {
                        ammoType = SHOTS.SHOTGUN;
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha3)) {
                        ammoType = SHOTS.BOUNCE;
                    }

                    spd = slowSpd;
                    if (Input.GetKeyDown(KeyCode.R) && displayAmmo != displayMaxAmmo) {
                        gunSource.GetComponent<AudioSource>().clip = reload[Random.Range(0, reload.Length)];
                        gunSource.Play(0);
                        reloading = true;
                    }
                    if ((Input.GetMouseButton(0) && displayAmmo >= 1) ||
                         Input.GetMouseButton(1) ||
                         Input.GetKeyDown(KeyCode.Alpha1) ||
                         Input.GetKeyDown(KeyCode.Alpha2) ||
                         Input.GetKeyDown(KeyCode.Alpha3) ||
                         displayAmmo == displayMaxAmmo) { /* Stop reloading if doing another action occurs */
                        reloading = false;
                    }


                    switch (ammoType) { /* This switch statement initizalies the different stats, bloom, firerates and recoil for each gun type */
                        case SHOTS.SPRAY:
                            if (!canReloadSpray) {
                                reloading = false;
                            }
                            temp = gunGlow.materials;
                            temp[2] = sprayGlow;
                            temp[3] = sprayGlow;
                            gunGlow.materials = temp;
                            currMaxShootCd = 0.15f;
                            maxBloom = 0.005f;
                            maxShake = 0.1f;
                            maxWeaponRecoil = 0.1f;
                            displayAmmo = currSprayAmmo;
                            displayMaxAmmo = maxSprayAmmo;
                            maxVertRecoil = 4f;
                            maxHorRecoil = 2f;
                            currReloadTime = 1f;
                            if (Input.GetMouseButton(0) && displayAmmo >= 1) { 
                                /* Shake the camera to simulate the vibrations of gun */
                                camera.transform.localPosition = cameraInitPos + new Vector3(0, cameraInitPos.y + Random.Range(0, maxShake), 0);
                            } else {
                                camera.transform.localPosition = cameraInitPos;
                            }

                            if (reloading) {
                                currSprayAmmo = Mathf.Clamp(currSprayAmmo + maxSprayAmmo * (Time.deltaTime / currReloadTime), 0, maxSprayAmmo);
                            }
                            break;

                        case SHOTS.SHOTGUN:
                            if (!canReloadShotGun) {
                                reloading = false;
                            }
                            temp = gunGlow.materials;
                            temp[2] = shotGunGlow;
                            temp[3] = shotGunGlow;
                            gunGlow.materials = temp;
                            currMaxShootCd = 0.5f;
                            maxBloom = 4;
                            maxShake = 0.5f;
                            maxWeaponRecoil = 0.5f;
                            displayAmmo = currShotGunAmmo;
                            displayMaxAmmo = maxShotGunAmmo;
                            maxVertRecoil = 0f;
                            maxHorRecoil = 0f;
                            currReloadTime = 2f;
                            if (Input.GetMouseButton(0) && displayAmmo >= 1 && shootCd == 0) {
                                /* Shake the camera to simulate the vibrations of gun */
                                camera.transform.localPosition = cameraInitPos + new Vector3(0, cameraInitPos.y + Random.Range(maxShake / 2, maxShake), 0);
                            } else {
                                camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, cameraInitPos, 4 * Time.deltaTime);
                            }
                            if (reloading) {
                                currShotGunAmmo = Mathf.Clamp(currShotGunAmmo + maxShotGunAmmo * (Time.deltaTime / currReloadTime), 0, maxShotGunAmmo);
                            }
                            break;

                        case SHOTS.BOUNCE:
                            temp = gunGlow.materials;
                            temp[2] = bounceGlow;
                            temp[3] = bounceGlow;
                            gunGlow.materials = temp;
                            currMaxShootCd = 0.5f;
                            maxBloom = 5f;
                            maxShake = 0.5f;
                            maxWeaponRecoil = 0.7f;
                            displayAmmo = currBounceAmmo;
                            displayMaxAmmo = maxBounceAmmo;
                            maxVertRecoil = 0f;
                            maxHorRecoil = 0f;
                            currReloadTime = 0.5f;
                            if (Input.GetMouseButton(0) && displayAmmo >= 1 && shootCd == 0) {
                                /* Shake the camera to simulate the vibrations of gun */
                                camera.transform.localPosition = cameraInitPos + new Vector3(0, cameraInitPos.y + Random.Range(maxShake / 2, maxShake), 0);
                            } else {
                                camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, cameraInitPos, 4 * Time.deltaTime);
                            }

                            break;
                    }


                    if (Input.GetMouseButtonDown(0) && displayAmmo >= 1) {
                        blooming = true;
                        recoiling = true;
                        alternator = 1;
                    } else if (Input.GetMouseButtonUp(0) || displayAmmo < 1) {
                        blooming = false;
                        recoiling = false;
                    }

                    if (displayAmmo < 1) {
                        temp = gunGlow.materials;
                        temp[3] = noMat;
                        gunGlow.materials = temp;
                    }
                    break;


            }

            if (blooming) {
                bloom = Mathf.Lerp(bloom, maxBloom, 1.5f * Time.deltaTime);
            } else {
                bloom = Mathf.Clamp(bloom - maxBloom * Time.deltaTime, 0, maxBloom);
            }
            if (recoiling) {
                vertRecoil = Mathf.Clamp(vertRecoil + maxVertRecoil * Time.deltaTime, 0, maxVertRecoil);
                if (vertRecoil >= maxVertRecoil) {
                    horRecoil = Mathf.Clamp(horRecoil + 2 * alternator * maxHorRecoil * Time.deltaTime, -maxHorRecoil, maxHorRecoil);
                    if (Mathf.Abs(horRecoil) >= maxHorRecoil) {
                        alternator *= -1;
                    }
                }

            } else {
                vertRecoil = Mathf.Clamp(vertRecoil - 2 * maxVertRecoil * Time.deltaTime, 0, maxVertRecoil);
                if (horRecoil < 0) {
                    horRecoil = Mathf.Clamp(horRecoil + 2 * maxHorRecoil * Time.deltaTime, -maxHorRecoil, 0);
                } else {
                    horRecoil = Mathf.Clamp(horRecoil - 2 * maxHorRecoil * Time.deltaTime, 0, maxHorRecoil);
                }
            }


            switch (state) {
                case STATES.MOVE:
                    if (Input.GetKey(KeyCode.Space)) {
                        /* We want you to be able to buffer jump so the time is more leniant this is for better gameplay and design */
                        jumpBuffer = maxJumpBuffer;
                    }
                    if (jumpBuffer > 0) {
                        jumpBuffer = Mathf.Clamp(jumpBuffer - Time.deltaTime, 0, maxJumpBuffer);
                    }
                    if (jumpBuffer > 0 && isGrounded && jumpCd == 0) {
                        jumpCd = maxJumpCd;
                        mySource.GetComponent<AudioSource>().clip = jump[Random.Range(0, jump.Length)];
                        mySource.Play(0);

                        isGrounded = false;
                        rb.position += new Vector3(0, 0.1f, 0); /* Epsilon */
                        rb.useGravity = false;
                        currJumpBoom = maxJumpBoom;
                        StartCoroutine(Jump());

                    }

                    if (Input.GetKeyDown(KeyCode.LeftShift)) {
                        dashBuffer = maxDashBuffer;
                    }
                    if (dashBuffer > 0) {
                        dashBuffer = Mathf.Clamp(dashBuffer - Time.deltaTime, 0, maxDashBuffer);
                    }

                    if (dashesLeft > 0 && dashBuffer > 0 && dashCd == 0 && (weaponMan.totalAniTime >= 0.3f || weaponMan.aniNumber == -1 || weaponMan.aniNumber == 2)) {
                        dashesLeft--;
                        bladeSource.GetComponent<AudioSource>().clip = dashAudio[Random.Range(0, dashAudio.Length)];
                        bladeSource.Play(0);
                        dashCd = maxDashCd;
                        state = STATES.DASH;
                        weaponMan.aniNumber = 1;
                        weaponMan.currAniIndex = 0;
                        weaponMan.currBladeAnimationPos = new Vector3(0, 0, 0);
                        weaponMan.currBladeAnimationRot = Quaternion.identity;
                        weaponMan.timeTaken = 0f;
                        weaponMan.dashAniTimer = 0.3f;
                        weaponMan.isQuick = true;
                        weaponMan.totalAniTime = 0f;

                        StopCoroutine(Jump());

                        currJumpBoom = 0;
                        rb.useGravity = false;
                        dashDir = camera.transform.forward;
                        timeInDash = 0;
                    }
                    break;

                case STATES.DASH:
                    blocking = false;
                    blooming = false;
                    recoiling = false;
                    weaponMan.bob = false;
                    break;

                case STATES.BOUNCE:
                    if (Input.GetKeyDown(KeyCode.LeftShift)) {
                        dashBuffer = maxDashBuffer;
                    }
                    if (dashBuffer > 0) {
                        dashBuffer = Mathf.Clamp(dashBuffer - Time.deltaTime, 0, maxDashBuffer);
                    }

                    /* We dont want to dash while in a sword swing animation: */
                    if (dashesLeft > 0 && dashBuffer > 0 && dashCd == 0 && (weaponMan.totalAniTime >= 0.3f || weaponMan.aniNumber == -1 || weaponMan.aniNumber == 2)) {
                        dashesLeft--;
                        bladeSource.GetComponent<AudioSource>().clip = dashAudio[Random.Range(0, dashAudio.Length)];
                        bladeSource.Play(0);
                        dashCd = maxDashCd;
                        state = STATES.DASH;
                        weaponMan.aniNumber = 1;
                        weaponMan.currAniIndex = 0;
                        weaponMan.currBladeAnimationPos = new Vector3(0, 0, 0);
                        weaponMan.currBladeAnimationRot = Quaternion.identity;
                        weaponMan.timeTaken = 0f;
                        weaponMan.dashAniTimer = 0.3f;
                        weaponMan.isQuick = true;
                        weaponMan.totalAniTime = 0f;

                        StopCoroutine(Jump());

                        currJumpBoom = 0;
                        rb.useGravity = false;
                        dashDir = camera.transform.forward;
                        timeInDash = 0;
                    }
                    break;
            }
        }
        if (isGrounded) {
            dashesLeft = maxDashesLeft;
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(Physics.gravity, ForceMode.Acceleration);

        colliders = Physics.OverlapBox(rb.position - new Vector3(0, 1, 0), new Vector3(0.25f, 0.01f, 0.25f), Quaternion.identity, 7);
        isGrounded = false;
        
        for (int i = 0; i < colliders.Length; i++) {
            if (colliders[i].gameObject.tag == "Wall" || colliders[i].gameObject.tag == "BounceRecharge" || colliders[i].gameObject.tag == "DashReset") {
                isGrounded = true;
            }
            if (colliders[i].gameObject.GetComponent<Collider>().tag == "BounceRecharge") {
                currBounceAmmo = Mathf.Clamp(currBounceAmmo + maxBounceAmmo * (Time.deltaTime / currReloadTime), 0, maxBounceAmmo);
            }
        }


        switch (state)
        {
            case STATES.MOVE:


                Move();

                break;

            case STATES.DASH:

                /* Check if hit enemy */
                colliders = Physics.OverlapBox((transform.position + cameraEncapsulator.transform.forward.normalized), new Vector3(1, 0.5f, meeleeRange), cameraEncapsulator.transform.localRotation);
                for (int i = 0; i < colliders.Length; i++) {
                    if (colliders[i].tag == "Spawner") {
                        if (!hitAlready.Contains(colliders[i].gameObject.GetInstanceID())) {
                            colliders[i].SendMessage("Damage", meeleeDmg);
                            hitAlready.Add(colliders[i].gameObject.GetInstanceID());
                        }
                    } else if (
                        (colliders[i].tag != "Enemy" && colliders[i].tag != "Boss" &&
                        colliders[i].gameObject.layer == 7 &&
                        !hitAlready.Contains(colliders[i].gameObject.GetComponent<EnemyHitBox>().him.GetInstanceID()))) {
                        colliders[i].SendMessage("Damage", -meeleeDmg); /* Negative means we ignore what body part we hit theres a check for this after its fed in */
                        hitAlready.Add(colliders[i].gameObject.GetComponent<EnemyHitBox>().him.GetInstanceID());
                    }
                }

                /* Check if hit wall with smaller raycast */
                if (Physics.Raycast(rb.position, transform.TransformDirection(Vector3.forward), out hit, 1.01f) || timeInDash >= maxTimeInDash)
                {
                    if (hit.collider != null) {
                        dashBuffer = 0;
                        if (hit.collider.gameObject.layer == 7) { /* hit an enemy dash through */
                            rb.velocity = dashDir * dashSpd;
                            timeInDash += Time.deltaTime;
                        } else { /* hit a wall stop */
                            hitAlready.Clear();
                            rb.useGravity = true;
                            rb.velocity = new Vector3(0, 0, 0);
                            weaponMan.isQuick = false;
                            nextDashStrong = onPrevDash;
                            onPrevDash = false;
                            state = STATES.MOVE;
                        }
                    } else { /* Assume its a wall here for some reason mesh .colliders return null */
                        hitAlready.Clear();
                        rb.useGravity = true;
                        rb.velocity = new Vector3(0, 0, 0);
                        weaponMan.isQuick = false;
                        nextDashStrong = onPrevDash;
                        onPrevDash = false;
                        state = STATES.MOVE;
                    }
                } else {
                    if (nextDashStrong && !onPrevDash) { /* Double the speed if we hit a dash reset */
                        rb.velocity = 2 * (new Vector3((dashDir * dashSpd).x, (dashDir * dashSpd).y, (dashDir * dashSpd).z));
                    } else {
                        rb.velocity = new Vector3((dashDir * dashSpd).x, (dashDir * dashSpd).y / 2, (dashDir * dashSpd).z);
                    }
                    timeInDash += Time.deltaTime;
                }


                break;

            case STATES.BOUNCE:
                Move();
                if (rb.velocity.y <= 0) {
                    state = STATES.MOVE;
                }
                break;
        }
        
    }

    /* Handles actual movement for player */
    void Move()
    {
        Vector3 movement = new Vector3(xaxis, 0, zaxis).normalized;
        if ((xaxis != 0 || zaxis != 0) && isGrounded) {
            weaponMan.bob = true;
        } else {
            weaponMan.bob = false;
        }
        movement = transform.forward * movement.z + transform.right * movement.x;

        movement = movement.normalized * spd;

        hitList = Physics.RaycastAll(transform.position, movement.normalized, 1, 7);
        if (hitList.Length > 0) {
            movement = new Vector3(0, 0, 0);
        }

        /* We allow the player to reset their dash after touching a blue pad 
           this falls under the grading category of good gameplay */
        for (int i = 0; i < hitList.Length; i++) {
            if (hitList[i].collider != null && hitList[i].collider.tag == "DashReset") {
                DashResetSrc dashR = hitList[i].collider.GetComponent<DashResetSrc>();
                if (!dashR.gone) {
                    dashR.gone = true;
                    nextDashStrong = true;
                    onPrevDash = (state == STATES.DASH);
                    hitList[i].collider.SendMessage("Blip");
   
                    dashCd = 0;
                    dashesLeft = maxDashesLeft;
                    dashR.timer = 1f;
                    dashBuffer = 0f;
                }
            }
        }

        /* We are using rigid body velocity that was talked about in lecture to move the player 
           this allows us to leverage Unitys built in collision system so the player doesnt run through walls */

        if (state == STATES.BOUNCE) {
            movement = movement / 3;
            rb.velocity = Vector3.Lerp(rb.velocity, rb.velocity + new Vector3(movement.x, 0, movement.z), 8 * Time.deltaTime);
        } else {
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(movement.x, rb.velocity.y, movement.z), 20 * Time.deltaTime);
        }

    }

    /* We implment a common first person perspective for our platformer and shooter 
       which falls under the Concept category for grading */
    void MouseLook() {
        /* Has delta time "built in" since it takes more time to move your mouse farther */

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        mouseX *= sensitivity;
        mouseY *= sensitivity;
        weaponMan.sway = Mathf.Clamp(mouseX, -maxSway, maxSway);

        // Vertical rotation for the camera only
        cameraVerticalRotation -= mouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90, 90);

        cameraHorizontalRotation += mouseX;
        if (cameraHorizontalRotation >= 360) {
            cameraHorizontalRotation = 0;
        }

        // Apply rotation to the camera
        cameraEncapsulator.transform.localRotation = Quaternion.Euler(cameraVerticalRotation - vertRecoil, cameraHorizontalRotation + horRecoil, 0);
        transform.localRotation = Quaternion.Euler(0, cameraHorizontalRotation, 0);
    }

    /* We have a coroutine to handle jumping 
       a jump should last until completion or 
       until we either hit the ground or the cieling 
       and yield break out */
    IEnumerator Jump()
    {
        float timeInJump = 0;
        float maxTimeInJump = 0.5f;
        while (timeInJump < maxTimeInJump) {
            weaponMan.isQuick = true;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit, 1f) || isGrounded) {
                weaponMan.isQuick = false;
                rb.useGravity = true;
                yield break;
            }
            currJumpBoom = Mathf.Lerp(currJumpBoom, 0, 2f * Time.deltaTime);
            rb.position += (new Vector3(0, currJumpBoom, 0)) * Time.deltaTime;
            timeInJump += Time.deltaTime;
            yield return null;
        }
        weaponMan.isQuick = false;
        rb.useGravity = true;
    }

    void DeathSound() {
        headSource.GetComponent<AudioSource>().clip = deathSound[Random.Range(0, deathSound.Length)];
        headSource.Play(0);
    }

    /* Crippling Damage cannot be fully blocked with the sword */
    void CripplingDamage(float dmg) {
        if (blocking && currBlock >= 1) { /* Blocking negates some damage */ 
            currBlock--;
            headSource.GetComponent<AudioSource>().clip = block[Random.Range(0, block.Length)];
            headSource.Play(0);
            hpSetter.hurtTimer = 0.1f;
            hp = Mathf.Clamp(hp - dmg/2, 0, maxHp);
        } else if (state != STATES.DASH) { /* Dashing negates damage */ 
            hpSetter.hurtTimer = 0.1f;
            mySource.GetComponent<AudioSource>().clip = hurt[Random.Range(0, hurt.Length)];
            mySource.Play(0);
            hp = Mathf.Clamp(hp - dmg, 0, maxHp);
        } else {
            headSource.GetComponent<AudioSource>().clip = block[Random.Range(0, block.Length)];
            headSource.Play(0);
        }
    }

    void Damage(float dmg) {


        if (blocking && currBlock >= 1) { /* Blocking negates damage */ 
            currBlock--;
            headSource.GetComponent<AudioSource>().clip = block[Random.Range(0, block.Length)];
            headSource.Play(0);
            if (SceneManager.GetActiveScene().name == "Tutorial" && tut.currPhase == Tutorial.TUTORIALPHASES.BLOCKING) {
                tut.blocked++;
            }
        } else if (state != STATES.DASH) { /* Dashing negates damage */ 
            hpSetter.hurtTimer = 0.1f;
            headSource.GetComponent<AudioSource>().clip = hurt[Random.Range(0, hurt.Length)];
            headSource.Play(0);
            hp = Mathf.Clamp(hp - dmg, 0, maxHp);
        } else {
            headSource.GetComponent<AudioSource>().clip = block[Random.Range(0, block.Length)];
            headSource.Play(0);
        }



    }

    /* Fall damage will hit NO MATTER WHAT when falling out of bounds */
    void FallDamage(float dmg) {
        hpSetter.hurtTimer = 0.1f;
        headSource.GetComponent<AudioSource>().clip = hurt[Random.Range(0, hurt.Length)];
        headSource.Play(0);
        hp = Mathf.Clamp(hp - dmg, 0, maxHp);
    }

    /* Called when we use our bouncy gun */
    void Bounce(Vector3 dir) {
        state = STATES.BOUNCE;
        rb.velocity = dir.normalized * maxBounce;
    }

    /* Check if we hit a blue dash reset platform */
    void OnCollisionEnter(Collision c) {

        if (c.collider.tag == "DashReset") {
            DashResetSrc dashR = c.collider.GetComponent<DashResetSrc>();
            if (!dashR.gone) {
                dashR.gone = true;
                nextDashStrong = true;
                onPrevDash = (state == STATES.DASH);
                dashCd = 0;
                dashesLeft = maxDashesLeft;
                c.collider.SendMessage("Blip");
                dashR.timer = 1f;
                dashBuffer = 0f;
            }
        } else if (c.collider.tag == "BossLevelComponent") {
            if (c.collider.name == "RaiseLava") {
                c.collider.SendMessage("Raise");
            }
        }
    }
}
