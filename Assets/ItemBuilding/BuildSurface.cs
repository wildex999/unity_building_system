using UnityEngine;
using System.Collections.Generic;

/*
 * A Build Surface is a surface that other objects can be placed on.
 * Objects placed on the surface will have an position offset and rotation offset.
 * Each surface have a number of slots, the max number of objects that can be placed on it.
 * 
 * Colliders are used to define the surface(s) shape, and Raycasting is used to get a point on the surface.
 * 
 * The Build Surface can also have snap-points. These are points on the surface that items will snap onto 
 * when not in freeform placement.
 * 
 * This base implementation is meant to use a plane collider(Box collider with one size set to 0)
 * 
 * Events:
 * canPlace(Surface, BuildObject, SurfacePosition) - Called to ask if a certain object can be placed
 * objectPlace(Surface, BuildObject, SurfacePosition, ObjectPosition, ObjectRotation) - Called before an object is placed.
 * objectRemove(Surface, BuildObject) - Called before an object is removed.
 * */

public class BuildSurface : MonoBehaviour
{
	private List<BuildObject> objectList;

	public int slots = 0; //Max number of objects that can be placed on the surface(0 = unlimited)
	public BuildEventHandler eventListener; //Allow event listener to be set from Editor
	public bool buildOutwards = true;
	public bool buildInwards = false;

	public bool forcePlaceOnSnapPoint = false; //Whether or not placing is only allowed on the snap point(s)
	public bool drawSnapPointsOnPlace = true; //Whether or not to show snap points when placing object on surface
	public float snapDistance = 0.5f; //The max distance between position and point for it to snap to the point
	public List<Vector3> snapPoints; //Snap points, in local coordinates

	private bool currentlyShowingSnapPoints = false;
	private List<GameObject> visibleSnapPoints; //List of all snap points currently being renderer

	// Use this for initialization
	public void Start ()
	{
		objectList = new List<BuildObject> ();
		//snapPoints = new List<Vector3> (); //Initialized by Unity since public & serialized
		visibleSnapPoints = new List<GameObject> ();
	}
	
	public virtual void showSnapPoints (bool show)
	{
		if (!drawSnapPointsOnPlace)
			return;
		if (show && currentlyShowingSnapPoints)
			return;
		if (!show && !currentlyShowingSnapPoints)
			return;

		if (show) {
			currentlyShowingSnapPoints = true;
			foreach (Vector3 point in snapPoints) {
				GameObject newObj = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				newObj.transform.parent = transform;
				newObj.transform.localPosition = point;
				newObj.transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);
				newObj.GetComponent<Collider> ().enabled = false; 
				newObj.GetComponent<Renderer> ().material.color = new Color (0.1f, 0.1f, 0.1f);
				visibleSnapPoints.Add (newObj);
			}
		} else {
			foreach (GameObject obj in visibleSnapPoints) {
				if (obj == null)
					return;
				Destroy (obj);
			}
			visibleSnapPoints.Clear ();
			currentlyShowingSnapPoints = false;
		}
	}

	//Get the closest snap point to the given location
	//position: Global position
	//out snapPoint: The new snap point returned. Will be the same as position if none was found
	//Return: True if a snap point was found, false if not
	public virtual bool getSnapPoint (Vector3 position, out Vector3 snapPoint)
	{
		//Convert global position into local
		Vector3 localPosition = transform.InverseTransformPoint (position);

		//Go through all points, and find the closest
		bool gotFirst = false;
		Vector3 closest = localPosition;
		float closestDistance = float.MaxValue;
		foreach (Vector3 point in snapPoints) {
			float nextDistance = Vector3.Distance (point, localPosition);
			//Find first point
			if (!gotFirst) {
				if (nextDistance > snapDistance)
					continue;

				closest = point;
				closestDistance = nextDistance;
				gotFirst = true;
				continue;
			}

			//Try to find a closer point
			if (nextDistance < closestDistance) {
				closest = point;
				closestDistance = nextDistance;
			}
		}

		if (!gotFirst) {
			snapPoint = position;
			return false;
		}

		snapPoint = transform.TransformPoint (closest);
		return true;
	}

	//Check if the current facing is correct
	//Return: True if placing allowed on the current facing
	public virtual bool checkFacing (BuildObject placeObject, Vector3 surfacePosition)
	{
		if (transform.parent == null) { //Can't check facing
			return true;//For now we default to allowing it
		} else {
			Vector3 inwardVector = transform.position - transform.parent.position;
			Vector3 facing = placeObject.transform.parent.position - transform.position;
			float angle = Vector3.Dot (facing, inwardVector);
			if (angle <= 0 && buildInwards) {
				return true;
			} else if (angle > 0 && buildOutwards) {
				return true;
			}
		}

		return false;
	}

	//Check whether the surface allows the given object to be placed here
	public virtual bool canPlace (BuildObject placeObject, Vector3 surfacePosition)
	{
		if (eventListener != null && !eventListener.canPlace (this, placeObject, surfacePosition)) {
			Debug.Log ("canPlaceFailed: eventCanPlace");
			return false;
		}
		if (objectList.Count >= slots && slots != 0) {
			Debug.Log ("canPlaceFailed: Slot count");
			return false;
		}
		if (!checkFacing (placeObject, surfacePosition)) {
			Debug.Log ("canPlaceFailed: facing");
			return false;
		}

		//TODO: Further checking should be done to verify that the object is not
		//being placed in the air at some random position(Cheating?), at least server side.

		return true;
	}

	//Place the object on the position with the given rotation
	//Assumes canPlace has already been called
	public virtual void placeObject (BuildObject placeObject, Vector3 position, Quaternion rotation)
	{
		if (eventListener != null)
			eventListener.objectPlace (this, placeObject, position, rotation);
		if (transform.parent == null)
			placeObject.transform.parent = transform;
		else
			placeObject.transform.parent = transform.parent; //TEST: Set as sibling instead of child

		placeObject.transform.position = position;
		placeObject.transform.rotation = rotation;
		//TODO: Here would be the place to inform the server.

		objectList.Add (placeObject);

	}

	//Remove the object from the surface.
	//Return true if the object was removed(If false, the object wasn't here to begin with)
	public virtual bool removeObject (BuildObject removeObject)
	{
		if (eventListener != null)
			eventListener.objectRemove (this, removeObject);

		bool removed = objectList.Remove (removeObject);
		if (removed)
			removeObject.transform.parent = null;

		return removed;
	}
}
