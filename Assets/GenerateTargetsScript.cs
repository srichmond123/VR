using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class GenerateTargetsScript : MonoBehaviour {

	// Use this for initialization

	public GameObject targetPrefab;
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

    public static int numUserTargets = 0;
    public static int numPeerTargets = 0;
    public int userTargetsCap;
    int userTargetsNoise = 0; //-2 through 2, equal prob.
    public int peerTargetsCap;
    int peerTargetsNoise = 0; //-2 through 2, equal prob.

    public static float userInterval = 2.0f;
    public static float peerInterval = 2.0f;



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
	}

    // Update is called once per frame
    void Update()
    {
        if (TargetShootScript.playing)
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
        }
    }
}



///OLD CODE:



//Pick data from trajectory_.csv for new target:
/*
if (targetIndex == totalTrajectories) { //If we somehow get more than 500, just go back to 0:
    targetIndex = 0;
}

string fileData = System.IO.File.ReadAllText ("Trajectories/trajectory" + targetIndex + ".csv");
string[] lines = fileData.Split("\n"[0]);
string[] firstLineData = (lines[1].Trim()).Split(","[0]);
float startMovementAngle; //
float startRadiusGaussian;
float startTheta;
float startPhi;
float.TryParse(firstLineData[1], out startMovementAngle);
float.TryParse (firstLineData [2], out startRadiusGaussian);
float.TryParse(firstLineData[3], out startTheta);
float.TryParse (firstLineData [4], out startPhi);
*/

/*  I only had to do this once:
for (int trajIndex = 0; trajIndex < 500; trajIndex++) {
string path = "Trajectories/trajectory" + trajIndex + ".csv";

new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write).Close();
StreamWriter streamWriter = new StreamWriter(path, true);

streamWriter.Write ("Random Normals for path:,Start movement angle:,Start radius,Start theta:,Start phi:\n");
for (int i = 0; i < 900; i++) {
    float randomGaussian = RandomFromDistribution.RandomFromStandardNormalDistribution () / 3.5f;
    randomGaussian = Mathf.Clamp (randomGaussian, -1.0f, 1.0f);
    string line = randomGaussian.ToString();
    if (i == 0) {
        float startAngleRand = Random.Range (0f, 2.0f * Mathf.PI);
        float randomGaussianRadius = RandomFromDistribution.RandomFromStandardNormalDistribution () / 3.5f;
        randomGaussianRadius = Mathf.Clamp (randomGaussian, -1.0f, 1.0f);
        float thetaRandom = Random.Range (-Mathf.PI, Mathf.PI);
        float phiRandom = Random.Range (0.146f, 1f);
        line += "," + startAngleRand.ToString() + "," + randomGaussianRadius.ToString() + "," + thetaRandom.ToString() + "," + phiRandom.ToString() + "\n";
    } else {
        line += "\n";
    }
    streamWriter.Write (line);
}
streamWriter.Close ();
}
*/
