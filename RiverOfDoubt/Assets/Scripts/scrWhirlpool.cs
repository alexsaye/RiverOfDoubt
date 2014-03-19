﻿using UnityEngine;
using System.Collections;

public class scrWhirlpool : MonoBehaviour
{
	private float spinSpeed = 200;
	private Transform[] segments;

	// Use this for initialization
	void Start ()
	{
		segments = new Transform[this.GetComponentsInChildren<Transform>().Length];

		for (int i = 0; i < segments.Length; i++)
		{
			segments[i] = this.transform.FindChild("Segment_" + i.ToString());
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		for (int i = 0; i < segments.Length; i++)
		{
			if (segments[i] != null)
				segments[i].Rotate(0, spinSpeed / (1 + i * 0.3f) * Time.deltaTime, 0);
		}
	}
}