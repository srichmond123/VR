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

	float x = 1.0f, y = 1.0f;

	float deltaX = 1.0f;
	float deltaY = 1.0f;
	float deltaRadius = 0.0f;
	float distanceFromCenter = 5.0f;

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
			float magnitude = Mathf.Sqrt (deltaX * deltaX + deltaY * deltaY);
			float relativeDirectionAngle = Mathf.Atan (deltaY / deltaX);
			if ((deltaX < 0 && deltaY < 0) || (deltaY > 0 && deltaX < 0))
				relativeDirectionAngle += Mathf.PI;
			
			relativeDirectionAngle += randomGaussian * (Mathf.PI / 3.0f);

			deltaY = magnitude * Mathf.Sin (relativeDirectionAngle);
			deltaX = magnitude * Mathf.Cos (relativeDirectionAngle);

			randomGaussian += radius - distanceFromCenter;

			deltaRadius = randomGaussian;

			frameCount = 0;
		}

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
