using UnityEngine;
using System.Collections;

public class ItemPlacer : MonoBehaviour
{
	public BuildObject[] inventory = new BuildObject[5];
	public int currentIndex = 0;

	private BuildObject itemInHand;
	private bool isShowingItem = false;

	public float placeDistance = 5f;
	
	void Start ()
	{
		//TEST: Get the first item from inventory for now
		//TODO: Allow selecting item using numbers/scroll
		instantiateHandItem (currentIndex);
	}

	//Instantiate an item from inventory(index) into the hand
	public void instantiateHandItem (int index)
	{
		if (index >= 0 && inventory.Length > index && inventory [index] != null) {
			BuildObject instance = Instantiate (inventory [index]);
			BuildObject oldItem = setItemInHand (instance);
			if (oldItem != null)
				Destroy (oldItem);
		}
	}

	void Update ()
	{
		Cursor.lockState = CursorLockMode.Locked;

		if (itemInHand == null)
			return;

		bool resetToHand = true;
		Camera cam = GetComponentInChildren<Camera> ();
		Ray ray = cam.ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0));
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, placeDistance)) {
			BuildSurface surface = hit.collider.GetComponent<BuildSurface> ();
			if (surface != null) {

				if (surface.canPlace (itemInHand, hit.point, transform.position, transform.rotation)) {
					Debug.DrawLine (transform.position, hit.point, Color.black, 0);
					Debug.DrawRay (hit.point, hit.normal, Color.black, 0);
					showItemOnSurface (surface, hit);
					resetToHand = false;

					if (Input.GetButtonDown ("Fire1")) {
						surface.placeObject (itemInHand, itemInHand.transform.position, itemInHand.transform.rotation);
						instantiateHandItem (currentIndex);
					}
				} else {

				}

			}
		}

		if (resetToHand) {
			//When not possible to place, show the item in our hand
			showItemInHand (true);
		}
	}

	//Set and show the current item in hand
	//Return: The previous item
	public BuildObject setItemInHand (BuildObject item)
	{
		BuildObject oldItem = null;
		if (itemInHand != null)
			oldItem = removeItemInHand ();

		itemInHand = item;
		showItemInHand (true);

		if (itemInHand.getBuildPointCount () > 0)
			itemInHand.setCurrentBuildPoint (itemInHand.getBuildPoint (0));

		return oldItem;
	}

	//Remove the current item from the hand
	//Return: The item removed
	public BuildObject removeItemInHand ()
	{
		BuildObject item = itemInHand;
		if (item == null)
			return null;

		showItemInHand (false);
		itemInHand = null;
		return item;
	}

	public void showItemInHand (bool show)
	{
		if (show && !isShowingItem) {
			itemInHand.transform.parent = transform;
			itemInHand.transform.localPosition = new Vector3 (0.8699999f, 0.19f, 1.09f); //Magic numbers FTW. TODO: Fix.
			itemInHand.transform.localRotation = new Quaternion (0, 0, 0, 0); //TODO: Use rotation stored in Object?
			itemInHand.transform.localScale = itemInHand.transform.localScale * itemInHand.sizeInHand;
			isShowingItem = true;
		} else if (!show && isShowingItem) {
			itemInHand.transform.parent = null;
			itemInHand.transform.localScale = itemInHand.transform.localScale / itemInHand.sizeInHand;
			isShowingItem = false;
		}
	}


	//Show the item as placed on the surface, will use the object's current Build Point.
	//Assumes a canPlace check has already been done on the surface.
	//Will color green if it can be placed, red if not.(canPlace check on Object)
	//ray: Defines the point and normal of the surface we are to show the item on
	public void showItemOnSurface (BuildSurface surface, RaycastHit ray)
	{
		if (itemInHand == null)
			return;

		BuildPoint currentPoint = itemInHand.getCurrentBuildPoint ();
		if (currentPoint == null)
			return;

		showItemInHand (false);

		//Rotate the object so that the current Build Point is placed on the surface according to the normal
		//First we find out how much to rotate.
		Vector3 vectorPoint = (currentPoint.transform.position - itemInHand.transform.position);
		//Vector3 vectorSurface = (surface.transform.position - itemInHand.transform.position); //Might use this one later if we want to rotate it towards a point on the surface
		Vector3 vectorSurface = -ray.normal;
		Quaternion rotDelta = Quaternion.FromToRotation (vectorPoint, vectorSurface);

		//Then we apply the rotation
		itemInHand.transform.rotation = rotDelta * itemInHand.transform.rotation;


		//Move the object so the Build Point is placed at the Ray point
		Vector3 newPosition = ray.point - vectorPoint;
		itemInHand.transform.position = newPosition;
	}
}
