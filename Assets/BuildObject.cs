using UnityEngine;
using System.Collections;

/*
 * All objects that can be placed on a Build Surface must have this Script(Or a Script that inherits it) as a component.
 * This will handle things like the contact points and collision checking.
 * When the object has been placed, this Script can be disabled or removed.
 * */

public class BuildObject : MonoBehaviour
{
	public bool ignoreCollisions = false; //Allow for ignoring any collisions when placing
	public GameObject buildPointsParent; //The parent object containing a list of Build Points as children
	public float sizeInHand = 0.5f; //The scale to use when the item is in our hand

	private int collisionCount = 0;
	private BuildPoint currentBuildPoint;

	public int getBuildPointCount ()
	{
		if (buildPointsParent == null)
			return 0;

		int pointCount = 0;
		foreach (Transform child in buildPointsParent.transform) {
			pointCount++;
		}

		return pointCount;
	}
	
	public int addBuildPoint (BuildPoint point)
	{
		if (buildPointsParent == null)
			return -1;
		point.transform.parent = buildPointsParent.transform;

		return getBuildPointCount () - 1;
	}

	public BuildPoint getBuildPoint (int index)
	{
		if (buildPointsParent == null)
			return null;

		int curIndex = 0;
		foreach (Transform child in buildPointsParent.transform) {
			if (curIndex == index)
				return child.GetComponent<BuildPoint> ();
			curIndex++;
		}
		return null;
	}

	//Returns the current Build Point for this BuildObject
	//This is set before calling canPlace and placeObject on BuildSurface.
	public BuildPoint getCurrentBuildPoint ()
	{
		return currentBuildPoint;
	}

	public bool canPlace (BuildSurface surface, Vector3 position, Quaternion rotation)
	{
		if (!ignoreCollisions && collisionCount != 0)
			return false;

		return true;
	}

	void OnCollisionEnter (Collision collision)
	{
		collisionCount++;
	}

	void OnCollisionExit (Collision collision)
	{
		collisionCount--;
	}
}
