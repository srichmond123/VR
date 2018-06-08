﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirtualPeerBehavior : MonoBehaviour {
    public static int peerPoints = 0;
    public static int peerGoal = 0;
    public int performanceConstant;
    int currPerformanceConstant = 0;
    float time = 0f;
    float timeChangeRandomNoise = 0f;
    float gameTime = 0f;
    float nextActionTime = -1f;
    bool gainPointsNextAction = false;
    int noise = 0;
    int threshold = 0;
    public Text peerPointsText;
    public static Text peerTextChangeFromOutside;

	// Use this for initialization
	void Start () {
        noise = Random.Range(-1, 2); //-1, 0, or 1
        threshold = currPerformanceConstant + noise;
        peerTextChangeFromOutside = peerPointsText;
	}

    // Update is called once per frame
    void Update()
    {
        if (TargetShootScript.playing)
        {

            gameTime += Time.deltaTime;
            time += Time.deltaTime;
            timeChangeRandomNoise += Time.deltaTime; //Change the noise every 10 seconds, randomly
            int currentUserScore = TargetShootScript.userScore;
            peerGoal = currentUserScore + threshold;

            if (time >= nextActionTime && nextActionTime > 0f)
            {
                if (gainPointsNextAction)
                {
                    GameObject[] peerTargets = GameObject.FindGameObjectsWithTag("PeerTarget");
                    if (peerTargets.Length != 0) //Destroy a random virtual peer target:
                    {
                        Destroy(peerTargets[Random.Range(0, peerTargets.Length)]);
                        peerPoints++;
                        DataCollector.WriteEvent("Peer hit peer's target");
                        if (peerPoints < 10 && peerPoints > -10)
                        {
                            if (peerPoints >= 0)
                                peerPointsText.text = "Peer      " + peerPoints;
                            else
                                peerPointsText.text = "Peer     " + peerPoints;
                        }
                        else if (peerPoints < 100 && peerPoints > -100)
                        {
                            if (peerPoints >= 0)
                                peerPointsText.text = "Peer     " + peerPoints;
                            else
                                peerPointsText.text = "Peer    " + peerPoints;
                        }
                        else
                        {
                            if (peerPoints >= 0)
                                peerPointsText.text = "Peer    " + peerPoints;
                            else
                                peerPointsText.text = "Peer   " + peerPoints;
                        }
                    }
                }
                else
                {
                    //Either miss (3/4 chance) or hit user's target (1/4 chance):
                    int randomNum = Random.Range(0, 4); //0, 1, 2, or 3
                    GameObject[] userTargets = GameObject.FindGameObjectsWithTag("UserTarget");
                    if (randomNum == 3 && userTargets.Length != 0)
                    {
                        Destroy(userTargets[Random.Range(0, userTargets.Length)]);
                        peerPoints--;
                        TargetShootScript.userScore++;
                        DataCollector.WriteEvent("Peer hit user's target");
                        if (TargetShootScript.userScore < 10 && TargetShootScript.userScore > -10)
                        {
                            if (TargetShootScript.userScore >= 0)
                                TargetShootScript.scoreTextChangeFromOutside.text = "You       " + TargetShootScript.userScore;
                            else
                                TargetShootScript.scoreTextChangeFromOutside.text = "You      " + TargetShootScript.userScore;
                        }
                        else if (TargetShootScript.userScore < 100 && TargetShootScript.userScore > -100)
                        {
                            if (TargetShootScript.userScore >= 0)
                                TargetShootScript.scoreTextChangeFromOutside.text = "You      " + TargetShootScript.userScore;
                            else
                                TargetShootScript.scoreTextChangeFromOutside.text = "You     " + TargetShootScript.userScore;
                        }
                        else
                        {
                            if (TargetShootScript.userScore >= 0)
                                TargetShootScript.scoreTextChangeFromOutside.text = "You     " + TargetShootScript.userScore;
                            else
                                TargetShootScript.scoreTextChangeFromOutside.text = "You    " + TargetShootScript.userScore;
                        }

                        if (peerPoints < 10 && peerPoints > -10)
                        {
                            if (peerPoints >= 0)
                                peerPointsText.text = "Peer      " + peerPoints;
                            else
                                peerPointsText.text = "Peer     " + peerPoints;
                        }
                        else if (peerPoints < 100 && peerPoints > -100)
                        {
                            if (peerPoints >= 0)
                                peerPointsText.text = "Peer     " + peerPoints;
                            else
                                peerPointsText.text = "Peer    " + peerPoints;
                        }
                        else
                        {
                            if (peerPoints >= 0)
                                peerPointsText.text = "Peer    " + peerPoints;
                            else
                                peerPointsText.text = "Peer   " + peerPoints;
                        }
                    }
                    else
                    {
                        peerPoints--; //Missed
                        DataCollector.WriteEvent("Peer missed");
                        if (peerPoints < 10 && peerPoints > -10)
                        {
                            if (peerPoints >= 0)
                                peerPointsText.text = "Peer      " + peerPoints;
                            else
                                peerPointsText.text = "Peer     " + peerPoints;
                        }
                        else if (peerPoints < 100 && peerPoints > -100)
                        {
                            if (peerPoints >= 0)
                                peerPointsText.text = "Peer     " + peerPoints;
                            else
                                peerPointsText.text = "Peer    " + peerPoints;
                        }
                        else
                        {
                            if (peerPoints >= 0)
                                peerPointsText.text = "Peer    " + peerPoints;
                            else
                                peerPointsText.text = "Peer   " + peerPoints;
                        }
                    }
                }
                time = 0f;
                nextActionTime = -1f;
            }

            if (peerPoints < peerGoal)
            {
                if (nextActionTime < 0f)
                {
                    time = 0f;
                    nextActionTime = Random.Range(0f, 3.5f) / (Mathf.Pow(Mathf.Abs(peerGoal - peerPoints), 1.3f));
                    gainPointsNextAction = true;
                }
            }
            else if (peerPoints > peerGoal)
            {
                if (nextActionTime < 0f)
                {
                    time = 0f;
                    nextActionTime = Random.Range(0f, 3.5f) / (Mathf.Pow(Mathf.Abs(peerGoal - peerPoints), 1.3f));
                    gainPointsNextAction = false;
                }
            }

            if (gameTime >= 1f)
            {
                if (currPerformanceConstant != performanceConstant)
                {
                    if (currPerformanceConstant < performanceConstant)
                        currPerformanceConstant++;
                    else
                        currPerformanceConstant--;
                }
                gameTime = 0f;
            }

            if (timeChangeRandomNoise >= 5f)
            {
                noise = Random.Range(-1, 2); //-1, 0, 1
                threshold = currPerformanceConstant + noise;
                timeChangeRandomNoise = 0f;
            }
        }
    }
}
