using UnityEngine;
using System.Collections;

public class ItemPlacer : MonoBehaviour
{
	private Object itemInHand;
	public float placeDistance = 5f;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		Camera cam = GetComponentInChildren<Camera> ();
		Ray ray = cam.ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0));
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, placeDistance)) {
			Debug.DrawLine (transform.position, hit.point, Color.black, 0);
			Debug.DrawRay (hit.point, hit.normal, Color.black, 0);
		}
	}
}
