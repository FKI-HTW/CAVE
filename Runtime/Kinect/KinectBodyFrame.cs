using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using HTW.CAVE.Kinect.Utils;

namespace HTW.CAVE.Kinect
{
	/// <summary>
	/// Holds the tracking data of a <see cref="Windows.Kinect.Body"/> and
	/// the associated <see cref="Microsoft.Kinect.Face"/>.
	/// The transformed and processed joint data can be accessed
	/// via the <see cref="KinectBodyFrame.joints"/> array or with the
	/// index accessor.
	/// </summary>
	public class KinectBodyFrame
	{
		public KinectJoint this[JointType jointType] => joints[(int)jointType];

		/// <summary>
		/// Provides a unique identification number of the body.
		/// </summary>
		public ulong trackingId;
		
		/// <summary>
		/// The body leaning direction and strength.
		/// </summary>
		public Vector2 lean;
		
		/// <summary>
		/// The face rotation of a body measured by
		/// the sensor to a maximum of 60 degrees.
		/// </summary>
		public Quaternion faceRotation;

		/// <summary>
		/// The native body data.
		/// </summary>
		public Body body;

		/// <summary>
		/// The native face data.
		/// </summary>
		public FaceFrameResult face;

		/// <summary>
		/// The joint data relative to the Kinect coordinate system.
		/// </summary>
		public KinectJoint[] joints;

		/// <summary>
		/// Contains the raw joint positions without sensor rotation correction.
		/// </summary>
		public Dictionary<JointType, Windows.Kinect.Joint> rawJoints;

		/// <summary>
		/// Contains the raw joint orientations without sensor rotation correction.
		/// </summary>
		public Dictionary<JointType, JointOrientation> rawJointOrientations;

		public KinectBodyFrame()
		{
			trackingId = 0;
			lean = Vector2.zero;
			body = null;
			face = null;
			joints = new KinectJoint[KinectHelper.jointTypeCount];
			rawJoints = new Dictionary<JointType, Windows.Kinect.Joint>(KinectHelper.jointTypeCount);
			rawJointOrientations = new Dictionary<JointType, JointOrientation>(KinectHelper.jointTypeCount);
		}

		/// <summary>
		/// Refreshes preallocated buffers for frame and joint data.
		/// The goal is to avoid per frame allocations in the <see cref="Windows.Kinect.Body.Joints"/>
		/// and <see cref="Windows.Kinect.Body.JointOrientations"/> properties.
		/// </summary>
		public void RefreshFrameData(Body body, FaceFrameResult face, UnityEngine.Vector4 floorClipPlane)
		{
			this.body = body;
			this.face = face;
			this.trackingId = body.GetTrackingIdFast();
			this.lean = body.GetLeanDirection();
			this.faceRotation = face == null ? Quaternion.identity : KinectHelper.FaceRotationToRealSpace(face.FaceRotationQuaternion);
			body.RefreshJointsFast(rawJoints);
			body.RefreshJointOrientationsFast(rawJointOrientations);

			KinectJoint.RefreshJointData(joints, floorClipPlane, rawJoints, rawJointOrientations);
		}
	}

	/// <summary>
	/// Contains the tracking data of a specific <see cref="Windows.Kinect.JointType"/>.
	/// </summary>
	public readonly struct KinectJoint
	{
		public readonly Vector3 position;
		
		public readonly Quaternion rotation;

		public readonly TrackingState trackingState;

		public KinectJoint(Vector3 position, Quaternion rotation, TrackingState trackingState)
		{
			this.position = position;
			this.rotation = rotation;
			this.trackingState = trackingState;
		}

		public static void RefreshJointData(KinectJoint[] buffer, UnityEngine.Vector4 floorClipPlane,
			Dictionary<JointType, Windows.Kinect.Joint> joints, Dictionary<JointType, JointOrientation> jointOrientations)
		{
			var correction = KinectHelper.CalculateFloorRotationCorrection(floorClipPlane);
			var index = 0;

			// Trick: Because SpineShoulder is not the successor of SpineMid in the enum,
			// the loop does the first iteration for SpineShoulder and restarts at index = 0 = SpineBase.
			for (int i = (int)JointType.SpineShoulder; i < buffer.Length; i = index++)
			{
				var jointType = (JointType)i;
				var joint = joints[jointType];
				var jointOrientation = jointOrientations[jointType];

				var position = correction * KinectHelper.CameraSpacePointToRealSpace(joint.Position, floorClipPlane);
				var rotation = correction * KinectHelper.OrientationToRealSpace(jointOrientation.Orientation);

				if(rotation.IsZero())
				{
					var parent = KinectHelper.parentJointTypes[i];
					rotation = KinectHelper.InferrRotationFromParentPosition(position, buffer[(int)parent].position);
				}
				
				buffer[i] = new KinectJoint(position, rotation, joint.TrackingState);
			}
			
			// This is a fix for the head rotation. 
			// Normally the rotation should be inferred from the parent spine
			// like other joints but for some reason this does not work correctly. 
			var head = buffer[(int)JointType.Head];
			var neck = buffer[(int)JointType.Neck];
			var fixedRotation = Quaternion.LookRotation(neck.rotation * Vector3.forward, head.position - neck.position);
			
			buffer[(int)JointType.Head] = new KinectJoint(head.position, fixedRotation, head.trackingState);
		}
		
		/// <summary>
		/// Transforms the local joints from the given coordinate system to
		/// world space.
		/// </summary>
		/// <param name="src">The source buffer containing the joints in local space.</param>
		/// <param name="dst">The destination buffer in which the transformed joints will be written.</param>
		/// <param name="origin">The origin of the coordinate system.</param>
		public static void TransformJointData(KinectJoint[] src, KinectJoint[] dst, Transform origin)
		{
			for (int i = 0; i < src.Length; ++i)
			{
				var position = origin.TransformPoint(src[i].position);
				var rotation = origin.rotation * src[i].rotation;
				dst[i] = new KinectJoint(position, rotation, src[i].trackingState);
			}
		}	
	}
}
