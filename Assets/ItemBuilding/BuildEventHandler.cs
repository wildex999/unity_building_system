using UnityEngine;
using System.Collections;

/*
 * Base event handler for BuildSurface, extend this to handle events.
 * 
 * Events:
 * canPlace(Surface, BuildObject, SurfacePosition) - Called to ask if a certain object can be placed
 * objectPlace(Surface, BuildObject, ObjectPosition, ObjectRotation) - Called before an object is placed.
 * objectRemove(Surface, BuildObject) - Called before an object is removed.
 * */

public class BuildEventHandler : MonoBehaviour
{
	public virtual bool canPlace (BuildSurface surface, BuildObject obj, Vector3 surfacePosition)
	{
		return true;
	}
	public virtual void objectPlace (BuildSurface surface, BuildObject obj, Vector3 objectPosition, Quaternion objectRotation)
	{

	}
	public virtual void objectRemove (BuildSurface surface, BuildObject obj)
	{

	}
}
