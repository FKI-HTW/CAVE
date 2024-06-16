using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Windows.Kinect;
using HTW.CAVE.Kinect.Utils;

namespace HTW.CAVE.Kinect
{
	/// <summary>
	/// Need to be aligned with <see cref="KinectSkeletonEditor.Styles.bodyPart"/>.
	/// </summary>
	[Flags]
	internal enum KinectJointMask
	{
		None = 0,
		Torso = 1 << 0,
		Head = 1 << 1,
		LegLeft = 1 << 2,
		LegRight = 1 << 3,
		ArmLeft = 1 << 4,
		ArmRight = 1 << 5,
		HandLeft = 1 << 6,
		HandRight = 1 << 7,
		All = ~(-1 << 8)
	}

	[CustomEditor(typeof(KinectSkeleton))]
	public class KinectSkeletonEditor : Editor
	{
		/// <summary>
		/// Copied from AvatarMaskInspector.cs in the UnityCsReference.
		/// </summary>
		private static class Styles
		{
			public static GUIContent unityDude = EditorGUIUtility.IconContent("AvatarInspector/BodySIlhouette");
			
			public static GUIContent pickingTexture = EditorGUIUtility.IconContent("AvatarInspector/BodyPartPicker");

			public static GUIContent[] bodyPart =
			{
				EditorGUIUtility.IconContent("AvatarInspector/Torso"),

				EditorGUIUtility.IconContent("AvatarInspector/Head"),

				EditorGUIUtility.IconContent("AvatarInspector/LeftLeg"),
				EditorGUIUtility.IconContent("AvatarInspector/RightLeg"),

				EditorGUIUtility.IconContent("AvatarInspector/LeftArm"),
				EditorGUIUtility.IconContent("AvatarInspector/RightArm"),

				EditorGUIUtility.IconContent("AvatarInspector/LeftFingers"),
				EditorGUIUtility.IconContent("AvatarInspector/RightFingers")
			};

			public static Color[] maskBodyPartPicker =
			{
				new Color(  0 / 255.0f, 174 / 255.0f, 240 / 255.0f), // body
				new Color(171 / 255.0f, 160 / 255.0f,   0 / 255.0f), // head

				new Color(  0 / 255.0f, 255 / 255.0f, 255 / 255.0f), // ll
				new Color(247 / 255.0f, 151 / 255.0f, 121 / 255.0f), // rl

				new Color( 0 / 255.0f, 255 / 255.0f,   0 / 255.0f), // la
				new Color(86 / 255.0f, 116 / 255.0f, 185 / 255.0f), // ra

				new Color(255 / 255.0f,   255 / 255.0f,   0 / 255.0f), // lh
				new Color(130 / 255.0f,   202 / 255.0f, 156 / 255.0f), // rh

				new Color( 82 / 255.0f,    82 / 255.0f,  82 / 255.0f), // lfi
				new Color(255 / 255.0f,   115 / 255.0f, 115 / 255.0f), // rfi
				new Color(159 / 255.0f,   159 / 255.0f, 159 / 255.0f), // lhi
				new Color(202 / 255.0f,   202 / 255.0f, 202 / 255.0f), // rhi

				new Color(101 / 255.0f,   101 / 255.0f, 101 / 255.0f), // hi
			};
		}

		private KinectSkeleton m_Me;

		private bool m_ChangesToApply;

		private KinectJointMask m_JointMask;

		private KinectJointMask m_OriginalJointMask;

		private bool m_ApplyFilter;

		public void OnEnable()
		{
			m_Me = (KinectSkeleton)target;
			m_ChangesToApply = false;

			if(m_Me.jointCount > 0)
			{
				m_OriginalJointMask = m_JointMask = GetJointMaskFromHierarchy();
				m_ApplyFilter = m_Me.GetJoints().All(x => x.applyFilter);
			} else {
				m_OriginalJointMask = m_JointMask = KinectJointMask.None;
				m_ApplyFilter = false;
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			OnHumaniodMaskGUI();

			EditorGUILayout.Space();

			OnHumanuidOptionsGUI();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			using(new EditorGUI.DisabledScope(!m_ChangesToApply))
			{
				if(GUILayout.Button("Apply"))
					ApplyHumaniodJoints();
			}

			EditorGUILayout.EndHorizontal();
		}

		private void OnHumaniodMaskGUI()
		{
			EditorGUILayout.LabelField("Configure Joints", EditorStyles.boldLabel);	

			if(!Styles.unityDude.image)
			{
				EditorGUILayout.LabelField("Loading humaniod failed.");
				return;
			}

			Rect rect = GUILayoutUtility.GetRect(Styles.unityDude, GUIStyle.none, GUILayout.MaxWidth(Styles.unityDude.image.width));
			rect.x += (EditorGUIUtility.currentViewWidth  - rect.width) / 2;

			Color oldColor = GUI.color;

			GUI.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
			GUI.DrawTexture(rect, Styles.unityDude.image);

			for(int i = 0; i < Styles.bodyPart.Length; ++i)
			{
				var flag = (KinectJointMask)(1 << i);
				GUI.color = m_JointMask.HasFlag(flag) ? Color.green : Color.red;
				
				if(Styles.bodyPart[i].image)
					GUI.DrawTexture(rect, Styles.bodyPart[i].image);
			}

			GUI.color = oldColor;

			if(m_OriginalJointMask != KinectJointMask.All)
			{
				PickHumaniodJoint(rect);
				EditorGUIUtility.AddCursorRect(rect, MouseCursor.ArrowPlus);
			}
			
			UpdateChangesToApply(m_JointMask != m_OriginalJointMask);
		}

		private void PickHumaniodJoint(Rect rect)
		{
			if(!Styles.pickingTexture.image)
				return;
			
			int id = GUIUtility.GetControlID(FocusType.Passive, rect);
			Event evt = Event.current;

			if(evt.GetTypeForControl(id) != EventType.MouseDown || !rect.Contains(evt.mousePosition))
				return;

			evt.Use();

			int x = (int)evt.mousePosition.x - (int)rect.x;
			int y = Styles.unityDude.image.height - ((int)evt.mousePosition.y - (int)rect.y);

			Texture2D pickTexture = Styles.pickingTexture.image as Texture2D;
			Color color = pickTexture.GetPixel(x, y);

			bool anyBodyPartPick = false;
			for (int i = 0; i < Styles.bodyPart.Length; i++)
			{
				if (Styles.maskBodyPartPicker[i] == color)
				{
					GUI.changed = true;

					var flag = (KinectJointMask)(1 << i);

					if(!m_OriginalJointMask.HasFlag(flag))
					{
						if(m_JointMask.HasFlag(flag))
							m_JointMask &= ~flag;
						else
							m_JointMask |= flag;
					}
					
					anyBodyPartPick = true;
				}
			}

			if (!anyBodyPartPick)
			{
				m_JointMask = m_JointMask != KinectJointMask.All
					? KinectJointMask.All
					: m_OriginalJointMask;

				GUI.changed = true;
			}
		}


		private void OnHumanuidOptionsGUI()
		{
			EditorGUILayout.LabelField("Joint Options", EditorStyles.boldLabel);	

			var applyFilter = EditorGUILayout.Toggle("Filter Position And Rotation", m_ApplyFilter);

			UpdateChangesToApply(m_ApplyFilter != applyFilter);

			m_ApplyFilter = applyFilter;

			EditorGUILayout.Space();
		}

		private void ApplyHumaniodJoints()
		{
			var diffMask = ~m_OriginalJointMask & m_JointMask;

			if(diffMask == KinectJointMask.All)
			{
				m_Me.CreateJointTree(JointType.SpineBase);
			} else {
				if(diffMask.HasFlag(KinectJointMask.Torso))
					m_Me.CreateJoints(PredefinedJointTypes.torso);
				
				if(diffMask.HasFlag(KinectJointMask.Head))
					m_Me.CreateJoints(PredefinedJointTypes.head);

				if(diffMask.HasFlag(KinectJointMask.ArmLeft))
					m_Me.CreateJoints(PredefinedJointTypes.armLeft);

				if(diffMask.HasFlag(KinectJointMask.HandLeft))
					m_Me.CreateJoints(PredefinedJointTypes.handLeft);
				
				if(diffMask.HasFlag(KinectJointMask.ArmRight))
					m_Me.CreateJoints(PredefinedJointTypes.armRight);

				if(diffMask.HasFlag(KinectJointMask.HandRight))
					m_Me.CreateJoints(PredefinedJointTypes.handRight);

				if(diffMask.HasFlag(KinectJointMask.LegLeft))
					m_Me.CreateJoints(PredefinedJointTypes.legLeft);

				if(diffMask.HasFlag(KinectJointMask.LegRight))
					m_Me.CreateJoints(PredefinedJointTypes.legRight);
			}
			
			m_OriginalJointMask = m_JointMask;

			foreach(var joint in m_Me.GetJoints())
				joint.applyFilter = m_ApplyFilter;

			
			m_ChangesToApply = false;
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		private KinectJointMask GetJointMaskFromHierarchy()
		{
			var mask = KinectJointMask.None;

			if(ContainsAllJointTypes(PredefinedJointTypes.head))
				mask |= KinectJointMask.Head;
			
			if(ContainsAllJointTypes(PredefinedJointTypes.torso))
				mask |= KinectJointMask.Torso;

			if(ContainsAllJointTypes(PredefinedJointTypes.armLeft))
				mask |= KinectJointMask.ArmLeft;

			if(ContainsAllJointTypes(PredefinedJointTypes.armRight))
				mask |= KinectJointMask.ArmRight;

			if(ContainsAllJointTypes(PredefinedJointTypes.handLeft))
				mask |= KinectJointMask.HandLeft;

			if(ContainsAllJointTypes(PredefinedJointTypes.handRight))
				mask |= KinectJointMask.HandRight;

			if(ContainsAllJointTypes(PredefinedJointTypes.legLeft))
				mask |= KinectJointMask.LegLeft;

			if(ContainsAllJointTypes(PredefinedJointTypes.legRight))
				mask |= KinectJointMask.LegRight;

			return mask;
		}

		private bool ContainsAllJointTypes(JointType[] jointTypes)
		{
			foreach(var jointType in jointTypes)
				if(m_Me[jointType] == null)
					return false;

			return true;
		}
		
		public void UpdateChangesToApply(bool changed)
		{
			if(!m_ChangesToApply)
				m_ChangesToApply = changed;
		}

		[DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy)]
		public static void DrawJointGizmos(KinectSkeleton skeleton, GizmoType type)
		{
			Gizmos.color = KinectEditorUtils.darkGreen;

			foreach(var jointType in KinectHelper.allJointTypes)
			{
				var joint = skeleton[jointType];

				if(joint == null)
					continue;

				var parentJointType = KinectHelper.parentJointTypes[(int)jointType];
				var parentJoint = skeleton[parentJointType];

				if(parentJoint == null || joint == parentJoint)
					continue;
				
				Gizmos.DrawLine(joint.transform.position, parentJoint.transform.position);
			}

			var head = skeleton[JointType.Head];

			if(head != null)
				Gizmos.DrawWireSphere(head.transform.position, 0.14f);
		}

		[DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy)]
		public static void DrawBoundsGizmos(KinectSkeleton skeleton, GizmoType type)
		{
			Gizmos.color = KinectEditorUtils.darkGreen;
			Gizmos.DrawWireCube(skeleton.bounds.center, skeleton.bounds.size);
		}
	}
}