using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirtualPeerBehavior : MonoBehaviour {
    public static int peerPoints = 0;
    public static int peerGoal = 0;
    public int performanceConstant;
    float time = 0f;
    float timeChangeRandomNoise = 0f;
    float nextActionTime = -1f;
    bool gainPointsNextAction = false;
    int noise = 0;
    int threshold = 0;
    public Text peerPointsText;
    public static Text peerTextChangeFromOutside;

	// Use this for initialization
	void Start () {
        noise = Random.Range(-1, 2); //-1, 0, or 1
        threshold = performanceConstant + noise;
        peerTextChangeFromOutside = peerPointsText;
	}
	
	// Update is called once per frame
	void Update () {
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
                    peerPointsText.text = "Peer's points = " + peerPoints;
                }
            }
            else
            {
                //Either miss (3/4 chance) or hit user's target (1/4 chance):
                int randomNum = Random.Range(0, 4); //0, 1, 2, or 3
                GameObject[] userTargets = GameObject.FindGameObjectsWithTag("UserTarget");
                if (randomNum == 3 && userTargets.Length != 0 && peerPoints >= peerGoal + 2)
                {
                    Destroy(userTargets[Random.Range(0, userTargets.Length)]);
                    peerPoints--;
                    TargetShootScript.userScore++;
                    TargetShootScript.scoreTextChangeFromOutside.text = "User's Score " + TargetShootScript.userScore;
                    peerPointsText.text = "Peer's Score = " + peerPoints;
                }
                else
                {
                    peerPoints--; //Missed
                    peerPointsText.text = "Peer's points = " + peerPoints;
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
                nextActionTime = Random.Range(0f, 3.5f) / ((peerGoal - peerPoints) * (peerGoal - peerPoints));
                gainPointsNextAction = true;
            }
        }
        else if (peerPoints > peerGoal)
        {
            if (nextActionTime < 0f)
            {
                time = 0f;
                nextActionTime = Random.Range(0f, 3.5f) / ((peerPoints - peerGoal) * (peerPoints - peerGoal));
                gainPointsNextAction = false;
            }
        }

        if (timeChangeRandomNoise >= 10f)
        {
            noise = Random.Range(-1, 2); //-1, 0, 1
            threshold = performanceConstant + noise;
            timeChangeRandomNoise = 0f;
        }
	}
}
