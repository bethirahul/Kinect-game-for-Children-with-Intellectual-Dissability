
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    //------------------------- Global Variables -----------------------//
    #region Global Variables
    //--> Other Variables
    #region Other Variables
    public bool isGamePaused;
    public bool isGameWon;
    public int score;
    public int addScore;
    public int removeScore;
    public int lastTileAwarded;
    public int lastLevelScore;
    public struct timerStruct
    {
        public bool  isSet;
        public float totalTime;
        public float endTime;
        public void setTimer(bool l_bool)
        {
            this.isSet = l_bool;
            if(l_bool)
                this.endTime = this.totalTime + Time.time;
        }
        public bool isTimerEnded()
        {
            if (this.isSet)
            {
                if (this.endTime < Time.time)
                {
                    this.isSet = false;
                    return true;
                }
                else
                    return false;
            }
            else
                return true;
        }
    };
    public timerStruct nextLevelScreenTimer;
    public timerStruct finishScreenTimer;
    public timerStruct lostLifeScreenTimer;
    public enum resetEnum : int { start, reset, levelChange };
    public resetEnum reset;
    #endregion

    //--> Audio
    #region Audio
    public bool audioLevel;
    public int totalAudioSources;
    public AudioClip buttonSound;
    public AudioClip muteSound;
    public AudioClip levelStartSound;
    public AudioClip gameLostSound;
    public AudioClip gameWinSound;
    public AudioSource[] audioSource;
    void play(AudioClip l_clip)
    {
        int i;

        for (i = 1; i < totalAudioSources; i++)
        {
            if (!audioSource[i].isPlaying)
            {
                audioSource[i].clip = l_clip;
                audioSource[i].Play();
                break;
            }
        }
        if (i >= totalAudioSources)
            Debug.Log("Error: All Audio Sources are busy playing");
    }
    void playOnlyThis(AudioClip l_clip)
    {
        int i;
        for (i = 0; i < totalAudioSources; i++)
        {
            if (audioSource[i].isPlaying)
                audioSource[i].Stop();
        }
        audioSource[1].clip = l_clip;
        audioSource[1].Play();
    }
    void changeAudioLevel(bool l_bool)
    {
        int i;

        audioLevel = l_bool;
        for (i = 0; i < totalAudioSources; i++)
        {
            if(audioLevel)
                audioSource[i].volume = 1.0f;
            else
                audioSource[i].volume = 0.0f;
        }
    }
    #endregion

    //--> GUI
    #region GUI
    //--> --> Screens
    //--> --> --> Kinect Indicator
    public GameObject kinectTrackingGO;
    public Image kinectIndicator;

    //--> --> --> game UI
    public GameObject gameUIGO;
    public Text livesText;
    public Text scoreText;
    public Text levelText;
    public Text maxHeightText;
    public Image healthBar;
    public Vector2 originalHealthBarSize;
    public Image invincibleBar;
    public Vector2 originalInvincibleBarSize;

    //--> --> --> Main Menu
    public GameObject mainMenuGO;
    public Button startButton;
    public Button highScoresButton;
    public Toggle audioToggle;
    public Button exitButton;

    //--> --> --> High Scores
    public GameObject highScoresGO;
    public Text namesText;
    public Text scoresText;
    public Text heightsText;
    public Button highScoresToMMButton;

    //--> --> --> Pause Menu
    public GameObject pauseMenuGO;
    public Button resumeButton;
    public Button pauseRestartButton;
    public Button pauseToMMButton;

    //--> --> --> Game Over
    public GameObject gameOverGO;
    public GameObject lostGO;
    public GameObject winGO;
    public GameObject enterNameGO;
    public Text gameOverText;
    public InputField enterNameInputField;
    public Button GORestartButton;
    public Button GOToMMButton;

    //--> --> --> Next Level
    public GameObject nextLevelGO;
    public Text levelNumberText;

    //--> --> --> Lost life
    public GameObject lostLifeGO;

    //--> --> --> Finish
    public GameObject finishMenuGO;

    //--> --> --> Finding Start
    public timerStruct findingStartTimer;
    public GameObject findingStartGO;
    public Button findingStartToMMButton;
    public Text findingStartTimeText;

    //--> --> --> Finding Difficulty
    public GameObject findingDifficultyGO;
    public Button startGameButton;
    public Button findingDiffToMMButton;
    public Text maxUpText;

    //--> --> --> Error
    public GameObject errorGO;
    public Button errorExitButton;
    public Text errorText;

    public enum screenEnum : int
    {
        mainMenu,
        gameUI,
        pauseMenu,
        gameOver,
        options,
        highScores,
        nextLevel,
        finish,
        lostLife,
        findingStart,
        findingDifficulty
    };
    public screenEnum screen;
    public Rect screenSize;
    #endregion

    //--> Levels
    #region Levels
    public int totalPackages;
    public int totalLevels;
    public int startingLevel;
    public int currentLevel;
    public int totalLevelColumns;
    public int centerColumn;
    public struct levelStruct
    {
        public int totalRows;
        public List<string> tileDefinition;
        public List<string> powerUpDefinition;
        public int packageID;
    };
    public List<levelStruct> level = new List<levelStruct>();
    public levelStruct l_level;
    #endregion

    //--> Game Objects
    #region Game Objects
    //--> --> Cameras
    #region Cameras
    public GameObject backCameraGO;
    public GameObject topCameraGO;
    #endregion

    //--> --> Player
    #region Player
    public GameObject playerGO;
    public AudioClip playerRunSound;
    public AudioClip playerDuckSound;
    public AudioClip playerJumpSound;
    public AudioClip playerLandSound;
    public AudioClip playerHurtSound;
    public Texture2D[] runTexture       = new Texture2D[6];
    public Texture2D[] jumpStartTexture = new Texture2D[3];
    public Texture2D[] jumpEndTexture   = new Texture2D[3];
    public Texture2D[] duckStartTexture = new Texture2D[1];
    public Texture2D[] duckTexture      = new Texture2D[22];
    public Texture2D[] duckEndTexture   = new Texture2D[2];
    public enum actionEnum : int { leapLeft, leap, leapRight, jump, duck, run};
    public enum texEnum : int { run, jumpStart, jumpEnd, duckStart, duck, duckEnd };
    public struct playerStruct
    {
        //--> --> --> Details
        public actionEnum action;
        public float totalHealth;
        public float currentHealth;
        public float removeHealth;
        public int   totalLives;
        public int   currentLives;
        public int   currentColumn;
        public timerStruct blinkTimer;
        //--> --> --> Speeds
        public float runSpeed;
        public float kinectJumpUpClampSpeed;
        public float kinectJumpUpSetSpeed;
        public float kinectJumpDownSpeed;
        public float jumpUpSpeed;
        public float jumpDownSpeed;
        public float leapUpSpeed;
        public float leapDownSpeed;
        public float leapSideSpeed;
        public float leapSideUpSpeed;
        public float leapSideDownSpeed;
        public float currentUpSpeed;
        public float currentDownSpeed;
        public float currentSideSpeed;
        public float duckDistance;
        public float duckEndDistance;
        public float jumpMaxHeight;
        public float[] jumpHeightLevel;
        //--> --> --> Input Flags
        public bool isKinectJumpInput;
        public bool isJumpInput;
        public bool isLeapInput;
        public bool isLeapLeftInput;
        public bool isLeapRightInput;
        public bool isDuckInput;
        //--> --> --> Not Hurt
        public int   lastHurtRow;
        public bool  isNoHurt;
        public float noHurtDistance;
        public float noHurtEndDistance;
        //--> --> --> PowerUp
        public bool  isInvincible;
        public float invincibleDistance;
        public float invincibleEndDistance;
        //--> --> --> Positions
        public float width;
        public Vector3 startPosition;
        public Vector3 backCameraDistance;
        public Vector3 topCameraDistance;
        public Vector3 sceneryDistance;
        // Textures
        public Renderer renderer;
        public int currentTexture;
        public texEnum texType;
        public float changeTextureTime;
        public float changeTextureEndTime;
    };
    public playerStruct player;
    #endregion

    //--> --> Tiles
    #region Tiles
    public AudioClip healthPowerUpSound;
    public AudioClip invinciblePowerUpSound;
    public AudioClip finishSound;
    public GameObject originalTileGO;
    public float tileSize;
    public Vector3 objectSize;
    public int visibleRows;
    public int startingRow;
    public int endingRow;
    public int initialRow;
    public int actualStartingRow;
    public int actualEndingRow1;
    public int actualEndingRow2;
    public enum tileEnum : int
    {
        none   = -1,
        tree   =  0,
        bush   =  1,
        stone  =  2,
        log    =  3,
        finish =  4,
        pond   =  5,
        path   =  6,
        grass  =  7,
        water  =  8,
        river  =  9,
        singleTileObstacle = 10,
        fullRowObstacle    = 11,
        environment = 12

    };
    public enum powerUpEnum : int { health = 0, invincible = 1, none = -1 };
    public string tileRepresentation;
    public string powerUpRepresentation;
    public struct tileStruct
    {
        public GameObject GO;
        public int tileObjectID;
        public int powerUpID;
        public tileEnum tileType;
        public powerUpEnum powerUpType;
        public Renderer renderer;
    };
    public tileStruct[,] tile;
    #endregion

    //--> --> Other Props
    #region Other Props
    public GameObject sceneryGO;
    public int objectTypes;
    public int powerUpTypes;
    public int totalObjectTypes;
    public int visibleObjectsEach;
    public struct propStruct
    {
        public GameObject GO;
        public bool isAnimate;
        public int noOfImages;
        public bool isActive;
        public int type;
        public void setActive(bool l_bool)
        {
            if (l_bool == true && this.isActive == true)
                this.GO.SetActive(true);
            else
                this.GO.SetActive(false);
        }
    };
    public propStruct[,] prop;
    public GameObject originalTreeGO;
    public GameObject originalStoneGO;
    public GameObject originalBushGO;
    public GameObject originalLogGO;
    public GameObject originalHealthGO;
    public GameObject originalInvincibleGO;
    public GameObject finishGO;
    #endregion

    //--> --> Textures
    #region Textures
    public struct packageStruct
    {
        public Texture2D pathTex;
        public Texture2D grassTex;
        public Texture2D grassShadowTex;
        public Texture2D pathShadowTex;
        public List<Texture2D> treeTex;
        public List<Texture2D> bushTex;
        public List<Texture2D> stoneTex;
        public Texture2D scenery;
    };
    public packageStruct l_package;
    public List<packageStruct> package = new List<packageStruct>();
    public Material tileDefaultMaterial;
    public Material riverMaterial;
    public Material lavaMaterial;
    public Material waterMaterial;
    public Texture2D finishTex;
    public Texture2D healthTex;
    public Texture2D invincibleTex;
    #endregion
    #endregion

    //--> Temporary Variables
    #region Temp
    public float   tempFloat;
    public Vector3 tempVec3;
    public int     tempInt;
    #endregion

    //--> Kinect
    #region Kinect
    public GameObject spineBaseGO;
    public SpineBase spineBase;

    public enum kinectEnum : int { start, calibrating, end };
    public kinectEnum kinectCalib;
    public List<Vector3> kinectStartPositionList;
    public Vector3 kinectStartPosition;

    public struct kinectAvgInputStruct
    {
        public Vector3 position;
        public float time;
    };
    public kinectAvgInputStruct[] kinectAvgInput;
    public int prevFrame;
    public int startFrame;
    public Vector3[] kinectInput;
    public int avgFrames;

    public float jumpStartThreshold;
    public float leapStartThreshold;
    public float leapStartSideThreshold;
    public float duckStartThreshold;
    public float jumpThreshold;
    public float leapThreshold;
    public float leapLeftThreshold;
    public float leapRightThreshold;
    public float duckThreshold;
    public float kinectRatio;
    public float maxUp;
    public float maxDown;
    public float maxLeft;
    public float maxRight;
    public float maxUpPos;
    public float maxDownPos;
    public float maxLeftPos;
    public float maxRightPos;
    public string kinectSays;
    #endregion

    public struct highScoresDataStruct
    {
        public string name;
        public int score;
        public float height;
    };
    public highScoresDataStruct[] highScoresData;
    #endregion

    //--------------------------- A W A K E ----------------------------//
    #region Awake
    void Awake ()
    {
        int i, j, l, r, c, rand;
        bool found;
        string filePath, line, path;
        StreamReader file;
        Vector3 l_rotation;
        WWW www;
        WaitForSeconds w;
        //List<string> lines;
        string longString;

        addScore = 10;
        removeScore = 10;

        //--> Audio
        #region Audio
        changeAudioLevel(audioLevel);
        totalAudioSources = 5;
        audioSource = new AudioSource[totalAudioSources];
        audioSource = GetComponents<AudioSource>();
        audioSource[0].clip = playerRunSound;
        #endregion

        //--> GUI
        #region GUI Elements
        //--> --> Screens
        screen     = screenEnum.mainMenu;
        screenSize = new Rect(0, 0, Screen.width, Screen.height);

        //--> --> --> game UI
        originalHealthBarSize = healthBar.rectTransform.sizeDelta;
        originalInvincibleBarSize = invincibleBar.rectTransform.sizeDelta;
        invincibleBar.rectTransform.sizeDelta = new Vector2(0, originalInvincibleBarSize.y);

        //--> --> --> Main Menu
        startButton.onClick.AddListener(startGame);
        highScoresButton.onClick.AddListener(gotoHighScores);
        exitButton.onClick.AddListener(exit);

        //--> --> --> High Scores
        highScoresToMMButton.onClick.AddListener(goFromHSToMM);

        //--> --> --> Pause
        resumeButton.onClick.AddListener(resumeGame);
        pauseRestartButton.onClick.AddListener(pauseRestartGame);
        pauseToMMButton.onClick.AddListener(goFromPauseToMM);

        //--> --> --> Game Over
        GORestartButton.onClick.AddListener(GORestartGame);
        GOToMMButton.onClick.AddListener(goFromGOToMM);
        enterNameInputField.ActivateInputField();

        //--> --> --> Finding Start
        findingStartTimer.totalTime = 3.0f;//3.0f///////////////////////////////////////////////
        findingStartToMMButton.onClick.AddListener(goFromFindingStartToMM);

        //--> --> --> Finding Difficulty
        startGameButton.onClick.AddListener(startActualGame);
        findingDiffToMMButton.onClick.AddListener(goFromFindingDiffToMM);

        //--> --> --> Error
        errorExitButton.onClick.AddListener(exit);
        #endregion

        //--> Levels
        #region Levels
        nextLevelScreenTimer.totalTime = 2.0f;
        finishScreenTimer.totalTime    = 1.0f;
        lostLifeScreenTimer.totalTime  = 1.0f;
        startingLevel = 1;
        totalLevelColumns = 15;
        centerColumn  = totalLevelColumns / 2;
        tileRepresentation    = "tbslfp.gwr120";
        powerUpRepresentation = "hi";

        l = 0;
        while(l < 100)
        {
            filePath = "file:///" + Application.streamingAssetsPath + "/Levels/" + (l+1).ToString() + ".txt";
            www = new WWW(filePath);
            while (!www.isDone)
                w = new WaitForSeconds(0.05f);
            if(www.error != "")
            {
                if(l == 0)
                {
                    Debug.Log("No level files found");
                    errorText.text = "Error!\nNo level files found!";
                    errorGO.SetActive(true);
                    return;
                }
                totalLevels = l;
                Debug.Log("Total levels: " + totalLevels);
                break;
            }

            string[] lines = Regex.Split(www.text, "\r\n|\r|\n");

            l_level.tileDefinition = new List<string>();
            l_level.powerUpDefinition = new List<string>();
            l_level.totalRows = 0;
            for(j=0; j<lines.Length; j++)
            {
                while(lines[j].Length < totalLevelColumns)
                    lines[j] += "g";
                l_level.tileDefinition.Add(lines[j]);
                j++; // second line
                while(lines[j].Length < totalLevelColumns)
                    lines[j] += "-";
                l_level.powerUpDefinition.Add(lines[j]);

                for(c=0; c<totalLevelColumns; c++)
                {
                    if(c < centerColumn-1 || c > centerColumn+1)
                    {
                        if(l_level.tileDefinition[l_level.totalRows][c] != 'g' && l_level.tileDefinition[l_level.totalRows][c] != 'r')
                        {
                            found = false;
                            if(l_level.tileDefinition[l_level.totalRows][c] == tileRepresentation[12])
                                found = true;
                            else
                                for(i=0; i<2; i++)
                                    if(l_level.tileDefinition[l_level.totalRows][c] == tileRepresentation[i])
                                    {
                                        found = true;
                                        i = 2;
                                    }
                            if(!found)
                            {
                                Debug.Log("Level: " + l + ", letter " + l_level.tileDefinition[l_level.totalRows][c] + " at [" + l_level.totalRows + "," + c + "] not found!");
                                l_level.tileDefinition[l_level.totalRows] = replaceChar(c, "g", l_level.tileDefinition[l_level.totalRows]);
                            }
                        }
                    }
                    else
                    {
                        if(l_level.tileDefinition[l_level.totalRows][c] != '.')
                        {
                            found = false;
                            for(i=2; i<12; i++)
                                if(l_level.tileDefinition[l_level.totalRows][c] == tileRepresentation[i])
                                {
                                    found = true;
                                    i = 12;
                                }
                            if(!found)
                            {
                                Debug.Log("Level: " + l + ", letter " + l_level.tileDefinition[l_level.totalRows][c] + " at [" + l_level.totalRows + "," + c + "] not found!");
                                l_level.tileDefinition[l_level.totalRows] = replaceChar(c, ".", l_level.tileDefinition[l_level.totalRows]);
                            }
                        }
                    }
                }
                l_level.totalRows++;
            }
            level.Add(l_level);
            l++;
        }
        #endregion

        //--> Game Objects
        #region Game Objects
        //--> --> Player
        #region Player
        player.totalLives             =   3   ;
        player.totalHealth            = 100.0f;
        player.removeHealth           =  25.0f;//25.0f/////////////
        player.runSpeed               =  12.0f;
        player.kinectJumpUpClampSpeed =  25.0f;
        player.kinectJumpUpSetSpeed   =   0.0f;
        player.kinectJumpDownSpeed    =  42.0f;
        player.jumpUpSpeed            =  18.0f;
        player.jumpDownSpeed          =  45.0f;
        player.leapUpSpeed            =  12.0f;
        player.leapDownSpeed          =  19.0f;
        player.leapSideUpSpeed        =  18.0f;
        player.leapSideDownSpeed      =  45.0f;
        player.leapSideSpeed          =   5.9f;
        player.duckDistance           =  10.0f;
        player.invincibleDistance     =  50.0f;
        player.width                  =   2.5f;
        player.jumpMaxHeight          =   3.5f;
        player.backCameraDistance     = new Vector3(0.0f,  3.0f,  -19.0f);
        player.topCameraDistance      = new Vector3(0.0f, 20.00f,   0.0f);
        player.sceneryDistance        = new Vector3(0.0f,  0.00f, 130.0f);
        player.startPosition          = new Vector3(0.0f,  3.0f,    0.0f);
        player.changeTextureTime      = (1.0f/12.0f);
        player.jumpHeightLevel        = new float[jumpStartTexture.Length];
        for(i=0; i<player.jumpHeightLevel.Length; i++)
            player.jumpHeightLevel[i] =  (i + 1) * ( player.jumpMaxHeight / (player.jumpHeightLevel.Length + 1) );
        
        player.renderer = playerGO.GetComponent<Renderer>();
        #endregion

        //--> --> Tiles
        #region Tiles
        tileSize    =  5.0f;
        objectSize  = new Vector3(tileSize*0.4f, tileSize*0.4f, tileSize*0.1f);
        visibleRows = 25;
        initialRow  =  0;
        tile = new tileStruct[visibleRows, totalLevelColumns];
        for(r=0; r<visibleRows; r++)
        {
            for(c=0; c<totalLevelColumns; c++)
            {
                tile[r,c].GO = Instantiate(originalTileGO, new Vector3(0,0,0), Quaternion.identity);
                tile[r,c].renderer = tile[r,c].GO.GetComponent<Renderer>();
            }
        }
        originalTileGO.SetActive(false);
        #endregion

        //--> --> Other Props
        #region Other Props
        l_rotation  = new Vector3(90.0f, 180.0f, 0.0f);
        visibleObjectsEach = 40;
        objectTypes = 4;
        powerUpTypes = 2;
        totalObjectTypes = objectTypes + powerUpTypes;
        prop = new propStruct[totalObjectTypes, visibleObjectsEach];

        loadTextures();

        //--> --> --> Trees
        l = 0;
        for(i=0; i<visibleObjectsEach; i++)
        {
            prop[l,i].GO = Instantiate(originalTreeGO, new Vector3(0,5.0f, 0), Quaternion.Euler(l_rotation));
            prop[l,i].isAnimate = false;
            prop[l,i].type = l;
        }
        originalTreeGO.SetActive (false);

        //--> --> --> Bushes
        l++;
        for(i=0; i<visibleObjectsEach; i++)
        {
            prop[l,i].GO = Instantiate(originalBushGO, new Vector3(0,1.6f,0), Quaternion.Euler(l_rotation));
            prop[l,i].isAnimate = false;
            prop[l,i].type = l;
        }
        originalBushGO.SetActive (false);

        //--> --> --> Stones
        l++;
        for(i=0; i<visibleObjectsEach; i++)
        {
            prop[l,i].GO = Instantiate(originalStoneGO, new Vector3(0,1.66f,0), Quaternion.Euler(l_rotation));
            prop[l,i].isAnimate = false;
            prop[l,i].type = l;
        }
        originalStoneGO.SetActive(false);

        //--> --> --> Logs
        l++;
        for(i=0; i<visibleObjectsEach; i++)
        {
            prop[l,i].GO = Instantiate(originalLogGO, new Vector3(0,0,0), Quaternion.Euler(l_rotation));
            prop[l,i].isAnimate = false;
        }
        originalLogGO.SetActive(false);

        //--> --> --> Healths
        l++;
        for(i=0; i<visibleObjectsEach; i++)
        {
            prop[l,i].GO = Instantiate(originalHealthGO, new Vector3(0,2.0f,0), Quaternion.Euler(l_rotation));
            prop[l,i].isAnimate = false;
        }
        originalHealthGO.SetActive(false);

        //--> --> --> Invincibles
        l++;
        for(i=0; i<visibleObjectsEach; i++)
        {
            prop[l,i].GO = Instantiate(originalInvincibleGO, new Vector3(0,2.0f,0), Quaternion.Euler(l_rotation));
            prop[l,i].isAnimate = false;
        }
        originalInvincibleGO.SetActive(false);
        finishGO.SetActive(false);
        #endregion
        #endregion

        setActiveAll(false);

        //--> Temporary Variables
        tempInt = 0;

        highScoresData = new highScoresDataStruct[5];
        loadHighScoresData();
	}
    #endregion

    void loadTextures()
    {
        int i, l;
        WWW www;
        WaitForSeconds w;

        i = 1;
        while(i <= 100)
        {
            if(!Directory.Exists(Application.streamingAssetsPath + "/Packages/" + i))
            {
                if(i==1)
                {
                    Debug.Log("No packages found!");
                    errorText.text = "Error!\nNo packages found!";
                    errorGO.SetActive(true);
                    return;
                }
                totalPackages = i-1;
                Debug.Log("Total packages: " + totalPackages);
                break;
            }

            l_package.treeTex  = new List<Texture2D>();
            l_package.bushTex  = new List<Texture2D>();
            l_package.stoneTex = new List<Texture2D>();

            //--> --> --> Tree Textures
            l = 1;
            while(l <= 100)
            {
                www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Trees/" + l + ".png");
                while (!www.isDone)
                    w = new WaitForSeconds(0.05f);
                if(www.error != "")
                {
                    www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Trees/" + l + ".jpg");
                    while (!www.isDone)
                        w = new WaitForSeconds(0.05f);
                }
                if(www.error != "")
                {
                    if(l==1)
                    {
                        Debug.Log("No Trees found! - " + www.error);
                        errorText.text = "Error!\nNo Tree Textures found in package " + i;
                        errorGO.SetActive(true);
                        return;
                    }
                    Debug.Log("Total Trees: " + (l-1));
                    break;
                }
                l_package.treeTex.Add(www.texture);
                l++;
            }

            //--> --> --> Bush Textures
            l = 1;
            while(l <= 100)
            {
                www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Bushes/" + l + ".png");
                while (!www.isDone)
                    w = new WaitForSeconds(0.05f);
                if(www.error != "")
                {
                    www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Bushes/" + l + ".jpg");
                    while (!www.isDone)
                        w = new WaitForSeconds(0.05f);
                }
                if(www.error != "")
                {
                    if(l==1)
                    {
                        Debug.Log("No Bushes found!");
                        errorText.text = "Error!\nNo Bush textures found in package " + i;
                        errorGO.SetActive(true);
                        return;
                    }
                    Debug.Log("Total Bushes: " + (l-1));
                    break;
                }
                l_package.bushTex.Add(www.texture);
                l++;
            }

            //--> --> --> Stone Textures
            l = 1;
            while(l <= 100)
            {
                www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Stones/" + l + ".png");
                while (!www.isDone)
                    w = new WaitForSeconds(0.05f);
                if(www.error != "")
                {
                    www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Stones/" + l + ".jpg");
                    while (!www.isDone)
                        w = new WaitForSeconds(0.05f);
                }
                if(www.error != "")
                {
                    if(l==1)
                    {
                        Debug.Log("No Stones found!");
                        errorText.text = "Error!\nNo Stone Textures found in package " + i;
                        errorGO.SetActive(true);
                        return;
                    }
                    Debug.Log("Total Stones: " + (l-1));
                    break;
                }
                l_package.stoneTex.Add(www.texture);
                l++;
            }

            //--> --> --> Scenery
            www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Scenery.png");
            while (!www.isDone)
                w = new WaitForSeconds(0.05f);
            if(www.error != "")
            {
                www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Scenery.jpg");
                while (!www.isDone)
                    w = new WaitForSeconds(0.05f);
            }
            if(www.error != "")
            {
                Debug.Log("No Scenery found!");
                errorText.text = "Error!\nNo Scenery Image found in package " + i;
                errorGO.SetActive(true);
                return;
            }
            l_package.scenery = www.texture;

            //--> --> --> Path
            www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Path/Path.png");
            while (!www.isDone)
                w = new WaitForSeconds(0.05f);
            if(www.error != "")
            {
                www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Path/Path.jpg");
                while (!www.isDone)
                    w = new WaitForSeconds(0.05f);
            }
            if(www.error != "")
            {
                Debug.Log("No Path found!");
                errorText.text = "Error!\nNo Path Image found in package " + i;
                errorGO.SetActive(true);
                return;
            }
            l_package.pathTex = www.texture;

            //--> --> --> Path Shadow
            www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Path/Shadow.png");
            while (!www.isDone)
                w = new WaitForSeconds(0.05f);
            if(www.error != "")
            {
                www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Path/Shadow.jpg");
                while (!www.isDone)
                    w = new WaitForSeconds(0.05f);
            }
            if(www.error != "")
            {
                Debug.Log("No Path Shadow found!");
                errorText.text = "Error!\nNo Path Shadow Image found in package " + i;
                errorGO.SetActive(true);
                return;
            }
            l_package.pathShadowTex = www.texture;

            //--> --> --> Grass
            www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Grass/Grass.png");
            while (!www.isDone)
                w = new WaitForSeconds(0.05f);
            if(www.error != "")
            {
                www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Grass/Grass.jpg");
                while (!www.isDone)
                    w = new WaitForSeconds(0.05f);
            }
            if(www.error != "")
            {
                Debug.Log("No Grass found!");
                errorText.text = "Error!\nNo Grass Image found in package " + i;
                errorGO.SetActive(true);
                return;
            }
            l_package.grassTex = www.texture;

            //--> --> --> Grass Shadow
            www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Grass/Shadow.png");
            while (!www.isDone)
                w = new WaitForSeconds(0.05f);
            if(www.error != "")
            {
                www = new WWW("file:///" + Application.streamingAssetsPath + "/Packages/" + i + "/Grass/Shadow.jpg");
                while (!www.isDone)
                    w = new WaitForSeconds(0.05f);
            }
            if(www.error != "")
            {
                Debug.Log("No Grass Shadow found!");
                errorText.text = "Error!\nNo Grass Shadow Image found in package " + i;
                errorGO.SetActive(true);
                return;
            }
            l_package.grassShadowTex = www.texture;

            package.Add(l_package);
            Debug.Log("Loaded package " + package.Count);
            i++;
        }
    }

    IEnumerator WaitForReq(WWW www)
    {
        while (!www.isDone)
        {
        }
        yield return www;
    }

    //--------------------------- S T A R T ----------------------------//
    #region Start
    void Start()
    {
        //--> Kinect
        #region Kinect
        spineBase   = spineBaseGO.GetComponent<SpineBase>();
        avgFrames   = 3;
        kinectInput = new Vector3[avgFrames];
        prevFrame   = 3;
        startFrame  = 0;
        kinectAvgInput = new kinectAvgInputStruct[prevFrame];
        jumpStartThreshold     = 0.2f;
        leapStartThreshold     = 2.5f;
        leapStartSideThreshold = 2.5f;
        duckStartThreshold     = 1.2f;
        kinectRatio = 1.0f;
        maxDown  = 0;
        maxLeft  = 0;
        maxRight = 0;
        #endregion
    }

    string replaceChar(int position, string newC, string s)
    {
        int i;
        string newS = "";

        for(i=0; i<s.Length; i++)
        {
            if(i == position)
                newS += newC;
            else
                newS += s[i];
        }

        return newS;
    }

    void loadHighScoresData()
    {
        int i;

        for (i = 0; i < highScoresData.Length; i++)
        {
            highScoresData[i].name   = PlayerPrefs.GetString("player" + i + "Name"  );
            highScoresData[i].score  = PlayerPrefs.GetInt   ("player" + i + "Score" );
            highScoresData[i].height = PlayerPrefs.GetFloat ("player" + i + "Height");
        }

        updateHighScoresUI();
    }

    bool checkHighScoresData()
    {
        for (int i = 0; i < highScoresData.Length; i++)
        {
            if (highScoresData[i].score < score)
                return true;
        }
        return false;
    }

    void saveHighScoresData()
    {
        int i, j;

        for (i = 0; i < highScoresData.Length; i++)
        {
            if (highScoresData[i].score < score)
                break;
        }

        for (j = highScoresData.Length-1; j > i; j--)
            highScoresData[j] = highScoresData[j - 1];

        highScoresData[i].name   = enterNameInputField.text;
        if (highScoresData[i].name.Length > 12)
            highScoresData[i].name = highScoresData[i].name.Substring(0, 12);
        highScoresData[i].score  = score;
        highScoresData[i].height = maxUp;

        for (i = 0; i < highScoresData.Length; i++)
        {
            PlayerPrefs.SetString("player" + i + "Name"  , highScoresData[i].name);
            PlayerPrefs.SetInt   ("player" + i + "Score" , highScoresData[i].score);
            PlayerPrefs.SetFloat ("player" + i + "Height", highScoresData[i].height);
        }

        updateHighScoresUI();
    }

    void updateHighScoresUI()
    {
        namesText.text   = " ";
        scoresText.text  = "";
        heightsText.text = "";
        for (int i = 0; i < highScoresData.Length; i++)
        {
            namesText.text   += (i + 1) + ". " + highScoresData[i].name;
            scoresText.text  +=                  highScoresData[i].score.ToString();
            heightsText.text += (((float)((int)(highScoresData[i].height * 1000f))) / 100f).ToString() + " cm";
            if (i <= highScoresData.Length - 1)
            {
                namesText.text   += "\n";
                scoresText.text  += "\n";
                heightsText.text += "\n";
            }
        }
    }

    Rect correctRect(Rect l_rect)
    {
        float l_x, l_y, l_width, l_height;

        l_width  =  (Screen.width  * l_rect.width )/100;
        l_height =  (Screen.height * l_rect.height)/100;
        l_x      = ((Screen.width  * l_rect.x     )/100) - (l_width /2);
        l_y      = ((Screen.height * l_rect.y     )/100) - (l_height/2);
        l_rect = new Rect (l_x, l_y, l_width, l_height);

        return l_rect;
    }

    void setActiveAll(bool l_bool)
    {
        int l, i, r, c;

        gameUIGO.SetActive(l_bool);
        playerGO.SetActive    (l_bool);
        topCameraGO.SetActive (l_bool);
        backCameraGO.SetActive(l_bool);
        sceneryGO.SetActive   (l_bool);

        for (l = 0; l < totalObjectTypes; l++)
            for (i = 0; i < visibleObjectsEach; i++)
                prop[l,i].setActive(l_bool);
        for(r=0; r<visibleRows; r++)
            for (c = 0; c < totalLevelColumns; c++)
                tile[r,c].GO.SetActive(l_bool);
        finishGO.SetActive(l_bool);
    }

    //--> Button Functions
    #region Button functions
    //--> --> Main Menu
    //--> --> --> Start
    void startGame()
    {
        playOnlyThis(buttonSound);
        Debug.Log("Start Button pressed at Main Menu");
        screen = screenEnum.findingStart;
        kinectTrackingGO.SetActive(true);
        findingStartTimer.setTimer(true);
        mainMenuGO.SetActive(false);
        findingStartGO.SetActive(true);

        kinectStartPositionList = new List<Vector3>();
    }
    //--> --> --> High Scores
    void gotoHighScores()
    {
        playOnlyThis(buttonSound);
        screen = screenEnum.highScores;
        Debug.Log("HighScores button pressed");
        mainMenuGO.SetActive  (false);
        highScoresGO.SetActive(true );
    }
    //--> --> --> Exit
    void exit()
    {
        Application.Quit();
    }

    //--> --> High Scores
    //--> --> --> Main Menu
    void goFromHSToMM()
    {
        playOnlyThis(buttonSound);
        screen = screenEnum.mainMenu;
        Debug.Log("Main Menu button pressed in High Scores menu");
        highScoresGO.SetActive(false);
        mainMenuGO.SetActive  (true );
    }

    //--> --> Pause
    //--> --> --> Resume
    void resumeGame()
    {
        playOnlyThis(buttonSound);
        screen = screenEnum.gameUI;
        Debug.Log("Resume button pressed");
        pauseMenuGO.SetActive(false);
        isGamePaused = false;
    }
    //--> --> --> Restart
    void pauseRestartGame()
    {
        playOnlyThis(levelStartSound);
        Debug.Log("Restart Button pressed at Pause menu");
        pauseMenuGO.SetActive(false);
        resetGameVariables(resetEnum.start);
    }
    //--> --> --> Main Menu
    void goFromPauseToMM()
    {
        playOnlyThis(buttonSound);
        screen = screenEnum.pauseMenu;
        Debug.Log("Main Menu button pressed at Pause menu");
        setActiveAll(false);
        pauseMenuGO.SetActive(false);
        mainMenuGO.SetActive(true);
    }

    //--> --> Game Over
    //--> --> --> Restart - Game Over
    void GORestartGame()
    {
        playOnlyThis(levelStartSound);
        Debug.Log("Restart Button pressed at Game Over");
        gameOverGO.SetActive(false);
        resetGameVariables(resetEnum.start);
    }
    //--> --> --> Main Menu - Game Over
    void goFromGOToMM()
    {
        playOnlyThis(buttonSound);
        screen = screenEnum.mainMenu;
        Debug.Log("Main Menu button pressed at Game Over menu");
        setActiveAll(false);
        gameOverGO.SetActive(false);
        mainMenuGO.SetActive(true);
        if (isGameWon == true && checkHighScoresData())
            saveHighScoresData();
    }

    //--> --> Finding Start
    //--> --> --> Main Menu
    void goFromFindingStartToMM()
    {
        kinectTrackingGO.SetActive(false);
        playOnlyThis(buttonSound);
        screen = screenEnum.mainMenu;
        Debug.Log("Main Menu button pressed at Finding Start");
        findingStartGO.SetActive(false);
        mainMenuGO.SetActive(true);
    }

    //--> --> Finding Difficulty
    //--> --> --> Start
    void startActualGame()
    {
        Debug.Log("Start Button pressed at Finding Difficulty");
        if (maxUp > 1.0f)
        {//add this///////////////////////////////////////////////////////
            findingDifficultyGO.SetActive(false);
            kinectRatio = maxUp / 3.0f;
            jumpThreshold = kinectStartPosition.y + jumpStartThreshold;
            duckThreshold = kinectStartPosition.y - duckStartThreshold;
            Debug.Log("Kinect Ratio, jump and duck Thresholds are " + kinectRatio + ", " + jumpThreshold + ", " + duckThreshold);
            maxUpPos = kinectStartPosition.y;
            resetGameVariables(resetEnum.start);
        }
        else
            Debug.Log("Error: maxUp is ver low");// add this//////////////////////
    }
    //--> --> Main Menu
    void goFromFindingDiffToMM()
    {
        playOnlyThis(buttonSound);
        screen = screenEnum.mainMenu;
        findingDifficultyGO.SetActive(false);
        mainMenuGO.SetActive(true);
    }
    #endregion
    #endregion

    //--------------------------- R E S E T ----------------------------//
    #region Reset
    void resetGameVariables(resetEnum l_reset)
    {
        float l_time;
        int r, c, i, l, rand;
        int[] unusedPackageIDs;

        //--> Game Start *
        if (l_reset == resetEnum.start)
        {
            player.currentLives = player.totalLives;
            player.currentHealth = player.totalHealth;
            player.isInvincible = false;
            currentLevel = startingLevel;
            score = 0;
            lastLevelScore = 0;
            maxUp = 0;

            kinectCalib = kinectEnum.start;
            for (i = 1; i < avgFrames; i++)
                kinectInput[i] = Vector3.zero;
            for (i = 0; i < prevFrame - 1; i++)
                kinectAvgInput[i].position = Vector3.zero;

            if (totalPackages > 1)
            {
                unusedPackageIDs = new int[totalPackages - 1];
                l_level = level[0];
                l_level.packageID = Random.Range(0, totalPackages);
                level[0] = l_level;
                for (l = 1; l < totalLevels; l++)
                {
                    r = 0;
                    for (i = 0; i < totalPackages; i++)
                    {
                        if (i != level[l - 1].packageID)
                        {
                            unusedPackageIDs[r] = i;
                            r++;
                        }
                    }
                    l_level = level[l];
                    l_level.packageID = unusedPackageIDs[Random.Range(0, totalPackages - 1)];
                    level[l] = l_level;
                }
            }
            else
            {
                for (l = 0; l < totalLevels; l++)
                {
                    l_level = level[l];
                    l_level.packageID = 0;
                    level[l] = l_level;
                }
            }
        }
        else
        {
            for (i = 1; i < avgFrames; i++)
                kinectInput[i] = kinectStartPosition;
            for (i = 0; i < prevFrame - 1; i++)
                kinectAvgInput[i].position = kinectStartPosition;
        }

        l_time = Time.time;
        for (i = 0; i < prevFrame - 1; i++)
        {
            kinectAvgInput[i].position = kinectStartPosition;
            kinectAvgInput[i].time = l_time;
        }

        //--> Game Objects
        #region Game Objects
        //--> --> Cameras
        #region Cameras
        /*Debug.Log("Cameras");*/
        backCameraGO.transform.position = resetPosition(backCameraGO, player.backCameraDistance);
        topCameraGO.transform.position  = resetPosition(topCameraGO,  player.topCameraDistance );
        #endregion

        //--> --> Player
        #region Player
        i=0;
        while(i==0)
            if(playerGO != null)
                break;
        player.invincibleEndDistance = player.startPosition.z;
        playerGO.transform.position = player.startPosition;
        player.action        = actionEnum.run;
        player.currentColumn = centerColumn;
        player.isKinectJumpInput = false;
        player.isJumpInput       = false;
        player.isLeapInput       = false;
        player.isLeapLeftInput   = false;
        player.isLeapRightInput  = false;
        player.isDuckInput       = false;
        player.isNoHurt          = false;
        player.currentHealth = player.totalHealth;
        player.blinkTimer.setTimer(false);
        player.lastHurtRow = -1;
        playerGO.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        player.currentTexture = 0;
        playerGO.GetComponent<Renderer>().material.mainTexture = runTexture[0];
        player.texType = texEnum.run;
        #endregion

        //--> --> Tiles
        #region Tiles
        startingRow       = initialRow;
        endingRow         = visibleRows - 1;
        actualStartingRow = initialRow;
        actualEndingRow1  = initialRow + endingRow;
        actualEndingRow2  = actualEndingRow1;
        for(l=0; l<totalObjectTypes; l++)
            for (i=0; i<visibleObjectsEach; i++)
            {
                prop[l,i].isActive = false;
                prop[l,i].setActive(false);
            }
        for(r=0; r<visibleRows; r++)
        {
            for(c=0; c<totalLevelColumns; c++)
                tile[endingRow, c].tileObjectID = -1;
            assignTiles(r, r, r+initialRow, false);
        }
        #endregion

        //--> --> Other Props
        sceneryGO.SetActive(true);
        sceneryGO.transform.position = resetPosition(sceneryGO, player.sceneryDistance);
        sceneryGO.GetComponent<Renderer>().material.mainTexture = package[level[currentLevel-1].packageID].scenery;
        #endregion

        //--> Other Variables & Functions
        #region Other Variables & Functions
        isGameWon    = false;
        isGamePaused = false;
        lastTileAwarded = -1;

        setActiveAll(true);
        nextLevelScreenTimer.setTimer(true);
        playOnlyThis(levelStartSound);
        nextLevelGO.SetActive(true);
        levelNumberText.text = currentLevel.ToString();
        screen = screenEnum.nextLevel;
        #endregion
    }

    void assignTiles(int r, int actualEndingRow1, int actualEndingRow2, bool isRecycling)
    {
        int l, i, c, rand;

        // setting Types
        for (c = 0; c < totalLevelColumns; c++)
        {
            // Tile Type
            tile[r, c].tileType = tileEnum.none;
            for (i = 0; i < tileRepresentation.Length; i++)
            {
                if (level[currentLevel - 1].tileDefinition[actualEndingRow1][c] == tileRepresentation[i])
                    tile[r, c].tileType = (tileEnum)i;
            }
                if (actualEndingRow1 == 66)
                {
                    Debug.Log(c + " " + (int)tile[r, c].tileType);
                }
            // Default Tile Type
            if (tile[r, c].tileType == tileEnum.none)
            {
                if (c < centerColumn && c > centerColumn)
                    tile[r, c].tileType = tileEnum.grass;
                else
                    tile[r, c].tileType = tileEnum.path;
            }
            // Power-up Type
            tile[r, c].powerUpType = powerUpEnum.none;
            for (i = 0; i < powerUpRepresentation.Length; i++)
            {
                if (level[currentLevel - 1].powerUpDefinition[actualEndingRow1][c] == powerUpRepresentation[i])
                    tile[r, c].powerUpType = (powerUpEnum)i;
            }
        }
        // setting Random Types
        if (tile[r, centerColumn].tileType == tileEnum.fullRowObstacle)
        {
            rand = Random.Range(0,2);
            if (rand == 0)
            {
                for (c = 0; c < totalLevelColumns; c++)
                {
                    if (tile[r, c].tileType == tileEnum.environment)
                    {
                        rand = Random.Range(0,2);
                        if (rand == 0)
                            tile[r, c].tileType = tileEnum.tree;
                        else
                            tile[r, c].tileType = tileEnum.bush;
                    }
                }
                tile[r, centerColumn].tileType = tileEnum.log;
            }
            else
                for (c = 0; c < totalLevelColumns; c++)
                    tile[r, c].tileType = tileEnum.river;
        }
        else
            for (c = 0; c < totalLevelColumns; c++)
            {

                if (tile[r, c].tileType == tileEnum.singleTileObstacle)
                {
                    rand = Random.Range(0,2);
                    if (rand == 0)
                        tile[r, c].tileType = tileEnum.stone;
                    else
                        tile[r, c].tileType = tileEnum.water;
                }
                else if (tile[r, c].tileType == tileEnum.environment)
                {
                    rand = Random.Range(0,2);
                    if (rand == 0)
                        tile[r, c].tileType = tileEnum.tree;
                    else
                        tile[r, c].tileType = tileEnum.bush;
                }
            }
        // setting Materials and positions
        for (c = 0; c < totalLevelColumns; c++)
        {
            //material
            tile[r,c].renderer.material = setTileMaterial(tile[r,c].tileType);
            tile[r,c].renderer.material.color = Color.white;
            tile[r,c].renderer.material.SetTextureOffset("_MainTex", Vector2.zero);
            //position
            setTilePosition(r, actualEndingRow2, c);

            if (isRecycling)
            {
                l = (int)tile[endingRow, c].tileType;
                if (l >= 0 && l < objectTypes)
                {
                    prop[l, tile[endingRow, c].tileObjectID].setActive(true);
                }
                l = (int)tile[endingRow, c].powerUpType;
                if (l >= 0 && l < powerUpTypes)
                {
                    prop[l + objectTypes, tile[endingRow, c].powerUpID].setActive(true);
                }
            }
        }

        if(tile[r,centerColumn].tileType == tileEnum.log)
        {
            tile[r,centerColumn-1].tileType = tileEnum.log;
            tile[r,centerColumn+1].tileType = tileEnum.log;
        }
        else if(tile[r,centerColumn].tileType == tileEnum.finish)
        {
            tile[r,centerColumn-1].tileType = tileEnum.finish;
            tile[r,centerColumn+1].tileType = tileEnum.finish;
            Vector3 l_position = finishGO.transform.position;
            finishGO.transform.position = new Vector3(l_position.x, l_position.y, tile[endingRow, centerColumn + 1].GO.transform.position.z);
        }
    }

    Material setTileMaterial(tileEnum l_tileType)
    {
        Material l_material;

        if (l_tileType == tileEnum.water)
            l_material = waterMaterial;
        else if (l_tileType == tileEnum.river)
            l_material = riverMaterial;
        else if (l_tileType == tileEnum.pond)
            l_material = lavaMaterial;
        else
        {
            l_material = tileDefaultMaterial;
            if (l_tileType == tileEnum.grass)
            {
                l_material.mainTexture = package[level[currentLevel - 1].packageID].grassTex;
            }
            else if (l_tileType == tileEnum.tree || l_tileType == tileEnum.bush)
                l_material.mainTexture = package[level[currentLevel-1].packageID].grassShadowTex;
            else if (l_tileType == tileEnum.stone)
                l_material.mainTexture = package[level[currentLevel-1].packageID].pathShadowTex;
            else
                l_material.mainTexture = package[level[currentLevel-1].packageID].pathTex;
        }

        return l_material;
    }

    void setTilePosition(int r, int l_row, int l_column)
    {
        int i, l, rand;
        float l_x, l_y, l_z;

        l_x = (float)((l_column - (totalLevelColumns / 2)) * tileSize);
        l_z = (l_row * tileSize);
        tile[r,l_column].GO.transform.position = new Vector3(l_x, 0.0f, l_z);

        l = (int)tile[r,l_column].tileType;
        if (l >= 0 && l < objectTypes)
        {
            for (i = 0; i < visibleObjectsEach; i++)
            {
                if (prop[l, i].isActive == false)
                {
                    tile[r, l_column].tileObjectID = i;
                    prop[l, i].isActive = true;
                    if(l == 0)
                    {
                        if(package[level[currentLevel-1].packageID].treeTex.Count == 1)
                            rand = 0;
                        else
                            rand = Random.Range(0,package[level[currentLevel-1].packageID].treeTex.Count);
                        prop[l, i].GO.GetComponent<Renderer>().material.mainTexture = package[level[currentLevel-1].packageID].treeTex[rand];
                    }
                    else if(l == 1)
                    {
                        if(package[level[currentLevel-1].packageID].bushTex.Count == 1)
                            rand = 0;
                        else
                            rand = Random.Range(0,package[level[currentLevel-1].packageID].bushTex.Count);
                        prop[l, i].GO.GetComponent<Renderer>().material.mainTexture = package[level[currentLevel-1].packageID].bushTex[rand];
                    }
                    else if(l == 2)
                    {
                        if(package[level[currentLevel-1].packageID].stoneTex.Count == 1)
                            rand = 0;
                        else
                            rand = Random.Range(0,package[level[currentLevel-1].packageID].stoneTex.Count);
                        prop[l, i].GO.GetComponent<Renderer>().material.mainTexture = package[level[currentLevel-1].packageID].stoneTex[rand];
                    }
                    break;
                }
            }
            l_y = prop[l,i].GO.transform.position.y;

            prop[l,i].GO.transform.position = new Vector3(l_x, l_y, l_z);
        }

        l = (int)tile[r,l_column].powerUpType;
        if (l >= 0 && l < powerUpTypes)
        {
            for (i = 0; i < visibleObjectsEach; i++)
                if (prop[l+objectTypes, i].isActive == false)
                {
                    tile[r,l_column].powerUpID = i;
                    prop[l+objectTypes, i].isActive = true;
                    break;
                }
            l_y = prop[l+objectTypes, tile[r,l_column].powerUpID].GO.transform.position.y;
            prop[l+objectTypes, tile[r,l_column].powerUpID].GO.transform.position = new Vector3(l_x, l_y, l_z);
        }
    }

    Vector3 resetPosition(GameObject l_GO, Vector3 l_distance)
    {
        Vector3 l_position = new Vector3(player.startPosition.x + l_distance.x,
            player.startPosition.y + l_distance.y,
            player.startPosition.z + l_distance.z);
        return l_position;
    }
    #endregion
	
    //-------------------------- U P D A T E ---------------------------//
    #region Update
	void Update()
    {
        int i;

        //--> Kinect Input & Average
        if ((screen == screenEnum.findingDifficulty ||
             screen == screenEnum.gameUI) &&
            spineBase.isTracked == true)
        {
            for (i=avgFrames-1; i>0; i--)
                kinectInput[i] = kinectInput[i-1];
            kinectInput[0] = spineBase.currentPosition;

            for (i=prevFrame-1; i>0; i--)
                kinectAvgInput[i] = kinectAvgInput[i-1];

            kinectAvgInput[0].position = Vector3.zero;
            for (i = 0; i < avgFrames; i++)
                kinectAvgInput[0].position = kinectAvgInput[0].position + kinectInput[i];
            kinectAvgInput[0].position = kinectAvgInput[0].position / avgFrames;
            kinectAvgInput[0].time = Time.time;
        }

        if (screen == screenEnum.gameUI)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                audioSource[0].Stop();
                screen = screenEnum.pauseMenu;
                pauseMenuGO.SetActive(true);
                return;
            }
            movePlayer();
            updateWorld();
            checkCollisions();
        }
        updateUI();
    }

    //--> Player
    #region Player
    void movePlayer()
    {
        Vector3 l_playerPosition;
        float l_posX, l_posY;
        float l_distance, l_timeDifference;

        //--> Processing Kinect input
        if (spineBase.isTracked)
        {
            //--> jump
            if (kinectAvgInput[0].position.y > jumpThreshold && kinectAvgInput[1].position.y <= jumpThreshold && player.isKinectJumpInput == false)
            {
                player.isKinectJumpInput = true;
                kinectSays = "Jump";
                l_distance = kinectAvgInput[startFrame].position.y - kinectAvgInput[prevFrame - 1].position.y;
                l_timeDifference = kinectAvgInput[startFrame].time - kinectAvgInput[prevFrame - 1].time;
                player.kinectJumpUpSetSpeed = ((l_distance / l_timeDifference) * 1.2f) / kinectRatio; // Kinect Ratio - Jump Speed
                if (player.kinectJumpUpSetSpeed > player.kinectJumpUpClampSpeed)
                {
                    player.kinectJumpUpSetSpeed = player.kinectJumpUpClampSpeed; /////////
                    /*Debug.Log("Speed clamped to " + player.kinectJumpUpSetSpeed);*/
                }
            }

            //--> duck
            else if (kinectAvgInput[0].position.y < duckThreshold)
            {
                player.isDuckInput = true;
                kinectSays = "Duck";
            }

            //--> sideways
            l_posX = (float)((kinectAvgInput[0].position.x - kinectStartPosition.x) * (1.25f/kinectRatio));
            if(l_posX > player.startPosition.x + (tileSize * 1.1f))
                l_posX = (tileSize * 1.1f);
            else if(l_posX < player.startPosition.x - (tileSize * 1.1f))
                l_posX = -(tileSize * 1.1f);
            l_playerPosition = playerGO.transform.position;
            playerGO.transform.position = new Vector3(l_posX, l_playerPosition.y, l_playerPosition.z);
        }

        //--> --> Run
        #region Run
        playerGO.transform.Translate(new Vector3(0.0f, 0.0f, player.runSpeed)*Time.deltaTime, Space.World);
        backCameraGO.transform.position = moveAlongPlayer(backCameraGO, player.backCameraDistance);
        topCameraGO.transform.position  = moveAlongPlayer(topCameraGO,  player.topCameraDistance );
        sceneryGO.transform.position    = moveAlongPlayer(sceneryGO,    player.sceneryDistance   );
        if(player.action == actionEnum.run && player.changeTextureEndTime < Time.time)
        {
            player.currentTexture = (player.currentTexture + 1) % runTexture.Length;
            player.renderer.material.mainTexture = runTexture[player.currentTexture];
            player.changeTextureEndTime = Time.time + player.changeTextureTime;
        }

        #endregion

        //--> --> Jump
        #region Jump
        //--> --> --> Start
        if(player.action == actionEnum.run)
        {
            if(!GetComponent<AudioSource>().isPlaying)
                audioSource[0].Play();
            
            if(player.isKinectJumpInput == true)
            {
                audioSource[0].Stop();
                Debug.Log("Jump Start");
                play(playerJumpSound);

                setPlayerSpeeds(actionEnum.jump, player.kinectJumpUpSetSpeed, player.kinectJumpDownSpeed, 0.0f);
                player.isKinectJumpInput = false;

                player.texType = texEnum.jumpStart;
                player.currentTexture = 0;
                player.renderer.material.mainTexture = jumpStartTexture[0];
            }
        }
        //--> --> --> Perform
        if  (
            player.action == actionEnum.jump      ||
            player.action == actionEnum.leap      ||
            player.action == actionEnum.leapLeft  ||
            player.action == actionEnum.leapRight
        )
        {
            playerGO.transform.Translate(new Vector3( player.currentSideSpeed,
                                                      player.currentUpSpeed,
                                                      0.0f                    )*Time.deltaTime, Space.World);

            l_playerPosition = playerGO.transform.position;

            for(int i=0; i<player.jumpHeightLevel.Length; i++)
            {
                if(l_playerPosition.y < (player.startPosition.y + player.jumpHeightLevel[i]))
                {
                    if(player.currentUpSpeed > 0)
                    {
                        if(player.currentTexture != i)
                        {
                            player.currentTexture = i;
                            player.renderer.material.mainTexture = jumpStartTexture[i];
                        }
                        break;
                    }
                    else if(player.currentUpSpeed <= 0)
                    {
                        if(player.texType == texEnum.jumpStart)
                        {
                            player.texType = texEnum.jumpEnd;
                            player.currentTexture = i;
                            player.renderer.material.mainTexture = jumpEndTexture[i];
                        }
                        if(player.currentTexture != i)
                        {
                            player.currentTexture = i;
                            player.renderer.material.mainTexture = jumpEndTexture[i];
                        }
                        break;
                    }
                }
            }
            
            player.currentUpSpeed = player.currentUpSpeed - (player.currentDownSpeed * Time.deltaTime);

            //--> --> --> End
            if(l_playerPosition.y < player.startPosition.y)
            {
                Debug.Log("Jump End");
                play(playerLandSound);

                playerGO.transform.position = new Vector3( l_playerPosition.x,
                    player.startPosition.y,
                    l_playerPosition.z      );
                player.action = actionEnum.run;
                player.isJumpInput      = false;
                player.isLeapInput      = false;
                player.isLeapLeftInput  = false;
                player.isLeapRightInput = false;

                player.texType = texEnum.run;
                player.currentTexture = 0;
                player.renderer.material.mainTexture = runTexture[0];
                player.changeTextureEndTime = Time.time + player.changeTextureTime;
            }
        }
        #endregion

        //--> --> Duck
        #region Duck
        //--> --> --> Start
        if(player.action == actionEnum.run && player.isDuckInput == true)
        {
            audioSource[0].Stop();
            Debug.Log("Duck Start");
            play(playerDuckSound);

            player.action = actionEnum.duck;

            player.texType = texEnum.duckStart;
            player.currentTexture = 0;
            player.renderer.material.mainTexture = duckStartTexture[0];
            player.changeTextureEndTime = Time.time + player.changeTextureTime;
        }
        //--> --> --> Perform
        if(player.action == actionEnum.duck)
        {
            
            //--> --> --> End
            l_playerPosition = playerGO.transform.position;
            if(kinectAvgInput[0].position.y > duckThreshold)
            {
                Debug.Log("Duck End");
                playerGO.transform.position = new Vector3(l_playerPosition.x, player.startPosition.y, l_playerPosition.z);
                player.action = actionEnum.run;
                player.isDuckInput = false;

                player.texType = texEnum.run;
                player.currentTexture = 0;
                player.renderer.material.mainTexture = runTexture[0];
                player.changeTextureEndTime = Time.time + player.changeTextureTime;
            }
            else
            {
                if(player.texType == texEnum.duckStart && player.changeTextureEndTime < Time.time)
                {
                    player.currentTexture = player.currentTexture + 1;
                    if(player.currentTexture >= duckStartTexture.Length)
                    {
                        player.texType = texEnum.duck;
                        player.currentTexture = 0;
                        player.renderer.material.mainTexture = duckTexture[0];
                        player.changeTextureEndTime = Time.time + (player.changeTextureTime / 4.0f);
                    }
                    else
                    {
                        player.renderer.material.mainTexture = duckStartTexture[player.currentTexture];
                        player.changeTextureEndTime = Time.time + player.changeTextureTime;
                    }
                }
                else if(player.changeTextureEndTime < Time.time)
                {
                    player.currentTexture = (player.currentTexture + 1) % duckTexture.Length;
                    player.renderer.material.mainTexture = duckTexture[player.currentTexture];
                    player.changeTextureEndTime = Time.time + (player.changeTextureTime / 4.0f);
                }
                l_posY = (float)(((kinectAvgInput[0].position.y - duckThreshold) * (1.5f/kinectRatio)) + player.startPosition.y);
                if(l_posY < 0)
                    l_posY = 0;
                playerGO.transform.position = new Vector3(l_playerPosition.x, l_posY, l_playerPosition.z);
            }
        }
        #endregion
    }

    void setPlayerSpeeds(actionEnum l_action, float l_upSpeed, float l_downSpeed, float l_sideSpeed)
    {
        player.action           = l_action;
        player.currentUpSpeed   = l_upSpeed;
        player.currentDownSpeed = l_downSpeed;
        player.currentSideSpeed = l_sideSpeed;
    }

    Vector3 moveAlongPlayer(GameObject l_GO, Vector3 l_distance)
    {
        Vector3 l_playerPosition, l_objectPosition;

        l_playerPosition = playerGO.transform.position;
        l_objectPosition = l_GO.transform.position;
        l_objectPosition = new Vector3(l_objectPosition.x, l_objectPosition.y, l_playerPosition.z + l_distance.z);
        return l_objectPosition;
    }
    #endregion

    //--> Other Props
    void updateWorld()
    {
        int r, c, i, l;
        bool l_found;
        float l_z, l_offset;

        l_z = tile[startingRow, 0].GO.transform.position.z - playerGO.transform.position.z;

        // Score
        if (l_z < -(tileSize / 2))
        {
            if (lastTileAwarded < actualStartingRow && player.lastHurtRow < actualStartingRow)
            {
                l_found = false;
                for (c = centerColumn - 1; c <= centerColumn + 1; c++)
                {
                    if (tile[startingRow, c].tileType != tileEnum.path)
                        l_found = true;
                }
                if (l_found)
                {
                    lastTileAwarded = actualStartingRow;
                    score = score + 10;
                }
            }
        }

        // River Animation
        for (r = 0; r < visibleRows; r++)
        {
            for (c = 0; c < totalLevelColumns; c++)
            {
                if(tile[r,c].tileType == tileEnum.river)
                {
                    l_offset = Time.time * 0.5f;
                    tile[r, c].GO.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(l_offset, 0));
                }
            }
        }

        // Recycling Tiles
        if(l_z < -tileSize)
        {
            endingRow = startingRow;
            startingRow++;
            if(startingRow >= visibleRows)
                startingRow = 0;

            actualStartingRow++;
            actualEndingRow1++;
            actualEndingRow2++;
            if(actualEndingRow1 >= level[currentLevel-1].totalRows)
                actualEndingRow1 = initialRow;

            for (c = 0; c < totalLevelColumns; c++)
            {
                // Deactivating assigned Objects
                l = (int)tile[endingRow, c].tileType;
                if (l >= 0 && l < objectTypes)
                {
                    if (tile[endingRow, c].tileType == tileEnum.log)
                    {
                        if (c == centerColumn)
                        {
                        
                            prop[l, tile[endingRow, c].tileObjectID].isActive = false;
                            prop[l, tile[endingRow, c].tileObjectID].setActive(false);
                        }
                    }
                    else
                    {
                        prop[l, tile[endingRow, c].tileObjectID].isActive = false;
                        prop[l, tile[endingRow, c].tileObjectID].setActive(false);
                    }
                    tile[endingRow, c].tileObjectID = -1;
                }
                l = (int)tile[endingRow, c].powerUpType;
                if (l >= 0 && l < powerUpTypes)
                {
                    prop[l + objectTypes, tile[endingRow, c].powerUpID].isActive = false;
                    prop[l + objectTypes, tile[endingRow, c].powerUpID].setActive(false);
                }
            }

            assignTiles(endingRow, actualEndingRow1, actualEndingRow2, true);
        }
    }

    //--> Collision
    #region Collision
    void checkCollisions()
    {
        int i, r, c;
        Vector3 l_playerPos = playerGO.transform.position;
        Vector3 l_tilePos;
        float l_diffZ;
        tileEnum l_tileType;

        r = startingRow;
        // Each row
        for(i=0; i<2; i++)
        {
            l_diffZ = tile[r, 0].GO.transform.position.z - l_playerPos.z;
            // Any column
            //--> --> Power-up
            for (c = centerColumn - 1; c <= centerColumn + 1; c++)
            {
                l_tilePos = tile[r, c].GO.transform.position;
                if  (
                        (
                            tile[r, c].powerUpType == powerUpEnum.health     ||
                            tile[r, c].powerUpType == powerUpEnum.invincible
                        ) &&
                        (
                            Mathf.Abs(l_diffZ                    ) < ( tileSize * 0.1f                  ) &&
                            Mathf.Abs(l_tilePos.x - l_playerPos.x) < ((tileSize * 0.4f)+(player.width/2))
                        )
                    )
                    takePowerUp(r, c, tile[r, c].powerUpType);
            }

            //--> --> Finish
            if (
                i == 1 &&
                tile[r, centerColumn].tileType == tileEnum.finish &&
                Mathf.Abs(l_diffZ) < (tileSize * 0.45f)) // 2nd row  - finish line
            {
                scoreText.text = score.ToString();
                playOnlyThis(finishSound);
                Debug.Log("Finish screen");
                finishScreenTimer.setTimer(true);
                screen = screenEnum.finish;
                lastLevelScore = score;
            }

            //--> --> Log, objects which cover whole row
            else if (
                        i == 1 && // 2nd row only
                        (
                            player.action == actionEnum.run || // Run, Jump
                            player.action == actionEnum.jump
                        ) &&
                        tile[r, centerColumn].tileType == tileEnum.log &&
                        Mathf.Abs(l_diffZ) < (tileSize * 0.1f)
                    )
                playerHurt(l_diffZ, actualStartingRow + i, r, centerColumn);
            
            else if (
                        player.action == actionEnum.duck && // Duck
                        tile[r, centerColumn].tileType == tileEnum.log &&
                        (
                            Mathf.Abs(l_diffZ) < (tileSize * 0.1f              ) &&
                            l_playerPos.y      > (player.startPosition.y - 1.0f)
                        )
                    )
                    playerHurt(l_diffZ, actualStartingRow + i, r, centerColumn);

            //--> --> Remaining
            else
                // Each column
                for (c = centerColumn - 1; c <= centerColumn + 1; c++)
                {
                    l_tileType = tile[r, c].tileType;
                    l_tilePos = tile[r, c].GO.transform.position;

                    // 2D
                    if  (
                            (
                                player.action == actionEnum.run || // Run, Duck
                                player.action == actionEnum.duck
                            ) &&
                            (
                                l_tileType == tileEnum.pond  || // Objects with no height
                                l_tileType == tileEnum.water ||
                                l_tileType == tileEnum.river
                            ) &&
                            (
                                Mathf.Abs(l_diffZ                    ) < ( tileSize * 0.45f                  ) &&
                                Mathf.Abs(l_tilePos.x - l_playerPos.x) < ((tileSize * 0.45f)+(player.width/2))
                            )
                        )
                        playerHurt(l_diffZ, actualStartingRow+i, r, c);
                    
                    else if (
                                (
                                    player.action == actionEnum.run || //Run, Duck
                                    player.action == actionEnum.duck
                                ) &&
                                l_tileType == tileEnum.stone && // Stone
                                (
                                    Mathf.Abs(l_diffZ                    ) < ( tileSize * 0.1f                  ) &&
                                    Mathf.Abs(l_tilePos.x - l_playerPos.x) < ((tileSize * 0.4f)+(player.width/2))
                                )
                            )
                            playerHurt(l_diffZ, actualStartingRow+i, r, c);

                    // 3D
                    else if (
                                player.action == actionEnum.jump && // Jump
                                l_tileType == tileEnum.stone && // Stone
                                (
                                    Mathf.Abs(l_diffZ                    ) < ( tileSize * 0.1f                          ) &&
                                    Mathf.Abs(l_playerPos.x - l_tilePos.x) < ((tileSize * 0.4f )+(player.width/2)       ) &&
                                             (l_playerPos.y - l_tilePos.y) < ((tileSize * 0.45f)+ player.startPosition.y)
                                )
                            )
                            playerHurt(l_diffZ, actualStartingRow+i, r, c);
                }
            r++;
            if (r == visibleRows)
                r = 0;
        }

        //--> --> PowerUps
        if (player.isNoHurt == true && playerGO.transform.position.z > player.noHurtEndDistance)
            player.isNoHurt = false;
        if (player.isInvincible == true && l_playerPos.z > player.invincibleEndDistance)
        {
            player.isInvincible = false;
            Color l_color = player.renderer.material.color;
            player.renderer.material.color = new Color(l_color.r, l_color.g, l_color.b, 1.0f);
        }
    }

    void takePowerUp(int r, int c, powerUpEnum l_type)
    {
        prop[(int)tile[r, c].powerUpType+objectTypes, tile[r, c].powerUpID].isActive = false;
        prop[(int)tile[r, c].powerUpType+objectTypes, tile[r, c].powerUpID].setActive(false);
        tile[r, c].powerUpType = powerUpEnum.none;
        Debug.Log("Player hit powerUp");

        if (l_type == powerUpEnum.health)
        {
            play(healthPowerUpSound);
            player.currentHealth = player.totalHealth;
        }
        else if (l_type == powerUpEnum.invincible)
        {
            play(invinciblePowerUpSound);
            player.isInvincible = true;
            player.invincibleEndDistance = playerGO.transform.position.z + player.invincibleDistance;
            Color l_color = player.renderer.material.color;
            player.renderer.material.color = new Color(l_color.r, l_color.g, l_color.b, 0.5f);
        }

    }

    void playerHurt(float l_distance, int l_row, int r, int c)
    {
        float l_z;

        if (player.isNoHurt == false && player.isInvincible == false)
        {
            l_z = tile[r, c].GO.transform.position.z - playerGO.transform.position.z;
            play(playerHurtSound);
            tile[r, c].GO.GetComponent<Renderer>().material.color = Color.red;
            player.currentHealth = player.currentHealth - player.removeHealth;
            score = score - 10;
            if (score < 0)
                score = 0;
            if (player.currentHealth <= 0)
            {
                score = lastLevelScore;
                player.currentHealth = 0;
                player.currentLives--;
                if (player.currentLives < 0)
                    player.currentLives = 0;
                float l_sizeX = originalHealthBarSize.x * (player.currentHealth / player.totalHealth);
                healthBar.rectTransform.sizeDelta = new Vector2(l_sizeX, originalHealthBarSize.y);
                livesText.text = player.currentLives.ToString();
                audioSource[0].Stop();
            Debug.Log("Lost Life screen" + player.currentLives);
                lostLifeScreenTimer.setTimer(true);
                screen = screenEnum.lostLife;
            }
            else
            {
                player.lastHurtRow = l_row;
                player.isNoHurt = true;
                player.noHurtEndDistance = playerGO.transform.position.z + l_distance + (tileSize / 2);
            }
        }
        else
        {
            if(tile[r, c].GO.GetComponent<Renderer>().material.color != Color.red)
                tile[r, c].GO.GetComponent<Renderer>().material.color = Color.blue;
        }
        
    }
    #endregion

    //--> UI
    void updateUI()
    {
        int l_count, i;
        float l_time;

        //--> --> Kinect Tracking
        if(spineBase.isTracked)
            kinectIndicator.color = Color.green;
        else
            kinectIndicator.color = Color.red;
        //--> --> Screens
        //--> --> --> Main Menu
        if (audioLevel != audioToggle.isOn)
            changeAudioLevel(audioToggle.isOn);

        //--> --> --> Finding Start
        if (screen == screenEnum.findingStart)
        {
            if (spineBase.isTracked == true)
                kinectStartPositionList.Add(spineBase.currentPosition);
            else
                findingStartTimer.endTime += Time.deltaTime;//add this/////////////////////////////
            
            l_time = findingStartTimer.endTime - Time.time;
            findingStartTimeText.text = (((float)((int)(l_time * 100f))) / 100f).ToString() + " secs";

            if(findingStartTimer.isTimerEnded())
            {
                kinectStartPosition = Vector3.zero;
                l_count = kinectStartPositionList.Count;
                for (i = 0; i < l_count; i++)
                    kinectStartPosition = kinectStartPosition + kinectStartPositionList[i];
                kinectStartPosition = kinectStartPosition / l_count;

                /*kinectStartPosition = new Vector3(0,0,0); */// remove this ////////////////////////////////////////////////

                Debug.Log("Kinect Calibration Ended. Start Position is " + kinectStartPosition + ", from " + l_count + " values");

                for (i = 0; i < avgFrames - 1; i++)
                    kinectInput[i] = kinectStartPosition;

                l_time = Time.time;
                for (i = 0; i < prevFrame - 1; i++)
                {
                    kinectAvgInput[i].position = kinectStartPosition;
                    kinectAvgInput[i].time = l_time;
                }

                maxUpPos = kinectStartPosition.y;
                maxUp = 0.0f;

                Debug.Log("Moving to Finding Difficulty");
                screen = screenEnum.findingDifficulty;
                findingStartTimer.setTimer(false);
                findingStartGO.SetActive(false);
                findingDifficultyGO.SetActive(true);
            }
        }

        //--> --> --> Finding Difficulty
        if(screen == screenEnum.findingDifficulty)
        {
            if (kinectAvgInput[0].position.y > maxUpPos)
            {
                maxUpPos = kinectAvgInput[0].position.y;
                maxUp = kinectAvgInput[0].position.y - kinectStartPosition.y;
            }
            maxUpText.text = (((float)((int)(maxUp * 1000f))) / 100f).ToString() + " cm";
        }

        //--> --> --> game UI
        else if (screen == screenEnum.gameUI)
        {
            if (kinectAvgInput[0].position.y > maxUpPos)
            {
                maxUpPos = kinectAvgInput[0].position.y;
                maxUp = kinectAvgInput[0].position.y - kinectStartPosition.y;
            }
            maxHeightText.text = (((float)((int)(maxUp * 1000f))) / 100f).ToString() + " cm";
            livesText.text = player.currentLives.ToString();
            scoreText.text = score.ToString();
            levelText.text = currentLevel.ToString();

            float l_sizeX = originalHealthBarSize.x * (player.currentHealth / player.totalHealth);
            healthBar.rectTransform.sizeDelta = new Vector2(l_sizeX, originalHealthBarSize.y);

            l_sizeX = player.invincibleEndDistance - playerGO.transform.position.z;
            if (l_sizeX > 0)
                l_sizeX = originalInvincibleBarSize.x * (l_sizeX / player.invincibleDistance);
            else
                l_sizeX = 0;
            invincibleBar.rectTransform.sizeDelta = new Vector2(l_sizeX, originalInvincibleBarSize.y);
        }

        //--> --> Next Level
        else if (screen == screenEnum.nextLevel && nextLevelScreenTimer.isTimerEnded())
        {
            nextLevelScreenTimer.isSet = false;
            screen = screenEnum.gameUI;
            nextLevelGO.SetActive(false);

            player.renderer.material.mainTexture = runTexture[0];
            player.changeTextureEndTime = Time.time + player.changeTextureTime;
        }

        //--> --> Finish
        else if (screen == screenEnum.finish && finishScreenTimer.isTimerEnded())
        {
            currentLevel++;
            Debug.Log("Level increased to " + currentLevel + ", totalLevels " + totalLevels);

            finishGO.SetActive(false);

            if (currentLevel > totalLevels)
            {
                isGameWon = true;
                playOnlyThis(gameWinSound);
                screen = screenEnum.gameOver;
                gameOverGO.SetActive(true);
                lostGO.SetActive(false);
                winGO.SetActive(true);
                if (checkHighScoresData())
                {
                    gameOverText.text = "High ";
                    enterNameGO.SetActive(true);
                }
                else
                {
                    gameOverText.text = "        ";
                    enterNameGO.SetActive(false);
                }
                string s = (((float)((int)(maxUp * 1000f))) / 100f).ToString() + " cm";
                gameOverText.text += "Score   " + score + "\nMax Height   " + s;
            }
            else
            {
                finishScreenTimer.isSet = false;
                resetGameVariables(resetEnum.reset);
            }
        }

        //--> --> Lost Life
        else if (screen == screenEnum.lostLife && lostLifeScreenTimer.isTimerEnded())
        {
            lostLifeGO.SetActive(false);
            lostLifeScreenTimer.isSet = false;
            if (player.currentLives <= 0)
            {
                setActiveAll(false);
                gameOverGO.SetActive(true);
                winGO.SetActive(false);
                lostGO.SetActive(true);

                isGameWon = false;
                playOnlyThis(gameLostSound);
                screen = screenEnum.gameOver;
                string s = (((float)((int)(maxUp * 1000f))) / 100f).ToString() + " cm";
                gameOverText.text = "        Score   " + score + "\nMax Height   " + s;
            }
            else
                resetGameVariables(resetEnum.reset);
        }
    }
    #endregion
}