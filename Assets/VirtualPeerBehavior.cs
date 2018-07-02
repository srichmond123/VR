using System.Collections;
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
    float randomNoiseScheduledChangeTime = 0f;
    float gameTime = 0f;
    float noiseCapIncrementTime = 0f;
    float nextActionTime = -1f;
    bool gainPointsNextAction = false;
    int noise = 0;
    int threshold = 0;
    public Text peerPointsText;
    public static Text peerTextChangeFromOutside;
    AudioSource peerAudioSource;
    int noiseCap = 1;

    public GameObject blueBalloonUIElement;
    public GameObject redBalloonUIElement;

	// Use this for initialization
	void Start () {
        noise = Random.Range(-noiseCap, noiseCap + 1); //-3, -2, -1, 0, 1, 2, or 3
        noiseCap++;
        randomNoiseScheduledChangeTime = Random.Range(3f, 5f);
        threshold = currPerformanceConstant + noise;
        peerTextChangeFromOutside = peerPointsText;
        peerAudioSource = GameObject.Find("Peer Audio Source").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetShootScript.playing && !GenerateTargetsScript.inTutorial)
        {

            gameTime += Time.deltaTime;
            noiseCapIncrementTime += Time.deltaTime;
            time += Time.deltaTime;
            timeChangeRandomNoise += Time.deltaTime; //Change the noise every 10 seconds, randomly
            int currentUserScore = TargetShootScript.userScore;
            threshold = currPerformanceConstant + noise;
            peerGoal = Mathf.Max(0, currentUserScore + threshold);

            if (time >= nextActionTime && nextActionTime > 0f)
            {
                if (gainPointsNextAction)
                {
                    GameObject[] peerTargets = GameObject.FindGameObjectsWithTag("PeerTarget");
                    if (peerTargets.Length != 0) //Destroy a random virtual peer target:
                    {
                        int randIndex = Random.Range(0, peerTargets.Length);
                        peerAudioSource.transform.localPosition = peerTargets[randIndex].transform.localPosition;
                        peerAudioSource.Play();

                        GameObject.Find("whitenPanel").GetComponent<ScoreFlashScript>().flash(Color.red);

                        /*
                         GameObject pref = Instantiate(redBalloonUIElement);
                         pref.transform.localPosition = peerTargets[randIndex].transform.localPosition;
                         pref.transform.localPosition = Vector3.Lerp(pref.transform.localPosition, new Vector3(0, 5, 0), 0.65f);
                         pref.transform.SetParent(GameObject.Find("Canvas").transform);
                       */

                        //Destroy(peerTargets[randIndex]);
                        SetLayerRecursively(peerTargets[randIndex], LayerMask.NameToLayer("Ignore Raycast"));
                        //peerTargets[randIndex].layer = LayerMask.NameToLayer("Ignore Raycast");
                        peerTargets[randIndex].tag = "Destroying";
                        peerPoints++;
                        DataCollector.WriteEvent("Peer hit peer's target", peerTargets[randIndex].transform.localPosition);

                        GenerateTargetsScript.numPeerTargets--;
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
                    //Either miss or hit user's target:
                    float randomNum = Random.Range(0f, 1f);
                    GameObject[] userTargets = GameObject.FindGameObjectsWithTag("UserTarget");
                    if (randomNum < 0.05f && userTargets.Length != 0)
                    {
                        int randIndex = Random.Range(0, userTargets.Length);
                        peerAudioSource.transform.localPosition = userTargets[randIndex].transform.localPosition;
                        peerAudioSource.Play();
                        //Destroy(userTargets[randIndex]);

                        SetLayerRecursively(userTargets[randIndex], LayerMask.NameToLayer("Ignore Raycast"));
                        userTargets[randIndex].tag = "Destroying";

                        peerPoints--;
                        peerPoints = Mathf.Max(0, peerPoints);
                        TargetShootScript.userScore++;
                        DataCollector.WriteEvent("Peer hit user's target", userTargets[randIndex].transform.localPosition);
                        GameObject.Find("whitenPanel").GetComponent<ScoreFlashScript>().flash(Color.blue);
                        GenerateTargetsScript.numUserTargets--;
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
                    else if (peerPoints > 0)
                    {
                        peerPoints--; //Missed
                        DataCollector.WriteEvent("Peer missed", Vector3.zero);
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
                    nextActionTime = Mathf.Max(0.3f, Random.Range(0f, 3.5f) / (Mathf.Pow(Mathf.Abs(peerGoal - peerPoints), 1.3f)));
                    gainPointsNextAction = true;
                }
            }
            else if (peerPoints > peerGoal)
            {
                if (nextActionTime < 0f)
                {
                    time = 0f;
                    nextActionTime = Mathf.Max(0.3f, Random.Range(0f, 3.5f) / (Mathf.Pow(Mathf.Abs(peerGoal - peerPoints), 1.3f)));
                    gainPointsNextAction = false;
                }
            }

            if (gameTime >= 2f)
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

            if (noiseCapIncrementTime >= 3f)
            {
                if (noiseCap < 3)
                    noiseCap++;

                noiseCapIncrementTime = 0f;
            }

            if (timeChangeRandomNoise >= randomNoiseScheduledChangeTime)
            {
                noise = Random.Range(-noiseCap, noiseCap + 1);

                threshold = currPerformanceConstant + noise;
                timeChangeRandomNoise = 0f;
                randomNoiseScheduledChangeTime = Random.Range(3f, 5f);
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
