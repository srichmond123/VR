using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScript : MonoBehaviour {

    public Sprite[] images;
    int frameCount = 0;
    int index = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (GetComponent<RawImage>().enabled)
        {
            if (frameCount > 3)
            {
                index %= 29;
                GetComponent<RawImage>().texture = images[index].texture;
                index++;
                frameCount = 0;
            }

            frameCount++;
        }
	}
}
