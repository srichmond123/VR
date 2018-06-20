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
    AudioSource userAudioSource;

    public GameObject blueBalloonUIElement;
    public GameObject redBalloonUIElement;
	// Use this for initialization
	void Start () {
        scoreTextChangeFromOutside = scoreText;
        userAudioSource = GameObject.Find("User Audio Source").GetComponent<AudioSource>();
;	}

    // Update is called once per frame
    void Update()
    {
        if (!playing)
        {
            GetComponent<MeshRenderer>().enabled = false;
            gameObject.transform.parent.GetChild(0).GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }

        if (thisControllerInUse)
        {
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            {
                RaycastHit hit;

                if (Physics.Raycast(transform.parent.position, transform.parent.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
                {
                    //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                    if (hit.transform.parent.parent.parent.tag.Equals("UserTarget"))
                    {
                        userScore++;
                        DataCollector.WriteEvent("User hit user's target");
                        GenerateTargetsScript.numUserTargets--;
                        GameObject.Find("whitenPanel").GetComponent<ScoreFlashScript>().flash(Color.blue);

                        /*
                        GameObject pref = Instantiate(blueBalloonUIElement);
                        pref.transform.localPosition = hit.transform.parent.transform.localPosition;
                        pref.transform.localPosition = Vector3.Lerp(pref.transform.localPosition, new Vector3(0, 5, 0), 0.65f);
                        pref.transform.SetParent(GameObject.Find("Canvas").transform);
                        */
                        userAudioSource.transform.localPosition = hit.transform.parent.parent.parent.localPosition;
                        userAudioSource.Play();
                        //Destroy(hit.transform.parent.gameObject);
                        SetLayerRecursively(hit.transform.parent.parent.parent.gameObject, LayerMask.NameToLayer("Ignore Raycast"));
                        hit.transform.parent.parent.parent.tag = "Destroying";
                    }
                    else if (hit.transform.parent.parent.parent.tag.Equals("PeerTarget"))
                    {
                        userScore--;
                        userScore = Mathf.Max(0, userScore);
                        VirtualPeerBehavior.peerPoints++;
                        DataCollector.WriteEvent("User hit peer's target");
                        GameObject.Find("whitenPanel").GetComponent<ScoreFlashScript>().flash(Color.red);
                        GenerateTargetsScript.numPeerTargets--;

                        /*
                        GameObject pref = Instantiate(redBalloonUIElement);
                        pref.transform.localPosition = hit.transform.parent.transform.localPosition;
                        pref.transform.localPosition = Vector3.Lerp(pref.transform.localPosition, new Vector3(0, 5, 0), 0.65f);
                        pref.transform.SetParent(GameObject.Find("Canvas").transform);
                        */
                        userAudioSource.transform.localPosition = hit.transform.parent.parent.parent.localPosition;
                        userAudioSource.Play();
                        //Destroy(hit.transform.parent.gameObject);
                        SetLayerRecursively(hit.transform.parent.parent.parent.gameObject, LayerMask.NameToLayer("Ignore Raycast"));
                        hit.transform.parent.parent.parent.tag = "Destroying";

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
                }
                else
                {
                    userScore--;
                    userScore = Mathf.Max(0, userScore);
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
                initializeController();
            }
        }
        if (!playing && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            if (gameObject.tag.Equals("LeftHandLaser"))
            {
                initializeController();
            }
        }
    }

    void initializeController()
    {
        playing = true;
        thisControllerInUse = true;
        //DataCollector.collectingData = true;
        GameObject.Find("ControllerImage").SetActive(false);
        GameObject.Find("BeginText").SetActive(false);
        gameObject.transform.parent.GetChild(0).GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.parent.parent.parent.tag.Equals("UserTarget") || collision.transform.parent.parent.parent.tag.Equals("PeerTarget"))
        {
            float distanceFromCenter = Vector3.Distance(collision.gameObject.transform.position, (transform.position - new Vector3(0, transform.localScale.y, 0)));
            transform.localScale = new Vector3(transform.localScale.x, distanceFromCenter/4f, transform.localScale.z);
            transform.localPosition = new Vector3(0, 0, transform.localScale.y);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.parent.parent.parent.tag.Equals("UserTarget") || collision.transform.parent.parent.parent.tag.Equals("PeerTarget"))
        {
            float distanceFromCenter = Vector3.Distance(collision.gameObject.transform.position, (transform.position - new Vector3(0, transform.localScale.y, 0)));
            transform.localScale = new Vector3(transform.localScale.x, distanceFromCenter / 4f, transform.localScale.z);
            transform.localPosition = new Vector3(0, 0, transform.localScale.y);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        transform.localScale = new Vector3(transform.localScale.x, 8.2f, transform.localScale.z);
        transform.localPosition = new Vector3(0, 0, transform.localScale.y);
    }
}
