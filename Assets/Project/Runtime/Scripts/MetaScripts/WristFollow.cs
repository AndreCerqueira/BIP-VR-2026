using UnityEngine;

public class WristFollow : MonoBehaviour
{
    [SerializeField]
	Transform trackingWrist;

	Rigidbody rb;
	void Start()
    {        
		if(TryGetComponent<Rigidbody>(out Rigidbody rigi))
		{
			rb = rigi;
			rb.isKinematic = true;
			rb.useGravity = false;
		}


	}

	private void FixedUpdate()
	{
		if(rb == null) return;
		rb.MovePosition(trackingWrist.position);
		rb.MoveRotation(trackingWrist.rotation);
	}
}
