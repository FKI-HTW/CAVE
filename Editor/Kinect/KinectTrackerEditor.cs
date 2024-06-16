using System;
using UnityEngine;
using UnityEditor;

namespace HTW.CAVE.Kinect
{
	[CustomEditor(typeof(KinectTracker))]
	public class KinectTrackerEditor : Editor
	{
		private KinectTracker m_Me;

		private SerializedProperty m_ConstructionTypeProperty;

		private SerializedProperty m_PrefabProperty;
	
		public void OnEnable()
		{
			m_Me = (KinectTracker)target;
			m_ConstructionTypeProperty = serializedObject.FindProperty("constructionType");
			m_PrefabProperty = serializedObject.FindProperty("prefab");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_ConstructionTypeProperty);

			if(m_Me.constructionType == KinectActorConstructionType.Prefab)
			{
				EditorGUILayout.PropertyField(m_PrefabProperty);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				if(m_Me.prefab == null && GUILayout.Button("Create Actor"))
				{
					var gameObject = new GameObject("Kinect Actor");
					var actor = gameObject.AddComponent<KinectActor>();
					m_PrefabProperty.objectReferenceValue = actor;
					Selection.activeGameObject = gameObject;
				}

				EditorGUILayout.EndHorizontal();
			}	

			serializedObject.ApplyModifiedProperties();
		}
	}
}