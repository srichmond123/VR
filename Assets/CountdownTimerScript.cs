using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimerScript : MonoBehaviour {

    float time = 0f;
    // Use this for initialization
    Color startColor;
	void Start () {
        startColor = GetComponent<Text>().color;
	}
	
	// Update is called once per frame
	void Update () {
        if (GetComponent<Text>().color.r > 0.01f)
        {
            time += Time.deltaTime;
            Color curr = GetComponent<Text>().color;
            float val = -0.5f * (float) System.Math.Tanh(2.2 * time - 2.0) + 0.5f;
            GetComponent<Text>().color = new Color(val, val, val);
        }
    }

    public void flash(bool sound)
    {
        time = 0f;
        if (sound)
        {
            GetComponent<AudioSource>().Play();
        }

        GetComponent<Text>().color = Color.white;
    }
}
