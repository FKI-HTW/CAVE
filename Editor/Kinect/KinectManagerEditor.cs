using System;
using UnityEngine;
using UnityEditor;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using HTW.CAVE.Kinect.Utils;

namespace HTW.CAVE.Kinect
{
	[CustomEditor(typeof(KinectManager))]
	public class KinectManagerEditor : Editor
	{
		private KinectManager m_Me;

		private SerializedProperty m_FaceFrameFeatureTypeProperty;

		private Vector2 m_ScrollPosition;
		
		public void OnEnable()
		{
			m_Me = (KinectManager)target;
			m_FaceFrameFeatureTypeProperty = serializedObject.FindProperty("faceFrameFeatureType");

			EditorApplication.update += UpdateKinectData;
		}

		public void OnDisable()
		{
			EditorApplication.update -= UpdateKinectData;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_FaceFrameFeatureTypeProperty);

			if(!KinectSdkUtil.IsSDKInstalled())
				EditorGUILayout.HelpBox("Unable to find Kinect 2.0 SDK installation.", MessageType.Warning);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();

			OnKinectSensorGUI();
			
			if(!Application.isPlaying)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				if(GUILayout.Button("Help"))
					KinectManagerHelpWindow.Open();

				EditorGUILayout.EndHorizontal();
			}
		}

		private void OnKinectSensorGUI()
		{
			if(m_Me.sensor == null)
				return;

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Kinect Sensor Data", EditorStyles.boldLabel);

			string open = m_Me.sensor.IsOpen ? "Open" : "Closed";
			string available = m_Me.sensor.IsAvailable ? "Available" : "Not Available";

			EditorGUILayout.LabelField("Status", open + ", " + available);

			if(!m_Me.sensor.IsOpen)
				return;

			EditorGUILayout.LabelField("Tilt", m_Me.tilt.ToString());
			EditorGUILayout.LabelField("Floor Plane", m_Me.floorClipPlane.ToString());

			var frame = m_Me.AcquireFrames(out Body[] bodies, out FaceFrameResult[] faces, out int count);

			EditorGUILayout.LabelField("Frame Number", frame.ToString());
			EditorGUILayout.LabelField("Tracked Body Count", count.ToString());

			if(count > 0)
			{
				m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
				for(int i = 0; i < count; ++i)
				{
					EditorGUILayout.LabelField("Tracking Id", bodies[i].TrackingId.ToString(), EditorStyles.helpBox);
				}
				EditorGUILayout.EndScrollView();
			}
		}

		private void UpdateKinectData()
		{
			if(m_Me.sensor != null && m_Me.sensor.IsOpen)
				Repaint();
		}
		
		[DrawGizmo(GizmoType.Active | GizmoType.Selected)]
		public static void DrawGizmos(KinectManager manager, GizmoType type)
		{
			if(manager.sensor == null || !manager.sensor.IsAvailable)
				return;

			Transform transform = manager.transform;

			var position = transform.position;
			var rotation = transform.rotation;
			var correction = KinectHelper.CalculateFloorRotationCorrection(manager.floorClipPlane);

			transform.position = transform.position + KinectHelper.SensorFloorPlaneYOffset(manager.floorClipPlane);
			transform.rotation = Quaternion.Inverse(correction) * transform.rotation;

			Gizmos.color = KinectEditorUtils.green;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawFrustum(Vector3.zero, KinectHelper.fieldOfView.y, KinectHelper.clippingPlane.x,
				KinectHelper.clippingPlane.y, KinectHelper.aspectRatio); 

			transform.rotation = rotation;
			transform.position = position;
		}
	}
	
	public class KinectManagerHelpWindow : EditorWindow
	{
		public static void Open()
		{
			var window = EditorWindow.GetWindow(typeof(KinectManagerHelpWindow), true, "Kinect Manager Help", true);
			window.minSize = new Vector2(350f, 600f);
		}

		public void OnGUI()
		{
			var install = "Before you connect with the Kinect v2 sensor "
			            + "install the Kinect v2 SDK. "
						+ "Make sure to restart your pc after the installation was completed.";
		
			var issueMicrophone = "Make sure that the 'allow desktop apps to access your microphone' "
			                    + "option is enabled in the Window privacy settings. "
								+ "For more information see the link below.";
			
			var issueWindowsUpdate = "This can happen after the Windows 10 Creator Update. "
			                       + "See the link below for further instructions.";
			
			var textBoxStyle = EditorStyles.helpBox;
			textBoxStyle.fontSize = EditorStyles.boldLabel.fontSize;
		
			// INSTALL SDK
			EditorGUILayout.LabelField("Kinect SDK", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(install, textBoxStyle);
			
			EditorGUILayout.BeginHorizontal();
			
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Download SDK"))
				Application.OpenURL("https://www.microsoft.com/en-us/download/details.aspx?id=44561");
			EditorGUILayout.EndHorizontal();
		
			EditorGUILayout.LabelField("Issues", EditorStyles.boldLabel);
			
			// ISSUE MICROPHONE OR ADAPTER
			EditorGUILayout.LabelField("The Kinect sensor is disconnecting after about 10 seconds.");
			EditorGUILayout.LabelField(issueMicrophone, textBoxStyle);
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Open Link"))
				Application.OpenURL("https://social.msdn.microsoft.com/Forums/sqlserver/en-US/bcd775ef-64b0-4e94-8d26-ce297d6d60ea/kinect-v2-keeps-disconnecting-after-every-10-seconds-on-windows-10-1903?forum=kinectv2sdk");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			
			// ISSUE WINDOWS UPDATE
			EditorGUILayout.LabelField("The Kinect sensor is not recognized.");
			EditorGUILayout.LabelField(issueWindowsUpdate, textBoxStyle);
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Open Link"))
				Application.OpenURL("https://skarredghost.com/2018/03/01/fix-kinect-v2-not-working-windows-10-creators-update/");
			EditorGUILayout.EndHorizontal();
		}
	}
}