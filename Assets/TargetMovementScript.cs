using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovementScript : MonoBehaviour {
	/*
	 * (Remember to use radians) - the target will be moving in a correlated random walk, with the 
	 * probability of it going further smoothly decreasing as it approaches the prohibited area around the
	 * user's feet (this doesn't apply for anywhere else).
	 */

	const float velocity = 8.0f;
	bool flippedDirection = false;

	float x = 1.0f, y = 1.0f;
	int turns = 0;

	float deltaX = 1.0f;
	float deltaY = 1.0f;
	float deltaRadius = 0.0f;
	float distanceFromCenter = 5.0f;

	float relativeDirectionAngle;

	const float radius = 5.0f;

	static int frameCount = 0;

	Transform cameraTransform;

	void Start () {
		float random = Random.Range (-1.0f, 1.0f);
		deltaX = velocity * random;
		deltaY = Mathf.Sqrt (velocity * velocity - deltaX * deltaX);
		//deltaRadius = velocity * random;
		cameraTransform = FindObjectOfType<Camera> ().transform;
	}
	
	// Update is called once per frame
	void Update () {
		//Move phi, theta, then p:
		if (frameCount > 12) {
			float randomGaussian = RandomFromDistribution.RandomFromStandardNormalDistribution () / 3.5f;
			randomGaussian = Mathf.Clamp (randomGaussian, -1.0f, 1.0f);
			relativeDirectionAngle = Mathf.Atan (deltaY / deltaX);
			if ((deltaX < 0 && deltaY < 0) || (deltaY > 0 && deltaX < 0))
				relativeDirectionAngle += Mathf.PI;
			
			relativeDirectionAngle += randomGaussian * (Mathf.PI / 3.0f);
			randomGaussian += radius - distanceFromCenter;

			deltaRadius = randomGaussian;
			frameCount = 0;
		}
		if (transform.localPosition.y < 2.5f && turns < 10) { //Start turning smoothly
			turns++;
			if (relativeDirectionAngle > 0 || turns > 1)
				relativeDirectionAngle -= Mathf.PI / 10.0f;
			else
				relativeDirectionAngle += Mathf.PI / 10.0f;
			
		} else if (transform.localPosition.y >= 3.0f) {
			turns = 0;
		}
			/*if (transform.localPosition.y < 2.0f && !flippedDirection) { //Turn abruptly
				if (relativeDirectionAngle > 0) {
					relativeDirectionAngle -= Mathf.PI;
				} else {
					relativeDirectionAngle += Mathf.PI;
				}
				flippedDirection = true;
			} else {
				flippedDirection = false;
			}*/

		deltaY = velocity * Mathf.Sin (relativeDirectionAngle);
		deltaX = velocity * Mathf.Cos (relativeDirectionAngle);

			
		//Set to rotate about sphere:

		Vector3 oldPosition = transform.localPosition;

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
}
