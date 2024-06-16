using UnityEngine;
using Windows.Kinect;
using HTW.CAVE.Kinect.Utils;

namespace HTW.CAVE.Kinect
{
	/// <summary>
	/// Represents a tracked <see cref="Windows.Kinect.JointType"/> in the
	/// <see cref="KinectSkeleton"/> hierachy.
	/// The <see cref="transform"/> position and rotation will be updated according
	/// to the tracked position and rotation of the <see cref="jointType"/>.
	/// </summary>
	[ExecuteInEditMode]
	public class KinectSkeletonJoint : MonoBehaviour
	{
		/// <summary>
		/// Defines the parameters that are used by the filter
		/// when <see cref="filterPositionAndRotation"/> is set.
		/// </summary>
		public static OneEuroParams filterParams = new OneEuroParams(1f);

		/// <summary>
		/// Gets whether or not the joint is actively tracked or if
		/// the tracking data is inferred.
		/// </summary>
		public TrackingState trackingState => m_TrackingState;

		/// <summary>
		/// Defines the type of the tracked joint.
		/// </summary>
		public JointType jointType
		{
			get => m_JointType;
			set
			{
				if (m_JointType != value)
				{
					m_JointType = value;
					RebuildSkeleton();
				}
			}
		}

		/// <summary>
		/// Defines if a filter will by applied on the tracking data when updating
		/// the <see cref="transform.position"/> and <see cref="transform.rotation"/>.
		/// </summary>
		public bool applyFilter;

		[SerializeField]
		private JointType m_JointType;

		private TrackingState m_TrackingState;

		private OneEuroFilter3 m_PositionFilter;

		private OneEuroFilter4 m_RotationFilter;

		public void OnTransformChildrenChanged()
		{
			RebuildSkeleton();
		}

		internal void UpdateTrackingData(Transform root, KinectBodyFrame bodyFrame, ref Bounds bounds)
		{
			var joint = bodyFrame[m_JointType];

			m_TrackingState = joint.trackingState;

			if (joint.trackingState == TrackingState.NotTracked)
			{
				transform.localPosition = Vector3.zero;
				return;
			}
			
			transform.position = root.TransformPoint(joint.position);
			transform.rotation = root.rotation * joint.rotation;

			if (applyFilter)
			{
				// By using the local position the translation or rotation of the
				// coordinate origin has no impact on the filter.
				transform.localPosition = m_PositionFilter.Filter(transform.localPosition, KinectHelper.frameTime, in filterParams);
				transform.localRotation = m_RotationFilter.Filter(transform.localRotation, KinectHelper.frameTime, in filterParams);
			}

			bounds.Encapsulate(transform.position);
		}

		private void RebuildSkeleton()
		{
			var skeleton = GetComponentInParent<KinectSkeleton>();

			if (skeleton != null)
				skeleton.RebuildJointHierarchy();
		}	

		internal static KinectSkeletonJoint Create(JointType jointType)
		{
			var gameObject = new GameObject(jointType.MakeHumanReadable());
			var joint = gameObject.AddComponent<KinectSkeletonJoint>();
			joint.m_JointType = jointType;

			return joint;
		}
	} 
}