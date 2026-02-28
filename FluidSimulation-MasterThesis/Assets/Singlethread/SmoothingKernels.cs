using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothingKernels : MonoBehaviour
{
	static public float Poly6(/*float  particleMass,*/float smoothingRadius,float smoothinRadiusSq, float r2)
	{
		return (315.0f / (64.0f * Mathf.PI * Mathf.Pow(smoothingRadius, 9.0f))) * Mathf.Pow(smoothinRadiusSq - r2, 3.0f);
	}

	static public Vector3 GradientSpiky(Vector3 rij, float smoothingRadius, float r)
	{
	//	print(rij.normalized);
		return rij.normalized * (-45.0f / (Mathf.PI * Mathf.Pow(smoothingRadius, 6.0f))) * Mathf.Pow(smoothingRadius - r, 2.0f);
	}

	static public float ViscosityLaplacian(float smoothingRadius, float r)
	{
		return (45.0f / (Mathf.PI * Mathf.Pow(smoothingRadius, 6.0f))) * (smoothingRadius - r);
	}

	////////////////////// DOYUB KIM ////////////////////
	///
	static public float SpikySecondDerivative(float smoothingRadius, float r)
    {
		return 90.0f / (Mathf.PI * Mathf.Pow(smoothingRadius, 5.0f)) * (smoothingRadius - r);

	}

	static public Vector3 SpikyGradient(float r, Vector3 directionfromCenter, Vector3 rij, float smoothingRadius)
    {
		Vector3 temp = GradientSpiky(rij, smoothingRadius, r);
		return new Vector3(temp.x * directionfromCenter.x, temp.y * directionfromCenter.y, temp.z * directionfromCenter.z);
    }
}
