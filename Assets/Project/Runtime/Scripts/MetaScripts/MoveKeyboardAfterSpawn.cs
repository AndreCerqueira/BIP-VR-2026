using Meta.XR.MRUtilityKit;
using UnityEngine;

public class MoveKeyboardAfterSpawn : MonoBehaviour
{
	[SerializeField]
	Transform moveToPos;

	[SerializeField]
	Transform referencePoint;

	[SerializeField]
	float delay = 0.5f;

	[SerializeField]
	float surfaceOffset = 0.02f;

	[SerializeField]
	float pianoSizeOffset = 0.1f;

	[SerializeField]
	bool currentRoomOnly = true;

	[SerializeField]
	bool alignRotationToTableEdge = true;

	[SerializeField]
	float keyboardYawOffset = 0f;

	[SerializeField]
	int mrukInitRetries = 20;

	[SerializeField]
	float mrukRetryDelay = 0.25f;

	[SerializeField]
	int placementRetries = 12;

	[SerializeField]
	float placementRetryDelay = 0.35f;

	[SerializeField]
	MRUKAnchor.SceneLabels allowedSurfaceLabels = MRUKAnchor.SceneLabels.TABLE | MRUKAnchor.SceneLabels.OTHER | MRUKAnchor.SceneLabels.STORAGE;

	[SerializeField]
	float edgeInsetTowardTableCenter = 0.08f;

	[SerializeField]
	[Range(0f, 1f)]
	float edgeCentering = 0.75f;

	int initRetryCount;
	int placementRetryCount;

	private void Start()
	{
		TryScheduleMove();
	}

	private void TryScheduleMove()
	{
		if (MRUK.Instance == null)
		{
			if (initRetryCount < mrukInitRetries)
			{
				initRetryCount++;
				Invoke(nameof(TryScheduleMove), mrukRetryDelay);
				return;
			}

			Debug.LogWarning("MoveKeyboardAfterSpawn: MRUK.Instance stayed null. Using fallback.");
			Invoke(nameof(MoveToFallback), delay);
			return;
		}

		if (MRUK.Instance.IsInitialized)
		{
			Invoke(nameof(MoveKeyboard), delay);
		}
		else
		{
			MRUK.Instance.RegisterSceneLoadedCallback(() =>
			{
				Invoke(nameof(MoveKeyboard), delay);
			});
		}
	}

	private void MoveKeyboard()
	{
		bool moved = TryMoveToClosestTableTop();
		if (moved) return;

		if (placementRetryCount < placementRetries)
		{
			placementRetryCount++;
			Invoke(nameof(MoveKeyboard), placementRetryDelay);
			return;
		}

		Debug.LogWarning("MoveKeyboardAfterSpawn: No valid surface found after retries. Using fallback.");
		MoveToFallback();
	}

	private bool TryMoveToClosestTableTop()
	{
		if (MRUK.Instance == null) return false;

		Transform refTransform = GetReferenceTransform();
		if (refTransform == null) return false;

		MRUKAnchor closestAnchor = FindClosestAnchor(refTransform.position, out Vector3 bestPoint, out Vector3 bestNormal, out Vector3 bestForward);
		if (closestAnchor == null) return false;

		Vector3 adjustedPoint = MovePointTowardAnchorCenterOnSurface(
			closestAnchor,
			bestPoint,
			bestNormal,
			edgeInsetTowardTableCenter
		);

		transform.position = adjustedPoint + bestNormal * surfaceOffset;
		transform.position -= new Vector3(0f, pianoSizeOffset, 0f);

		if (alignRotationToTableEdge)
		{
			Quaternion targetRotation = Quaternion.LookRotation(bestForward, bestNormal);
			transform.rotation = targetRotation * Quaternion.Euler(0f, keyboardYawOffset, 0f);

			EnsureFacingReference(refTransform.position, bestNormal);
		}

		return true;
	}

	private MRUKAnchor FindClosestAnchor(
		Vector3 referencePosition,
		out Vector3 bestPoint,
		out Vector3 bestNormal,
		out Vector3 bestForward)
	{
		bestPoint = Vector3.zero;
		bestNormal = Vector3.up;
		bestForward = Vector3.forward;

		MRUKAnchor bestAnchor = null;
		float bestDistance = float.MaxValue;

		if (MRUK.Instance == null || MRUK.Instance.Rooms == null)
			return null;

		for (int r = 0; r < MRUK.Instance.Rooms.Count; r++)
		{
			MRUKRoom room = MRUK.Instance.Rooms[r];
			if (room == null) continue;

			if (currentRoomOnly)
			{
				MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
				if (currentRoom != null && room != currentRoom) continue;
			}

			for (int i = 0; i < room.Anchors.Count; i++)
			{
				MRUKAnchor anchor = room.Anchors[i];
				if (anchor == null) continue;

				bool allowed = (anchor.Label & allowedSurfaceLabels) != 0;
				if (!allowed) continue;

				if (!TryGetPlacementOnAnchorEdge(anchor, referencePosition, edgeCentering, out Vector3 point, out Vector3 normal, out Vector3 forward))
					continue;

				float distance = Vector3.Distance(referencePosition, point);
				if (distance < bestDistance)
				{
					bestDistance = distance;
					bestAnchor = anchor;
					bestPoint = point;
					bestNormal = normal.normalized;
					bestForward = forward.normalized;
				}
			}
		}

		return bestAnchor;
	}

	private static bool TryGetPlacementOnAnchorEdge(
		MRUKAnchor anchor,
		Vector3 referencePosition,
		float edgeCentering,
		out Vector3 placementPoint,
		out Vector3 surfaceNormal,
		out Vector3 forwardDirection)
	{
		edgeCentering = Mathf.Clamp01(edgeCentering);

		if (anchor.VolumeBounds.HasValue)
		{
			Bounds bounds = anchor.VolumeBounds.Value;
			Vector3 localRef = anchor.transform.InverseTransformPoint(referencePosition);

			Vector3 topLocalNormal = GetTopLocalNormal(anchor.transform);
			int topAxis = GetDominantAxisIndex(topLocalNormal);

			int axisA;
			int axisB;
			GetOtherAxes(topAxis, out axisA, out axisB);

			Vector3 localPoint = localRef;
			localPoint.x = Mathf.Clamp(localPoint.x, bounds.min.x, bounds.max.x);
			localPoint.y = Mathf.Clamp(localPoint.y, bounds.min.y, bounds.max.y);
			localPoint.z = Mathf.Clamp(localPoint.z, bounds.min.z, bounds.max.z);

			if (topAxis == 0) localPoint.x = topLocalNormal.x > 0f ? bounds.max.x : bounds.min.x;
			if (topAxis == 1) localPoint.y = topLocalNormal.y > 0f ? bounds.max.y : bounds.min.y;
			if (topAxis == 2) localPoint.z = topLocalNormal.z > 0f ? bounds.max.z : bounds.min.z;

			// On that top face, choose nearest edge to player.
			float distAxisAMin = Mathf.Abs(localRef[axisA] - bounds.min[axisA]);
			float distAxisAMax = Mathf.Abs(bounds.max[axisA] - localRef[axisA]);
			float distAxisBMin = Mathf.Abs(localRef[axisB] - bounds.min[axisB]);
			float distAxisBMax = Mathf.Abs(bounds.max[axisB] - localRef[axisB]);

			float nearest = distAxisAMin;
			int edgeCase = 0; // 0:Amin, 1:Amax, 2:Bmin, 3:Bmax

			if (distAxisAMax < nearest) { nearest = distAxisAMax; edgeCase = 1; }
			if (distAxisBMin < nearest) { nearest = distAxisBMin; edgeCase = 2; }
			if (distAxisBMax < nearest) { edgeCase = 3; }

			int tangentAxis;
			if (edgeCase == 0 || edgeCase == 1)
			{
				localPoint[axisA] = edgeCase == 0 ? bounds.min[axisA] : bounds.max[axisA];
				localPoint[axisB] = Mathf.Lerp(localPoint[axisB], bounds.center[axisB], edgeCentering);
				tangentAxis = axisB;
			}
			else
			{
				localPoint[axisB] = edgeCase == 2 ? bounds.min[axisB] : bounds.max[axisB];
				localPoint[axisA] = Mathf.Lerp(localPoint[axisA], bounds.center[axisA], edgeCentering);
				tangentAxis = axisA;
			}

			placementPoint = anchor.transform.TransformPoint(localPoint);
			surfaceNormal = anchor.transform.TransformDirection(topLocalNormal).normalized;

			if (Vector3.Dot(surfaceNormal, Vector3.up) < 0.3f)
			{
				forwardDirection = Vector3.forward;
				return false;
			}

			Vector3 tangentLocal = Vector3.zero;
			tangentLocal[tangentAxis] = 1f;

			Vector3 tangentWorld = anchor.transform.TransformDirection(tangentLocal).normalized;
			tangentWorld = Vector3.ProjectOnPlane(tangentWorld, surfaceNormal).normalized;

			Vector3 toPlayer = Vector3.ProjectOnPlane(referencePosition - placementPoint, surfaceNormal);
			if (toPlayer.sqrMagnitude > 0.0001f && Vector3.Dot(tangentWorld, toPlayer.normalized) < 0f)
				tangentWorld = -tangentWorld;

			forwardDirection = tangentWorld.sqrMagnitude > 0.0001f ? tangentWorld : Vector3.forward;
			return true;
		}

		if (anchor.PlaneRect.HasValue)
		{
			Rect rect = anchor.PlaneRect.Value;
			Vector3 localRef = anchor.transform.InverseTransformPoint(referencePosition);

			float distMinX = Mathf.Abs(localRef.x - rect.xMin);
			float distMaxX = Mathf.Abs(rect.xMax - localRef.x);
			float distMinY = Mathf.Abs(localRef.y - rect.yMin);
			float distMaxY = Mathf.Abs(rect.yMax - localRef.y);

			float nearest = distMinX;
			int nearestEdge = 0; // 0:xMin,1:xMax,2:yMin,3:yMax

			if (distMaxX < nearest) { nearest = distMaxX; nearestEdge = 1; }
			if (distMinY < nearest) { nearest = distMinY; nearestEdge = 2; }
			if (distMaxY < nearest) { nearestEdge = 3; }

			Vector3 localPoint = Vector3.zero;
			Vector3 tangentLocal = Vector3.zero;

			switch (nearestEdge)
			{
				case 0:
					localPoint.x = rect.xMin;
					localPoint.y = Mathf.Lerp(Mathf.Clamp(localRef.y, rect.yMin, rect.yMax), rect.center.y, edgeCentering);
					tangentLocal = Vector3.up;
					break;
				case 1:
					localPoint.x = rect.xMax;
					localPoint.y = Mathf.Lerp(Mathf.Clamp(localRef.y, rect.yMin, rect.yMax), rect.center.y, edgeCentering);
					tangentLocal = Vector3.up;
					break;
				case 2:
					localPoint.x = Mathf.Lerp(Mathf.Clamp(localRef.x, rect.xMin, rect.xMax), rect.center.x, edgeCentering);
					localPoint.y = rect.yMin;
					tangentLocal = Vector3.right;
					break;
				default:
					localPoint.x = Mathf.Lerp(Mathf.Clamp(localRef.x, rect.xMin, rect.xMax), rect.center.x, edgeCentering);
					localPoint.y = rect.yMax;
					tangentLocal = Vector3.right;
					break;
			}

			localPoint.z = 0f;
			placementPoint = anchor.transform.TransformPoint(localPoint);

			Vector3 normalForward = anchor.transform.forward.normalized;
			Vector3 normalBackward = -normalForward;
			surfaceNormal = Vector3.Dot(normalForward, Vector3.up) >= Vector3.Dot(normalBackward, Vector3.up) ? normalForward : normalBackward;

			if (Vector3.Dot(surfaceNormal, Vector3.up) < 0.3f)
			{
				forwardDirection = Vector3.forward;
				return false;
			}

			Vector3 tangentWorld = anchor.transform.TransformDirection(tangentLocal).normalized;
			tangentWorld = Vector3.ProjectOnPlane(tangentWorld, surfaceNormal).normalized;

			Vector3 toPlayer = Vector3.ProjectOnPlane(referencePosition - placementPoint, surfaceNormal);
			if (toPlayer.sqrMagnitude > 0.0001f && Vector3.Dot(tangentWorld, toPlayer.normalized) < 0f)
				tangentWorld = -tangentWorld;

			forwardDirection = tangentWorld.sqrMagnitude > 0.0001f ? tangentWorld : Vector3.forward;
			return true;
		}

		placementPoint = Vector3.zero;
		surfaceNormal = Vector3.up;
		forwardDirection = Vector3.forward;
		return false;
	}

	private static Vector3 GetTopLocalNormal(Transform anchorTransform)
	{
		Vector3[] localNormals =
		{
			Vector3.right, Vector3.left,
			Vector3.up, Vector3.down,
			Vector3.forward, Vector3.back
		};

		float bestDot = float.MinValue;
		Vector3 best = Vector3.up;

		for (int i = 0; i < localNormals.Length; i++)
		{
			float dot = Vector3.Dot(anchorTransform.TransformDirection(localNormals[i]).normalized, Vector3.up);
			if (dot > bestDot)
			{
				bestDot = dot;
				best = localNormals[i];
			}
		}

		return best;
	}

	private static int GetDominantAxisIndex(Vector3 v)
	{
		float ax = Mathf.Abs(v.x);
		float ay = Mathf.Abs(v.y);
		float az = Mathf.Abs(v.z);

		if (ax > ay && ax > az) return 0;
		if (ay > az) return 1;
		return 2;
	}

	private static void GetOtherAxes(int axis, out int a, out int b)
	{
		if (axis == 0) { a = 1; b = 2; return; }
		if (axis == 1) { a = 0; b = 2; return; }
		a = 0; b = 1;
	}

	private Transform GetReferenceTransform()
	{
		if (referencePoint != null) return referencePoint;
		if (Camera.main != null) return Camera.main.transform;
		return transform;
	}

	private void MoveToFallback()
	{
		if (moveToPos == null) return;
		transform.position = moveToPos.position;
		transform.position -= new Vector3(0f, pianoSizeOffset, 0f);
		// transform.rotation = moveToPos.rotation;
	}

	private static Vector3 MovePointTowardAnchorCenterOnSurface(
		MRUKAnchor anchor,
		Vector3 edgePoint,
		Vector3 surfaceNormal,
		float insetDistance)
	{
		if (insetDistance <= 0f) return edgePoint;

		Vector3 center = GetAnchorCenterWorld(anchor);
		Vector3 towardCenterOnSurface = Vector3.ProjectOnPlane(center - edgePoint, surfaceNormal);

		float magnitude = towardCenterOnSurface.magnitude;
		if (magnitude <= 0.0001f) return edgePoint;

		float move = Mathf.Min(insetDistance, magnitude);
		return edgePoint + towardCenterOnSurface / magnitude * move;
	}

	private static Vector3 GetAnchorCenterWorld(MRUKAnchor anchor)
	{
		if (anchor.VolumeBounds.HasValue)
		{
			Bounds bounds = anchor.VolumeBounds.Value;
			return anchor.transform.TransformPoint(bounds.center);
		}

		if (anchor.PlaneRect.HasValue)
		{
			Rect rect = anchor.PlaneRect.Value;
			Vector3 localCenter = new Vector3(rect.center.x, rect.center.y, 0f);
			return anchor.transform.TransformPoint(localCenter);
		}

		return anchor.transform.position;
	}

	private void EnsureFacingReference(Vector3 referencePosition, Vector3 surfaceNormal)
	{
		Vector3 toReference = Vector3.ProjectOnPlane(referencePosition - transform.position, surfaceNormal);
		if (toReference.sqrMagnitude < 0.0001f) return;

		Vector3 currentForward = Vector3.ProjectOnPlane(transform.forward, surfaceNormal);
		if (currentForward.sqrMagnitude < 0.0001f) return;

		if (Vector3.Dot(currentForward.normalized, toReference.normalized) < 0f)
		{
			transform.rotation = Quaternion.AngleAxis(180f, surfaceNormal.normalized) * transform.rotation;
		}
	}
}
