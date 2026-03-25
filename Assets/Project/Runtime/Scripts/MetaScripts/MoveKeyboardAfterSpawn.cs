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



	private void Start()
	{
		if (MRUK.Instance == null)
		{
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
		if (!moved)
			MoveToFallback();
	}

	private bool TryMoveToClosestTableTop()
	{
		Transform refTransform = GetReferenceTransform();
		if (refTransform == null) return false;

		MRUKRoom room = currentRoomOnly ? MRUK.Instance.GetCurrentRoom() : GetClosestRoom(refTransform.position);
		if (room == null) return false;

		bool found = false;
		float bestDistance = float.MaxValue;
		Vector3 bestPoint = Vector3.zero;
		Vector3 bestNormal = Vector3.up;

		for (int i = 0; i < room.Anchors.Count; i++)
		{
			MRUKAnchor anchor = room.Anchors[i];
			if (anchor == null) continue;

			bool isTable = (anchor.Label & MRUKAnchor.SceneLabels.TABLE) != 0;
			if (!isTable) continue;

			float distance = anchor.GetClosestSurfacePosition(
				refTransform.position,
				out Vector3 point,
				out Vector3 normal,
				MRUKAnchor.ComponentType.All
			);

			// Keep only upward-facing surfaces (table top, not sides/bottom)
			if (Vector3.Dot(normal.normalized, Vector3.up) < 0.6f)
				continue;

			if (distance < bestDistance)
			{
				bestDistance = distance;
				bestPoint = point;
				bestNormal = normal.normalized;
				found = true;
			}
		}

		if (!found) return false;

		transform.position = bestPoint + bestNormal * surfaceOffset;
		transform.position -= new Vector3(0, pianoSizeOffset, 0); // Ensure the piano doesn't intersect with the table surface
		return true;
	}

	private Transform GetReferenceTransform()
	{
		if (referencePoint != null) return referencePoint;
		if (Camera.main != null) return Camera.main.transform;
		return transform;
	}

	private MRUKRoom GetClosestRoom(Vector3 worldPosition)
	{
		MRUKRoom bestRoom = null;
		float bestDistance = float.MaxValue;

		foreach (MRUKRoom room in MRUK.Instance.Rooms)
		{
			float distance = Vector3.Distance(room.transform.position, worldPosition);
			if (distance < bestDistance)
			{
				bestDistance = distance;
				bestRoom = room;
			}
		}

		return bestRoom;
	}

	private void MoveToFallback()
	{
		if (moveToPos == null) return;
		transform.position = moveToPos.position;
	}
}
