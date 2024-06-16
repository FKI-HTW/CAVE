using HTW.CAVE.Kinect;
using UnityEngine;

namespace HTW.CAVE
{
	public static class VirtualUtility
	{
		public static VirtualEnvironment CreateEnvironment(Vector3 dimensions, bool stereo)
		{
			var environment = new GameObject("Virtual Environment").AddComponent<VirtualEnvironment>();
			
			environment.Resize(dimensions);
			environment.eyes = new GameObject("Virtual Eyes").AddComponent<VirtualEyes>();
			environment.eyes.transform.parent = environment.transform;
			environment.eyes.transform.localPosition = new Vector3(0f, 1.7f, 0f);
			environment.eyes.transform.localRotation = Quaternion.identity;
			
			if (stereo)
			{
				for (int i = 0; i < 12; ++i)
					CreateCamera(environment, (VirtualScreen.Kind)(i / 2),
						i % 2 == 0 ? VirtualEyes.StereoTarget.Left : VirtualEyes.StereoTarget.Right);
			
			} else {
				for (int i = 0; i < 6; ++i)
					CreateCamera(environment, (VirtualScreen.Kind)i,
						VirtualEyes.StereoTarget.Mono);
			}

			environment.gameObject.AddComponent<VirtualCalibrator>();
		
			return environment;
		}

		public static VirtualEnvironment CreateEnvironment(Vector3 dimensions, bool stereo,
			Vector3 trackerOrigin)
		{
			var environment = CreateEnvironment(dimensions, stereo);
			var tracker = new GameObject("Kinect Tracker").AddComponent<KinectTracker>();
			environment.gameObject.AddComponent<VirtualEyeTracking>().tracker = tracker;

			tracker.transform.parent = environment.transform;
			tracker.transform.localPosition = trackerOrigin;
			tracker.transform.LookAt(environment.transform.position, Vector3.up);
			tracker.transform.SetSiblingIndex(0);

			return environment;
		}

		public static VirtualCamera CreateCamera(VirtualEnvironment environment,
			VirtualScreen.Kind kind, VirtualEyes.StereoTarget target)
		{
			var camera = new GameObject($"Virtual Camera {kind}")
				.AddComponent<VirtualCamera>();

			if (target != VirtualEyes.StereoTarget.Mono)
				camera.name += target == VirtualEyes.StereoTarget.Left ? " L" : " R";

			camera.stereoTarget = target;
			camera.screenKind = kind;
			camera.transform.parent   = environment.transform;
			camera.transform.position = environment.eyes.transform.position;
			camera.transform.rotation = environment.transform.rotation * VirtualScreen.GetLocalRotation(kind);
			
			return camera;
		}

		public static VirtualScreen CreateScreen(VirtualScreen.Kind kind, Vector3 dimensions, Transform parent)
		{
			var screen = new GameObject($"Virtual Screen {kind}").AddComponent<VirtualScreen>();
			var transform = screen.transform;
			var size = Vector2.zero;
			
			screen.kind = kind;
			transform.parent = parent;
			transform.localPosition = VirtualScreen.GetLocalPosition(kind, dimensions);
			transform.localRotation = VirtualScreen.GetLocalRotation(kind);

			switch (kind)
			{
				case VirtualScreen.Kind.Front:
				case VirtualScreen.Kind.Back:
					screen.width = dimensions.x;
					screen.height = dimensions.y;
					break;
				case VirtualScreen.Kind.Left:
				case VirtualScreen.Kind.Right:
					screen.width = dimensions.z;
					screen.height = dimensions.y;
					break;
				case VirtualScreen.Kind.Top:
				case VirtualScreen.Kind.Bottom:
					screen.width = dimensions.x;
					screen.height = dimensions.z;
					break;
			}
			
			return screen;
		}

		/// <summary>
		/// Collects the calibration data of every camera in the environment.
		/// </summary>
		/// <param name="environment"></param>
		/// <returns>All camera calibrations of the specified environment.</returns>
		public static VirtualCamera.Calibration[] CollectCalibrations(VirtualEnvironment environment)
		{
			var cameras = environment.cameras;
			var calibrations = new VirtualCamera.Calibration[cameras.Length];

			for (int i = 0; i < cameras.Length; ++i)
				calibrations[i] = cameras[i].GetCalibration();
				
			return calibrations;
		}

		/// <summary>
		/// Matches and applies the new calibrations to the cameras
		/// in the environment.
		/// </summary>
		/// <param name="environment">The target environment.</param>
		/// <param name="calibrations">The new calibrations.</param>
		public static void MatchAndApplyCalibrations(VirtualEnvironment environment,
			VirtualCamera.Calibration[] calibrations)
		{
			var cameras = environment.cameras;

			for (int i = 0; i < calibrations.Length; ++i)
			for (int j = 0; j < cameras.Length; ++j)
			{
				if (cameras[j].name == calibrations[i].name)
				{
					cameras[j].ApplyCalibration(calibrations[i]);
					break;
				}
			}
		}

		/// <summary>
		/// Overwrites specified destination calibrations with calibrations from another source
		/// by matching the names of the cameras in both calibrations.
		/// </summary>
		/// <param name="source">The source calibrations.</param>
		/// <param name="destination">The destination calibrations that will be overwritten.</param>
		public static void MatchAndOverwriteCalibrations(VirtualCamera.Calibration[] source,
			VirtualCamera.Calibration[] destination)
		{
			for (int i = 0; i < destination.Length; ++i)
			for (int j = 0; j < source.Length; ++j)
			{
				if (destination[i].name == source[j].name)
				{
					destination[i] = source[j];
					break;
				}
			}
		}

		public static int GetHighestVirtualDisplay(VirtualCamera.Calibration[] calibrations)
		{
			var display = 0;

			for (int i = 0; i < calibrations.Length; ++i)
				if (calibrations[i].virtualDisplay > display)
					display = calibrations[i].virtualDisplay;

			return display;
		}
	}
}
