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
 * canPlace(Surface, BuildObject, SurfacePosition, ObjectPosition, ObjectRotation) - Called to ask if a certain object can be placed
 * objectPlace(Surface, BuildObject, SurfacePosition, ObjectPosition, ObjectRotation) - Called before an object is placed.
 * objectRemove(Surface, BuildObject) - Called before an object is removed.
 * */

public class BuildSurface : MonoBehaviour
{

	private Vector3 inwardVector;
	private List<BuildObject> objectList;

	public int slots; //Max number of objects that can be placed on the surface
	public BuildEventHandler eventListener; //Allow event listener to be set from Editor
	public bool buildOutwards = true;
	public bool buildInwards = false;

	public bool forcePlaceOnSnapPoint = false; //Whether or not placing is only allowed on the snap point(s)
	public bool drawSnapPointsOnHover = false; //Whether or not to show snap points when placing object on surface
	private bool drawSnapPoints = false;

	// Use this for initialization
	void Start ()
	{
		objectList = new List<BuildObject> ();

		//Only need to call this client side
		calculateFacing ();
	}

	//Calculate the "inward" facing vector.
	//Should be called in canPlace the relative positions change often.
	public void calculateFacing ()
	{
		inwardVector = transform.position - transform.parent.position;
	}
	
	public void showSnapPoints (bool show)
	{
		drawSnapPoints = show;
	}

	//Get the closest snap point to the given location
	public Vector3 getSnapPoint (Vector3 position)
	{
		return position;
	}

	//Check whether the given object can be placed here
	public bool canPlace (BuildObject placeObject, Vector3 surfacePosition, Vector3 objectPosition, Quaternion objectRotation)
	{
		//if(eventListener != null)
		//	eventListener.canPlace();

		if (objectList.Count >= slots)
			return false;

		//Check facing
		Vector3 facing = objectPosition - transform.position;
		float angle = Vector3.Dot (facing, inwardVector);
		if (angle <= 0 && buildInwards) {
			return true;
		} else if (angle > 0 && buildOutwards) {
			return true;
		}

		//Chekc if object agrees(Does collision check etc.)
		if (!placeObject.canPlace (this, objectPosition, objectRotation))
			return false;

		//TODO: Further checking should be done to verify that the object is not
		//being placed in the air at some random position(Cheating?), at least server side.

		return false;
	}

	//Place the object on the position with the given rotation
	//Assumes canPlace has already been called
	public void placeObject (BuildObject placeObject, Vector3 position, Vector3 rotation)
	{
		//if(eventListener != null)
		//	eventListener.objectPlace();

		//TODO: Here would be the place to inform the server.

		objectList.Add (placeObject);
	}

	//Remove the object from the surface.
	//Return true if the object was removed(If false, the object wasn't here to begin with)
	public bool removeObject (BuildObject removeObject)
	{
		//if(eventListener != null)
		//	eventListener.objectRemove();

		return objectList.Remove (removeObject);
	}
}
