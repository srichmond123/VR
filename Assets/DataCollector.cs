using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class DataCollector : MonoBehaviour {

    static string dataPath = "Data/";
    static string userPath = "";
    static bool writtenMovementDataColumnNames = false;
    static bool writtenScoreDataColumnNames = false;

    static GameObject ctrCamera;
    static GameObject handAnchor;
    static int userID;
    static float time;
    public static bool collectingData = false;

    int userPoints = 0;
    int peerPoints = 0;

    // Use this for initialization
    void Start () {
        ctrCamera = GameObject.Find("CenterEyeAnchor");

        //Right handed:
        Directory.CreateDirectory(dataPath);
        userID = Directory.GetDirectories(dataPath).Length;

        userPath = dataPath + "User-" + userID + '/';
        Directory.CreateDirectory(userPath);

    }
	
	// Update is called once per frame
	void Update () {
        if (collectingData)
        {
            if (handAnchor == null)
            {
                if (GameObject.FindGameObjectWithTag("RightHandLaser").GetComponent<MeshRenderer>().enabled)
                {
                    handAnchor = GameObject.Find("RightHandAnchor");
                }
                else
                {
                    handAnchor = GameObject.Find("LeftHandAnchor");
                }
            }

            string path;
            StreamWriter streamWriter;

            if (!writtenMovementDataColumnNames)
            {
                writtenMovementDataColumnNames = true;
                path = userPath + "MovementData.csv";

                new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write).Close();
                streamWriter = new StreamWriter(path, true, Encoding.ASCII);
                string handStr = "Left";
                if (handAnchor.transform.GetChild(0).tag.Equals("RightHandLaser"))
                    handStr = "Right";
                streamWriter.Write("Time (s):,Headset position x:,Headset position y:,Headset position z:,Headset rotation x:,Headset rotation y:,Headset rotation z:," +
                    handStr + " Hand Position x:," + handStr + " Hand Position y:," + handStr + " Hand Position z:," 
                    + handStr + " Hand rotation x:," + handStr + " Hand rotation y:," + handStr + " Hand rotation z:\n");
                streamWriter.Close();
            }

            time += Time.deltaTime;
            path = userPath + "MovementData.csv";

            new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write).Close();
            streamWriter = new StreamWriter(path, true, Encoding.ASCII);

            string line = time.ToString() + ","
                + ctrCamera.transform.localPosition.x.ToString() + ","
                + ctrCamera.transform.localPosition.y.ToString() + ","
                + ctrCamera.transform.localPosition.z.ToString() + ","
                + ctrCamera.transform.localEulerAngles.x.ToString() + ","
                + ctrCamera.transform.localEulerAngles.y.ToString() + ","
                + ctrCamera.transform.localEulerAngles.z.ToString() + ","
                + handAnchor.transform.localPosition.x.ToString() + ","
                + handAnchor.transform.localPosition.y.ToString() + ","
                + handAnchor.transform.localPosition.z.ToString() + ","
                + handAnchor.transform.localEulerAngles.x.ToString() + ","
                + handAnchor.transform.localEulerAngles.y.ToString() + ","
                + handAnchor.transform.localEulerAngles.z.ToString() + "\n";
            streamWriter.Write(line);
            streamWriter.Close();
        }
    }

    public static void WriteEvent(string a) {
        if (collectingData) {

            string path;
            StreamWriter streamWriter;

            if (!writtenScoreDataColumnNames)
            {
                writtenScoreDataColumnNames = true;

                path = userPath + "ScoreData.csv";
                new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write).Close();
                streamWriter = new StreamWriter(path, true, Encoding.ASCII);
                string handStr = "Left";
                if (handAnchor.tag.Equals("RightHandAnchor"))
                    handStr = "Right";
                streamWriter.Write("Time (s):,User Points:,Peer Points:,Action type:,Headset position x:,Headset position y:,Headset position z:,Headset rotation x:,Headset rotation y:,Headset rotation z:," +
                    handStr + " Hand Position x:," + handStr + " Hand Position y:," + handStr + " Hand Position z:,"
                    + handStr + " Hand rotation x:," + handStr + " Hand rotation y:," + handStr + " Hand rotation z:\n");
                streamWriter.Close();
            }

            int userPoints = TargetShootScript.userScore;
            int peerPoints = VirtualPeerBehavior.peerPoints; //Update values

            path = userPath + "ScoreData.csv";

            new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write).Close();
            streamWriter = new StreamWriter(path, true, Encoding.ASCII);

            string line = time.ToString() + ","
                + userPoints.ToString() + ","
                + peerPoints.ToString() + ","
                + a + ","
                + ctrCamera.transform.localPosition.x.ToString() + ","
                + ctrCamera.transform.localPosition.y.ToString() + ","
                + ctrCamera.transform.localPosition.z.ToString() + ","
                + ctrCamera.transform.localEulerAngles.x.ToString() + ","
                + ctrCamera.transform.localEulerAngles.y.ToString() + ","
                + ctrCamera.transform.localEulerAngles.z.ToString() + ","
                + handAnchor.transform.localPosition.x.ToString() + ","
                + handAnchor.transform.localPosition.y.ToString() + ","
                + handAnchor.transform.localPosition.z.ToString() + ","
                + handAnchor.transform.localEulerAngles.x.ToString() + ","
                + handAnchor.transform.localEulerAngles.y.ToString() + ","
                + handAnchor.transform.localEulerAngles.z.ToString() + "\n";
            streamWriter.Write(line);
            streamWriter.Close();
        }
    }
}
