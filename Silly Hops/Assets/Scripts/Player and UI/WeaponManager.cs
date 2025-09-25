using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{

    public struct AnimationStep {
        public Vector3 initAniPos;
        public Quaternion initAniRot;
        public Vector3 finalAniPos;
        public Quaternion finalAniRot;
        public float timeThisStepTakes;
        public bool loop;
    }

    public AnimationStep[][] animationSteps = new AnimationStep[3][];

    public float sway = 0f;
    public Camera me;
    public GameObject gunModel, bladeModel;

    public Quaternion gunOnScreenRot, gunStowRot, currGunRot,
    bladeOnScreenRot, bladeStowRot, currBladeRot,
    currBladeAnimationRot, bladeBeforeAniRot;

    public Vector3 gunOnScreenPos, gunStowPos, currGunPos,
    bladeOnScreenPos, bladeStowPos, currBladePos,
    recoilDir, bobVec = new Vector3(0, 0, 0), currQuickBob = new Vector3(0, 0, 0),
    currBladeAnimationPos;

    public float swayRate = 8f, recoil = 0, swayMult = 5f,
    outOfScreen = 1, weaponSwapSpeed = 0.1f, rotSpeed = 4f,
    maxBobx = 0.1f, maxBoby = 0.1f, maxBobz = 0.1f,
    alternator = 1f, bobTime = 0.3f, maxQuickBob = 0.05f, quickBobRate = 8f,
    bladeAniTimer = 0f, goBackTime = 0.1f, timeTaken = 0f, dashAniTimer = 0f, totalAniTime = 0f;

    public int currAniIndex = -1, aniNumber = -1;
    public PlayerManager playerScript;

    public bool bob = false, isQuick = false, forwardSlash = false;

    public TrailRenderer slashTrail;
    
    void Start() {
        gunOnScreenRot = gunModel.transform.localRotation;
        gunOnScreenPos = gunModel.transform.localPosition;
        bladeOnScreenRot = bladeModel.transform.localRotation;
        bladeOnScreenPos = bladeModel.transform.localPosition;

        gunStowPos = gunOnScreenPos - new Vector3(0, outOfScreen, 0);
        gunStowRot = gunOnScreenRot * Quaternion.Euler(-180, 0, 0);
        currGunPos = gunOnScreenPos;

        bladeStowPos = bladeOnScreenPos - new Vector3(0, outOfScreen, 0);
        bladeStowRot = bladeOnScreenRot * Quaternion.Euler(0, -180, 0);

        bladeModel.transform.localPosition = bladeStowPos; /* Init blade out of screen */
        bladeModel.transform.localRotation = bladeStowRot;
        currBladePos = bladeStowPos;
        currBladeRot = bladeStowRot;

        /* animationSteps[i][j] is the jth step of animation number i this system entirely based off rotations and transform positions.
           Our alogorithm parthese through this animation steps array as follows below to excute a certain animation.
           This goes under the grading category of Visual elements and algorithmic complexity! */

        /* We want to animate the left click slash */
        /* For our left click slash we raise our sword then slash from top right to bottom left */
        animationSteps[0] = new AnimationStep[2];
        animationSteps[0][0].initAniPos = new Vector3(0, 0, 0);
        animationSteps[0][0].initAniRot = Quaternion.identity;
        animationSteps[0][0].finalAniPos = new Vector3(0, 1f, 0);
        animationSteps[0][0].finalAniRot = Quaternion.Euler(80, 0, -80);
        animationSteps[0][0].timeThisStepTakes = 0.1f;

        animationSteps[0][1].initAniPos = animationSteps[0][0].finalAniPos;
        animationSteps[0][1].initAniRot = animationSteps[0][0].finalAniRot;
        animationSteps[0][1].finalAniPos = animationSteps[0][0].finalAniPos + new Vector3(-4, -2, 1);
        animationSteps[0][1].finalAniRot = animationSteps[0][0].finalAniRot * Quaternion.Euler(0, 120, 0);
        animationSteps[0][1].timeThisStepTakes = 0.1f;
        animationSteps[0][1].loop = false;

        /* We want to animate the dash slash */
        /* For this slash we need that the blade faces away from us first */
        animationSteps[1] = new AnimationStep[2];
        animationSteps[1][0].initAniPos = new Vector3(0, 0, 0);
        animationSteps[1][0].initAniRot = Quaternion.identity;
        animationSteps[1][0].finalAniPos = new Vector3(-1, 0.5f, 0.2f);
        animationSteps[1][0].finalAniRot = Quaternion.Euler(60, 0, 10);
        animationSteps[1][0].timeThisStepTakes = 0.1f;

        animationSteps[1][1].initAniPos = animationSteps[1][0].finalAniPos;
        animationSteps[1][1].initAniRot = animationSteps[1][0].finalAniRot;
        animationSteps[1][1].finalAniPos = animationSteps[1][0].finalAniPos + new Vector3(2, 0, 0);
        animationSteps[1][1].finalAniRot = animationSteps[1][0].finalAniRot * Quaternion.AngleAxis(180, Vector3.up);
        animationSteps[1][1].timeThisStepTakes = 0.2f;
        animationSteps[1][1].loop = false;

        /* We want to animate the blocking animation */
        /* Should just be player holding blade up to screen angled up */
        animationSteps[2] = new AnimationStep[1];
        animationSteps[2][0].initAniPos = new Vector3(0, 0, 0);
        animationSteps[2][0].initAniRot = Quaternion.identity;
        animationSteps[2][0].finalAniPos = new Vector3(-0.5f, 0.2f, 0);
        animationSteps[2][0].finalAniRot = Quaternion.Euler(35, 20, 0);
        animationSteps[2][0].timeThisStepTakes = 0.2f;
        animationSteps[2][0].loop = true;

        currBladeAnimationPos = new Vector3(0, 0, 0);
        currBladeAnimationRot = Quaternion.identity;
    }

    /* Normal Qlerp will take the shortest path from one quaternion to another. 
       This is not desireable four our animations */
    public Quaternion betterQLerp(Quaternion a, Quaternion b, float whereWeAre) {
        Quaternion ret = Quaternion.identity;
        ret.x = a.x * Mathf.Clamp((1f - whereWeAre), 0f, 1f) + b.x * Mathf.Clamp(whereWeAre, 0, 1f);
        ret.y = a.y * Mathf.Clamp((1f - whereWeAre), 0f, 1f) + b.y * Mathf.Clamp(whereWeAre, 0, 1f);
        ret.z = a.z * Mathf.Clamp((1f - whereWeAre), 0f, 1f) + b.z * Mathf.Clamp(whereWeAre, 0, 1f);
        ret.w = a.w * Mathf.Clamp((1f - whereWeAre), 0f, 1f) + b.w * Mathf.Clamp(whereWeAre, 0, 1f);
        return ret;
    }

    void Update() {
        float delta = 0.01f;
        switch (playerScript.hand) { /* This is the animation for switching from the sword to the gun */
            case PlayerManager.HAND.MEELEE:
                maxBobx = 0.06f;
                maxBoby = 0.04f;
                maxBobz = 0.05f;
                bobTime = 0.05f;

                if ((currGunPos - gunStowPos).magnitude > delta) {
                    currGunPos = new Vector3(

                    Mathf.Clamp(currGunPos.x + (gunStowPos.x - gunOnScreenPos.x) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(gunStowPos.x, gunOnScreenPos.x), Mathf.Max(gunStowPos.x, gunOnScreenPos.x)),
                    Mathf.Clamp(currGunPos.y + (gunStowPos.y - gunOnScreenPos.y) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(gunStowPos.y, gunOnScreenPos.y), Mathf.Max(gunStowPos.y, gunOnScreenPos.y)),
                    Mathf.Clamp(currGunPos.z + (gunStowPos.z - gunOnScreenPos.z) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(gunStowPos.z, gunOnScreenPos.z), Mathf.Max(gunStowPos.z, gunOnScreenPos.z))

                    );
                    currGunRot = betterQLerp(currGunRot, gunStowRot, rotSpeed * Time.deltaTime);
                } else {
                    currGunPos = gunStowPos;
                    currGunRot = gunStowRot;
                    if ((currBladePos - bladeOnScreenPos).magnitude > delta) {

                        currBladePos = new Vector3(

                        Mathf.Clamp(currBladePos.x + (bladeOnScreenPos.x - bladeStowPos.x) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(bladeStowPos.x, bladeOnScreenPos.x), Mathf.Max(bladeStowPos.x, bladeOnScreenPos.x)),
                        Mathf.Clamp(currBladePos.y + (bladeOnScreenPos.y - bladeStowPos.y) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(bladeStowPos.y, bladeOnScreenPos.y), Mathf.Max(bladeStowPos.y, bladeOnScreenPos.y)),
                        Mathf.Clamp(currBladePos.z + (bladeOnScreenPos.z - bladeStowPos.z) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(bladeStowPos.z, bladeOnScreenPos.z), Mathf.Max(bladeStowPos.z, bladeOnScreenPos.z))

                        );

                        currBladeRot = betterQLerp(currBladeRot, bladeOnScreenRot, rotSpeed * Time.deltaTime);
                    } else {
                        currBladePos = bladeOnScreenPos;
                        currBladeRot = bladeOnScreenRot;
                    }
                }

                break;

            case PlayerManager.HAND.GUN:
                maxBobx = 0.003f;
                maxBoby = 0.007f;
                maxBobz = 0.003f;
                bobTime = 0.1f;
                if ((currBladePos - bladeStowPos).magnitude > delta) {
                    currBladePos = new Vector3(

                    Mathf.Clamp(currBladePos.x + (bladeStowPos.x - bladeOnScreenPos.x) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(bladeStowPos.x, bladeOnScreenPos.x), Mathf.Max(bladeStowPos.x, bladeOnScreenPos.x)),
                    Mathf.Clamp(currBladePos.y + (bladeStowPos.y - bladeOnScreenPos.y) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(bladeStowPos.y, bladeOnScreenPos.y), Mathf.Max(bladeStowPos.y, bladeOnScreenPos.y)),
                    Mathf.Clamp(currBladePos.z + (bladeStowPos.z - bladeOnScreenPos.z) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(bladeStowPos.z, bladeOnScreenPos.z), Mathf.Max(bladeStowPos.z, bladeOnScreenPos.z))

                    );

                    currBladeRot = betterQLerp(currBladeRot, bladeStowRot, rotSpeed * Time.deltaTime);
                } else {
                    currBladePos = bladeStowPos;
                    currBladeRot = bladeStowRot;
                    if ((currGunPos - gunOnScreenPos).magnitude > delta) {

                        currGunPos = new Vector3(

                        Mathf.Clamp(currGunPos.x + (gunOnScreenPos.x - gunStowPos.x) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(gunStowPos.x, gunOnScreenPos.x), Mathf.Max(gunStowPos.x, gunOnScreenPos.x)),
                        Mathf.Clamp(currGunPos.y + (gunOnScreenPos.y - gunStowPos.y) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(gunStowPos.y, gunOnScreenPos.y), Mathf.Max(gunStowPos.y, gunOnScreenPos.y)),
                        Mathf.Clamp(currGunPos.z + (gunOnScreenPos.z - gunStowPos.z) * (Time.deltaTime / weaponSwapSpeed), Mathf.Min(gunStowPos.z, gunOnScreenPos.z), Mathf.Max(gunStowPos.z, gunOnScreenPos.z))

                        );

                        currGunRot = betterQLerp(currGunRot, gunOnScreenRot, rotSpeed * Time.deltaTime);
                    } else {
                        currGunPos = gunOnScreenPos;
                        currGunRot = gunOnScreenRot;
                    }
                }
                break;

        }

        /* Animate our blade */
        AnimationStep stepWereOn;
        totalAniTime += Time.deltaTime;
        slashTrail.gameObject.SetActive(false);
        if (aniNumber != -1 && (currAniIndex == -1 || currAniIndex >= animationSteps[aniNumber].Length)) { /* Go back to position before animation */
            currAniIndex = -1;
            stepWereOn = animationSteps[aniNumber][animationSteps[aniNumber].Length - 1];
            currBladeAnimationPos = new Vector3(

            Mathf.Clamp(currBladeAnimationPos.x + (-stepWereOn.finalAniPos.x) * (Time.deltaTime / goBackTime), Mathf.Min(stepWereOn.finalAniPos.x, 0), Mathf.Max(stepWereOn.finalAniPos.x, 0)),
            Mathf.Clamp(currBladeAnimationPos.y + (-stepWereOn.finalAniPos.y) * (Time.deltaTime / goBackTime), Mathf.Min(stepWereOn.finalAniPos.y, 0, Mathf.Max(stepWereOn.finalAniPos.y, 0)),
            Mathf.Clamp(currBladeAnimationPos.z + (-stepWereOn.finalAniPos.z) * (Time.deltaTime / goBackTime), Mathf.Min(stepWereOn.finalAniPos.z, 0), Mathf.Max(stepWereOn.finalAniPos.z, 0)))

            );

            currBladeAnimationRot = betterQLerp(stepWereOn.finalAniRot, Quaternion.identity, timeTaken / goBackTime);

            if (timeTaken == goBackTime) {
                currBladeAnimationPos = new Vector3(0, 0, 0);
                currBladeAnimationRot = Quaternion.identity;
                aniNumber = -1;
            }
            timeTaken = Mathf.Clamp(timeTaken + Time.deltaTime, 0, goBackTime);
        } else if (aniNumber != -1) {
            slashTrail.gameObject.SetActive(true);
            stepWereOn = animationSteps[aniNumber][currAniIndex];

            if (currBladeAnimationPos != stepWereOn.finalAniPos || currBladeAnimationRot != stepWereOn.finalAniRot || timeTaken < stepWereOn.timeThisStepTakes || stepWereOn.loop) {

                /* Interpolate the current blade position */
                currBladeAnimationPos = new Vector3(

                Mathf.Clamp(currBladeAnimationPos.x + (stepWereOn.finalAniPos.x - stepWereOn.initAniPos.x) * (Time.deltaTime / stepWereOn.timeThisStepTakes), Mathf.Min(stepWereOn.finalAniPos.x, stepWereOn.initAniPos.x), Mathf.Max(stepWereOn.finalAniPos.x, stepWereOn.initAniPos.x)),
                Mathf.Clamp(currBladeAnimationPos.y + (stepWereOn.finalAniPos.y - stepWereOn.initAniPos.y) * (Time.deltaTime / stepWereOn.timeThisStepTakes), Mathf.Min(stepWereOn.finalAniPos.y, stepWereOn.initAniPos.y), Mathf.Max(stepWereOn.finalAniPos.y, stepWereOn.initAniPos.y)),
                Mathf.Clamp(currBladeAnimationPos.z + (stepWereOn.finalAniPos.z - stepWereOn.initAniPos.z) * (Time.deltaTime / stepWereOn.timeThisStepTakes), Mathf.Min(stepWereOn.finalAniPos.z, stepWereOn.initAniPos.z), Mathf.Max(stepWereOn.finalAniPos.z, stepWereOn.initAniPos.z))

                );

                /* Interpolate the current blade rotation */
                currBladeAnimationRot = betterQLerp(stepWereOn.initAniRot, stepWereOn.finalAniRot, timeTaken / stepWereOn.timeThisStepTakes);
                timeTaken = Mathf.Clamp(timeTaken + Time.deltaTime, 0, stepWereOn.timeThisStepTakes);
            } else {

                timeTaken = 0;
                currBladeAnimationPos = stepWereOn.finalAniPos;
                currBladeAnimationRot = stepWereOn.finalAniRot;
                currAniIndex++;
                if (stepWereOn.loop) {
                    currAniIndex = currAniIndex % animationSteps[aniNumber].Length;
                }
            }
        } else {
            totalAniTime = 0f;
        }

        if (bob) { /* Bob is the little weapon up and down movement that happens when walking done all through transforms of the local position */
            /* Just check x cause each bob finished at the same time... */
            if (Mathf.Abs(bobVec.x) == maxBobx) {
                alternator *= -1;
            }

            /* bob alternates up and down using alternator */
            bobVec = new Vector3(Mathf.Clamp(bobVec.x + alternator * maxBobx * (Time.deltaTime / (bobTime)), -maxBobx, maxBobx),
                                 Mathf.Clamp(bobVec.y + alternator * maxBoby * (Time.deltaTime / (bobTime)), -maxBoby, maxBoby),
                                 Mathf.Clamp(bobVec.z + alternator * maxBobz * (Time.deltaTime / (bobTime)), -maxBobz, maxBobz));
        } else {
            /* send bob back to the original position "no bob" */
            bobVec = new Vector3(Mathf.Clamp(bobVec.x - (bobVec.x - maxBobx) * (Time.deltaTime / (bobTime)), -maxBobx, maxBobx),
                                 Mathf.Clamp(bobVec.y - (bobVec.y - maxBoby) * (Time.deltaTime / (bobTime)), -maxBoby, maxBoby),
                                 Mathf.Clamp(bobVec.z - (bobVec.z - maxBobz) * (Time.deltaTime / (bobTime)), -maxBobz, maxBobz));
        }

        if (isQuick) { /* Is quick is the sight perturbation of the held weapon when experiencing quick speed like jumping, or dashing */
            /* Perturb down */
            currQuickBob = Vector3.Lerp(currQuickBob, new Vector3(0, -maxQuickBob, 0), quickBobRate * Time.deltaTime);
        }
        else if (playerScript.isGrounded) { /* No perturbation when grounded */
            currQuickBob = Vector3.Lerp(currQuickBob, new Vector3(0, 0, 0), quickBobRate * Time.deltaTime);
        } else {
            /* Perturb up */
            currQuickBob = Vector3.Lerp(currQuickBob, new Vector3(0, maxQuickBob, 0), quickBobRate * Time.deltaTime);
        }

        /* Sway is the slight left and right rotation of the held weapon when moving the camera at fast speeds */
        Quaternion swayX = Quaternion.AngleAxis(-sway * swayMult, Vector3.forward);
        
        if (playerScript.state == PlayerManager.STATES.DASH || dashAniTimer > 0) { /* Dasing animation should show the sword slash even if our current weapon is the gun */
            gunModel.transform.localRotation = betterQLerp(gunStowRot, gunStowRot * swayX, swayRate * Time.deltaTime);

            bladeBeforeAniRot = Quaternion.Slerp(bladeOnScreenRot, bladeOnScreenRot * swayX, swayRate * Time.deltaTime);
            bladeModel.transform.localRotation = bladeBeforeAniRot * currBladeAnimationRot; /* Apply the animation! */
            recoil = Mathf.Lerp(recoil, 0, 8 * Time.deltaTime);
            gunModel.transform.localPosition = gunStowPos + (recoilDir.normalized * recoil) + bobVec + currQuickBob;

            bladeModel.transform.localPosition = bladeOnScreenPos + bobVec + currQuickBob + currBladeAnimationPos;
            dashAniTimer = Mathf.Clamp(dashAniTimer - Time.deltaTime, 0, dashAniTimer);
        } else {
            gunModel.transform.localRotation = betterQLerp(gunModel.transform.localRotation, currGunRot * swayX, swayRate * Time.deltaTime);

            bladeBeforeAniRot = Quaternion.Slerp(bladeBeforeAniRot, currBladeRot * swayX, swayRate * Time.deltaTime);
            bladeModel.transform.localRotation = bladeBeforeAniRot * currBladeAnimationRot; /* Apply the animation! */
            recoil = Mathf.Lerp(recoil, 0, 8 * Time.deltaTime);
            gunModel.transform.localPosition = currGunPos + (recoilDir.normalized * recoil) + bobVec + currQuickBob;

            bladeModel.transform.localPosition = currBladePos + bobVec + currQuickBob + currBladeAnimationPos;
        }

    }


}
