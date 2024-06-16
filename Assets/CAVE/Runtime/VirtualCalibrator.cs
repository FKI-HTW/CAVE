using System;
using System.IO;
using HTW.CAVE.SimpleTcp;
using UnityEngine;

namespace HTW.CAVE
{
	[AddComponentMenu("HTW/CAVE/Virtual Calibrator")]
	[RequireComponent(typeof(VirtualEnvironment))]
	public sealed partial class VirtualCalibrator : MonoBehaviour
	{
		/// <summary>
		/// Available calibration messages.
		/// </summary>
		public static class Message
		{
			public const int Disconnect = 1;

			public const int Sync = 10;
			
			public const int Calibration = 11;
			
			public const int ShowHelpers = 30;

			public const int LockCameras = 31;

			public const int OnlyCameraDisplay = 32;
		}
		
		/// <summary>
		/// Stores the calibration data of multiple cameras.
		/// Used for sending and receiving calibrations, as well as
		/// for loading and saving calibrations from or to disk.
		/// </summary>
		public struct Package
		{
			public bool isEmpty => calibrations == null || calibrations.Length == 0;

			public string timestamp;

			public VirtualOutputTarget outputTarget;

			public int highestVirtualDisplay;

			public VirtualCamera.Calibration[] calibrations;

			public Package(DateTime timestamp, VirtualOutputTarget outputTarget,
				int highestActiveDisplay, VirtualCamera.Calibration[] calibrations)
			{
				this.timestamp = timestamp.ToString("o");
				this.outputTarget = outputTarget;
				this.highestVirtualDisplay = highestActiveDisplay;
				this.calibrations = calibrations;
			}
		}

		/// <summary>/
		/// The localhost address.
		/// </summary>
		public static string localhost = "127.0.0.1";
	
		/// <summary>
		/// The port used to establish a connection between
		/// the calibration editor and application.
		/// </summary>
		public static ushort port = 55555;

		private SimpleTcpServer m_Server;
		
		private VirtualEnvironment m_Environment;
		
		public void Awake()
		{
			m_Server = new SimpleTcpServer();
			m_Environment = GetComponent<VirtualEnvironment>();
			
			RenderSingleCamera(-1);
		}

		public void OnEnable()
		{
			if (!m_Server.Listen(port))
				Debug.LogError($"Failed to listen on port {port}.", this);
		}

		public void Start()
		{
			// Try to load the latest calibration in the standalone,
			// so that we do not need to connect with the server everytime.
			if (!Application.isEditor && TryLoadCalibrationsFromDisk(out VirtualOutputTarget outputTarget,
				out VirtualCamera.Calibration[] calibrations))
			{
				VirtualUtility.MatchAndApplyCalibrations(m_Environment, calibrations);
				m_Environment.SetOutputTarget(outputTarget);
			}
		}
		
		public void Update()
		{
			while(m_Server.messageQueue.TryDequeue(out SimpleTcpMessage message))
				Execute(message);
		}
		
		public void OnDisable()
		{
			m_Server.SendMessage(new SimpleTcpMessage(Message.Disconnect));
			m_Server.Stop();
		}
		
		private void Execute(SimpleTcpMessage message)
		{
			switch (message.type)
			{
				case Message.Calibration:
				{
					var json = message.GetString();
					var package = JsonUtility.FromJson<Package>(json);

					VirtualUtility.MatchAndApplyCalibrations(m_Environment, package.calibrations);
					m_Environment.SetOutputTarget(package.outputTarget);

					try
					{
						File.WriteAllText(GetPersistentCalibrationFilePath(), json);
					} catch {
						Debug.LogError("Failed to write calibration to disk.", this);
					}

					goto case Message.Sync;
				}
				case Message.Sync:
				{
					var calibrations = VirtualUtility.CollectCalibrations(m_Environment);
					var highestActiveDisplay = VirtualUtility.GetHighestVirtualDisplay(calibrations);
					var package = new Package(DateTime.Now, m_Environment.outputTarget, highestActiveDisplay, calibrations);
					var json = JsonUtility.ToJson(package);
					
					m_Server.SendMessage(new SimpleTcpMessage(Message.Calibration, json));
					m_Server.SendMessage(new SimpleTcpMessage(Message.ShowHelpers, showHelpers));
					m_Server.SendMessage(new SimpleTcpMessage(Message.LockCameras, m_Environment.lockCameras));
					m_Server.SendMessage(new SimpleTcpMessage(Message.OnlyCameraDisplay, m_RenderSingleCameraDisplay));
					break;
				}
				case Message.ShowHelpers:
					showHelpers = message.GetBool();
					m_Server.SendMessage(new SimpleTcpMessage(Message.ShowHelpers, showHelpers));
					break;
				case Message.LockCameras:
					m_Environment.lockCameras = message.GetBool();
					if(m_Environment.lockCameras)
						m_Environment.LockCamerasToPosition(m_Environment.center);
					m_Server.SendMessage(new SimpleTcpMessage(Message.LockCameras, m_Environment.lockCameras));
					break;
				case Message.OnlyCameraDisplay:
					RenderSingleCamera(message.GetInt());
					m_Server.SendMessage(new SimpleTcpMessage(Message.OnlyCameraDisplay, m_RenderSingleCameraDisplay));
					break;
				default:
					Debug.LogWarning("Received unknown calibration message: " + message.type, this);
					break;
			}
		}

		internal static bool TryLoadCalibrationsFromDisk(out VirtualOutputTarget outputTarget,
			out VirtualCamera.Calibration[] calibrations)
		{
			try
			{
				var json = File.ReadAllText(GetPersistentCalibrationFilePath());
				var package = JsonUtility.FromJson<Package>(json);

				outputTarget = package.outputTarget;
				calibrations = package.calibrations;
				return true;
			} catch {
				outputTarget = default;
				calibrations = default;
				return false;
			}
		}
		
		internal static string GetPersistentCalibrationFilePath()
		{
			// Use the buildGUID to identify the application. If there is a new build,
			// the calibration of older builds should not be used because the
			// keys of the serialized JSON can change.
			var name = Application.buildGUID + ".calibration.json";
			return Path.Combine(Application.persistentDataPath, name);
		}
	}
}
