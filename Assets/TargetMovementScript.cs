using System.Collections;
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
	int changeAngleFrames = 49;  //Change this to modify how frequently the angle changes

	public float relativeDirectionAngle;
	public float goalDirectionAngle;
	public float oldDirectionAngle;
	public float step = 0f;

	public static float radius = 5.0f;

	int frameCount = 0;


    public float theta;
    public float phi;
    float deltaTheta;
    float deltaPhi;
    float gameTime = 0f;
    const float sigma = 0.03f;
    const float sigmar = 0.1f;
    float omegaPhi = 0.5f;
    float omegaTheta = 0.5f;

    public Transform cameraTransform;

	void Start () {
        
		float random = Random.Range (0.0f, Mathf.PI * 2.0f);
        relativeDirectionAngle = random;
		if (transform.localPosition.y < 2.4f) {
			relativeDirectionAngle /= 2.0f;
		}
		deltaX = velocity * Mathf.Cos(relativeDirectionAngle);
		deltaY = velocity * Mathf.Sin (relativeDirectionAngle);
        
        
        deltaRadius = Random.Range(0, 2) * 2 - 1;
        cameraTransform = GameObject.FindGameObjectWithTag("VRCamera").transform;
		oldY = transform.localPosition.y;

		frameCount = 41; //Change goalDirectionAngle immediately so it begins by gradually changing angle
		goalDirectionAngle = relativeDirectionAngle;

        //New spherical method:
        omegaPhi = Random.Range(-1f, 1f);
        omegaTheta = Random.Range(-1f, 1f);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (!tag.Equals("Destroying"))
        {
            if (frameCount > changeAngleFrames)
            {

                float randomGaussian = RandomFromStandardNormalDistribution();
                //randomGaussian = Mathf.Clamp (randomGaussian, -1.0f, 1.0f);
                //if (randomGaussianIndex == randomGaussianArr.Length) {
                //	randomGaussianIndex = 0;
                //}
                //relativeDirectionAngle = Mathf.Atan (deltaY / deltaX);
                //if ((deltaX < 0 && deltaY < 0) || (deltaY > 0 && deltaX < 0))
                //	relativeDirectionAngle += Mathf.PI;

                goalDirectionAngle += randomGaussian * (Mathf.PI / 3.0f);

                goalDirectionAngle %= (Mathf.PI * 2f);
                if (goalDirectionAngle < 0)
                {
                    goalDirectionAngle += Mathf.PI * 2f;
                }


                float totalChange; //totalChange/40 = step size, must make sure it's in the right direction too:
                if (Mathf.Abs(goalDirectionAngle - relativeDirectionAngle) > Mathf.PI)
                { //Should take shortest path:
                    totalChange = Mathf.PI * 2f - Mathf.Abs(goalDirectionAngle - relativeDirectionAngle);
                }
                else
                {
                    totalChange = Mathf.Abs(goalDirectionAngle - relativeDirectionAngle);
                }
                if (goalDirectionAngle > relativeDirectionAngle && Mathf.Abs(goalDirectionAngle - relativeDirectionAngle) > Mathf.PI)
                {
                    step = -1f;
                }
                else if (goalDirectionAngle > relativeDirectionAngle)
                {
                    step = 1f;
                }
                else if (goalDirectionAngle < relativeDirectionAngle && Mathf.Abs(goalDirectionAngle - relativeDirectionAngle) > Mathf.PI)
                {
                    step = 1f;
                }
                else
                {
                    step = -1f;
                }
                step *= totalChange / (changeAngleFrames * 1f);

                oldDirectionAngle = relativeDirectionAngle;

                frameCount = 0;
            }

            if (frameCount % 10 == 0)
            {
                float randomGaussianRadius = RandomFromStandardNormalDistribution() * 3.5f; //Constant can change

                if (distanceFromCenter > radius + randomGaussianRadius)
                {
                    deltaRadius = -0.75f;
                }
                else
                {
                    deltaRadius = 0.75f;
                }
            }

            //Gradually change angle at t to intended angle at t+E*dt:
            relativeDirectionAngle += step;


            if ((transform.localPosition.y < 3.0f && transform.localPosition.y - oldY < 0f) ||
                (transform.localPosition.y > 8.2f && transform.localPosition.y - oldY > 0f))
            { //Start turning smoothly if it's in the prohibited zone and moving down/up into it:
                turns++;
                float addAngle = relativeDirectionAngle + Mathf.PI / 50.0f;
                float subAngle = relativeDirectionAngle - Mathf.PI / 50.0f;
                Vector3 newPosAdd = transform.localPosition;
                newPosAdd += transform.right * velocity * Mathf.Cos(addAngle) * Time.deltaTime;
                newPosAdd += transform.up * velocity * Mathf.Sin(addAngle) * Time.deltaTime;

                Vector3 newPosSub = transform.localPosition;
                newPosSub += transform.right * velocity * Mathf.Cos(subAngle) * Time.deltaTime;
                newPosSub += transform.up * velocity * Mathf.Sin(subAngle) * Time.deltaTime;

                if ((transform.localPosition.y < 3.0f && newPosAdd.y > newPosSub.y)
                    || (transform.localPosition.y > 8.2f && newPosAdd.y < newPosSub.y))
                {
                    if (step < 0)
                    {
                        step = -step;
                    }
                    relativeDirectionAngle = addAngle;
                }
                else
                {
                    if (step > 0)
                    {
                        step = -step;
                    }
                    relativeDirectionAngle = subAngle;
                }

            }
            else if (turns > 0 && transform.localPosition.y > 3.2f)
            {
                turns = 0;
            }


            deltaY = velocity * Mathf.Sin(relativeDirectionAngle);
            deltaX = velocity * Mathf.Cos(relativeDirectionAngle);


            //Some vector math so that the orbit is spherical and not rectangular: 

            Vector3 oldPosition = transform.localPosition;

            oldY = transform.localPosition.y;

            transform.localPosition += transform.right * deltaX * Time.deltaTime;
            transform.localPosition += transform.up * deltaY * Time.deltaTime;

            Vector3 directionMovedIn = transform.localPosition - oldPosition;

            Vector3 targetDirection = transform.localPosition - cameraTransform.localPosition;
            transform.RotateAround(transform.localPosition, Vector3.Cross(directionMovedIn, -transform.forward), Vector3.Angle(transform.forward, targetDirection));
            transform.GetChild(0).GetChild(0).LookAt(new Vector3(0, 100000, 0));
            //transform.GetChild(1).localPosition = 
             //   transform.GetChild(0).GetChild(0).gameObject.GetComponent<Rigidbody>().centerOfMass;

            distanceFromCenter += deltaRadius * Time.deltaTime;

            float newDistanceFromCenter = Vector3.Magnitude(transform.localPosition - cameraTransform.localPosition);
            transform.localPosition += transform.forward * -(newDistanceFromCenter - distanceFromCenter);

            relativeDirectionAngle %= (Mathf.PI * 2f);
            if (relativeDirectionAngle < 0)
            {
                relativeDirectionAngle += Mathf.PI * 2f;
            }

            /*
            //New method: spherical coordinates
            ///////////////////



            gameTime += Time.deltaTime;
            if (frameCount > 12)
            {

                float randomGaussian = RandomNormalDistribution(0f, sigma * Mathf.Sqrt(Time.deltaTime));
                deltaTheta = (1f / Mathf.Sin(phi)) * (randomGaussian) + (omegaTheta) * Mathf.Sin(phi) * Time.deltaTime;

                randomGaussian = RandomNormalDistribution(0f, sigma * Mathf.Sqrt(Time.deltaTime));
                deltaPhi = randomGaussian + ( (sigma * sigma) / (2f * Mathf.Tan(phi)) ) * Time.deltaTime + (omegaPhi) * Time.deltaTime;

                randomGaussian = RandomFromStandardNormalDistribution() * 1f; //Constant can change

                if (distanceFromCenter > radius + randomGaussian)
                {
                    deltaRadius = -0.75f;
                }
                else
                {
                    deltaRadius = 0.75f;
                }
                frameCount = 0;


            }

            if ((phi < Mathf.PI/4f && deltaPhi < 0) || (phi > 3f*(Mathf.PI/4f) && deltaPhi > 0) )
            {
                deltaPhi = -deltaPhi;
                omegaPhi = -omegaPhi;
            }

            float randomGaussian2 = RandomNormalDistribution(0f, sigmar * Mathf.Sqrt(Time.deltaTime));
            deltaRadius = randomGaussian2;

            theta %= Mathf.PI * 2f;
            phi %= Mathf.PI * 2f;

            theta += deltaTheta;
            phi += deltaPhi;
            distanceFromCenter = radius + 1f * Mathf.Cos(4f * gameTime); //Harmonic oscillation

            transform.localPosition
            = new Vector3(distanceFromCenter * Mathf.Sin(phi) * Mathf.Cos(theta), distanceFromCenter * Mathf.Cos(phi) + radius, distanceFromCenter * Mathf.Sin(phi) * Mathf.Sin(theta));


            ////


        */
            /////////////////////////keep this:
            frameCount++;

        }
        else
        {
            if (transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color.a > 0.01f)
            {
                Color c = transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color;
                transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer> ().material.color = new Color(c.r, c.g, c.b, Mathf.Max(0f, c.a - 0.05f));
                transform.localPosition += new Vector3(0, 0.1f, 0);
            }
            else
            {
                Destroy(gameObject);
            }
        }
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

    public float RandomNormalDistribution(float mean, float std_dev)
    {

        // Get random normal from Standard Normal Distribution
        float random_normal_num = RandomFromStandardNormalDistribution();

        // Stretch distribution to the requested sigma variance
        random_normal_num *= std_dev;

        // Shift mean to requested mean:
        random_normal_num += mean;

        // now you have a number selected from a normal distribution with requested mean and sigma
        return random_normal_num;

    }
}
