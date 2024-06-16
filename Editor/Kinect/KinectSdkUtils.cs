using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR_WIN
using Microsoft.Win32;
#endif

namespace HTW.CAVE.Kinect
{
	public static class KinectSdkUtil
	{
#if UNITY_EDITOR_WIN
		private static bool s_SearchedInstall = false;

		private static bool s_IsInstalled = false;

		public static bool IsSDKInstalled()
		{
			if(s_SearchedInstall)
				return s_IsInstalled;

			string sdk = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Kinect\\v2.0", "SDKInstallPath", null) as string;

			s_SearchedInstall = true;
			s_IsInstalled = !string.IsNullOrEmpty(sdk);

			return s_IsInstalled;
		}
#else
		public static bool IsSDKInstalled()
		{
			return false;
		}
#endif
	}
}