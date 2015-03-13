using UnityEngine;
using System.Collections;

public class ItemPlacer : MonoBehaviour
{
	public BuildObject[] inventory = new BuildObject[5];

	private BuildObject itemInHand;
	private bool isShowingItem = false;

	public float placeDistance = 5f;
	
	void Start ()
	{
		//TEST: Get the first item from inventory for now
		//TODO: Allow selecting item using numbers/scroll
		if (inventory [0] != null) {
			BuildObject instance = Instantiate (inventory [0]);
			setItemInHand (instance);
		}
	}

	void Update ()
	{
		if (itemInHand == null)
			return;

		Camera cam = GetComponentInChildren<Camera> ();
		Ray ray = cam.ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0));
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, placeDistance)) {
			BuildSurface surface = hit.collider.GetComponent<BuildSurface> ();
			if (surface != null) {

				if (surface.canPlace (itemInHand, hit.point, transform.position, transform.rotation)) {
					Debug.DrawLine (transform.position, hit.point, Color.black, 0);
					Debug.DrawRay (hit.point, hit.normal, Color.black, 0);
				} else {
					//When not possible to place, show the item in our hand

				}

			}
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

		return oldItem;
	}

	//Remove the current item form the hand
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
			itemInHand.transform.localPosition = new Vector3 (0.8699999f, 0.19f, 1.09f); //Magic numbers FTW. TODO: Fix
			itemInHand.transform.localScale = itemInHand.transform.localScale * itemInHand.sizeInHand;
			isShowingItem = true;
		} else if (!show && isShowingItem) {
			itemInHand.transform.parent = null;
			itemInHand.transform.localScale = itemInHand.transform.localScale / itemInHand.sizeInHand;
			isShowingItem = false;
		}
	}


	//Show the item as placed on the surface
	public void showItemOnSurface ()
	{
		if (itemInHand == null)
			return;
	}
}
