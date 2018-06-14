using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreFlashScript : MonoBehaviour {
    float time = 0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (gameObject.GetComponent<Image>().color.a > 0.00001f)
        {
            time += 3f * Time.deltaTime;
            Color curr = gameObject.GetComponent<Image>().color;
            gameObject.GetComponent<Image>().color = new Color(curr.r, curr.g, curr.b, (float) (-System.Math.Tanh(time - 0.25f) + 1.0));
        }
	}

    public void flash(Color c)
    {
        time = 0f;
        gameObject.GetComponent<Image>().color = c;
    }
}
