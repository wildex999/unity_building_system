using UnityEngine;
using System.Collections.Generic;

public class ItemPlacer : MonoBehaviour
{
	public BuildObject[] inventory = new BuildObject[5];
	public int currentIndex = 0;
	public int currentPointIndex = 0;

	private BuildObject itemInHand;
	private bool isShowingItem = false;
	private bool hasPlaced = false; //Used to make sure we only do one placement per Update
	private Quaternion itemRotation; //Store the initial rotation of the item
	private BuildSurface prevSurface;

	public float placeDistance = 5f;
	public int layerSurfacesDisabled = 2;

	public class ObjectLayer
	{
		public GameObject obj;
		public int layer;
		public ObjectLayer (GameObject obj, int layer)
		{
			this.obj = obj;
			this.layer = layer;
		}
	}
	private List<ObjectLayer> disabledColliders;


	void Start ()
	{
		disabledColliders = new List<ObjectLayer> ();
		//TEST: Get the first item from inventory for now
		//TODO: Allow selecting item using numbers/scroll
		instantiateItemInHand (currentIndex);
	}

	//Instantiate an item from inventory(index) into the hand
	public void instantiateItemInHand (int index)
	{
		if (index >= 0 && inventory.Length > index && inventory [index] != null) {
			BuildObject instance = Instantiate (inventory [index]);

			BuildObject oldItem = setItemInHand (instance);
			if (oldItem != null)
				Destroy (oldItem.gameObject);
		}
	}

	void Update ()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		hasPlaced = false;
		if (itemInHand == null)
			return;

		//Check if we are changing current Item
		float scroll = Input.GetAxis ("Mouse ScrollWheel");
		if (scroll != 0f) {
			if (scroll < 0) {
				currentIndex++;
				if (currentIndex >= inventory.Length)
					currentIndex = 0;
			} else {
				currentIndex--;
				if (currentIndex < 0)
					currentIndex = inventory.Length - 1;
			}
			instantiateItemInHand (currentIndex);
		}

		//Check if we are changing current Build point
		if (itemInHand != null && Input.GetButtonDown ("Fire2")) {
			int count = itemInHand.getBuildPointCount ();
			if (count != 0) {
				if (++currentPointIndex >= count)
					currentPointIndex = 0;
				itemInHand.setCurrentBuildPoint (itemInHand.getBuildPoint (currentPointIndex));
			}

		}

		if (itemInHand != null) {
			//Do placement check
			if (doPlaceUpdate ()) {
				//When not possible to place, show the item in our hand
				showItemInHand (true);
			}
		}


	}

	//Updater placement.
	//Return false if item is not currently showing on a surface
	public bool doPlaceUpdate ()
	{
		bool resetToHand = true;
		Camera cam = GetComponentInChildren<Camera> ();
		Ray ray = cam.ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0));
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, placeDistance)) {
			BuildSurface surface = hit.collider.GetComponent<BuildSurface> ();
			
			//Debugging
			Debug.DrawLine (transform.position, hit.point, Color.black, 0);
			Debug.DrawRay (hit.point, hit.normal, Color.black, 0);
			
			if (surface != null) {
				
				if (surface.canPlace (itemInHand, hit.point)) {
					if (prevSurface != surface && prevSurface != null)
						prevSurface.showSnapPoints (false);
					prevSurface = surface;
					showItemOnSurface (surface, hit);
					surface.showSnapPoints (true);
					resetToHand = false;
					
					if (!hasPlaced && Input.GetButtonDown ("Fire1")) {
						
						//Check if object allows us to place it there(Does collision check etc.)
						if (itemInHand.canPlace (surface, itemInHand.getCurrentBuildPoint ().transform.position, itemInHand.transform.rotation)) {
							surface.placeObject (itemInHand, itemInHand.transform.position, itemInHand.transform.rotation);
							removeItemInHand (); //Do this so the new instantiate doesn't delete it
						
							//Get a new of the same item for our hand, would probably do a check here to se if we are allowed(Empty?)
							instantiateItemInHand (currentIndex);
							hasPlaced = true;
							return doPlaceUpdate (); //Do placement update for the new object
						}
					}
				} else {
					//TODO: In some way show that the item can not be placed on this surface(Maybe show an red X?)
				}
				
			}
		}

		if (resetToHand) {
			if (prevSurface != null) {
				prevSurface.showSnapPoints (false);
				prevSurface = null;
			}
		}

		return resetToHand;
	}

	//Set and show the current item in hand
	//Return: The previous item
	public BuildObject setItemInHand (BuildObject item)
	{
		BuildObject oldItem = null;
		if (itemInHand != null)
			oldItem = removeItemInHand ();

		itemInHand = item;
		itemRotation = item.transform.localRotation;
		showItemInHand (true);
		ignoreCollidersInHand ();

		int count = itemInHand.getBuildPointCount ();
		if (count > 0) {
			if (count <= currentPointIndex)
				currentPointIndex = 0;
			itemInHand.setCurrentBuildPoint (itemInHand.getBuildPoint (currentPointIndex));
		}

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
		itemInHand.onSurface (false, false);
		restoreCollidersInHand ();
		itemInHand = null;

		return item;
	}

	public void showItemInHand (bool show)
	{
		if (itemInHand == null)
			return;

		if (show && !isShowingItem) {
			itemInHand.transform.parent = transform;
			itemInHand.transform.localPosition = new Vector3 (0.8699999f, 0.19f, 1.09f); //Magic numbers FTW. TODO: Fix.
			itemInHand.transform.localRotation = new Quaternion (0, 0, 0, 0); //TODO: Use rotation stored in Object?
			itemInHand.transform.localScale = itemInHand.transform.localScale * itemInHand.sizeInHand;
			itemInHand.onSurface (false, false);

			isShowingItem = true;
		} else if (!show && isShowingItem) {
			itemInHand.transform.localScale = itemInHand.transform.localScale / itemInHand.sizeInHand;

			isShowingItem = false;
		}
	}

	//Go through and store, and ignore all Collider(Put them into No Raycast layer) for the current item in hand
	public void ignoreCollidersInHand ()
	{
		if (itemInHand == null)
			return;

		Transform[] objects = GetComponentsInChildren<Transform> ();
		foreach (Transform obj in objects) {
			disabledColliders.Add (new ObjectLayer (obj.gameObject, obj.gameObject.layer));
			obj.gameObject.layer = layerSurfacesDisabled;
		}
	}

	//Restore any disabled colliders
	public void restoreCollidersInHand ()
	{
		if (itemInHand == null)
			return;

		foreach (ObjectLayer disabledCollider in disabledColliders) {
			if (disabledCollider.obj == null) //Can have been destroyed at this point
				continue;
			disabledCollider.obj.layer = disabledCollider.layer;
		}
		disabledColliders.Clear ();
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

		//Set the old rotation, so the rotation when on the surface isn't dependent on the global rotation from the Character
		itemInHand.transform.rotation = itemRotation; 

		//If snap is on, find the nearest Snap point is show it there
		Vector3 snapPoint;
		if (!Input.GetButton ("ToggleSnap"))
			surface.getSnapPoint (ray.point, out snapPoint);
		else
			snapPoint = ray.point;


		//Rotate the object so that the current Build Point is placed on the surface according to the normal
		//First we find out how much to rotate.
		Vector3 vectorPoint = currentPoint.transform.position - itemInHand.transform.position;
		//Vector3 vectorSurface = (surface.transform.position - itemInHand.transform.position); //Might use this one later if we want to rotate it towards a point on the surface
		Vector3 vectorSurface = -ray.normal;
		Quaternion rotDelta = Quaternion.FromToRotation (vectorPoint, vectorSurface);

		//Then we apply the rotation
		itemInHand.transform.rotation = rotDelta * itemInHand.transform.rotation;


		//Move the object so the Build Point is placed at the Ray point
		//We multiply the point vector by 1.01f to leave a gap enough for them to not collide
		//TODO: Find a better fix(Allow a certain % of collision?)
		Vector3 pointDelta = currentPoint.transform.position - itemInHand.transform.position;
		Vector3 newPosition = snapPoint - (pointDelta * 1.01f);
		itemInHand.transform.position = newPosition;


		//Allow object to shade as it wish when on surface
		itemInHand.onSurface (true, itemInHand.canPlace (surface, itemInHand.getCurrentBuildPoint ().transform.position, itemInHand.transform.rotation));
	}
}
