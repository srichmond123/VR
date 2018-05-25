using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetShootScript : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        
;	}
	
	// Update is called once per frame
	void Update () {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.parent.position, transform.parent.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                Destroy(hit.transform.gameObject);
            }
            else
            {
                //Debug.DrawRay(transform.parent.transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
                //Debug.Log("Did not Hit");
            }
        }
    }
}
