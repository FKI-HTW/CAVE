using System;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using HTW.CAVE.Kinect.Utils;

namespace HTW.CAVE.Kinect
{
	/// <summary>
	/// Represents a tracked person. Automatically updates
	/// <see cref="KinectTrackable"/> components in the child hierarchy
	/// with the provided tracking data from the <see cref="KinectActorTracker"/>.
	/// </summary>
	[AddComponentMenu("HTW/CAVE/Kinect/Kinect Actor")]
	public sealed class KinectActor : MonoBehaviour
	{
		public event Action onTrackingDataUpdated;
	
		/// <summary>
		/// The tracking id of the actor which corresponds to a
		/// tracking id of a body.
		/// </summary>
		public ulong trackingId => m_BodyFrame.trackingId;

		/// <summary>
		/// Provides the last updated body data.
		/// </summary>
		public KinectBodyFrame bodyFrame => m_BodyFrame;

		/// <summary>
		/// Gets the direction and strength a body is leaning, which is a number between -1 (leaning left or back)
		/// and 1 (leaning right or front).
		/// Leaning left and right corresponds to X movement and leaning back and forward corresponds to Y movement.
		/// </summary>
		public UnityEngine.Vector2 lean => m_BodyFrame.lean;
		
		/// <summary>
		/// Approximation of the persons height. This is the maximum detected distance
		/// between the head and the foot joints. Becomes more accurate the longer the person
		/// stands straight and the longer the person is tracked.
		/// </summary>
		public float height => m_Height;

		/// <summary>
		/// The world bounds of the actor.
		/// Includes head, spine and foot positions.
		/// </summary>
		public Bounds bounds => m_Bounds;
		

		/// <summary>
		/// The time at the creation of the component.
		/// </summary>
		public float createdAt => m_CreatedAt;
		
		/// <summary>
		/// The tracked joint positions and rotations in world space.
		/// </summary>
		public KinectJoint[] joints => m_Joints;
		
		internal long updatedAtFrame;
		
		private KinectBodyFrame m_BodyFrame;
		
		private KinectJoint[] m_Joints;

		private Bounds m_Bounds;

		private float m_CreatedAt;

		private float m_Height;

		public void Awake()
		{
			m_BodyFrame = new KinectBodyFrame();
			m_Joints = new KinectJoint[KinectHelper.jointTypeCount];
			m_CreatedAt = Time.time;
		}
		
		public KinectJoint GetJoint(JointType jointType) => m_Joints[(int)jointType];

		internal void OnUpdateTrackingData(KinectManager manager, Body body, FaceFrameResult face, long frame)
		{
			updatedAtFrame = frame;
			m_BodyFrame.RefreshFrameData(body, face, manager.floorClipPlane);
			
			KinectJoint.TransformJointData(m_BodyFrame.joints, m_Joints, manager.transform);

			RecalculatePositionAndBounds();

			onTrackingDataUpdated?.Invoke();
		}

		private void RecalculatePositionAndBounds()
		{		
			var joint = GetJoint(JointType.SpineBase);

			transform.position = joint.position;

			m_Bounds = new Bounds(joint.position, Vector3.zero);
		
			joint = GetJoint(JointType.Head);

			if (joint.trackingState != TrackingState.NotTracked)
				m_Bounds.Encapsulate(joint.position);
			
			joint = GetJoint(JointType.FootLeft);
		
			if (joint.trackingState != TrackingState.NotTracked)
				m_Bounds.Encapsulate(joint.position);

			joint = GetJoint(JointType.FootRight);

			if (joint.trackingState != TrackingState.NotTracked)
				m_Bounds.Encapsulate(joint.position);
			
			// Compensation because the head joint is in the middle of the head.
			m_Height = Mathf.Max(m_Height, m_Bounds.size.y + 0.1f);
		}
	}
}
