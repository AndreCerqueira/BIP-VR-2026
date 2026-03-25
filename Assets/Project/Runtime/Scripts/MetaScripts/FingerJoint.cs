using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class FingerJoint : MonoBehaviour
{
	[SerializeField]
	Transform animationTarget;

	[SerializeField]
	Rigidbody connectedBody;

	[SerializeField]
	bool followPosition = false;

	[SerializeField]
	bool configureOnAwake = true;

	[SerializeField]
	bool useAccelerationDrive = true;

	[SerializeField]
	float positionSpring = 30000f;

	[SerializeField]
	float positionDamper = 2500f;

	[SerializeField]
	float rotationSpring = 80000f;

	[SerializeField]
	float rotationDamper = 4000f;

	[SerializeField]
	float maxForce = 1000000000f;

	[SerializeField]
	float lowTwistLimit = -20f;

	[SerializeField]
	float highTwistLimit = 20f;

	[SerializeField]
	float swingYLimit = 25f;

	[SerializeField]
	float swingZLimit = 25f;

	private ConfigurableJoint joint;
	private Vector3 startLocalPosition;
	private Quaternion startLocalRotation;

	private void Awake()
	{
		joint = GetComponent<ConfigurableJoint>();

		startLocalPosition = transform.localPosition;
		startLocalRotation = transform.localRotation;

		if (configureOnAwake)
		{
			ConfigureJoint();
		}
		else
		{
			ApplyDrives();
		}
	}

	private void OnValidate()
	{
		if (joint == null)
			joint = GetComponent<ConfigurableJoint>();

		if (joint != null)
			ApplyDrives();
	}

	private void ConfigureJoint()
	{
		if (connectedBody != null)
			joint.connectedBody = connectedBody;

		joint.autoConfigureConnectedAnchor = false;
		joint.configuredInWorldSpace = false;
		joint.anchor = Vector3.zero;
		joint.connectedAnchor = Vector3.zero;

		joint.xMotion = ConfigurableJointMotion.Locked;
		joint.yMotion = ConfigurableJointMotion.Locked;
		joint.zMotion = ConfigurableJointMotion.Locked;

		joint.angularXMotion = ConfigurableJointMotion.Limited;
		joint.angularYMotion = ConfigurableJointMotion.Limited;
		joint.angularZMotion = ConfigurableJointMotion.Limited;

		SoftJointLimit lowX = joint.lowAngularXLimit;
		lowX.limit = lowTwistLimit;
		joint.lowAngularXLimit = lowX;

		SoftJointLimit highX = joint.highAngularXLimit;
		highX.limit = highTwistLimit;
		joint.highAngularXLimit = highX;

		SoftJointLimit yLimit = joint.angularYLimit;
		yLimit.limit = swingYLimit;
		joint.angularYLimit = yLimit;

		SoftJointLimit zLimit = joint.angularZLimit;
		zLimit.limit = swingZLimit;
		joint.angularZLimit = zLimit;

		joint.projectionMode = JointProjectionMode.PositionAndRotation;
		joint.projectionDistance = 0.005f;
		joint.projectionAngle = 2f;

		ApplyDrives();
	}

	private void ApplyDrives()
	{
		JointDrive disabledLinear = new JointDrive
		{
			positionSpring = 0f,
			positionDamper = 0f,
			maximumForce = 0f,
			useAcceleration = useAccelerationDrive
		};

		JointDrive angularDrive = new JointDrive
		{
			positionSpring = Mathf.Max(0f, rotationSpring),
			positionDamper = Mathf.Max(0f, rotationDamper),
			maximumForce = Mathf.Max(0f, maxForce),
			useAcceleration = useAccelerationDrive
		};

		joint.xDrive = disabledLinear;
		joint.yDrive = disabledLinear;
		joint.zDrive = disabledLinear;

		joint.rotationDriveMode = RotationDriveMode.Slerp;
		joint.slerpDrive = angularDrive;
		joint.angularXDrive = angularDrive;
		joint.angularYZDrive = angularDrive;
	}

	private void FixedUpdate()
	{
		if (animationTarget == null)
			return;

		joint.targetPosition = Vector3.zero;
		SetTargetRotationLocal(joint, animationTarget.localRotation, startLocalRotation);
	}

	private static void SetTargetRotationLocal(ConfigurableJoint configurableJoint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
	{
		Quaternion jointSpace = GetJointSpaceRotation(configurableJoint);
		Quaternion result = Quaternion.Inverse(jointSpace) * (Quaternion.Inverse(targetLocalRotation) * startLocalRotation) * jointSpace;
		configurableJoint.targetRotation = result;
	}

	private static Quaternion GetJointSpaceRotation(ConfigurableJoint configurableJoint)
	{
		Vector3 right = configurableJoint.axis.normalized;
		Vector3 forward = Vector3.Cross(configurableJoint.axis, configurableJoint.secondaryAxis).normalized;
		Vector3 up = Vector3.Cross(forward, right).normalized;
		return Quaternion.LookRotation(forward, up);
	}
}
