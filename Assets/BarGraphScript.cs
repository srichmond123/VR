using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarGraphScript : MonoBehaviour {

    // Use this for initialization
    Color blue = new Color(0, 0, 255, 0.65f); //Includes transparency
    Color red = new Color(255, 0, 0, 0.65f);
    float rightStart;
    float leftStart;
    Text text;
    RectTransform rect;
	void Start () {
        rect = GetComponent<RectTransform>();
        rightStart = rect.offsetMax.x;
        leftStart = rect.offsetMin.x;
        text = transform.parent.GetComponentInChildren<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        int diff = TargetShootScript.userScore - VirtualPeerBehavior.peerPoints;
        if (diff >= 0) //Bar should be blue or invisible
        {
            GetComponent<Image>().color = blue;
            rect.offsetMax = new Vector2(6.1f + diff * 6.1f + rightStart, rect.offsetMax.y);
            rect.offsetMin = new Vector2(6.1f + leftStart, rect.offsetMin.y);
            if (diff == 0)
            {
                text.text = "<color=yellow>0</color>";
            }
            else
            {
                text.text = "<color=blue>" + diff + "</color>";
            }
        }
        else
        {
            GetComponent<Image>().color = red;
            rect.offsetMin = new Vector2(diff * 6.1f + leftStart - 6.1f, rect.offsetMin.y);
            rect.offsetMax = new Vector2(rightStart - 6.1f, rect.offsetMax.y);
            text.text = "<color=red>" + Mathf.Abs(diff) + "</color>";
        }
	}
}
