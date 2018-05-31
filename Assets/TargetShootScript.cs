using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetShootScript : MonoBehaviour {

    public static int userScore = 0;
    public Text scoreText;
    public static Text scoreTextChangeFromOutside;
	// Use this for initialization
	void Start () {
        scoreTextChangeFromOutside = scoreText;
;	}
	
	// Update is called once per frame
	void Update () {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.parent.position, transform.parent.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                if (hit.transform.parent.tag.Equals("UserTarget"))
                {
                    userScore++;
                } else
                {
                    userScore--;
                    VirtualPeerBehavior.peerPoints++;
                    VirtualPeerBehavior.peerTextChangeFromOutside.text = "Peer's Score = " + VirtualPeerBehavior.peerPoints;

                }
                Destroy(hit.transform.gameObject);
            }
            else
            {
                userScore--;
                //Debug.DrawRay(transform.parent.transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
                //Debug.Log("Did not Hit");
            }
            scoreText.text = "Your Score = " + userScore;
        }

    }
}
