﻿using UnityEngine;
using System.Collections;

public class scrAnimal : MonoBehaviour
{
	// public static float comboTimer; ??
	public GameObject SplashEffect;
	public GameObject DeathEffect;
	public Vector3 DeathEffectPosition;
	public float DeathEffectScale = 1;
	public AudioClip AudioHurt, AudioHurtAlternate, AudioDeath, AudioDeathAlternate;
	public AudioClip[] AudioIdle;
	public float Health = 1;
	private float idleTimer = 0, idleDelay = 0;
	protected bool killed = false;

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	protected virtual void Update ()
	{
		if (Time.timeScale == 0)
			return;

		if (Health <= 0)
		{
			// Fade the animal over time.
			//MeshRenderer[] meshRenderers = this.GetComponentsInChildren<MeshRenderer>();

			//foreach (MeshRenderer mr in meshRenderers)
			//{
			//	Color colour = mr.material.color;
			//	colour.a = 1 - deathFadeTimer / deathFadeDelay;
			//	mr.material.color = colour;
			//}
			//
			//if (deathFadeTimer >= deathFadeDelay)
			//{
			//	Destroy (this.gameObject);
			//}
			//
			//deathFadeTimer += Time.deltaTime;
		}
		else
		{
			if (AudioIdle.Length != 0)
			{
				idleTimer += Time.deltaTime;
				if (idleTimer >= idleDelay)
				{
					idleTimer = 0;
					idleDelay = Random.Range (10, 30);
					audio.PlayOneShot(AudioIdle[Random.Range(0, AudioIdle.Length)]);
				}
			}
		}
	}
	
	public virtual void Shoot(float damage)
	{
		if (Health > 0)
		{
			Health -= damage;

			if (Health <= 0)
			{
				Kill ();
			}
			else
			{
				audio.pitch = Random.Range (0.9f, 1.1f);
				if (Random.Range (0, 2) == 0)
					audio.PlayOneShot(AudioHurt);
				else
					audio.PlayOneShot(AudioHurtAlternate);
			}
		}
	}

	public virtual void Kill()
	{
		if (killed) return;

		// Explode
		DeathEffect = (GameObject)Instantiate(DeathEffect, this.transform.TransformPoint(DeathEffectPosition), this.transform.rotation);
		DeathEffect.particleSystem.startSpeed *= DeathEffectScale;
		DeathEffect.particleSystem.startSize *= DeathEffectScale;
		audio.pitch = Random.Range (0.9f, 1.1f);
		if (Random.Range (0, 2) == 0)
			audio.PlayOneShot(AudioDeath);
		else
			audio.PlayOneShot(AudioDeathAlternate);

		foreach (Transform child in GetComponentsInChildren<Transform>())
			child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

		rigidbody.constraints = RigidbodyConstraints.None;

		Health = 0;

		killed = true;
	}

	void OnTriggerEnter(Collider other)
	{
		Debug.Log (other.name);
		if (other.name == "Explosion(Clone)")
		{
			Debug.Log (this.name);
			Kill ();
		}

		if (Health <= 0 && other.gameObject.layer == LayerMask.NameToLayer("Water"))
			Instantiate (SplashEffect, this.transform.position, Quaternion.identity);
	}
}
