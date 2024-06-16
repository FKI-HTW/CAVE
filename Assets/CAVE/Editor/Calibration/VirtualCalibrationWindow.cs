using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace HTW.CAVE.Calibration
{
	public class VirtualCalibrationWindow : EditorWindow
	{
		[MenuItem("Window/Virtual Calibration")]
		public static void Open()
		{
			var window = EditorWindow.GetWindow(typeof(VirtualCalibrationWindow), false,
				"Virtual Calibration", true);
			window.minSize = new Vector2(500f, 250f);
		}

		public static string defaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

		private VirtualCalibratorClient m_Client;

		private GUIContent m_ShowHelpersContent;

		private GUIContent m_HideHelpersContent;

		private GUIContent m_LockCamerasContent;

		private GUIContent m_UnlockCamerasContent;

		private Vector2 m_ScrollPosition;
		
		private int m_SelectedIndex;
		
		private string m_Host;

		private string[] m_DisplayList;

		private static Texture2D GetIcon(string name)
		{
			name = EditorGUIUtility.isProSkin ? name + "_Dark" : name + "_Light";
			return Resources.Load<Texture2D>("Icons/" + name);
		}

		private static void RememberFolderOfFile(string file)
		{
			if(!string.IsNullOrEmpty(file))
				defaultFolder = Path.GetDirectoryName(file);
		}

		public void OnEnable()
		{
			m_Client = new VirtualCalibratorClient();
			
			EditorApplication.playModeStateChanged += OnPlayModeStateChange;
		
			m_ShowHelpersContent = new GUIContent(GetIcon("ShowHelpers@32_32"));
			m_HideHelpersContent = new GUIContent(GetIcon("HideHelpers@32_32"));

			m_LockCamerasContent = new GUIContent(GetIcon("LockCameras@32_32"));
			m_UnlockCamerasContent = new GUIContent(GetIcon("UnlockCameras@32_32"));

			m_ScrollPosition = Vector2.zero;
			m_SelectedIndex = 0;
			m_Host = VirtualCalibratorClient.savedHostOrLocalhost;
			m_DisplayList = Array.Empty<string>();
		}

		public void OnGUI()
		{
			if(m_Client.Refresh())
			{
				OnToolbarGUI();
				OnCalibrationGUI();

				if(m_Client.HasStatus(VirtualCalibratorClient.Status.Initialized))
					OnCommandsGUI();
			} else {
				OnConnectGUI();
			}
		}

		private void OnPlayModeStateChange(PlayModeStateChange state)
		{
			m_Client.Disconnect();
		}
		
		private void OnConnectGUI()
		{
			CalibrationGUILayout.BeginToolbar();
			EditorGUILayout.Space();
			
			// Because the calibrator server listens on the same port
			// as the client, only allow a local connection in play mode.
			if(Application.isPlaying)
			{
				if(CalibrationGUILayout.ToolbarButton("Local Connect", 120f))
					m_Client.LocalConnect();
			} else {
				m_Host = CalibrationGUILayout.ToolbarTextField(m_Host);
				
				if(CalibrationGUILayout.ToolbarButton("Connect", 120f))
					m_Client.Connect(m_Host, true);
			}
			
			CalibrationGUILayout.EndToolbar();
		
			EditorGUILayout.BeginHorizontal();
			CalibrationGUILayout.ScrollViewDummy(240f);
			EditorGUILayout.BeginVertical();
			
			if(m_Client.HasStatus(VirtualCalibratorClient.Status.ConnectionFailed))
				EditorGUILayout.HelpBox(GetConnectionFailedText(), MessageType.Error);
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			
			CalibrationGUILayout.BeginLowerToolbar();
			CalibrationGUILayout.EndLowerToolbar();
		}
		
		private void OnToolbarGUI()
		{
			CalibrationGUILayout.BeginToolbar();

			EditorGUILayout.LabelField("Cameras", EditorStyles.miniLabel, GUILayout.Width(170f));
			GUILayout.Space(5f);
			EditorGUILayout.LabelField("Displays", EditorStyles.miniLabel, GUILayout.Width(56f));
			
			if(CalibrationGUILayout.ToolbarButton("Load", 120f))
			{
				var file = EditorUtility.OpenFilePanel("Load Calibration", defaultFolder, "json");
				m_Client.Load(file);
				RememberFolderOfFile(file);
			}

			if(CalibrationGUILayout.ToolbarButton("Save As", 120f))
			{
				var file = EditorUtility.SaveFilePanel("Save Calibration", defaultFolder,
					"calibration-" + DateTime.Now.ToString("yyyy-MM-dd"), "json");
				m_Client.Save(file);
				RememberFolderOfFile(file);
			}
			
			EditorGUILayout.Space();
			
			if(CalibrationGUILayout.ToolbarButton("Disconnect", 120f))
				m_Client.Disconnect();
			
			CalibrationGUILayout.EndToolbar();
		}
		
		private void OnCalibrationGUI()
		{
			// Only show the calibration interface if
			// there are available calibrations. 
			if(m_Client.package.isEmpty)
			{
				EditorGUILayout.BeginHorizontal();
				CalibrationGUILayout.ScrollViewDummy(240f);
				EditorGUILayout.BeginVertical();
				
				// If the client is connected but not initialized, then it is still syncing.
				if(m_Client.HasStatus(VirtualCalibratorClient.Status.Initialized))
					EditorGUILayout.HelpBox("Unable to load calibration: No cameras detected.", MessageType.Warning);
				else
					EditorGUILayout.LabelField("Syncing...");
					
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
				
				return;
			}
			
			var calibrations = m_Client.package.calibrations;

			if(m_DisplayList.Length < m_Client.package.highestVirtualDisplay)
				m_DisplayList = GetDisplayList(m_Client.package.highestVirtualDisplay);
		
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(240f));
			m_ScrollPosition = CalibrationGUILayout.BeginScrollView(m_ScrollPosition);
			
			for(int i = 0; i < calibrations.Length; ++i)
			{
				EditorGUILayout.BeginHorizontal();

				if(CalibrationGUILayout.ScrollViewButton(calibrations[i].name, i == m_SelectedIndex, GUILayout.Width(180f)))
				{
					m_SelectedIndex = i;
					EditorGUI.FocusTextInControl(null);
				}

				calibrations[i].virtualDisplay = CalibrationGUILayout.ScrollViewPopup(calibrations[i].virtualDisplay,
					m_DisplayList, GUILayout.MinWidth(45f));

				EditorGUILayout.EndHorizontal();
				
				CalibrationGUILayout.ScrollViewSeperator();
			}
			
			CalibrationGUILayout.EndScrollView();
			CalibrationGUILayout.ScrollViewFlexibleSpace();
			EditorGUILayout.EndVertical();
			
			
			EditorGUILayout.BeginVertical();
			
			OnProjectionCalibrationGUI(ref calibrations[m_SelectedIndex]);
									
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

		}
		
		private void OnProjectionCalibrationGUI(ref VirtualCamera.Calibration calibration)
		{
			var singleRenderDisplay = m_Client.singleRenderDisplay == calibration.virtualDisplay;
			var singleRenderToggle = EditorGUILayout.Toggle("Turn Others Off", singleRenderDisplay);
			
			if(singleRenderToggle != singleRenderDisplay)
				m_Client.SendSingleRenderDisplay(singleRenderToggle ? calibration.virtualDisplay : -1);

			calibration.projectionCorrection = EditorGUILayout.Toggle("Enable Correction", calibration.projectionCorrection);
	
			EditorGUILayout.Space();

			using(new EditorGUI.DisabledScope(!calibration.projectionCorrection))
				calibration.projectionQuad = CalibrationGUILayout.QuadField(calibration.projectionQuad);
		}
		
		private void OnCommandsGUI()
		{
			var layoutHeight = GUILayout.Height(24f);

			CalibrationGUILayout.BeginLowerToolbar();

			GUILayout.Space(20f);

			m_Client.package.outputTarget = (VirtualOutputTarget)EditorGUILayout.EnumPopup(
				m_Client.package.outputTarget, GUILayout.Width(200f));

			//GUILayout.Space(240f);
			GUILayout.Space(20f);

			if(GUILayout.Button(m_Client.showHelpers ? m_HideHelpersContent : m_ShowHelpersContent, GUILayout.Width(40f), layoutHeight))
				m_Client.SendShowHelpers(!m_Client.showHelpers);

			if(GUILayout.Button(m_Client.lockCameras ? m_UnlockCamerasContent : m_LockCamerasContent, GUILayout.Width(40f), layoutHeight))
				m_Client.SendLockCameras(!m_Client.lockCameras);
			
			GUILayout.FlexibleSpace();
			
			if(GUILayout.Button("Reset", GUILayout.Width(70f), layoutHeight))
			{
				m_Client.Sync();
				EditorGUI.FocusTextInControl(null);
			}

			if(GUILayout.Button("Apply", GUILayout.Width(70f), layoutHeight))
			{
				m_Client.SendPackage();
				EditorGUI.FocusTextInControl(null);
			}

			CalibrationGUILayout.EndLowerToolbar();
		}
	
		private string GetConnectionFailedText()
		{
			return "Could not connect to server.\n" 
			+ "Make sure that no connection to the server has been established yet "
			+ $"and that port {VirtualCalibrator.port} can be opened.";
		}

		private string[] GetDisplayList(int highestActiveDisplay)
		{
			var displays = new string[highestActiveDisplay + 1];

			for(int i = 0; i <= highestActiveDisplay; ++i)
				displays[i] = i.ToString();

			return displays;
		}
	}
}