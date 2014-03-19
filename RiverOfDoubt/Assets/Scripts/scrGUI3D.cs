﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class Collectable
{
	public Transform transform { get; private set; }
	public Vector3 InitialPosition { get; private set; }
	private float collectTimer;
	private float collectDelay;

	public float TimerProgress { get { return collectTimer / collectDelay; } }


	public Collectable(Transform transform, float collectDelay)
	{
		this.transform = transform;
		this.InitialPosition = transform.position;
		this.collectDelay = collectDelay;
		this.collectTimer = 0;
	}
	
	public void IncrementTimer()
	{
		collectTimer += Time.deltaTime;
	}

	public void Destroy()
	{
		GameObject.Destroy(this.transform.gameObject);
	}
}

public class scrGUI3D : MonoBehaviour
{	
	public static bool ReticleIsVisible = true;
	private static Rect reticleDestination { get { return new Rect(Screen.width / 2 - 16, Screen.height / 2 - 16, 32, 32); } }
	private static Rect reticleSource = new Rect(0, 0, 1, 1);

	private static List<Collectable> collectionItems = new List<Collectable>();
	private static Vector3 collectionPoint = new Vector3(-4.175f, -2, 5);
	private static float collectionStayTime = 1.5f;
	private static bool collecting = false;
	private static Transform chestLid;
	private static float chestTimer = 0;
	private static float chestDelay = 0.5f;

	public enum Parts { Feather, Tusk, Mask }
	private static int[] collectedParts = new int[3];

	private static scrGUI3D instance;
	private static Camera gunCamera;

	public Texture2D ReticleTexture;
	public AudioClip AudioCollect;

	// Use this for initialization
	void Start ()
	{
		instance = this;
		gunCamera = Camera.main.transform.FindChild("Gun Camera").camera;
		chestLid = this.transform.FindChild("Chest").FindChild("Lid");
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Whether to show the gun camera or not depends on whether the reticle is visible.
		gunCamera.enabled = ReticleIsVisible;

		// Reset collecting. It will be flagged during the collection checking loop.
		collecting = false;

		for (int i = collectionItems.Count - 1; i >= 0; i--)
		{
			// Run the collectable's collection timer.
			collectionItems[i].IncrementTimer();
			if (collectionItems[i].TimerProgress >= 1 + collectionStayTime)
			{
				Destroy (collectionItems[i].transform.gameObject);
				collectionItems.RemoveAt(i);
			}
			else
			{
				// Currently collecting an item.
				collecting = true;

				// Reduce size after the item has reached the collection point.
				if (collectionItems[i].TimerProgress >= 1)
				{
					collectionItems[i].transform.localScale = Vector3.Lerp (Vector3.one, Vector3.zero, Mathf.SmoothStep(0f, 1f, (collectionItems[i].TimerProgress - 1) / collectionStayTime));
				}

				// Smoothstep lerp the collectable towards the collection point.
				collectionItems[i].transform.position = Vector3.Lerp(collectionItems[i].InitialPosition, this.transform.TransformPoint(collectionPoint), Mathf.SmoothStep(0f, 1f, collectionItems[i].TimerProgress));

				// Rotate the collectable.
				collectionItems[i].transform.Rotate (0, 150 * Time.deltaTime, 0);
			}
		}

		// Run the chest timer forwards or backwards depending on the collection status.
		if (collecting == true)
		{
			chestTimer += Time.deltaTime;
			if (chestTimer >= chestDelay)
				chestTimer = chestDelay;
		}
		else
		{
			chestTimer -= Time.deltaTime;
			if (chestTimer <= 0)
				chestTimer = 0;
		}

		// Rotate the chest lid with the chest timer.
		chestLid.localEulerAngles = new Vector3(-60 * Mathf.SmoothStep(chestTimer, chestDelay, chestTimer / chestDelay), 0, 0);
	}

	void OnGUI()
	{
		if (ReticleIsVisible == true)
			GUI.DrawTextureWithTexCoords(reticleDestination, ReticleTexture, reticleSource);
	}

	public static void CollectItem(GameObject itemPrefab, Vector3 worldPosition, float timeToCollect, Parts part)
	{
		// Get the screen position of the item.
		Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

		// Set the new world position of the item.
		worldPosition = instance.camera.ScreenToWorldPoint(screenPosition);

		// Instantiate the item.
		Transform item = ((GameObject)Instantiate (itemPrefab, worldPosition, instance.transform.rotation)).transform;
		item.gameObject.layer = LayerMask.NameToLayer("GUI");
		foreach (Transform child in item.GetComponentsInChildren<Transform>())
		{
			child.gameObject.layer = item.gameObject.layer;
		}

		collectionItems.Add(new Collectable(item, timeToCollect));
		instance.audio.PlayOneShot(instance.AudioCollect);

		++collectedParts[(int)part];
	}
}