using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class GenerateTargetsScript : MonoBehaviour {

	// Use this for initialization

	public GameObject targetPrefab;
	float time = 0.0f;
	int targetIndex = 0;
	public static int totalTrajectories = 500;
	bool generatedTargetForUser = false; //So more than 1 aren't generated in the interval
    bool generatedTargetForPeer = false;
	float randomTimeUser = -1f; //-1 if a random number hasn't been generated: this will be reset to -1 after every 2 second interval:
    float randomTimePeer = -1f;
    public Material material1;
    public Material material2;
    float totalTime = 0f;

	void Start () {

	}

    // Update is called once per frame
    void Update()
    {
        if (TargetShootScript.playing)
        {

            time += Time.deltaTime;
            //totalTime += Time.deltaTime;
            if (randomTimeUser < 0f)
            {
                randomTimeUser = Random.Range(0.0f, 2.0f);
            }
            if (randomTimePeer < 0f)
            {
                randomTimePeer = Random.Range(0.0f, 2.0f);
            }
            if (!generatedTargetForUser)
            {
                if (time >= randomTimeUser)
                {
                    float radius = TargetMovementScript.radius;

                    //Now, uniformly choose random point on sphere, excluding bottom around feet:

                    float thetaRealPlayer = Random.Range(0.00001f, 2f * Mathf.PI);

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

                    generatedTargetForUser = true;
                    //targetIndex++;
                }
            }
            if (!generatedTargetForPeer)
            {
                if (time >= randomTimePeer)
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


                    generatedTargetForPeer = true;
                    //targetIndex++;
                }
            }
            if (generatedTargetForUser && generatedTargetForPeer)
            {
                if (time >= 2.0f)
                {
                    time -= 2.0f;
                    generatedTargetForUser = false;
                    generatedTargetForPeer = false;
                    randomTimeUser = -1f;
                    randomTimePeer = -1f;
                }
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
