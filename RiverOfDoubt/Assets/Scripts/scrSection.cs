﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrSection : MonoBehaviour
{
	private static scrGameManager gameManager;

	public static void Initialize(scrGameManager _gameManager)
	{
		gameManager = _gameManager;
	}

	// Properties of the section.
	public int RocksToGenerate = 10;
	public int TreeBirdsToGenerate = 5;
	public int OverheadBirdsToGenerate = 5;
	public int ElephantsToGenerate = 3;
	public int HutsOrCrocodilesToGenerate = 5;
	public Transform[] Connectors;
	public bool CanGenerateSplitters;
	public bool IsSplitterSection { get; protected set; }
	public int SectionIndex { get; protected set; }
	public scrSection PreviousSection { get; protected set; }
	private scrSection[] nextSections;
	private Transform[] rocks;
	private List<Transform> nativeAnimals = new List<Transform>();
	private bool entered = false;

	// Use this for initialization
	void Start ()
	{
		// Some sections have preset rocks. Deparent them if this is the case in order for them to function properly.
		if (name == "Section_Lake_Volcano_Huts(Clone)")
		{
			StartCoroutine(FreeChildRocks());
		}
	}

	private IEnumerator FreeChildRocks()
	{
		foreach (Transform child in GetComponentsInChildren<Transform>(true))
		{
			if (child.name.Contains("Rock"))
			{
				if (child.Find ("Graphics"))
					child.parent = null;
			}


			if (child.name == "LeftRocks")
				Debug.Log ("LeftRocks");

			if (child.name.Contains("rock"))
			{
				child.gameObject.SetActive(true);
			}

			yield return new WaitForSeconds(0.1f);
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (gameManager.Boat.transform.position.z - this.transform.position.z > 800)
			Destroy (this.gameObject);
	}

	public void GenerateNextSections(bool direct)
	{
		// Don't generate more sections if the sections have already been generated.
		if (entered == true) return;

		if (nextSections == null)
		{
			// Set the number of next sections to number of connectors.
			nextSections = new scrSection[Connectors.Length];
			
			// Generate the next sections for each connector.
			for (int i = 0; i < Connectors.Length; i++)
			{
				int section = 0;

				// Find and add a random section to the connector. (If the section can generate splitters, give it a 50% chance to do so).
				if (CanGenerateSplitters == true && Random.Range (0, 2) == 0)
				{
					// If splitters can be generated, and the 50% chance has been achieved, and the previous section's previous section isn't a splitter, then give an extra 25% chance to create a special section.
					if (PreviousSection != null && PreviousSection.IsSplitterSection == false && Random.Range (0, 4) == 0)
					{
						Debug.Log (PreviousSection.name);
						nextSections[i] = ((GameObject)Instantiate(gameManager.SpecialSections[section = Random.Range(0, gameManager.SpecialSections.Length)], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						nextSections[i].CanGenerateSplitters = false;	// Prevent the next section from creating splitters and special sections.
					}
					else
					{
						nextSections[i] = ((GameObject)Instantiate(gameManager.SplitterSections[section = (SectionIndex + Random.Range(1, gameManager.SplitterSections.Length - 2)) % gameManager.SplitterSections.Length], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						nextSections[i].IsSplitterSection = true;
					}
				}
				else
				{
					if (i > 0)
					{
						if (Connectors[i - 1].position.x < Connectors[i].position.x && nextSections[i - 1].name != "Section_Left(Clone)")
						{
							nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[section = 2], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						}
						else if (Connectors[i - 1].position.x > Connectors[i].position.x && nextSections[i - 1].name != "Section_Right(Clone)")
						{
							nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[section = 0], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						}
						else
						{
							nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[section = 1], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						}
					}
					else
					{
						nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[section = (SectionIndex + Random.Range(1, gameManager.Sections.Length - 1)) % gameManager.Sections.Length], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
					}
				}

				nextSections[i].SectionIndex = section;

				// Generate rocks for the next section.
				//nextSections[i].GenerateRocks(10);
				if (rocks == null && RocksToGenerate > 0)
					StartCoroutine(GenerateRocks(RocksToGenerate));

				//nextSections[i].GenerateAnimals(20, 4, 3, 10);
				if (nativeAnimals.Count == 0)
					StartCoroutine(GenerateAnimals(TreeBirdsToGenerate, OverheadBirdsToGenerate, ElephantsToGenerate, HutsOrCrocodilesToGenerate, Random.Range (0, 3) == 0));

				// Set the previous section of the next section to this section.
				nextSections[i].PreviousSection = this;
			}
		}

		if (direct == true)
		{
			for (int i = 0; i < nextSections.Length; i++)
			{
				nextSections[i].GenerateNextSections(false);
			}

			// Flag as entered.
			entered = true;
		}
	}

	public IEnumerator GenerateAnimals(int treeBirds, int overheadBirds, int elephants, int huts, bool crocodileHutOverride)
	{
		Transform[] parts = this.transform.GetComponentsInChildren<Transform>();

		List<Transform> palms = new List<Transform>();
		List<Transform> animalHooks = new List<Transform>();
		List<Transform> hutHooks = new List<Transform>();

		for (int i = 0; i < parts.Length; i++)
		{
			if (parts[i].name == "palm_trio")
				palms.Add(parts[i]);
			else if (parts[i].name == "AnimalHook")
				animalHooks.Add(parts[i]);
			else if (parts[i].name == "HutHook")
				hutHooks.Add(parts[i]);
		}

		#region Tree Birds
		while (palms.Count > 0 && treeBirds > 0)
		{
			int i = Random.Range (0, palms.Count);

//			if (palms[i] == null)
//			{
//				palms.RemoveAt (i);
//				continue;
//			}

			Transform replacement = ((GameObject)Instantiate (gameManager.PopulatedPalmPrefab, palms[i].position, palms[i].rotation)).transform;
			replacement.parent = palms[i].parent;
			foreach (scrBirdSitting bird in replacement.GetComponentsInChildren<scrBirdSitting>())
			{
				bird.transform.parent = null;
				nativeAnimals.Add(bird.transform);
			}
			//replacement.DetachChildren();
			Destroy (palms[i].gameObject);
			palms.RemoveAt(i);
			--treeBirds;

			yield return new WaitForSeconds(0.1f);
		}
		#endregion

		#region Elephants
		while (animalHooks.Count > 0 && elephants > 0)
		{
			int i = Random.Range (0, animalHooks.Count);
			Transform replacement = ((GameObject)Instantiate (gameManager.ElephantPrefab, animalHooks[i].position + animalHooks[i].forward + Vector3.up * 0.05f, animalHooks[i].rotation)).transform;
			
			// Choose whether to put a tree in front of the elephant.
			if (Random.Range(0, 2) == 0)
			{
				Transform fallingLog = ((GameObject)Instantiate(gameManager.FallingLogPrefab, animalHooks[i].position + animalHooks[i].forward + Vector3.up * gameManager.FallingLogPrefab.transform.localScale.y * 0.52f, animalHooks[i].rotation)).transform;
				nativeAnimals.Add(fallingLog);
				replacement.Translate (0, 0, -12, Space.Self);
				replacement.GetComponent<scrElephantStanding>().TreeToPush = fallingLog;
			}
			
			nativeAnimals.Add(replacement);
			Destroy(animalHooks[i].gameObject);
			animalHooks.RemoveAt(i);
			--elephants;
			
			yield return new WaitForSeconds(0.1f);;
		}
		#endregion

		#region Huts or Crocodiles
		// Check for huts.
		Transform hutGroup = this.transform.Find("Huts");

		// Check for unflippable huts.
		if (hutGroup == null)
			hutGroup = this.transform.Find ("Huts_Noflip");

		if (hutGroup != null)
		{
			// 50% chance for huts to be on either side.
			if (hutGroup.name != "Huts_Noflip" && Random.Range (0, 2) == 0)
				hutGroup.localScale = new Vector3(-hutGroup.localScale.x, hutGroup.localScale.y, hutGroup.localScale.z);
			
			while (hutHooks.Count > 0 && huts > 0)
			{
				int i = Random.Range (0, hutHooks.Count);

//				// I have ABSOLUTELY NO IDEA why this happens: Even though I only remove the hut hooks that I add huts to, they still get included in the search.
//				if (hutHooks[i] == null || hutHooks[i].Find ("Hut_A(Clone)") || hutHooks[i].Find ("Hut_B(Clone)") || hutHooks[i].Find ("Raft(Clone)"))
//				{
//					hutHooks.RemoveAt (i);
//					continue;
//				}

				Transform replacement;

				// Normal huts.
				if (crocodileHutOverride == false)
				{
					if (Random.Range (0, 2) == 0)
					{
						replacement = ((GameObject)Instantiate (gameManager.HutAPrefab, hutHooks[i].position + Vector3.up * Random.Range (2f, 3f), Quaternion.Euler(0, Random.Range (0, 360), 0))).transform;				
					}
					else
					{
						replacement = ((GameObject)Instantiate (gameManager.HutBPrefab, hutHooks[i].position + Vector3.up * Random.Range (2f, 3f), Quaternion.Euler(0, Random.Range (0, 360), 0))).transform;	
					}
					
					// 90% chance to have a native in the hut.
					if (Random.Range (0, 10) < 9)
					{
						Transform native = ((GameObject)Instantiate (gameManager.NativePrefab, replacement.transform.position + Vector3.up * 3f, Quaternion.identity)).transform;
						native.parent = replacement;
						nativeAnimals.Add (native);
					}
				}
				else
				{
					// Spawn a crocodile instead of a hut. This varies hut sections a little more.
					replacement = ((GameObject)Instantiate (gameManager.CrocodilePrefab, hutHooks[i].position, Quaternion.Euler(0, Random.Range (0, 360), 0))).transform;	
				}

				nativeAnimals.Add(replacement);
				Destroy(hutHooks[i].gameObject);
				hutHooks.RemoveAt(i);
				--huts;
				
				yield return new WaitForSeconds(0.1f);;
			}
		}
		#endregion

		#region Overhead Birds
		for (int i = 0; i < overheadBirds; i++)
		{
			// Instantiate a flying bird at a random position after the end of the section.
			Rigidbody bird = ((GameObject)Instantiate (gameManager.BirdFlyingPrefab, this.transform.position + new Vector3(Random.Range (-100f, 100f), Random.Range (25f, 100f), 400 + Random.Range (0f, 800f)), Quaternion.Euler(0, 180, 0))).rigidbody;

			// Give the bird force to make it move in the opposite direction to the general direction of the player.
			bird.AddForce(0, 0, -600);

			yield return new WaitForSeconds(0.1f);;
		}
		#endregion
	}

	public IEnumerator GenerateRocks(int numRocks)
	{
		if (IsSplitterSection == true && gameManager.SplitterSections[SectionIndex].name.Contains ("Huts") ||
		    IsSplitterSection == false && gameManager.Sections[SectionIndex].name.Contains ("Huts")) yield break;

		// Get all children of this object.
		Transform[] children = gameObject.GetComponentsInChildren<Transform>();
		List<Transform> waters = new List<Transform>();

		float minX = float.PositiveInfinity;

		// Filter the children to get the water planes.
		for (int i = 0; i < children.Length; i++)
		{
			if (children[i].gameObject.layer == LayerMask.NameToLayer("Water"))
			{
				waters.Add(children[i]);

				// Find the minimum furthest left water plane.
				float currentX = children[i].transform.position.x - transform.localScale.x * 10;
				if (currentX < minX)
					minX = currentX;
			}
		}

		// Get the large rock transform for convenience.
		Transform largeRock = gameManager.Rocks[gameManager.Rocks.Length - 1].transform;

		// Get the factor of the width and length of the water planes that corresponds to the size of a large rock.
		int rocksAcross = (int)((10 * waters[0].localScale.x * waters.Count) / largeRock.localScale.x);
		int rocksDown = (int)((10 * waters[0].localScale.z) / largeRock.localScale.z) + 1;

		// Create an array of available positions to perform the lottery operation on.
		Vector2[,] availablePositions = new Vector2[rocksAcross, rocksDown];
		for (int i = 0; i < rocksAcross; i++)
			for (int j = 0; j < rocksDown; j++)
				availablePositions[i, j] = new Vector2(i - 1, j);

		// Make sure there can't be more rocks across and down than there are spaces for rocks!
		if (numRocks > rocksAcross * rocksDown)
			numRocks = rocksAcross * rocksDown;

		// Create an array of rocks.
		rocks = new Transform[numRocks];

		// Generate rocks.
		for (int i = 0; i < rocksDown && numRocks > 0; i++)
		{
			for (int j = 0; j < rocksAcross && numRocks > 0; j++)
			{
				int positionDown = Random.Range (i, rocksDown);
				int positionAcross = Random.Range (j, rocksAcross);

				// Instantiate the rock.
				rocks[numRocks - 1] = ((GameObject)Instantiate (gameManager.Rocks[Random.Range (0, gameManager.Rocks.Length)],
	             	new Vector3(minX, waters[0].position.y, this.transform.position.z) + new Vector3(availablePositions[positionAcross, positionDown].x, 0, availablePositions[positionAcross, positionDown].y) * largeRock.localScale.x,
	            	gameManager.Rocks[0].transform.rotation)).transform;

				rocks[numRocks - 1].FindChild ("Graphics").rotation = Quaternion.Euler(Random.Range (0, 360), Random.Range (0, 360), Random.Range (0, 360));

				// Decrease the number of rocks.
				numRocks--;

				// Swap the current available position along and the found position.
				Vector2 temp = availablePositions[j, i];
				availablePositions[j, i] = availablePositions[positionAcross, positionDown];
				availablePositions[positionAcross, positionDown] = temp;

				yield return new WaitForSeconds(0.1f);
			}
		}
	}

	void OnDestroy()
	{
		if (rocks != null)
		{
			// Destroy all instances associated with this section.
			for (int i = 0; i < rocks.Length; i++)
				if (rocks[i] != null)
					Destroy (rocks[i].gameObject);
		}

		for (int i = nativeAnimals.Count - 1; i >= 0; i--)
		{
			if (nativeAnimals[i] != null)
				Destroy (nativeAnimals[i].gameObject);

			nativeAnimals.RemoveAt (i);
		}
	}

	/// <summary>
	/// Destroys the sections which are out of view or have not been taken.
	/// </summary>
	/// <param name="keepSection">The connected section which won't be destroyed (the section the boat is currently in).</param>
	public void DestroyRedundantSections(scrSection keepSection)
	{
		// Destroy untaken path(s).
		for (int i = 0; i < nextSections.Length; i++)
		{
			if (nextSections[i] != null && nextSections[i] != keepSection)
			{
				for (int j = 0; j < nextSections[i].nextSections.Length; j++)
					Destroy (nextSections[i].nextSections[j].gameObject);

				Destroy (nextSections[i].gameObject);
			}
		}

		// Destroy the previous section as it is out of view.
		if (PreviousSection != null)
			Destroy (PreviousSection.gameObject);
	}
}
