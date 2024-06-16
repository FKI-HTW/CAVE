using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using HTW.CAVE.SimpleTcp;

namespace HTW.CAVE.Calibration
{
	public class VirtualCalibratorClient
	{
		[Flags]
		public enum Status
		{
			Disconnected     = 0,
			Connected        = 1,
			ConnectionFailed = 2,
			Initialized      = 4 | Connected
		}

		public Status status => m_Status;

		public bool showHelpers => m_ShowHelpers;

		public bool lockCameras => m_LockCameras;

		public int singleRenderDisplay => m_SingleRenderDisplay;

		public VirtualCalibrator.Package package;

		private SimpleTcpClient m_Client;
						
		private Status m_Status;
				
		private bool m_ShowHelpers;

		private bool m_LockCameras;

		private int m_SingleRenderDisplay;
		
		public static string savedHostOrLocalhost
		{
			get
			{
				return EditorPrefs.HasKey("VIRTUAL_CALIBRATOR_HOSTNAME")
					? EditorPrefs.GetString("VIRTUAL_CALIBRATOR_HOSTNAME")
					: VirtualCalibrator.localhost;
			}
			set => EditorPrefs.SetString("VIRTUAL_CALIBRATOR_HOSTNAME", value);
		}

		public VirtualCalibratorClient()
		{
			m_Client = new SimpleTcpClient();
			m_Status = Status.Disconnected;
			m_ShowHelpers = false;
			m_LockCameras = false;
		}

		public bool HasStatus(Status status) => status.HasFlag(status);

		public bool Refresh()
		{
			if(HasStatus(VirtualCalibratorClient.Status.Connected))
			{
				while(m_Client.messageQueue.TryDequeue(out SimpleTcpMessage message))
					Execute(message);
				
				return true;
			}

			return false;
		}

		public void LocalConnect() => Connect(VirtualCalibrator.localhost, false);

		public void Connect(string host, bool save)
		{
			m_Status = Status.Disconnected;
			
			if(m_Client.Connect(host, VirtualCalibrator.port))
			{
				m_Status |= Status.Connected;
				Sync();
			} else {
				m_Status |= Status.ConnectionFailed;
			}

			if(save)
				savedHostOrLocalhost = host;
		}

		public void Disconnect()
		{
			m_Client.Stop();
			m_Status = Status.Disconnected;
			package = default;
		}

		public void Load(string file)
		{
			if(!string.IsNullOrEmpty(file))
			{
				var json = File.ReadAllText(file);
				var source = JsonUtility.FromJson<VirtualCalibrator.Package>(json);
				VirtualUtility.MatchAndOverwriteCalibrations(source.calibrations, package.calibrations);
			}
		}

		public void Save(string file)
		{
			if(!string.IsNullOrEmpty(file))
			{
				var json = JsonUtility.ToJson(package, prettyPrint: true);
				File.WriteAllText(file, json);
			}
		}

		public void Sync()
		{
			m_Client.SendMessage(new SimpleTcpMessage(VirtualCalibrator.Message.Sync));
		}

		public void SendPackage()
		{
			var json = JsonUtility.ToJson(package);
			m_Client.SendMessage(new SimpleTcpMessage(VirtualCalibrator.Message.Calibration, json));
		}

		public void SendShowHelpers(bool value)
		{
			m_ShowHelpers = value;
			m_Client.SendMessage(new SimpleTcpMessage(VirtualCalibrator.Message.ShowHelpers, m_ShowHelpers));
		}

		public void SendLockCameras(bool value)
		{
			m_LockCameras = value;
			m_Client.SendMessage(new SimpleTcpMessage(VirtualCalibrator.Message.LockCameras, m_LockCameras));
		}

		public void SendSingleRenderDisplay(int display)
		{
			m_SingleRenderDisplay = display;
			m_Client.SendMessage(new SimpleTcpMessage(VirtualCalibrator.Message.OnlyCameraDisplay, m_SingleRenderDisplay));
		}

		private void Execute(SimpleTcpMessage message)
		{
			switch(message.type)
			{
				case VirtualCalibrator.Message.Disconnect:
					Disconnect();
					break;
				case VirtualCalibrator.Message.Calibration:
					package = JsonUtility.FromJson<VirtualCalibrator.Package>(message.GetString());
					m_Status |= Status.Initialized;
					break;
				case VirtualCalibrator.Message.ShowHelpers:
					m_ShowHelpers = message.GetBool();
					break;
				case VirtualCalibrator.Message.LockCameras:
					m_LockCameras = message.GetBool();
					break;
				case VirtualCalibrator.Message.OnlyCameraDisplay:
					m_SingleRenderDisplay = message.GetInt();
					break;
				default:
					Debug.LogWarning("Received unknown calibration message: " + message.type);
					break;
			}
		}
	}
}