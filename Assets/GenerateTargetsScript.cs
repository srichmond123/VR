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


    List<string> modes;
    public static string currentMode = "";
    const int GAME_TIME = 120; //2 minutes
    float countdownTimer = GAME_TIME;



    ////////////
    //Toggle these to change how targets are generated, only make 1 true:
    ////////////

    public bool normalMode; //Targets generated in 2 second intervals with uniform probability
    public bool increaseIntervalBasedOnDensityMode; //Targets generated in X second intervals with intervals increasing with number of targets existing
    public bool constantBalloonsWithNoise; //A constant of X + w user&peer balloons should exist on average in this mode

	void Start () {

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
	}

    // Update is called once per frame
    void Update()
    {
        if (TargetShootScript.playing)
        {
            if (currentMode.Equals(""))
            {
                if (modes.Count > 0) //Picking next gameplay mode:
                {
                    int index = Random.Range(0, modes.Count);
                    currentMode = modes[index];
                    modes.RemoveAt(index);

                    foreach (GameObject g in GameObject.FindGameObjectsWithTag("PeerTarget"))
                    {
                        Destroy(g);
                    }
                    foreach (GameObject g in GameObject.FindGameObjectsWithTag("UserTarget"))
                    {
                        Destroy(g);
                    }

                    if (currentMode.Equals("Alone"))
                    {
                        TargetShootScript.scoreTextChangeFromOutside.transform.localPosition += new Vector3(0f, -0.0298f, 0f);
                        VirtualPeerBehavior.peerTextChangeFromOutside.enabled = false;
                        virtualPeer.GetComponent<VirtualPeerBehavior>().enabled = false;
                        barGraph.GetComponent<BarGraphScript>().enabled = false;
                        barGraph.GetComponent<Image>().enabled = true; //So it's not stuck on some score
                    }
                    else if (currentMode.Equals("Equally perform"))
                    {
                        resetElementsOnModeChange();
                        virtualPeer.GetComponent<VirtualPeerBehavior>().performanceConstant = 0;
                    }
                    else if (currentMode.Equals("Underperform"))
                    {
                        resetElementsOnModeChange();
                        virtualPeer.GetComponent<VirtualPeerBehavior>().performanceConstant = -2;
                    }
                    else
                    {
                        resetElementsOnModeChange();
                        virtualPeer.GetComponent<VirtualPeerBehavior>().performanceConstant = 2;
                    }
                    if (modes.Count < 3) //If this isn't the first mode change, then have a waiting screen:
                    {
                        waiting = true;
                        controllerImage.gameObject.SetActive(true);
                        beginText.gameObject.SetActive(true);
                        beginText.text = "Press this button to continue";
                    }
                }
                else
                {
                    waiting = true;
                    beginText.gameObject.SetActive(true);
                    beginText.text = "Game is over";
                }
            }
            if (waiting)
            {
                waitingTime += Time.deltaTime;
                if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)))
                {
                    if (waitingTime >= 1.0f && !beginText.text.Equals("Game is over"))
                    {
                        waiting = false;
                        controllerImage.gameObject.SetActive(false);
                        beginText.gameObject.SetActive(false);
                        waitingTime = 0f;
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
                    countdownText.text = Mathf.FloorToInt(countdownTimer + 1f).ToString();
                }
                else
                {
                    countdownText.text = "";
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
            barGraph.GetComponent<BarGraphScript>().enabled = true;
            barGraph.GetComponent<Image>().enabled = true;
        }
    }
}

