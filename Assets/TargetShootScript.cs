using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetShootScript : MonoBehaviour {

    public static int userScore = 0;
    public Text scoreText;
    public static Text scoreTextChangeFromOutside;
    public static bool playing = false; //On waiting screen:
    private bool thisControllerInUse = false;
	// Use this for initialization
	void Start () {
        scoreTextChangeFromOutside = scoreText;
;	}

    // Update is called once per frame
    void Update()
    {
        if (!playing)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }

        if (thisControllerInUse)
        {
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            {
                RaycastHit hit;

                if (Physics.Raycast(transform.parent.position, transform.parent.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
                {
                    //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                    if (hit.transform.parent.tag.Equals("UserTarget"))
                    {
                        userScore++;
                        DataCollector.WriteEvent("User hit user's target");
                    }
                    else
                    {
                        userScore--;
                        VirtualPeerBehavior.peerPoints++;
                        DataCollector.WriteEvent("User hit peer's target");

                        if (VirtualPeerBehavior.peerPoints < 10 && VirtualPeerBehavior.peerPoints > -10)
                        {
                            if (VirtualPeerBehavior.peerPoints >= 0)
                                VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer      " + VirtualPeerBehavior.peerPoints;
                            else
                                VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer     " + VirtualPeerBehavior.peerPoints;
                        }
                        else if (VirtualPeerBehavior.peerPoints < 100 && VirtualPeerBehavior.peerPoints > -100)
                        {
                            if (VirtualPeerBehavior.peerPoints >= 0)
                                VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer     " + VirtualPeerBehavior.peerPoints;
                            else
                                VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer    " + VirtualPeerBehavior.peerPoints;
                        }
                        else
                        {
                            if (VirtualPeerBehavior.peerPoints >= 0)
                                VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer    " + VirtualPeerBehavior.peerPoints;
                            else
                                VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer   " + VirtualPeerBehavior.peerPoints;
                        }

                    }
                    Destroy(hit.transform.parent.gameObject);
                }
                else
                {
                    userScore--;
                    //Debug.DrawRay(transform.parent.transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
                    //Debug.Log("Did not Hit");
                    DataCollector.WriteEvent("User missed");
                }
                if (userScore < 10 && userScore > -10)
                {
                    if (userScore >= 0)
                        scoreText.text = "You       " + userScore;
                    else
                        scoreText.text = "You      " + userScore;
                }
                else if (userScore < 100 && userScore > -100)
                {
                    if (userScore >= 0)
                        scoreText.text = "You      " + userScore;
                    else
                        scoreText.text = "You     " + userScore;
                }
                else
                {
                    if (userScore >= 0)
                        scoreText.text = "You     " + userScore;
                    else
                        scoreText.text = "You    " + userScore;
                }

            }

        }
        if (!playing && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) //Set to use right handed controller:
        {
            if (gameObject.tag.Equals("RightHandLaser")) //Meaning we're in the right hand script and the user pressed their right hand:
            {
                playing = true;
                thisControllerInUse = true;
                DataCollector.collectingData = true;
                GameObject.Find("ControllerImage").SetActive(false);
                GameObject.Find("BeginText").SetActive(false);
                GetComponent<MeshRenderer>().enabled = true;
            }
        }
        if (!playing && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            if (gameObject.tag.Equals("LeftHandLaser"))
            {
                playing = true;
                thisControllerInUse = true;
                DataCollector.collectingData = true;
                GameObject.Find("ControllerImage").SetActive(false);
                GameObject.Find("BeginText").SetActive(false);
                GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }
}
