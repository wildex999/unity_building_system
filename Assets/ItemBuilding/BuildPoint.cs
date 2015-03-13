using UnityEngine;
using System.Collections;

/*
 * Defines the point on an Object that will be placed on the Build Surface.
 * Also allows you to define whether you can rotate or move freely with this point.
 * 
 * TODO: Further development might be to allow setting a specific range of movement of rotation around the point.
 * That could be especially useful if used with Snap Points on Surfaces where you are only allowed to build on the snap point.
 * */

public class BuildPoint : MonoBehaviour
{
	public bool canRotate = true;
	public bool canMoveFree = true;
}
