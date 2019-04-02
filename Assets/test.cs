using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{

	// Use this for initialization
	void Start()
	{

	}

	void LateUpdate()
	{
		InstantiateCube();
	}

	void InstantiateCube()
	{
		if (Input.GetButtonDown("Fire1"))
		{
			Vector3 screenPos = Input.mousePosition;
			screenPos.z = 1000f;
			var target = Camera.main.ScreenToWorldPoint(screenPos);

			Debug.Log(target);
			Debug.Log("Click!");
		}

		if (Input.GetAxis("Mouse ScrollWheel") != 0)
		{
			Debug.Log("Wheel!");
		}
	}

}