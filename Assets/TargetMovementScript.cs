﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovementScript : MonoBehaviour {
	/*
	 * The target will be moving in a correlated random walk, with the 
	 * probability of it going further smoothly decreasing as it approaches the prohibited area around the
	 * user's feet (this doesn't apply for anywhere else).
	 */

	const float velocity = 5f;
	int turns = 0;

	float oldY;

	float deltaX = 1.0f;
	float deltaY = 1.0f;
	float deltaRadius = 0.0f;
	float distanceFromCenter = 5.0f;

	public float relativeDirectionAngle;
	//public float[] randomGaussianArr;
	//int randomGaussianIndex = 0;

	public static float radius = 5.0f;

	int frameCount = 0;

	public Transform cameraTransform;

	void Start () {
		float random = Random.Range (0.0f, Mathf.PI * 2.0f);
		if (transform.localPosition.y < 2.4f) {
			relativeDirectionAngle /= 2.0f;
		}
		deltaX = velocity * Mathf.Cos(relativeDirectionAngle);
		deltaY = velocity * Mathf.Sin (relativeDirectionAngle);
        deltaRadius = Random.Range(0, 2) * 2 - 1;
        cameraTransform = GameObject.FindGameObjectWithTag("VRCamera").transform;
		oldY = transform.localPosition.y;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//Move phi, theta, then p:
		if (frameCount > 12) {

            float randomGaussian = RandomFromStandardNormalDistribution () / 3.5f;
			//randomGaussian = Mathf.Clamp (randomGaussian, -1.0f, 1.0f);
			//if (randomGaussianIndex == randomGaussianArr.Length) {
			//	randomGaussianIndex = 0;
			//}
			//relativeDirectionAngle = Mathf.Atan (deltaY / deltaX);
			//if ((deltaX < 0 && deltaY < 0) || (deltaY > 0 && deltaX < 0))
			//	relativeDirectionAngle += Mathf.PI;
			
			relativeDirectionAngle += randomGaussian * (Mathf.PI / 3.0f);
			relativeDirectionAngle %= (Mathf.PI * 2f);

			randomGaussian = RandomFromStandardNormalDistribution () * 0.5f; //Constant 0.5 can change

			if (distanceFromCenter > radius + randomGaussian) {
				deltaRadius = -0.75f;
			} else {
				deltaRadius = 0.75f;
			}

			frameCount = 0;
		}


		if (transform.localPosition.y < 3.0f && transform.localPosition.y - oldY < 0f) { //Start turning smoothly if it's in this zone and moving down:
			turns++;
			float newAngle = relativeDirectionAngle + Mathf.PI / 15.0f;
			if (Mathf.Sin (newAngle) * transform.up.y > Mathf.Sin (relativeDirectionAngle) * transform.up.y) {
				relativeDirectionAngle += Mathf.PI / 15.0f;
			} else {
				relativeDirectionAngle -= Mathf.PI / 15.0f;
			}
			
		} else if (turns > 0 && transform.localPosition.y > 3.2f) {
			turns = 0;
		}
			
		deltaY = velocity * Mathf.Sin (relativeDirectionAngle);
		deltaX = velocity * Mathf.Cos (relativeDirectionAngle);


		//Some vector math so that the orbit is spherical and not rectangular: 

		Vector3 oldPosition = transform.localPosition;

		oldY = transform.localPosition.y;

		transform.localPosition += transform.right * deltaX * Time.deltaTime;
		transform.localPosition += transform.up * deltaY * Time.deltaTime;

		Vector3 directionMovedIn = transform.localPosition - oldPosition;

		Vector3 targetDirection = transform.localPosition - cameraTransform.localPosition;
		transform.RotateAround (transform.localPosition, Vector3.Cross (directionMovedIn, -transform.forward), Vector3.Angle(transform.forward, targetDirection) );

		distanceFromCenter += deltaRadius * Time.deltaTime;

		float newDistanceFromCenter = Vector3.Magnitude (transform.localPosition - cameraTransform.localPosition);
		transform.localPosition += transform.forward * -(newDistanceFromCenter - distanceFromCenter);

		frameCount++;
	}


    float RandomFromStandardNormalDistribution()
    {
        // also known as Marsaglia polar method:


        // calculate points on a circle
        float u, v;

        float s; // this is the hypotenuse squared.
        do
        {
            u = Random.Range(-1f, 1f);
            v = Random.Range(-1f, 1f);
            s = (u * u) + (v * v);
        } while (!(s != 0 && s < 1)); // keep going until s is nonzero and less than one

        // TODO allow a user to specify how many random numbers they want!
        // choose between u and v for seed (z0 vs z1)
        float seed;
        if (Random.Range(0, 2) == 0)
        {
            seed = u;
        }
        else
        {
            seed = v;
        }

        // create normally distributed number.
        float z = seed * Mathf.Sqrt(-2.0f * Mathf.Log(s) / s);

        return z;
    }
}
