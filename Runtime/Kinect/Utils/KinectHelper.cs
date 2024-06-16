using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Kinect;
using UnityEngine;

namespace HTW.CAVE.Kinect.Utils
{
	/// <summary>
	/// Contains helper functions for processing Kinect v2
	/// coordinate mappings and joint transforms.
	/// </summary>
	public static class KinectHelper
	{
		/// <summary>
		/// Returns the parent joint by using the numeric value of
		/// the <see cref="Windows.Kinect.JointType"/> as the index.
		/// </summary>
		public static readonly JointType[] parentJointTypes = {
			JointType.SpineBase, JointType.SpineBase, JointType.SpineShoulder, JointType.Neck, // Spine to head.
			JointType.SpineShoulder, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, // Shoulder to left wrist.
			JointType.SpineShoulder, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, // Shoulder to right wrist.
			JointType.SpineBase, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, // Spine to left foot.
			JointType.SpineBase, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, // Spine to right foot.
			JointType.SpineMid, // Only shoulder.
			JointType.HandLeft, JointType.HandLeft, // Left hand tip and thumb. 
			JointType.HandRight, JointType.HandRight, // Left hand tip and thumb. 
		};

		/// <summary>
		/// All available <see cref="Windows.Kinect.JointType"/> values.
		/// </summary>
		public static readonly JointType[] allJointTypes = Enum.GetValues(typeof(JointType)).Cast<JointType>().ToArray();

		/// <summary>
		/// Count of <see cref="Windows.Kinect.JointType"/> values.
		/// </summary>
		public static readonly int jointTypeCount = allJointTypes.Length;

		/// <summary>
		/// T-pose positions for <see cref="Windows.Kinect.JointType"/> values.
		/// </summary>
		public static readonly Vector3[] tPose = {
			new Vector3(0f, 1.12f, 0f), new Vector3(0f, 1.3f, 0f), // SpineBase, SpineMid
			new Vector3(0f, 1.6f, 0f), new Vector3(0f, 1.7f, 0f), // Neck, Head
			new Vector3(0.165f, 1.45f, 0f), new Vector3(0.25f, 1.23f, 0f), // ShoulderLeft, ElbowLeft
			new Vector3(0.35f, 1f, 0f), new Vector3(0.38f, 0.95f, 0f), // WristLeft, HandLeft
			new Vector3(-0.165f, 1.45f, 0f), new Vector3(-0.25f, 1.23f, 0f), // ShoulderRight, ElbowRight
			new Vector3(-0.35f, 1f, 0f), new Vector3(-0.38f, 0.95f, 0f), // WristRight, HandRight ...
			new Vector3(0.07f, 0.95f, 0f), new Vector3(0.1f, 0.6f, 0f),
			new Vector3(0.14f, 0f, 0f), new Vector3(0.24f, 0f, -0.15f),
			new Vector3(-0.07f, 0.95f, 0f), new Vector3(-0.1f, 0.6f, 0f),
			new Vector3(-0.14f, 0f, 0f), new Vector3(-0.24f, 0f, -0.15f),
			new Vector3(0f, 1.5f, 0f),
			new Vector3(0.44f, 0.85f, 0f), new Vector3(0.374f, 0.916f, -0.06f),
			new Vector3(-0.44f, 0.85f, 0f), new Vector3(-0.374f, 0.916f, -0.06f)
		};

		/// <summary>
		/// The field of view of the Kinect sensor.
		/// </summary>
		public static readonly Vector2 fieldOfView = new Vector2(70.6f, 60f);

		/// <summary>
		/// The minimum and maximum tracking range of the Kinect sensor.
		/// </summary>
		public static readonly Vector2 clippingPlane = new Vector2(0.5f, 4.5f);

		/// <summary>
		/// Aspect ratio of the FOV.
		/// </summary>
		public static float aspectRatio => fieldOfView.x / fieldOfView.y;
		
		/// <summary>
		/// The frame time per image capture of the sensor.
		/// </summary>
		public static float frameTime = 1f / 30f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 SensorFloorPlaneYOffset(UnityEngine.Vector4 floorClipPlane) =>
			Vector3.up * floorClipPlane.w;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion CalculateFloorRotationCorrection(UnityEngine.Vector4 floorClipPlane)
		{
			if (Mathf.Approximately(floorClipPlane.sqrMagnitude, 0f))
				return Quaternion.identity;

			Vector3 up = floorClipPlane;
			Vector3 right = Vector3.Cross(up, Vector3.forward);
			Vector3 forward = Vector3.Cross(right, up);

			return Quaternion.LookRotation(new Vector3(forward.x, -forward.y, forward.z), new Vector3(up.x, up.y, -up.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion InferrRotationFromParentPosition(Vector3 position, Vector3 parentPosition)
		{
			Vector3 direction = position - parentPosition;
			Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
			Vector3 normal = Vector3.Cross(perpendicular, direction);

			// In the reference implementation on github.com/kinect/samples the
			// normal is the forward and the direction the upwards vector but
			// this results in hand tip and thumb are not pointing along the forward axis:
			// Quaternion.LookRotation(normal, direction);

			return Quaternion.LookRotation(direction + 0.0001f * Vector3.forward, normal + 0.0001f * Vector3.up);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DistanceFromCameraSpacePoint(CameraSpacePoint point, UnityEngine.Vector4 floorClipPlane) =>
			(floorClipPlane.x * point.X + floorClipPlane.y * point.Y + floorClipPlane.z * point.Z + floorClipPlane.w) /
			(floorClipPlane.x * floorClipPlane.x + floorClipPlane.y * floorClipPlane.y + floorClipPlane.z * floorClipPlane.z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Vector3 CameraSpacePointToRealSpace(CameraSpacePoint point, UnityEngine.Vector4 floorClipPlane) =>
			new Vector3(-point.X, point.Y + floorClipPlane.w, point.Z); // Mirror on the x axis.

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion OrientationToRealSpace(Windows.Kinect.Vector4 orientation) =>
			new Quaternion(orientation.X, -orientation.Y, -orientation.Z, orientation.W); // Mirror on the x axis.
	
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Quaternion FaceRotationToRealSpace(Windows.Kinect.Vector4 rotation) =>
			new Quaternion(0f, 1f, 0f, 0f) * new Quaternion(-rotation.X, -rotation.Y, rotation.Z, rotation.W);
	
		public static bool InJointTypeHierachy(JointType parent, JointType child)
		{
			while (child != parent)
			{
				var next = parentJointTypes[(int)child];

				if (next == child)
					return false;

				child = next;
			}

			return child == parent;
		}
	}
}
