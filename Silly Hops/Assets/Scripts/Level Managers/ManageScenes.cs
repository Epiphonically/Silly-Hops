using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ManageScenes : MonoBehaviour {

    public enum PAUSELAYER {
        LAYER1,
        SETTINGS,
        PLAY
    }

    public static bool paused = false, tutorialDone = false, lv1Done = false, lv2Done = false, lv3Done = false, bossDone = false, lost = false, win = false, activatingRune = false, doneActivatingRune = false;
    public static float minFov = 25, maxFov = 100, currFov;
    public static int lives = 0;

    public GameObject pauseMenu, settings, playPage, lvSelector, titleScreen, lostScreen, runeWinScreen, winScreen;
    public GameObject[] findPlayer;

    public AudioSource mySource;
    public AudioClip escape, runeActivate;
    public static float minSens = 1, maxSens = 10, currSens;

    public Slider sensSlider, fovSlider;
    public PAUSELAYER currLayer = PAUSELAYER.LAYER1;
    public GameObject events;

    public Image tutorialImage, lv1Image, lv2Image, lv3Image, bossImage;
    public Sprite lockedSprite, lv1Sprite, lv2Sprite, lv3Sprite, bossSprite;

    public RectTransform skull, sword;

    private float skullAnim = 0, swordAnim = 0;
    private float skullMaxAnim = 15, swordMaxAnim = 15;
    private Vector3 skullOrigPos, swordOrigPos;

    public Material tutorialRuneMat, lv1RuneMat, lv2RuneMat, lv3RuneMat, runeMatOn, runeMatOff, runeMatGG;
    public GameObject runeMats, theRune;
    public Color white, green;
    void Awake() {

        skullOrigPos = skull.localPosition;
        swordOrigPos = sword.localPosition;
        events = GameObject.Find("EventSystem");
        DontDestroyOnLoad(gameObject);
        fovSlider.value = 0.5f;
        sensSlider.value = 0.5f;

        tutorialRuneMat = runeMatOff;
        lv1RuneMat = runeMatOff;
        lv2RuneMat = runeMatOff;
        lv3RuneMat = runeMatOff;
        SceneManager.LoadScene("Menu");
    }

    void Update() {
       
        /* If the level is locked it should be the locked sprite */
        lv1Image.sprite = (tutorialDone ? lv1Sprite : lockedSprite);
        lv2Image.sprite = (lv1Done ? lv2Sprite : lockedSprite);
        lv3Image.sprite = (lv2Done ? lv3Sprite : lockedSprite);
        bossImage.sprite = (lv3Done ? bossSprite : lockedSprite);
        
        tutorialImage.color = (tutorialDone ? green : white);
        lv1Image.color = (lv1Done ? green : white);
        lv2Image.color = (lv2Done ? green : white);
        lv3Image.color = (lv3Done ? green : white);
        bossImage.color = (bossDone ? green : white);

       

        if (SceneManager.GetActiveScene().name == "Menu") {
            activatingRune = false;
            doneActivatingRune = false;
            skullAnim = 0;
            lostScreen.SetActive(false);
            winScreen.SetActive(false);
            pauseMenu.SetActive(false);
            titleScreen.SetActive(true);
            lost = false;
            win = false;
            paused = false;
            Cursor.lockState = CursorLockMode.None;
            runeWinScreen.SetActive(false);
            switch (currLayer) {
                
                case PAUSELAYER.LAYER1:
                    playPage.SetActive(true);
                    settings.SetActive(false);
                    lvSelector.SetActive(false);
                    break;

                case PAUSELAYER.SETTINGS:
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        mySource.GetComponent<AudioSource>().clip = escape;
                        mySource.Play(0);

                        currLayer = PAUSELAYER.LAYER1;
                    }
                    playPage.SetActive(false);
                    settings.SetActive(true);
                    lvSelector.SetActive(false);
                    break;

                case PAUSELAYER.PLAY:
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        mySource.GetComponent<AudioSource>().clip = escape;
                        mySource.Play(0);

                        currLayer = PAUSELAYER.LAYER1;
                    }
                    playPage.SetActive(false);
                    settings.SetActive(false);
                    lvSelector.SetActive(true);
                    break;
            }
        } else {
            if (win) {
                
                if (!activatingRune && !doneActivatingRune) {
                    /* Play the little activate rune animation through coroutine */
                    GameObject[] GUIs = GameObject.FindGameObjectsWithTag("GUI");
                    for (int i = 0; i < GUIs.Length; i++) {
                        GUIs[i].SetActive(false);
                    }
                    
                    runeWinScreen.SetActive(true);
                    winScreen.SetActive(false);
                    activatingRune = true;
                    StartCoroutine(ActivateRune());
                } else if (doneActivatingRune) {
                    runeWinScreen.SetActive(false);
                    winScreen.SetActive(true);
                }
                paused = true;
                lost = false;
                switch (SceneManager.GetActiveScene().name) {
                    case "Tutorial":
                        tutorialDone = true;
                        break;

                    case "Level1":
                        lv1Done = true;
                        break;

                    case "Level2":
                        lv2Done = true;
                        break;

                    case "Level3":
                        lv3Done = true;
                        break;

                    case "BossLevel":
                        bossDone = true;
                        break;
                }
                swordAnim = Mathf.Clamp(swordAnim + 2 * Time.deltaTime, 0, 360);
                if (swordAnim >= 360) {
                    swordAnim = 0;
                }

                sword.localPosition = swordOrigPos + new Vector3(0, swordMaxAnim * Mathf.Sin(swordAnim), 0);
                pauseMenu.SetActive(false);
                settings.SetActive(false);

                Cursor.lockState = CursorLockMode.None;
            } else if (lost) {
                runeWinScreen.SetActive(false);
                activatingRune = false;
                doneActivatingRune = false;
                lostScreen.SetActive(true);
                pauseMenu.SetActive(false);
                settings.SetActive(false);
                winScreen.SetActive(false);
                skullAnim = Mathf.Clamp(skullAnim + 2 * Time.deltaTime, 0, 360);
                if (skullAnim >= 360) {
                    skullAnim = 0;
                }

                skull.localPosition = skullOrigPos + new Vector3(0, skullMaxAnim * Mathf.Sin(skullAnim), 0);


                paused = true;

                Cursor.lockState = CursorLockMode.None;
            } else {
                runeWinScreen.SetActive(false);
                activatingRune = false;
                doneActivatingRune = false;
                skullAnim = 0;
                if (currLayer == PAUSELAYER.PLAY)
                {
                    currLayer = PAUSELAYER.LAYER1;
                }
                winScreen.SetActive(false);
                lostScreen.SetActive(false);
                titleScreen.SetActive(false);
                playPage.SetActive(false);
                lvSelector.SetActive(false);
                findPlayer = GameObject.FindGameObjectsWithTag("Player");
                if (findPlayer.Length > 0)
                {
                    (findPlayer[0]).GetComponent<PlayerManager>().sensitivity = currSens;
                }


                if (Input.GetKeyDown(KeyCode.Escape)) {
                    mySource.GetComponent<AudioSource>().clip = escape;
                    mySource.Play(0);
                    paused = !paused;
                    if (!paused) {
                        currLayer = PAUSELAYER.LAYER1;
                    }
                }
                /* We use the time scale method to pause that was mentioned in an announcement */
                if (paused) {
                    Time.timeScale = 0;
                    Cursor.lockState = CursorLockMode.None;
                    switch (currLayer) {
                        case PAUSELAYER.LAYER1:
                            pauseMenu.SetActive(true);
                            settings.SetActive(false);

                            break;

                        case PAUSELAYER.SETTINGS:
                            pauseMenu.SetActive(false);
                            settings.SetActive(true);

                            break;
                    }
                } else {
                    Time.timeScale = 1;
                    Cursor.lockState = CursorLockMode.Locked;
                    pauseMenu.SetActive(false);
                    settings.SetActive(false);
                }
            }


        }

        /* Not everyone has the same hardware and likes the same mouse speed 
           so we allow you to change your sensitivity and field of vision in our settings */

        currFov = minFov * (1 - fovSlider.value) + maxFov * fovSlider.value;

        currSens = minSens * (1 - sensSlider.value) + maxSens * sensSlider.value;

    }

    public void BackOutOfLevelSelect() {
        mySource.GetComponent<AudioSource>().clip = escape;
        mySource.Play(0);
        currLayer = PAUSELAYER.LAYER1;
    }

    public void PlayAgainClick() {
        if (SceneManager.GetActiveScene().name == "Tutorial") {
            lives = -1;
        } else if (SceneManager.GetActiveScene().name == "BossLevel") {
            lives = 5;
        } else {
            lives = 3;
        }
        paused = false;
        Time.timeScale = 1;
        lost = false;
        win = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void PlayClick() {
        currLayer = PAUSELAYER.PLAY;
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void ResumeClick() {
        paused = false;
        Time.timeScale = 1;
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void SettingsClick() {
        currLayer = PAUSELAYER.SETTINGS;
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void BackToMenuClick() {
        paused = false;
        Time.timeScale = 1;
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        SceneManager.LoadScene("Menu");
    }

    public void QuitClick() {
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        Application.Quit();
    }

    public void ReturnClick() {
        currLayer = PAUSELAYER.LAYER1;
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void PlayTutorial() {
        lives = -1;
        SceneManager.LoadScene("Tutorial");
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void PlayLevel1() {

        if (tutorialDone) {
            lives = 3;
            SceneManager.LoadScene("Level1");

        }
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void PlayLevel2() {

        if (lv1Done) {
            lives = 3;
            SceneManager.LoadScene("Level2");

        }
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void PlayLevel3() {
        if (lv2Done) {
            lives = 3;
            SceneManager.LoadScene("Level3");

        }
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void PlayBoss() {
        if (lv3Done) {
            lives = 5;
            SceneManager.LoadScene("BossLevel");

        }
        events.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void UnlockAll() {
        tutorialDone = true;
        lv1Done = true;
        lv2Done = true;
        lv3Done = true;
    }

    /* Coroutine that animates the rune lighting up 
       theres just a 180 degree rotation at the beginning */
    public IEnumerator ActivateRune() {
        activatingRune = true;
        Material[] temp = runeMats.GetComponent<MeshRenderer>().materials;
        temp[0] = runeMatOn;
        temp[4] = tutorialRuneMat;
        temp[2] = lv1RuneMat;
        temp[3] = lv2RuneMat;
        temp[1] = lv3RuneMat;
        runeMats.GetComponent<MeshRenderer>().materials = temp;
        theRune.transform.localRotation = Quaternion.Euler(0, 0, 0);
        float aniTimer = 0, maxAniTimer = 2;
  
        /* Make the rune do a little animation */
        while (aniTimer < maxAniTimer) {
            
            aniTimer = Mathf.Clamp(aniTimer + Time.deltaTime/maxAniTimer, 0, maxAniTimer);
            theRune.transform.localRotation = Quaternion.Euler(0, 180*(aniTimer/maxAniTimer), 0);
            yield return null;
        }
       
        if (!bossDone) {
            switch (SceneManager.GetActiveScene().name) {
                case "Tutorial":
                    temp[4] = runeMatOn;
                    tutorialRuneMat = runeMatOn;
                break;

                case "Level1":
                    temp[2] = runeMatOn;
                    lv1RuneMat = runeMatOn;
                break;

                case "Level2":
                    temp[3] = runeMatOn;
                    lv2RuneMat = runeMatOn;
                break;

                case "Level3":
                    temp[1] = runeMatOn;
                    lv3RuneMat = runeMatOn;
                break;

                case "BossLevel":
                    temp[1] = runeMatOff;
                    temp[2] = runeMatOff;
                    temp[3] = runeMatOff;
                    temp[4] = runeMatOff;
                    tutorialRuneMat = runeMatOff;
                    lv1RuneMat = runeMatOff;
                    lv2RuneMat = runeMatOff;
                    lv3RuneMat = runeMatOff;

                break;
            }
        }
       
        mySource.GetComponent<AudioSource>().clip = runeActivate;
        mySource.Play(0);
        runeMats.GetComponent<MeshRenderer>().materials = temp;

        
        /* These booleans are like a gate where done
           activating rune lets us know to put up the win screen */

        yield return new WaitForSeconds(5);
      
        activatingRune = false;
        doneActivatingRune = true;
    }
}
