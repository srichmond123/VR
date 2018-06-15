using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonUIBehavior : MonoBehaviour {

    Vector3 destination;
	// Use this for initialization
	void Start () {
        destination = GameObject.Find("userPointsText").transform.localPosition; //Goes towards scoreboard
	}
	
	// Update is called once per frame
	void Update () {
		if (Vector3.Distance(transform.localPosition, destination) > 0.2f) 
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, destination, 0.1f);
        }
        else
        {
            Destroy(gameObject);
        }
	}
}
