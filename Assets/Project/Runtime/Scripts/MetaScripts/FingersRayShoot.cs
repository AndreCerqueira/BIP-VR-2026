using UnityEngine;
using System.Collections.Generic;
using Project.Runtime.Scripts.Piano;

/// <summary>
/// this scripts gets all fingers automatically and finds the closest key using overlap sphere. 
/// to hack 10 finger settup in unity.
/// </summary>
public class FingersRayShoot : MonoBehaviour
{
	[SerializeField]
	List<GameObject> fingerTips = new();

	[SerializeField]
	float overlapRadius = 0.012f;

	[SerializeField]
	float pressDistance = 0.002f;

	[SerializeField]
	float releaseDistance = 0.004f;

	[SerializeField]
	float underKeyReleaseDistance = 0.006f;

	[SerializeField]
	LayerMask keyLayerMask = ~0;

	private KeyView[] lastPressedKeys = new KeyView[10];
	private readonly Collider[] overlapResults = new Collider[32];

	private void Awake()
	{
		Invoke("LateStart", 0.1f);
	}

	private void LateStart()
	{
		GetFingers();
	}

	private void GetFingers()
	{
		fingerTips.Clear();

		FingerTipTag[] fingerTipTags = GetComponentsInChildren<FingerTipTag>();
		lastPressedKeys = new KeyView[fingerTipTags.Length];

		foreach (FingerTipTag fingerTipTag in fingerTipTags)
		{
			fingerTips.Add(fingerTipTag.gameObject);
		}
	}

	private void Update()
	{
		CheckPianoKeyPress();
	}

	private void CheckPianoKeyPress()
	{
		for (int i = 0; i < fingerTips.Count; i++)
		{
			GameObject fingerTip = fingerTips[i];
			Transform fingerTransform = fingerTip.transform;

			int hitCount = Physics.OverlapSphereNonAlloc(
				fingerTransform.position,
				overlapRadius,
				overlapResults,
				keyLayerMask,
				QueryTriggerInteraction.Ignore
			);

			KeyView nearestKey = null;
			float nearestDistance = float.MaxValue;

			for (int h = 0; h < hitCount; h++)
			{
				Collider col = overlapResults[h];
				if (col == null) continue;

				KeyView key = col.GetComponent<KeyView>();
				if (key == null) key = col.GetComponentInParent<KeyView>();
				if (key == null) continue;

				float distance = DistanceToCollider(col, fingerTransform.position);
				if (distance < nearestDistance)
				{
					nearestDistance = distance;
					nearestKey = key;
				}
			}

			KeyView currentPressed = lastPressedKeys[i];

			if (currentPressed != null)
			{
				bool isUnderCurrentKey = IsUnderKey(currentPressed.transform, fingerTransform.position);
				float activeReleaseDistance = isUnderCurrentKey ? underKeyReleaseDistance : releaseDistance;

				bool switchedTarget = nearestKey != null && nearestKey != currentPressed;
				bool tooFar = nearestKey == currentPressed && nearestDistance > activeReleaseDistance;
				bool noKeyNearby = nearestKey == null;

				if (switchedTarget || tooFar || noKeyNearby)
				{
					if (currentPressed._isPlaying)
						currentPressed.ReleaseKey();

					lastPressedKeys[i] = null;
					currentPressed = null;
				}
			}

			if (nearestKey != null && nearestDistance <= pressDistance)
			{
				if (!nearestKey._isPlaying)
					nearestKey.PressKey();

				lastPressedKeys[i] = nearestKey;
			}
		}
	}

	private static bool IsUnderKey(Transform keyTransform, Vector3 fingerPosition)
	{
		Vector3 localPosition = keyTransform.InverseTransformPoint(fingerPosition);
		return localPosition.y < 0f;
	}

	private static float DistanceToCollider(Collider col, Vector3 point)
	{
		Vector3 closest = col.ClosestPoint(point);
		return Vector3.Distance(point, closest);
	}
}
