using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class GenerateTargetsScript : MonoBehaviour {

	// This script should handle the tutorial level as well as changing modes (playing alone, with peer, etc)

	public GameObject targetPrefab;
    public GameObject virtualPeer;
    public GameObject barGraph;
    public GameObject loadingScreen;
    public GameObject pieChart;
    public GameObject postGameScreen; //The "you won by X points" message
    public GameObject[] scoreBoardElements; //for convenience, during the tutorial this will be moved out of the way.
    public Text countdownText;
    public Text beginText;
    public RawImage controllerImage;
    //float time = 0.0f;
    float timeUser = 0.0f;
    float timePeer = 0.0f;
    float userRandomNoiseTimer = 0.0f;
    float userTimeUntilNextRandomNoiseAssignment = 0f;
    float peerRandomNoiseTimer = 0.0f;
    float peerTimeUntilNextRandomNoiseAssignment = 0f;
    int targetIndex = 0;
	public static int totalTrajectories = 500;
	bool generatedTargetForUser = false; //So more than 1 aren't generated in the interval
    bool generatedTargetForPeer = false;
	float randomTimeUser = -1f; //-1 if a random number hasn't been generated: this will be reset to -1 after every 2 second interval:
    float randomTimePeer = -1f;
    public Material material1;
    public Material material2;
    float totalTime = 0f;
    float waitingTime = 0f;
    bool deletedAllTargets = false;

    public static bool waiting = false;
    public static int numUserTargets = 0;
    public static int numPeerTargets = 0;
    public int userTargetsCap;
    int userTargetsNoise = 0; //-2 through 2, equal prob.
    public int peerTargetsCap;
    int peerTargetsNoise = 0; //-2 through 2, equal prob.

    public static float userInterval = 2.0f;
    public static float peerInterval = 2.0f;

    static int scoreDifference = 0;
    public static bool inTutorial = true;
    public static bool inPracticeRound = false;
    public static int stepOfTutorial = 1;
    static bool startedWaitingForPeer = false;

    float setWaitingTime = 0f;

    List<string> modes;
    public static string currentMode = "";
    public int GAME_TIME = 120; //2 minutes
    float countdownTimer;

    float timeSpentOnTutorialStep = 0f;

    bool gameIsOver = false;

    Vector3 countdownTimerPosition;

    ////////////
    //Toggle these to change how targets are generated, only make 1 true:
    ////////////

    public bool normalMode; //Targets generated in 2 second intervals with uniform probability
    public bool increaseIntervalBasedOnDensityMode; //Targets generated in X second intervals with intervals increasing with number of targets existing
    public bool constantBalloonsWithNoise; //A constant of X + w user&peer balloons should exist on average in this mode

    public int performanceConstant;

    void Start () {
        countdownTimer = GAME_TIME;
        countdownText.text = "";
        //postGameScreen.SetActive(false);
        postGameScreen.GetComponentInChildren<Text>().enabled = false;
        userTimeUntilNextRandomNoiseAssignment = Random.Range(7f, 12f);
        peerTimeUntilNextRandomNoiseAssignment = Random.Range(7f, 12f);
        userTargetsNoise = Random.Range(-1, 2); 
        peerTargetsNoise = Random.Range(-1, 2);

        if (constantBalloonsWithNoise)
        {
            userInterval = 2.0f;
            peerInterval = 2.0f;
        }

        modes = new List<string>();
        modes.Add("Alone");
        modes.Add("Equally perform");
        modes.Add("Underperform");
        modes.Add("Overperform");


        ///////////////

        foreach (GameObject i in scoreBoardElements)
        {
            i.transform.localPosition = new Vector3(i.transform.localPosition.x, i.transform.localPosition.y + 100f, i.transform.localPosition.z);
        }

        if (barGraph.activeInHierarchy)
        {
            barGraph.transform.parent.localPosition =
                new Vector3(barGraph.transform.parent.localPosition.x, barGraph.transform.parent.localPosition.y + 100f, barGraph.transform.parent.localPosition.z);
        }
        else
        {
            pieChart.transform.localPosition =
                new Vector3(pieChart.transform.localPosition.x, pieChart.transform.localPosition.y + 100f, pieChart.transform.localPosition.z);
        }
        /////////////////

        countdownTimerPosition = countdownText.gameObject.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetShootScript.playing)
        {
            if (Input.GetKeyDown(KeyCode.Space) && inTutorial) //Our shortcut to skip the tutorial
            {
                postGameScreen.GetComponentInChildren<Text>().enabled = true;
                postGameScreen.SetActive(false);
                beginText.gameObject.SetActive(false);
                controllerImage.gameObject.SetActive(false);

                TargetShootScript.userScore = 0;
                TargetShootScript.scoreTextChangeFromOutside.text = "You       0";
                VirtualPeerBehavior.peerPoints = 0;
                VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer      0";

                if (scoreBoardElements[0].transform.localPosition.y > -100f)
                {
                    foreach (GameObject i in scoreBoardElements)
                    {
                        i.transform.localPosition = new Vector3(i.transform.localPosition.x, i.transform.localPosition.y - 100f, i.transform.localPosition.z);
                    }
                }

                for (int i = scoreBoardElements.Length-1; i >= 0; i--)
                {
                    scoreBoardElements[i].transform.SetAsFirstSibling();
                }

                if (barGraph.activeInHierarchy)
                {
                    if (barGraph.transform.parent.localPosition.y > -100f)
                    {
                        barGraph.transform.parent.localPosition =
                        new Vector3(barGraph.transform.parent.localPosition.x, barGraph.transform.parent.localPosition.y - 100f, barGraph.transform.parent.localPosition.z);
                    }
                }
                else
                {
                    if (pieChart.transform.localPosition.y > -100f)
                    {
                        pieChart.transform.localPosition =
                            new Vector3(pieChart.transform.localPosition.x, pieChart.transform.localPosition.y - 100f, pieChart.transform.localPosition.z);
                    }
                }
                inTutorial = false;
            }
            if (inTutorial)
            {
                /*
                if (stepOfTutorial == 0)
                {
                    
                    timeSpentOnTutorialStep += Time.deltaTime;

                    beginText.GetComponent<RectTransform>().sizeDelta = new Vector2(590f, 17.8f);
                    controllerImage.gameObject.SetActive(true);
                    controllerImage.GetComponent<RectTransform>().sizeDelta = new Vector2(0.2f, 0.2f);
                    controllerImage.transform.localPosition = new Vector3(-34.2f, -196.1f, 13.73f);

                    postGameScreen.SetActive(true);
                    beginText.gameObject.SetActive(true);

                    beginText.color = Color.white;
                    postGameScreen.GetComponentInChildren<Text>().enabled = false;
                    beginText.fontSize = 35;
                    beginText.text = "Hello and thank you for your cooperation\n\n" +
                        "You will now go through a brief tutorial\n\n\n        (Press this button to continue)";
                    

                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;

                        beginText.text = "You will see 2 sets of balloons, colored <color=blue>BLUE</color> and " +
                            "<color=red>RED</color>.\n\nYour goal is to aim at and pop the <color=blue>BLUE</color> balloons." +
                            "\n\n\n        (Press this button to continue)";
                        controllerImage.transform.localPosition = new Vector3(-34.225f, -196.142f, 13.73f);
                    }
                }*/
                if (stepOfTutorial == 1)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;

                    beginText.gameObject.SetActive(true);
                    controllerImage.gameObject.SetActive(true);
                    beginText.text = "In this game, you will see two sets of balloons floating around, colored <color=blue>BLUE</color> and " +
                            "<color=red>RED</color>.\n\nYour goal is to aim at and pop <b>only</b> <color=blue>BLUE</color> balloons." +
                            "\n\n\n        (Press this button to continue)";
                    controllerImage.transform.localPosition = new Vector3(-34.225f, -196.187f, 13.73f);

                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;

                        controllerImage.transform.localPosition = new Vector3(-34.225f, -196.137f, 13.73f);

                        beginText.text = "For practice, a <color=blue>BLUE</color> balloon will now appear.\n\nTry to pop it by pressing the button near your index finger." +
                            "\n\n\n        (Press this button to continue)";
                    }
                }
                else if (stepOfTutorial == 2)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;

                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        stepOfTutorial++;

                        Vector3 vec = GameObject.Find("LeftEyeAnchor").transform.forward * 5f + new Vector3(0, 5, 0); //Instantiate a balloon right in front of their eyes

                        beginText.gameObject.SetActive(false);
                        controllerImage.gameObject.SetActive(false);
                        postGameScreen.SetActive(false);

                        GameObject targetPrefReal = Instantiate(targetPrefab) as GameObject;
                        targetPrefReal.transform.localPosition = vec;
                        targetPrefReal.transform.LookAt(new Vector3(0, 5, 0));
                        targetPrefReal.transform.GetChild(0).GetChild(0).LookAt(new Vector3(0, 100000, 0));
                        targetPrefReal.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().material = material1;
                        targetPrefReal.tag = "UserTarget";
                        numUserTargets++;
                        timeSpentOnTutorialStep = 0f;
                    }
                }
                else if (stepOfTutorial == 3)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if (numUserTargets == 0 && timeSpentOnTutorialStep >= 0.2f) //They popped the demo balloon and can go to the next step:
                    {
                        beginText.gameObject.SetActive(true);
                        controllerImage.gameObject.SetActive(true);
                        postGameScreen.SetActive(true);

                        stepOfTutorial++;

                        controllerImage.transform.localPosition = new Vector3(-34.169f, -195.992f, 13.73f);

                        beginText.transform.localPosition = new Vector3(-33.913f, -195.79f, 13.73f);

                        foreach (GameObject i in scoreBoardElements)
                        {
                            i.transform.localPosition = new Vector3(
                                i.transform.localPosition.x, i.transform.localPosition.y - 100f, i.transform.localPosition.z);
                        }
                        beginText.fontSize = 30;
                        postGameScreen.transform.SetAsFirstSibling();
                        beginText.text = "This scoreboard will display you and your peer's total number of points.\n\n" +
                            "Your goal is to gain as many points as possible.\n\n\n        (Press this button to continue)"; // +
                                                                                                                                          /* "You will be competing against a peer.\nTheir goal is to pop <color=red>RED</color> balloons.\n\n" +
                                                                                                                                           "If you <b>miss</b>, you will <b>lose</b> 1 point.\n\n" +
                                                                                                                                           "If you hit one of your peer's <color=red>RED</color> balloons, you will <b>lose</b> 1 point and " +
                                                                                                                                           "they will <b>gain</b> 1 point.\n\n\n        (Press this button to continue)";*/
                        timeSpentOnTutorialStep = 0f;
                    }
                }
                else if (stepOfTutorial == 4)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;

                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        beginText.fontSize = 35;

                        controllerImage.transform.localPosition = new Vector3(-34.202f, -196.076f, 13.73f);

                        beginText.text = "If you pop a <color=blue>BLUE</color> balloon you will <b>gain</b> one point.\n\n" +
                            "If you miss, you will <b>lose</b> one point." +
                            "\n\n\n\n        (Press this button to continue)";
                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }

                else if (stepOfTutorial == 5)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        if (barGraph.activeInHierarchy)
                        {
                            barGraph.transform.SetAsFirstSibling();
                        }
                        else
                        {
                            pieChart.transform.SetAsFirstSibling();
                        }
                        for (int i = scoreBoardElements.Length - 1; i >= 0; i--)
                        {
                            scoreBoardElements[i].transform.SetAsFirstSibling(); //So the black screen covers them again
                        }

                        /*
                        TargetShootScript.userScore = 0;
                        TargetShootScript.scoreTextChangeFromOutside.text = "You       0";
                        VirtualPeerBehavior.peerPoints = 0;
                        VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer      0";
                        */
                        beginText.transform.localPosition = new Vector3(-33.913f, -195.796f, 13.73f);
                        beginText.text = "You will play 4 rounds,\n" +
                            "each lasting " + (GAME_TIME / 60) + " minutes.\n\n\n" +
                            "For the first round, you will play alone.\n\n" +
                            "Then, you will be connected with a peer, whom you will compete against for the following 3 rounds." +
                            "\n\n\n        (Press this button to continue)";
                        controllerImage.transform.localPosition = new Vector3(-34.202f, -196.244f, 13.73f);
                        stepOfTutorial++;
                        timeSpentOnTutorialStep = 0f;
                    }
                }

                else if (stepOfTutorial == 6)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        controllerImage.transform.localPosition = new Vector3(-34.202f, -196.2819f, 13.73f);
                        
                        beginText.text = "Your peer will pop <color=RED>RED</color> balloons.\n\n\n" +
                            "If you pop a <color=RED>RED</color> balloon your peer will <b>gain</b> one point," +
                            " and you will lose one point for missing.\n\n" +
                            "If your peer pops a <color=BLUE>BLUE</color> balloon you will <b>gain</b> one point " +
                            "and they will lose one point for missing." +
                            "\n\n\n        (Press this button to continue)";
                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }

                else if (stepOfTutorial == 7)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;

                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {

                        beginText.fontSize = 32;
                        if (barGraph.activeInHierarchy)
                        {
                            barGraph.transform.parent.localPosition =
                                new Vector3(barGraph.transform.parent.localPosition.x, barGraph.transform.parent.localPosition.y - 100f, barGraph.transform.parent.localPosition.z);
                            beginText.text = "The bar above will show the difference between your score and your peer's score.\n" +
                                "\n\nIf you are leading, the bar will be <color=blue>BLUE</color>.\n\n\n" +
                                "If your peer is leading, the bar will be <color=red>RED</color>.\n\n\n\n" +
                                "        (Press this button to continue)";
                            beginText.transform.localPosition = new Vector3(-33.913f, -195.882f, 13.73f);
                            controllerImage.transform.localPosition = new Vector3(-34.202f, -196.298f, 13.73f);
                        }
                        else
                        {
                            pieChart.transform.localPosition =
                                new Vector3(pieChart.transform.localPosition.x, pieChart.transform.localPosition.y - 100f, pieChart.transform.localPosition.z);
                            beginText.text = "The pie chart above will show your score, in <color=blue>BLUE</color>, in comparison to " +
                                "your peer's score, in <color=red>RED</color>.\n\n\n" +
                                "        (Press this button to continue)";
                            controllerImage.transform.localPosition = new Vector3(-34.202f, -196.251f, 13.73f);
                            beginText.transform.localPosition = new Vector3(-33.913f, -196.069f, 13.73f);

                        }
                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 8)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        TargetShootScript.userScore = 12;
                        TargetShootScript.scoreTextChangeFromOutside.text = "You      12";
                        VirtualPeerBehavior.peerPoints = 15;
                        VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer     15";

                        beginText.text = "For example, this is what you will see if you have 12 points and your peer has 15." +
                            "\n\n\n        (Press this button to continue)";

                        if (barGraph.activeInHierarchy)
                        {
                            controllerImage.transform.localPosition = new Vector3(-34.202f, -196.061f, 13.73f);
                        }

                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 9)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        TargetShootScript.userScore = 6;
                        TargetShootScript.scoreTextChangeFromOutside.text = "You       6";
                        VirtualPeerBehavior.peerPoints = 3;
                        VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer      3";

                        beginText.text = "This is what you will see if you have 6 points and your peer has 3." +
                            "\n\n\n        (Press this button to continue)";
                        if (barGraph.activeInHierarchy)
                        {
                            controllerImage.transform.localPosition = new Vector3(-34.202f, -196.0351f, 13.73f);
                        }

                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 10)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        TargetShootScript.userScore = 8;
                        TargetShootScript.scoreTextChangeFromOutside.text = "You       8";
                        VirtualPeerBehavior.peerPoints = 8;
                        VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer      8";

                        beginText.text = "This is what you will see if you both have the same number of points." +
                            "\n\n\n        (Press this button to continue)";

                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 11)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        barGraph.transform.parent.GetComponentInChildren<Text>().enabled = false;
                        beginText.text = "Once you approach the final 10 seconds of the round, a timer" +
                            " will appear, showing the seconds remaining.\n\n\n        (Press this button to continue)";

                        beginText.transform.localPosition = new Vector3(-33.913f, -196.003f, 13.73f);
                        controllerImage.transform.localPosition = new Vector3(-34.202f, -196.1908f, 13.73f);

                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 12)
                {
                    if (timeSpentOnTutorialStep < 11f)
                    {
                        string newText = Mathf.FloorToInt(11f - timeSpentOnTutorialStep).ToString();
                        if (!newText.Equals(countdownText.text))
                        {
                            countdownText.gameObject.GetComponent<CountdownTimerScript>().flash(true);
                        }
                        countdownText.text = newText;
                    }
                    else
                    {
                        countdownText.text = "";
                    }

                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        TargetShootScript.userScore = 0;
                        TargetShootScript.scoreTextChangeFromOutside.text = "You       0";
                        VirtualPeerBehavior.peerPoints = 0;
                        VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer      0";

                        beginText.text = "Before you begin the first " + (GAME_TIME / 60) + " minute round, you will play" +
                            " a 30 second practice round alone, where no data will be collected.\n\n\n        (Press this button to start the\n         practice round)";
                        controllerImage.transform.localPosition = new Vector3(-34.202f, -196.239f, 13.73f);

                        timeSpentOnTutorialStep = 0f;
                        countdownText.text = "";
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 13)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        postGameScreen.GetComponentInChildren<Text>().enabled = true;
                        postGameScreen.SetActive(false);

                        beginText.gameObject.SetActive(false);
                        controllerImage.gameObject.SetActive(false);
                        //inTutorial = false;

                        TargetShootScript.scoreTextChangeFromOutside.transform.localPosition += new Vector3(0f, -0.0298f, 0f);
                        VirtualPeerBehavior.peerTextChangeFromOutside.enabled = false;
                        virtualPeer.GetComponent<VirtualPeerBehavior>().enabled = false;
                        if (barGraph.activeInHierarchy)
                        {
                            barGraph.GetComponent<BarGraphScript>().enabled = false;
                            barGraph.GetComponent<Image>().enabled = false; //So it's not stuck on some score
                            barGraph.transform.parent.GetComponentInChildren<Text>().enabled = false;
                        }
                        else
                        {
                            pieChart.GetComponent<PieGraphScript>().enabled = false;
                            foreach (Image i in pieChart.GetComponentsInChildren<Image>())
                            {
                                i.enabled = false;
                            }
                        }

                        timeSpentOnTutorialStep = 0f;
                        countdownTimer = 30f;
                        stepOfTutorial++;
                    }
                }
                //
                //Step 14 is the demo round
                //
                else if (stepOfTutorial == 15)
                {
                    postGameScreen.GetComponentInChildren<Text>().enabled = false;
                    postGameScreen.SetActive(true);
                    beginText.gameObject.SetActive(true);
                    controllerImage.gameObject.SetActive(true);
                    controllerImage.transform.localPosition = new Vector3(-34.231f, -196.221f, 13.73f);
                    
                    beginText.text = "You are now ready to begin the first round.\n\nDo you have any questions?\n\n" +
                        "\n\n     (If not, press to begin the first round)";

                    timeSpentOnTutorialStep += Time.deltaTime;

                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        postGameScreen.GetComponentInChildren<Text>().enabled = true;
                        postGameScreen.SetActive(false);
                        beginText.gameObject.SetActive(false);
                        controllerImage.gameObject.SetActive(false);

                        stepOfTutorial++;
                        inTutorial = false;
                    }
                }
            }
            if (!inTutorial || stepOfTutorial == 14)
            {
                if (!inTutorial)
                {
                    if (!DataCollector.collectingData)
                        DataCollector.collectingData = true;

                    if (currentMode.Equals(""))
                    {
                        if (modes.Count > 0) //Picking next gameplay mode:
                        {
                            if (modes.Count != 4)
                            {
                                int index = Random.Range(0, modes.Count);
                                currentMode = modes[index];
                                modes.RemoveAt(index);
                            }
                            else
                            {
                                currentMode = modes[0];
                                modes.RemoveAt(0); //Make Alone first mode
                            }

                            foreach (GameObject g in GameObject.FindGameObjectsWithTag("PeerTarget"))
                            {
                                Destroy(g);
                            }
                            foreach (GameObject g in GameObject.FindGameObjectsWithTag("UserTarget"))
                            {
                                Destroy(g);
                            }

                            resetElementsOnModeChange();

                            if (currentMode.Equals("Alone"))
                            {
                                TargetShootScript.scoreTextChangeFromOutside.transform.localPosition += new Vector3(0f, -0.0298f, 0f);
                                VirtualPeerBehavior.peerTextChangeFromOutside.enabled = false;
                                virtualPeer.GetComponent<VirtualPeerBehavior>().enabled = false;
                                if (barGraph.activeInHierarchy)
                                {
                                    barGraph.GetComponent<BarGraphScript>().enabled = false;
                                    barGraph.GetComponent<Image>().enabled = false; //So it's not stuck on some score
                                    barGraph.transform.parent.GetComponentInChildren<Text>().enabled = false;
                                }
                                else
                                {
                                    pieChart.GetComponent<PieGraphScript>().enabled = false;
                                    foreach (Image i in pieChart.GetComponentsInChildren<Image>())
                                    {
                                        i.enabled = false;
                                    }
                                }
                            }
                            else if (currentMode.Equals("Equally perform"))
                            {
                                virtualPeer.GetComponent<VirtualPeerBehavior>().performanceConstant = 0;
                            }
                            else if (currentMode.Equals("Underperform"))
                            {
                                virtualPeer.GetComponent<VirtualPeerBehavior>().performanceConstant = -performanceConstant;
                            }
                            else
                            {
                                virtualPeer.GetComponent<VirtualPeerBehavior>().performanceConstant = performanceConstant;
                            }
                            if (modes.Count < 3) //If this isn't the first mode change, then have a waiting screen:
                            {
                                waiting = true;
                                controllerImage.gameObject.SetActive(true);
                                postGameScreen.SetActive(true);

                                string plural = (Mathf.Abs(scoreDifference) > 1 || scoreDifference == 0) ? " points" : " point";
                                if (modes.Count != 2) //If they didn't just play alone
                                {

                                    if (scoreDifference > 0) //User won
                                        postGameScreen.GetComponentInChildren<Text>().text = "You won by " + scoreDifference + plural;
                                    else if (scoreDifference < 0) //Peer won
                                        postGameScreen.GetComponentInChildren<Text>().text = "Your peer won by " + Mathf.Abs(scoreDifference) + plural;
                                    else //Tie
                                        postGameScreen.GetComponentInChildren<Text>().text = "You tied with your peer";
                                }
                                else
                                {
                                    postGameScreen.GetComponentInChildren<Text>().text = "You scored " + scoreDifference + plural;
                                }

                                barGraph.transform.parent.GetComponentInChildren<Text>().enabled = false;
                                beginText.gameObject.SetActive(true);

                                //beginText.GetComponent<RectTransform>().sizeDelta = new Vector2(543.39f, 17.8f);

                                if (modes.Count == 2) //If the player just came from the playing alone screen, they need a unique message:
                                {
                                    beginText.text = "You will now compete against a peer.\n\n\n(Press this button to continue)";
                                    beginText.transform.localPosition =
                                        new Vector3(-33.913f, -195.875f, 13.73f);
                                    controllerImage.transform.localPosition =
                                        new Vector3(-34.237f, -196.004f, 13.73f);
                                    beginText.fontSize = 35;
                                }
                                else
                                {
                                    controllerImage.gameObject.SetActive(false);
                                    //controllerImage.transform.localPosition =
                                    //    new Vector3(-34.248f, -195.8729f, 13.73f);
                                    beginText.transform.localPosition =
                                        new Vector3(-34.008f, -195.875f, 13.73f);
                                    
                                    beginText.color = Color.white;
                                    beginText.text = "Next game starts in: ";
                                    countdownText.gameObject.transform.localPosition =
                                        new Vector3(-33.81499f, -195.8939f, 13.73f);
                                }
                            }
                        }
                        else
                        {
                            waiting = true;
                            beginText.gameObject.SetActive(true);
                            postGameScreen.SetActive(true);
                            barGraph.GetComponent<Image>().enabled = false;
                            barGraph.transform.parent.GetComponentInChildren<Text>().enabled = false;
                            beginText.transform.localPosition =
                                        new Vector3(-33.975f, -195.875f, 13.73f);
                            
                            beginText.text = "The game is over now,\nthank you for your cooperation!";
                            gameIsOver = true;
                            virtualPeer.GetComponent<VirtualPeerBehavior>().enabled = false;
                            DataCollector.collectingData = false;
                            string plural = (Mathf.Abs(scoreDifference) > 1 || scoreDifference == 0) ? " points" : " point";

                            if (scoreDifference > 0) //User won
                                postGameScreen.GetComponentInChildren<Text>().text = "You won by " + scoreDifference + plural;
                            else if (scoreDifference < 0) //Peer won
                                postGameScreen.GetComponentInChildren<Text>().text = "Your peer won by " + Mathf.Abs(scoreDifference) + plural;
                            else //Tie
                                postGameScreen.GetComponentInChildren<Text>().text = "You tied with your peer";
                        }
                    }
                    if (waiting)
                    {
                        if (pieChart.activeInHierarchy)
                        {
                            pieChart.GetComponent<PieGraphScript>().enabled = false;
                            foreach (Image i in pieChart.GetComponentsInChildren<Image>())
                            {
                                i.enabled = false;
                            }
                        }
                        waitingTime += Time.deltaTime;

                        if (modes.Count != 2 && !gameIsOver)
                        {
                            string newText = Mathf.FloorToInt(11f - waitingTime).ToString();
                            if (!newText.Equals(countdownText.text))
                            {
                                countdownText.gameObject.GetComponent<CountdownTimerScript>().flash(false);
                            }
                            countdownText.text = newText;
                        }

                        if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) || startedWaitingForPeer || (modes.Count != 2 && waitingTime >= 10f))
                        {
                            if (waitingTime >= 1.0f && !gameIsOver && modes.Count == 2 || (modes.Count != 2 && waitingTime >= 10f && !gameIsOver))
                            {
                                if (modes.Count != 2)
                                {
                                    waiting = false;
                                    waitingTime = 0f;
                                    countdownText.gameObject.transform.localPosition = countdownTimerPosition;
                                }

                                if (pieChart.activeInHierarchy)
                                {
                                    pieChart.GetComponent<PieGraphScript>().enabled = true;
                                    foreach (Image i in pieChart.GetComponentsInChildren<Image>())
                                    {
                                        i.enabled = true;
                                    }
                                }
                                controllerImage.gameObject.SetActive(false);
                                if (modes.Count != 2)
                                    postGameScreen.SetActive(false);
                                beginText.gameObject.SetActive(false);
                                barGraph.transform.parent.GetComponentInChildren<Text>().enabled = true;

                                if (modes.Count == 2 && !startedWaitingForPeer)
                                {
                                    postGameScreen.GetComponentInChildren<Text>().enabled = false;
                                    startedWaitingForPeer = true;
                                    loadingScreen.GetComponent<Text>().enabled = true;
                                    loadingScreen.GetComponentInChildren<RawImage>().enabled = true;
                                    setWaitingTime = Random.Range(2f, 9f);
                                    //setWaitingTime = Random.Range(0.5f, 3f);
                                    waitingTime = 0f;
                                }
                                else if (waitingTime >= setWaitingTime)
                                {
                                    waiting = false;
                                    startedWaitingForPeer = false;
                                    loadingScreen.GetComponent<Text>().enabled = false;
                                    loadingScreen.GetComponentInChildren<RawImage>().enabled = false;
                                    postGameScreen.GetComponentInChildren<Text>().enabled = true;
                                    postGameScreen.SetActive(false);
                                    waitingTime = 0f;
                                }
                            }
                        }
                        if (gameIsOver)
                        {
                            if (!deletedAllTargets)
                            {
                                deletedAllTargets = true;
                                foreach (GameObject g in GameObject.FindGameObjectsWithTag("PeerTarget"))
                                {
                                    Destroy(g);
                                }
                                foreach (GameObject g in GameObject.FindGameObjectsWithTag("UserTarget"))
                                {
                                    Destroy(g);
                                }
                            }
                        }
                    }
                }

                if (!waiting)
                {
                    userRandomNoiseTimer += Time.deltaTime;
                    peerRandomNoiseTimer += Time.deltaTime;

                    if (randomTimeUser > 0f)
                        timeUser += Time.deltaTime;
                    if (randomTimePeer > 0f)
                        timePeer += Time.deltaTime;


                    if (increaseIntervalBasedOnDensityMode)
                    {
                        userInterval = 2f + (0.5f * numUserTargets);
                        peerInterval = 2f + (0.5f * numPeerTargets);
                    }
                    if (randomTimeUser < 0f)
                    {
                        if (!constantBalloonsWithNoise || numUserTargets < (userTargetsCap + userTargetsNoise))
                        {
                            randomTimeUser = Random.Range(0.0f, userInterval);
                        }
                    }
                    if (randomTimePeer < 0f)
                    {
                        if (!constantBalloonsWithNoise || numPeerTargets < (peerTargetsCap + peerTargetsNoise))
                        {
                            randomTimePeer = Random.Range(0.0f, peerInterval);
                        }
                    }
                    if (!generatedTargetForUser)
                    {
                        if (timeUser >= randomTimeUser && randomTimeUser > 0f)
                        {
                            float radius = TargetMovementScript.radius;

                            //Now, uniformly choose random point on sphere, excluding bottom around feet:

                            float thetaRealPlayer = Random.Range(0f, 2f * Mathf.PI);

                            float phiRealPlayer = Mathf.Acos(2f * Random.Range(0.146f, 1f) - 1f);

                            GameObject targetPrefReal = Instantiate(targetPrefab) as GameObject;
                            targetPrefReal.transform.localPosition
                            = new Vector3(radius * Mathf.Sin(phiRealPlayer) * Mathf.Cos(thetaRealPlayer), radius * Mathf.Cos(phiRealPlayer) + 5, radius * Mathf.Sin(phiRealPlayer) * Mathf.Sin(thetaRealPlayer));
                            targetPrefReal.transform.LookAt(new Vector3(0, 5, 0));
                            targetPrefReal.transform.GetChild(0).GetChild(0).LookAt(new Vector3(0, 100000, 0));
                            targetPrefReal.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().material = material1;
                            targetPrefReal.tag = "UserTarget";
                            targetPrefReal.GetComponent<TargetMovementScript>().theta = thetaRealPlayer;
                            targetPrefReal.GetComponent<TargetMovementScript>().phi = phiRealPlayer;
                            numUserTargets++;

                            generatedTargetForUser = true;
                            //targetIndex++;
                        }
                    }
                    if (!generatedTargetForPeer)
                    {
                        if (timePeer >= randomTimePeer && randomTimePeer > 0f)
                        {
                            float radius = TargetMovementScript.radius;

                            //Now, uniformly choose random point on sphere, excluding bottom around feet:

                            float thetaVirtualPeer = Random.Range(0f, 2f * Mathf.PI);

                            float phiVirtualPeer = Mathf.Acos(2f * Random.Range(0.146f, 1f) - 1f);

                            GameObject targetPrefVirtual = Instantiate(targetPrefab) as GameObject;
                            targetPrefVirtual.transform.localPosition
                            = new Vector3(radius * Mathf.Sin(phiVirtualPeer) * Mathf.Cos(thetaVirtualPeer), radius * Mathf.Cos(phiVirtualPeer) + 5, radius * Mathf.Sin(phiVirtualPeer) * Mathf.Sin(thetaVirtualPeer));
                            targetPrefVirtual.transform.LookAt(new Vector3(0, 5, 0));
                            targetPrefVirtual.transform.GetChild(0).GetChild(0).LookAt(new Vector3(0, 100000, 0));
                            targetPrefVirtual.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().material = material2;
                            targetPrefVirtual.tag = "PeerTarget";
                            targetPrefVirtual.GetComponent<TargetMovementScript>().theta = thetaVirtualPeer;
                            targetPrefVirtual.GetComponent<TargetMovementScript>().phi = phiVirtualPeer;
                            numPeerTargets++;


                            generatedTargetForPeer = true;
                            //targetIndex++;
                        }
                    }
                    if (generatedTargetForUser)
                    {
                        if (timeUser >= userInterval || constantBalloonsWithNoise) //With constant balloons mode, it wouldn't be about intervals so much as getting balloons in the game
                        {
                            timeUser = 0f;
                            generatedTargetForUser = false;
                            randomTimeUser = -1f;
                        }
                    }
                    if (generatedTargetForPeer)
                    {
                        if (timePeer >= peerInterval || constantBalloonsWithNoise)
                        {
                            timePeer = 0f;
                            generatedTargetForPeer = false;
                            randomTimePeer = -1f;
                        }
                    }

                    //Random noise for constant amount of balloons:
                    if (userRandomNoiseTimer >= userTimeUntilNextRandomNoiseAssignment)
                    {
                        userTargetsNoise = Random.Range(-1, 2);
                        userTimeUntilNextRandomNoiseAssignment = Random.Range(7f, 12f);
                        userRandomNoiseTimer = 0f;
                    }
                    if (peerRandomNoiseTimer >= peerTimeUntilNextRandomNoiseAssignment)
                    {
                        peerTargetsNoise = Random.Range(-1, 2);
                        peerTimeUntilNextRandomNoiseAssignment = Random.Range(7f, 12f);
                        peerRandomNoiseTimer = 0f;
                    }



                    countdownTimer -= Time.deltaTime;
                    if (countdownTimer <= 0f)
                    {
                        scoreDifference = currentMode.Equals("Alone") || currentMode.Equals("") ?
                            TargetShootScript.userScore : TargetShootScript.userScore - VirtualPeerBehavior.peerPoints;
                        currentMode = "";

                        TargetShootScript.userScore = 0;
                        TargetShootScript.scoreTextChangeFromOutside.text = "You       0";
                        if (VirtualPeerBehavior.peerPoints != 0)
                        {
                            VirtualPeerBehavior.peerPoints = 0;
                            VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer      0";
                        }
                        numUserTargets = 0;
                        numPeerTargets = 0;
                        countdownTimer = GAME_TIME;

                        if (stepOfTutorial == 14) //If we're in the demo round:
                        {
                            foreach (GameObject g in GameObject.FindGameObjectsWithTag("PeerTarget"))
                            {
                                Destroy(g);
                            }
                            foreach (GameObject g in GameObject.FindGameObjectsWithTag("UserTarget"))
                            {
                                Destroy(g);
                            }
                            stepOfTutorial = 15;
                            inTutorial = true;
                            timeSpentOnTutorialStep = 0f;
                        }
                    }
                    if (countdownTimer < 10f)
                    {
                        string newText = Mathf.FloorToInt(countdownTimer + 1f).ToString();
                        if (!countdownText.text.Equals(newText))
                        {
                            countdownText.gameObject.GetComponent<CountdownTimerScript>().flash(true);
                        }
                        countdownText.text = newText;
                    }
                    else
                    {
                        countdownText.text = "";
                    }
                }
            }
        }
    }
    public void resetElementsOnModeChange()
    {
        if (!VirtualPeerBehavior.peerTextChangeFromOutside.enabled)
        {
            TargetShootScript.scoreTextChangeFromOutside.transform.localPosition += new Vector3(0f, 0.0298f, 0f);
            VirtualPeerBehavior.peerTextChangeFromOutside.enabled = true;
            virtualPeer.GetComponent<VirtualPeerBehavior>().enabled = true;

            if (barGraph.activeInHierarchy)
            {
                barGraph.GetComponent<BarGraphScript>().enabled = true;
                barGraph.transform.parent.GetComponentInChildren<Text>().enabled = true;
                barGraph.GetComponent<Image>().enabled = true; //So it's not stuck on some score
            }
            else
            {
                pieChart.GetComponent<PieGraphScript>().enabled = true;
                foreach (Image i in pieChart.GetComponentsInChildren<Image>())
                {
                    i.enabled = true;
                }
            }
        }
    }
}

