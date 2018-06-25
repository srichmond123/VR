using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieGraphScript : MonoBehaviour {

    public Image redImage;
    public Image blueImage;
	// Use this for initialization
	void Start () {
        redImage = transform.GetChild(0).GetComponent<Image>();
        blueImage = transform.GetChild(1).GetComponent<Image>();
        redImage.enabled = false;
        blueImage.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (TargetShootScript.playing)
        {
            if (!redImage.enabled)
                redImage.enabled = true;
            if (!blueImage.enabled)
                blueImage.enabled = true;

            if (VirtualPeerBehavior.peerPoints == TargetShootScript.userScore)
            {
                blueImage.fillAmount = 0.5f;
            }
            else
            {
                if (VirtualPeerBehavior.peerPoints == 0)
                {
                    blueImage.fillAmount = 1f;
                }
                else if (TargetShootScript.userScore == 0)
                {
                    blueImage.fillAmount = 0f;
                }
                else
                {
                    blueImage.fillAmount = (TargetShootScript.userScore * 1f) / ((TargetShootScript.userScore + VirtualPeerBehavior.peerPoints) * 1f);
                }
            }
        }
	}
}
