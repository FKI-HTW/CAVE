using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Windows.Kinect;
using HTW.CAVE.Kinect.Utils;

namespace HTW.CAVE.Kinect
{
	[CustomEditor(typeof(KinectSkeletonJoint))]
	public class KinectSkeletonJointEditor : Editor
	{
		private KinectSkeletonJoint m_Me;

		public void OnEnable()
		{
			m_Me = (KinectSkeletonJoint)target;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			m_Me.jointType = (JointType)EditorGUILayout.EnumPopup("Joint Type", m_Me.jointType);
			m_Me.applyFilter = EditorGUILayout.Toggle("Filter", m_Me.applyFilter);
			
			EditorGUI.EndChangeCheck();
		}

		[DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy)]
		public static void DrawJointGizmos(KinectSkeletonJoint joint, GizmoType type)
		{
			Gizmos.color = KinectEditorUtils.darkGreen;
			Gizmos.DrawWireSphere(joint.transform.position, 0.02f);
		}
	}
}