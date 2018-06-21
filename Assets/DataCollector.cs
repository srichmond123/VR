﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

public class DataCollector : MonoBehaviour {

    static string dataPath = "Data/";
    static string userPath = "";
    static bool writtenMovementDataColumnNames = false;
    static bool writtenScoreDataColumnNames = false;

    static GameObject ctrCamera;
    static GameObject handAnchor;
    static int userID;
    static float time;
    public bool makeUserPath;
    public static bool collectingData = false;

    int userPoints = 0;
    int peerPoints = 0;

    static string oldModePathMovementData = ""; //So we can rewrite the column headers each change
    static string oldModePathScoreData = "";
    static string aloneStr = "Alone/";
    static string underperformStr = "Underperforming Peer/";
    static string overperformStr = "Overperforming Peer/";
    static string equalperformStr = "Equally Performing Peer/";

    // Use this for initialization
    void Start () {
        if (makeUserPath)
        {
            ctrCamera = GameObject.Find("CenterEyeAnchor");

            //Right handed:
            Directory.CreateDirectory(dataPath);
            userID = Directory.GetDirectories(dataPath).Length;

            userPath = dataPath + "User-" + userID + '/';
            Directory.CreateDirectory(userPath);
            string modePath = userPath + "Alone/";
            Directory.CreateDirectory(modePath);
            modePath = userPath + "Underperforming Peer/";
            Directory.CreateDirectory(modePath);
            modePath = userPath + "Overperforming Peer/";
            Directory.CreateDirectory(modePath);
            modePath = userPath + "Equally Performing Peer/";
            Directory.CreateDirectory(modePath);
        }

    }
	
	// Update is called once per frame
	void Update () {
        if (collectingData && !GenerateTargetsScript.waiting)
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

            if (writtenMovementDataColumnNames)
            {
                if (!oldModePathMovementData.Equals(getModePath()) && getModePath() != null)
                {
                    writtenMovementDataColumnNames = false;
                }
            }

            if (!writtenMovementDataColumnNames)
            {
                writtenMovementDataColumnNames = true;

                path = userPath + getModePath() + "MovementData.csv";
                oldModePathMovementData = getModePath();

                new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write).Close();
                streamWriter = new StreamWriter(path, true, Encoding.ASCII);
                string handStr = "Left";
                if (handAnchor.transform.GetChild(1).tag.Equals("RightHandLaser"))
                    handStr = "Right";

                streamWriter.Write("Date and clock time (yyyy/MM/dd - hh:mm:ss.ffffff):,Gameplay Time (s):,Headset position x:,Headset position y:,Headset position z:,Headset rotation x:,Headset rotation y:,Headset rotation z:," +
                    handStr + " Hand Position x:," + handStr + " Hand Position y:," + handStr + " Hand Position z:," 
                    + handStr + " Hand rotation x:," + handStr + " Hand rotation y:," + handStr + " Hand rotation z:\n");
                streamWriter.Close();
            }

            time += Time.deltaTime;
            path = userPath + getModePath() + "MovementData.csv";

            if (getModePath() != null)
            {

                new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write).Close();
                streamWriter = new StreamWriter(path, true, Encoding.ASCII);

                string line = DateTime.Now.ToString("yyyy/MM/dd - hh:mm:ss.ffffff") + ","
                    + time.ToString() + ","
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

    public static void WriteEvent(string a) {
        if (collectingData) {

            string path;
            StreamWriter streamWriter;

            if (writtenScoreDataColumnNames)
            {
                if (!oldModePathScoreData.Equals(getModePath()) && getModePath() != null)
                {
                    writtenScoreDataColumnNames = false;
                }
            }

            if (!writtenScoreDataColumnNames)
            {
                writtenScoreDataColumnNames = true;

                path = userPath + getModePath() + "ScoreData.csv";
                oldModePathScoreData = getModePath();
                new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write).Close();
                streamWriter = new StreamWriter(path, true, Encoding.ASCII);
                string handStr = "Left";
                if (handAnchor.transform.GetChild(1).tag.Equals("RightHandLaser"))
                    handStr = "Right";
                streamWriter.Write("Date and clock time (yyyy/MM/dd - hh:mm:ss.ffffff):,Gameplay Time (s):,User Points:,Peer Points:,Action type:,Headset position x:,Headset position y:,Headset position z:,Headset rotation x:,Headset rotation y:,Headset rotation z:," +
                    handStr + " Hand Position x:," + handStr + " Hand Position y:," + handStr + " Hand Position z:,"
                    + handStr + " Hand rotation x:," + handStr + " Hand rotation y:," + handStr + " Hand rotation z:\n");
                streamWriter.Close();
            }

            int userPoints = TargetShootScript.userScore;
            int peerPoints = VirtualPeerBehavior.peerPoints; //Update values

            path = userPath + getModePath() + "ScoreData.csv";

            new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write).Close();
            streamWriter = new StreamWriter(path, true, Encoding.ASCII);

            string line = DateTime.Now.ToString("yyyy/MM/dd - hh:mm:ss.ffffff") + ","
                + time.ToString() + ","
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
    public static string getModePath()
    {
        if (GenerateTargetsScript.currentMode.Equals("Alone"))
        {
            return aloneStr;
        }
        else if (GenerateTargetsScript.currentMode.Equals("Equally perform"))
        {
            return equalperformStr;
        }
        else if (GenerateTargetsScript.currentMode.Equals("Underperform"))
        {
            return underperformStr;
        }
        else if (GenerateTargetsScript.currentMode.Equals("Overperform"))
        {
            return overperformStr;
        }
        else
        {
            return null;
        }
    }
}
