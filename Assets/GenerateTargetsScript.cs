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
    public static int stepOfTutorial = 0;


    List<string> modes;
    public static string currentMode = "";
    const int GAME_TIME = 120; //2 minutes
    float countdownTimer = GAME_TIME;

    float timeSpentOnTutorialStep = 0f; 



    ////////////
    //Toggle these to change how targets are generated, only make 1 true:
    ////////////

    public bool normalMode; //Targets generated in 2 second intervals with uniform probability
    public bool increaseIntervalBasedOnDensityMode; //Targets generated in X second intervals with intervals increasing with number of targets existing
    public bool constantBalloonsWithNoise; //A constant of X + w user&peer balloons should exist on average in this mode

	void Start () {
        countdownText.text = "";
        postGameScreen.SetActive(false);
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
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetShootScript.playing)
        {
            if (inTutorial)
            {
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
                        "You will now go through a brief tutorial\n\n\n     (Press this button to continue)";

                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;

                        beginText.text = "You will see 2 sets of balloons, colored <color=blue>BLUE</color> and " +
                            "<color=red>RED</color>.\n\nYour goal is to aim at and pop the <color=blue>BLUE</color> balloons." +
                            "\n\n\n     (Press this button to continue)";
                        controllerImage.transform.localPosition = new Vector3(-34.225f, -196.142f, 13.73f);
                    }
                }
                else if (stepOfTutorial == 1)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;

                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;

                        controllerImage.transform.localPosition = new Vector3(-34.225f, -196.17f, 13.73f);

                        beginText.text = "In the next step, a <color=blue>BLUE</color> balloon will appear.\n\nYour " +
                            "job will be to pop it, using the button by your index finger, in order to move to the next step.\n\n\n     (Press this button to continue)";
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

                        controllerImage.transform.localPosition = new Vector3(-34.225f, -196.237f, 13.73f);

                        beginText.transform.localPosition = new Vector3(-33.87299f, -195.79f, 13.73f);

                        foreach (GameObject i in scoreBoardElements)
                        {
                            i.transform.localPosition = new Vector3(
                                i.transform.localPosition.x, i.transform.localPosition.y - 100f, i.transform.localPosition.z);
                        }
                        beginText.fontSize = 30;
                        postGameScreen.transform.SetAsFirstSibling();
                        beginText.text = "Your goal is to gain as many points as possible.\n\n" +
                            "You will be competing against a peer.\nTheir goal is to pop <color=red>RED</color> balloons.\n\n" +
                            "If you <b>miss</b>, you will <b>lose</b> 1 point.\n\n" +
                            "If you hit one of your peer's <color=red>RED</color> balloons, you will <b>lose</b> 1 point and " +
                            "they will <b>gain</b> 1 point.\n\n\n     (Press this button to continue)";
                        timeSpentOnTutorialStep = 0f;
                    }
                }
                else if (stepOfTutorial == 4)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;

                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        beginText.fontSize = 35;

                        controllerImage.transform.localPosition = new Vector3(-34.225f, -196.12f, 13.73f);

                        beginText.text = "Likewise, if your peer misses, they will <b>lose</b> 1 point.\n\n" +
                            "If they hit one of your <color=blue>BLUE</color> balloons, you will <b>gain</b> 1 point " +
                            "and they will <b>lose</b> 1 point.\n\n\n     (Press this button to continue)";
                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 5)
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
                                "\nIf you are in the lead, the bar will be <color=blue>BLUE</color> by the amount you are ahead.\n\n" +
                                "If your peer is in the lead, the bar will be <color=red>RED</color> by the amount they are ahead.\n\n\n" +
                                "     (Press this button to continue)";
                            beginText.transform.localPosition = new Vector3(-33.87299f, -195.882f, 13.73f);
                            controllerImage.transform.localPosition = new Vector3(-34.225f, -196.298f, 13.73f);
                        }
                        else
                        {
                            pieChart.transform.localPosition =
                                new Vector3(pieChart.transform.localPosition.x, pieChart.transform.localPosition.y - 100f, pieChart.transform.localPosition.z);
                            beginText.text = "The pie chart above will show your score, in <color=blue>BLUE</color>, in comparison to " +
                                "your peer's score, in <color=red>RED</color>.\n\n\n" +
                                "     (Press this button to continue)";
                            controllerImage.transform.localPosition = new Vector3(-34.225f, -196.251f, 13.73f);
                            beginText.transform.localPosition = new Vector3(-33.87299f, -196.069f, 13.73f);

                        }
                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 6)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        TargetShootScript.userScore = 12;
                        TargetShootScript.scoreTextChangeFromOutside.text = "You      12";
                        VirtualPeerBehavior.peerPoints = 15;
                        VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer     15";

                        beginText.text = "For example, if you have 12 points and your peer has 15." +
                            "\n\n\n     (Press this button to continue)";

                        if (barGraph.activeInHierarchy)
                        {
                            controllerImage.transform.localPosition = new Vector3(-34.225f, -196.0351f, 13.73f);
                        }

                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 7)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        TargetShootScript.userScore = 6;
                        TargetShootScript.scoreTextChangeFromOutside.text = "You       6";
                        VirtualPeerBehavior.peerPoints = 3;
                        VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer      3";

                        beginText.text = "Or, if you have 6 points and your peer has 3." +
                            "\n\n\n     (Press this button to continue)";

                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 8)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        TargetShootScript.userScore = 8;
                        TargetShootScript.scoreTextChangeFromOutside.text = "You       8";
                        VirtualPeerBehavior.peerPoints = 8;
                        VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer      8";

                        beginText.text = "Or, if you both have the same number of points." +
                            "\n\n\n     (Press this button to continue)";

                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 9)
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

                        TargetShootScript.userScore = 0;
                        TargetShootScript.scoreTextChangeFromOutside.text = "You       0";
                        VirtualPeerBehavior.peerPoints = 0;
                        VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer      0";

                        beginText.transform.localPosition = new Vector3(-33.874f, -195.796f, 13.73f);
                        beginText.text = "You will play 4 rounds,\n" +
                            "each lasting 2 minutes.\n\n\n" +
                            "You will play alone for the first round.\n\nYou will compete against your peer for the next 3 rounds." +
                            "\n\n\n     (Press this button to continue)";
                        controllerImage.transform.localPosition = new Vector3(-34.225f, -196.166f, 13.73f);
                        stepOfTutorial++;
                        timeSpentOnTutorialStep = 0f;
                    }
                }
                else if (stepOfTutorial == 10)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        beginText.text = "Once you approach the final 10 seconds of the round, the timer, shown above," +
                            "will appear, showing the seconds remaining.\n\n\n     (Press this button to continue)";

                        beginText.transform.localPosition = new Vector3(-33.874f, -196.003f, 13.73f);
                        controllerImage.transform.localPosition = new Vector3(-34.225f, -196.1908f, 13.73f);
                        
                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 11)
                {
                    if (timeSpentOnTutorialStep < 11f)
                    {
                        string newText = Mathf.FloorToInt(11f - timeSpentOnTutorialStep).ToString();
                        if (!newText.Equals(countdownText.text))
                        {
                            countdownText.gameObject.GetComponent<CountdownTimerScript>().flash();
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
                        beginText.text = "That's all you need to know!\n\n\n     (Press this button to start playing)";
                        controllerImage.transform.localPosition = new Vector3(-34.225f, -196.121f, 13.73f);

                        timeSpentOnTutorialStep = 0f;
                        countdownText.text = "";
                        stepOfTutorial++;
                    }
                }
                else if (stepOfTutorial == 12)
                {
                    timeSpentOnTutorialStep += Time.deltaTime;
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) && timeSpentOnTutorialStep >= 0.75f)
                    {
                        postGameScreen.GetComponentInChildren<Text>().enabled = true;
                        postGameScreen.SetActive(false);
                        beginText.gameObject.SetActive(false);
                        controllerImage.gameObject.SetActive(false);
                        inTutorial = false;
                        timeSpentOnTutorialStep = 0f;
                        stepOfTutorial++;
                    }
                }
            }
            else
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
                            virtualPeer.GetComponent<VirtualPeerBehavior>().performanceConstant = -2;
                        }
                        else
                        {
                            virtualPeer.GetComponent<VirtualPeerBehavior>().performanceConstant = 2;
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

                            beginText.gameObject.SetActive(true);
                            beginText.GetComponent<RectTransform>().sizeDelta = new Vector2(543.39f, 17.8f);
                            if (modes.Count == 2) //If the player just came from the playing alone screen, they need a unique message:
                            {
                                beginText.text = "You will now compete against a peer.\n\n(Press this button to continue)";
                                beginText.transform.localPosition =
                                    new Vector3(-33.862f, -195.875f, 13.73f);
                                controllerImage.transform.localPosition =
                                    new Vector3(-34.248f, -196.004f, 13.73f);
                                beginText.fontSize = 35;
                            }
                            else {
                                beginText.color = Color.white;
                                beginText.text = "Press this button to continue";
                            }
                        }
                    }
                    else
                    {
                        waiting = true;
                        beginText.gameObject.SetActive(true);
                        postGameScreen.SetActive(true);
                        barGraph.GetComponent<Image>().enabled = false;
                        beginText.text = "Game is over";
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
                    if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)))
                    {
                        if (waitingTime >= 1.5f && !beginText.text.Equals("Game is over"))
                        {
                            waiting = false;
                            if (pieChart.activeInHierarchy)
                            {
                                pieChart.GetComponent<PieGraphScript>().enabled = true;
                                foreach (Image i in pieChart.GetComponentsInChildren<Image>())
                                {
                                    i.enabled = true;
                                }
                            }
                            controllerImage.gameObject.SetActive(false);
                            postGameScreen.SetActive(false);
                            beginText.gameObject.SetActive(false);
                            waitingTime = 0f;
                        }
                    }
                    if (beginText.text.Equals("Game is over"))
                    {
                        if (numUserTargets > 0 || numPeerTargets > 0)
                        {
                            numUserTargets = 0;
                            numPeerTargets = 0;
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
                    if (!generatedTargetForPeer && virtualPeer.GetComponent<VirtualPeerBehavior>().enabled)
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
                        currentMode = "";
                        scoreDifference = TargetShootScript.userScore - VirtualPeerBehavior.peerPoints;
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
                    }
                    if (countdownTimer < 10f)
                    {
                        string newText = Mathf.FloorToInt(countdownTimer + 1f).ToString();
                        if (!countdownText.text.Equals(newText))
                        {
                            countdownText.gameObject.GetComponent<CountdownTimerScript>().flash();
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

