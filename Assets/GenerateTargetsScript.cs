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
	bool generatedTargetForInterval = false; //So more than 1 aren't generated in the interval
	float randomTime = -1f; //-1 if a random number hasn't been generated: this will be reset to -1 after every 2 second interval:
    public Material material1;
    public Material material2;

	//Make some tables: (3 minutes of flight is 900 standard normals/3.5 clamped at -1, 1)
	//Include other columns for a random between 0 and 2pi (start angle), a random gaussian for start radius, 
	//And a Random.Range (-Mathf.PI, Mathf.PI) (for theta) and phi -> Random.Range (0.146f, 1f) 
	void Start () {
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
	}
	
	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		if (randomTime < 0f) {
			randomTime = Random.Range (0.0f, 2.0f);
		}
		if (!generatedTargetForInterval) {
			if (time > randomTime) {
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
				float radius = TargetMovementScript.radius;

				//Now, uniformly choose random point on sphere, excluding bottom around feet:
                
                float thetaRealPlayer = Random.Range(0f, 2f * Mathf.PI);
                float thetaVirtualPeer = Random.Range(0f, 2f * Mathf.PI);

                float phiRealPlayer = Mathf.Acos (2f * Random.Range(0.146f, 1f) - 1f);
                float phiVirtualPeer = Mathf.Acos(2f * Random.Range(0.146f, 1f) - 1f);
                /*
				float[] randomGaussianArr = new float[900];
				for (int i = 0; i < randomGaussianArr.Length; i++) {
					string[] currLine = (lines[i + 1].Trim()).Split(","[0]);
					float.TryParse (currLine [0], out randomGaussianArr [i]);
				}
                */
                GameObject targetPrefReal = Instantiate (targetPrefab) as GameObject;
				targetPrefReal.transform.localPosition
				= new Vector3 (radius * Mathf.Sin (phiRealPlayer) * Mathf.Cos (thetaRealPlayer), radius * Mathf.Cos (phiRealPlayer) + 5, radius * Mathf.Sin (phiRealPlayer) * Mathf.Sin (thetaRealPlayer));
				targetPrefReal.transform.LookAt (new Vector3 (0, 5, 0));
                targetPrefReal.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = material1;

                GameObject targetPrefVirtual = Instantiate(targetPrefab) as GameObject;
                targetPrefVirtual.transform.localPosition
                = new Vector3(radius * Mathf.Sin(phiVirtualPeer) * Mathf.Cos(thetaVirtualPeer), radius * Mathf.Cos(phiVirtualPeer) + 5, radius * Mathf.Sin(phiVirtualPeer) * Mathf.Sin(thetaVirtualPeer));
                targetPrefVirtual.transform.LookAt(new Vector3(0, 5, 0));
                targetPrefVirtual.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = material2;


                generatedTargetForInterval = true;
				targetIndex++;
			}
		}
		if (generatedTargetForInterval) {
			if (time >= 2.0f) {
				time -= 2.0f;
				generatedTargetForInterval = false;
				randomTime = -1f;
			}
		}
	}
}
