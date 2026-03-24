using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// this scripts gets all fingers automatically and shoots a ray from each finger to detect if it is touching something. 
/// to hack 10 finger settup in unity.
/// </summary>
public class FingersRayShoot : MonoBehaviour
{

	List<GameObject> fingerTips = new();
	private void Awake()
	{
		GetFingers();
	}

	private void GetFingers()
	{
		FingerTipTag[] fingerTipTags = GetComponentsInChildren<FingerTipTag>();
		foreach (FingerTipTag fingerTipTag in fingerTipTags)
		{
			fingerTips.Add(fingerTipTag.gameObject);
		}
	}

	private void FixedUpdate()
	{
		CheckPianoKeyPress();
	}

	private void CheckPianoKeyPress()
	{
		foreach (GameObject fingerTip in fingerTips)
		{
			Ray ray = new Ray(fingerTip.transform.position, -fingerTip.transform.up);
			if (Physics.Raycast(ray, out RaycastHit hit, 0.3f))
			{
				Debug.Log("Finger " + fingerTip.name + " is touching " + hit.collider.gameObject.name);
			}

			Debug.DrawRay(ray.origin, ray.direction * 0.3f, Color.red);
		}
	}
}
