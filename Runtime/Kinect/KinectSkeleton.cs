using System;
using UnityEngine;
using Windows.Kinect;
using HTW.CAVE.Kinect.Utils;

namespace HTW.CAVE.Kinect
{
	/// <summary>
	/// Constructs and updates an <see cref="KinectActor"/> skeleton made up
	/// of individual hierarchical <see cref="KinectSkeletonJoint"/> components.
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(KinectActor))]
	public class KinectSkeleton : MonoBehaviour
	{
		public KinectSkeletonJoint this[JointType jointType] => GetJoint(jointType);

		/// <summary>
		/// The world bounds of the skeleton.
		/// Includes all currently tracked joint positions.
		/// </summary>
		public Bounds bounds => m_Bounds;

		/// <summary>
		/// Number of joints in the skeleton.
		/// </summary>
		public int jointCount => m_JointCount;

		private KinectActor m_Actor;

		private Bounds m_Bounds;

		private KinectSkeletonJoint[] m_JointTypeToJoint;

		private KinectSkeletonJoint[] m_Joints;

		private int m_JointCount;

		private bool m_SkipRebuild;

		public void Awake()
		{
			m_Actor = GetComponent<KinectActor>();
			m_JointTypeToJoint = new KinectSkeletonJoint[KinectHelper.jointTypeCount];
			m_Joints = new KinectSkeletonJoint[KinectHelper.jointTypeCount];
			m_SkipRebuild = false;

			RebuildJointHierarchy();
		}

		public void OnEnable()
		{
			m_Actor.onTrackingDataUpdated += OnTrackingDataUpdated;
		}

		public void OnDisable()
		{
			m_Actor.onTrackingDataUpdated -= OnTrackingDataUpdated;
		}

		public void OnTransformChildrenChanged()
		{
			RebuildJointHierarchy();
		}

		/// <summary>
		/// Rebuilds the hierarchy of tracked joints in the skeleton.
		/// Use this method if there are joints in the hierarchy that where
		/// manually (not using the skeleton methods) created or destroyed.
		/// </summary>
		public void RebuildJointHierarchy()
		{
			if (m_SkipRebuild)
				return;

			for (int i = 0; i < m_JointTypeToJoint.Length; ++i)
				m_JointTypeToJoint[i] = null;

			m_JointCount = 0;

			var joints = GetComponentsInChildren<KinectSkeletonJoint>();

			if (joints.Length == 0)
				return;

			var count = joints.Length;
			var depths = new int[joints.Length];

			for (int i = 0; i < depths.Length; ++i)
				depths[i] = GetJointDepth(joints[i]);
			
			var depth = 1;

			// The algorithm works by scanning the reachable joints in
			// the hierarchy with increasing depth. The purpose is
			// that the parent joint should always be updated before the child joints
			// and therefore need to be at a lower index in the hierarchy.
			while (count > 0)
			{
				for (int i = 0; i < count;)
				{
					// Check if the type is already in the hierarchy.
					if (m_JointTypeToJoint[(int)joints[i].jointType] != null)
					{
						--count;
						joints[i] = joints[count];
						depths[i] = depths[count];
						continue;
					}

					// Skip the joint if it is not reachable at the current depth.
					if (depths[i] > depth)
					{
						++i;
						continue;
					}

					AddJointToHierarchy(joints[i]);

					--count;
					joints[i] = joints[count];
					depths[i] = depths[count];
				}

				++depth;
			}
		}

		/// <summary>
		/// Returns the <see cref="KinectSkeletonJoint"/> component instance of
		/// the given <see cref="JointType"/>.
		/// </summary>
		/// <param name="jointType">The target joint.</param>
		/// <returns>The component instance or <c>null</c>.</returns>
		public KinectSkeletonJoint GetJoint(JointType jointType) =>
			m_JointTypeToJoint[(int)jointType];

		/// <summary>
		/// Returns an array of all currently tracked
		/// <see cref="KinectSkeletonJoint"/> components.
		/// </summary>
		/// <returns>A copy of the <see cref="KinectSkeletonJoint"/> component array.</returns>
		public KinectSkeletonJoint[] GetJoints()
		{
			var joints = new KinectSkeletonJoint[m_JointCount];
			Array.Copy(m_Joints, joints, m_JointCount);

			return joints;
		}

		/// <summary>
		/// Returns the parent <see cref="KinectSkeletonJoint"/> of a given
		/// <see cref="JointType"/> in the hierarchy.
		/// </summary>
		/// <param name="jointType">The parent joint.</param>
		/// <returns>The component instance or <c>null</c>.</returns>
		public KinectSkeletonJoint GetParentJoint(JointType jointType)
		{
			KinectSkeletonJoint joint = null;

			do
			{
				var nextType = KinectHelper.parentJointTypes[(int)jointType];

				// Avoid endless loop on SpineBase.
				if (nextType == jointType)
					break;

				jointType = nextType;
				joint = GetJoint(jointType);
			} while(joint == null);

			return joint;
		}

		/// <summary>
		/// Gets or creates a joint in the skeleton hierarchy.
		/// </summary>
		/// <param name="jointType">The target joint.</param>
		/// <returns>The created or found component instance.</returns>
		public KinectSkeletonJoint GetOrCreateJoint(JointType jointType)
		{
			var joint = GetJoint(jointType);

			if(joint != null)
				return joint;

			var parent = GetParentJoint(jointType);
			joint = KinectSkeletonJoint.Create(jointType);
			joint.transform.position = transform.position + KinectHelper.tPose[(int)jointType];

			// This is required because changing the parent transform fires
			// the OnTransformChildrenChanged directly. Since rebuilding the
			// hierarchy is useless here, it will be skipped.
			SetJointParentWithoutRebuild(joint, parent == null ? transform : parent.transform);

			AddJointToHierarchy(joint);
			FixJointHierarchy(joint);

			return joint;
		}

		/// <summary>
		/// Creates <see cref="KinectSkeletonJoint"/> instances for all child joints in
		/// the hierarchy of the given root joint.
		/// </summary>
		/// <param name="rootJoint">The root joint.</param>
		/// <returns>The created or found component instance or the root joint.</returns>
		public KinectSkeletonJoint CreateJointTree(JointType rootJoint)
		{
			var joint = GetOrCreateJoint(rootJoint);

			foreach (var jointType in KinectHelper.allJointTypes)
			{
				var parentType = KinectHelper.parentJointTypes[(int)jointType];

				if (parentType != jointType && parentType == rootJoint)
					CreateJointTree(jointType);
			}

			return joint;
		}

		public void CreateJoints(JointType[] jointTypes)
		{
			Array.Sort(jointTypes);

			foreach(var jointType in jointTypes)
				GetOrCreateJoint(jointType);
		}

		public int GetJointDepth(KinectSkeletonJoint joint)
		{
			var parent = joint.transform;
			var depth = 0;

			while (transform != parent && parent != null)
			{
				++depth;
				parent = parent.parent;
			}

			return parent != null ? depth : -1;
		}

		internal void OnTrackingDataUpdated()
		{
			m_Bounds = m_Actor.bounds;

			// The root of all joints is the coordinate origin
			// and therefore the parent of the skeleton.
			var root = transform.parent;

			for (int i = 0; i < m_JointCount; ++i)
				m_Joints[i].UpdateTrackingData(root, m_Actor.bodyFrame, ref m_Bounds);
		}

		private void AddJointToHierarchy(KinectSkeletonJoint joint)
		{
			var depth = GetJointDepth(joint);

			// Fast path if the joint is added to the hierarchy of
			// the deepest child.
			if (m_JointCount == 0 || GetJointDepth(m_Joints[m_JointCount - 1]) <= depth)
			{
				InsertJointAt(joint, m_JointCount);
				return;
			}

			int index = 0;
			while (GetJointDepth(m_Joints[index]) < depth)
				++index;

			Array.Copy(m_Joints, index, m_Joints, index + 1, m_JointCount - index);

			InsertJointAt(joint, index);
		}

		private void InsertJointAt(KinectSkeletonJoint joint, int index)
		{
			m_Joints[index] = joint;
			m_JointTypeToJoint[(int)joint.jointType] = joint;
			++m_JointCount;
		}

		private void SetJointParentWithoutRebuild(KinectSkeletonJoint joint, Transform parent)
		{
			m_SkipRebuild = true;
			joint.transform.parent = parent;
			m_SkipRebuild = false;
		}

		private void FixJointHierarchy(KinectSkeletonJoint joint)
		{
			// I'm not 100% sure if this is right,
			// but at the moment it works fine.
			for (int i = 0; i < m_JointCount; ++i)
			{
				if (m_Joints[i].transform.parent == joint.transform.parent &&
					KinectHelper.InJointTypeHierachy(joint.jointType, m_Joints[i].jointType))
				{
					SetJointParentWithoutRebuild(m_Joints[i], joint.transform);
				}
			}
		}
	}
}