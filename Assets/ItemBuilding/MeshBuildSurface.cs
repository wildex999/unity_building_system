using UnityEngine;
using System.Collections;

/*
 * Modified BuildSurface designed for MeshCollier instead of Plane Collider
 * TODO: Finish this class.
 * Add things like proper facing check, generating Snap points etc.
 * */

public class MeshBuildSurface : BuildSurface
{
	public new void Start () //Override so Unity finds it
	{
		base.Start ();
	}

	public override bool checkFacing (BuildObject placeObject, Vector3 surfacePosition)
	{
		Vector3 inwardVector = surfacePosition - transform.position;
		Vector3 facing = placeObject.transform.parent.position - transform.position;
		//float angle = Vector3.Dot (facing, inwardVector);
		float angle = Vector3.Angle (facing, inwardVector);
		if (angle <= 0 && buildInwards) {
			return true;
		} else if (angle > 0 && buildOutwards) {
			return true;
		}
		
		return false;
	}
}
