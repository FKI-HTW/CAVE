using UnityEngine;

namespace HTW.CAVE
{
	[AddComponentMenu("HTW/CAVE/Virtual Environment")]
	public sealed class VirtualEnvironment : MonoBehaviour
	{
		/// <summary>
		/// Returns the current dimensions of the environment.
		/// Use <see cref="Resize"/> to change the dimensions.
		/// </summary>
		public Vector3 dimensions => m_Dimensions;

		/// <summary>
		/// The center point of the environment in world space.
		/// </summary>
		public Vector3 center => transform.TransformPoint(Vector3.up * 0.5f * dimensions.y);

		/// <summary>
		/// The bounds or volume of environment in local space
		/// based on the <see cref="dimensions"/>.
		/// </summary>
		public Bounds localBounds => new Bounds(Vector3.up * 0.5f * dimensions.y, dimensions);
		
		/// <summary>
		/// Returns the current camera render output target.
		/// </summary>
		public VirtualOutputTarget outputTarget => m_OutputTarget;

		/// <summary>
		/// All screens of the environment.
		/// </summary>
		public VirtualScreen[] screens => m_Screens;
		
		/// <summary>
		/// All detected cameras inside the environment.
		/// </summary>
		public VirtualCamera[] cameras => m_Cameras;

		/// <summary>
		/// The eyes from which the projection will be calculated.
		/// </summary>
		public VirtualEyes eyes
		{
			get => m_Eyes;
			set => m_Eyes = value;
		}

		/// <summary>
		/// Defines the eye seperation which is used for the stereo projection.
		/// </summary>
		public float eyeSeperation;
		
		/// <summary>
		/// Near clip plane of the perspective off-center projection.
		/// </summary>
		public float nearClipPlane;
		
		/// <summary>
		/// Far clip plane of the perspective off-center projection.
		/// </summary>
		public float farClipPlane;

		/// <summary>
		/// Lock the cameras to the current position.
		/// If set to <c>true<c/>, the cameras will stop following the eye position.
		/// </summary>
		public bool lockCameras;
				
		[SerializeField]
		private Vector3 m_Dimensions;

		[SerializeField]
		private VirtualOutputTarget m_OutputTarget;
		
		[SerializeField]
		private VirtualEyes m_Eyes;

		private VirtualCamera[] m_Cameras;
				
		private VirtualScreen[] m_Screens;

		public void OnEnable()
		{
			// If the environment is enabled the first time, auto assign
			// the virtual displays, otherwise keep the configuration.
			var keepVirtualDisplays = m_Cameras != null;
			m_Cameras = GetComponentsInChildren<VirtualCamera>();

			SetOutputTarget(m_OutputTarget, keepVirtualDisplays);
			Resize(m_Dimensions);
		}
		
		public void LateUpdate()
		{		
			for (int i = 0; i < m_Cameras.Length; ++i)
			{
				var screen = GetScreen(m_Cameras[i].screenKind);
				var eyePosition = lockCameras
					? m_Cameras[i].transform.position
					: m_Eyes.GetPosition(m_Cameras[i].stereoTarget, eyeSeperation);
				
				m_Cameras[i].UpdateCameraProjection(screen, eyePosition, nearClipPlane, farClipPlane);
				m_Cameras[i].UpdateCameraTransform(screen, eyePosition);
			}
		}
		
		public void Reset()
		{
			eyeSeperation = 0.06f;
			nearClipPlane = 0.1f;
			farClipPlane = 1000f;
			lockCameras = false;
			m_Dimensions = GetDefaultDimensions();
			m_Eyes = GetComponentInChildren<VirtualEyes>();
		}

		/// <summary>
		/// Resizes the environment to the given dimensions.
		/// Rebuilds the screens automatically.
		/// </summary>
		/// <param name="dimensions">The target dimensions.</param>
		public void Resize(Vector3 dimensions)
		{
			m_Dimensions = dimensions;
			
			// Do not rebuild screens if an environment is
			// instantiated or created in the editor.
			if (Application.isPlaying)
			{
				if (m_Screens == null)
				{
					m_Screens = new VirtualScreen[6];
				} else {
					for(int i = 0; i < m_Screens.Length; ++i)
						Destroy(m_Screens[i].gameObject);
				}
				
				for (int i = 0; i < m_Screens.Length; ++i)
					m_Screens[i] = VirtualUtility.CreateScreen((VirtualScreen.Kind)i, dimensions, transform);
			}
		}

		/// <summary>
		/// Sets <see cref="lockCameras"/> to <c>true</c> and positions the cameras
		/// to the target position.
		/// </summary>
		/// <param name="position">The locked target position.</param>
		public void LockCamerasToPosition(Vector3 position)
		{
			lockCameras = true;

			for (int i = 0; i < m_Cameras.Length; ++i)
			{
				var screen = GetScreen(m_Cameras[i].screenKind);
				m_Cameras[i].UpdateCameraTransform(screen, position);
			}
		}

		/// <summary>
		/// Sets the <see cref="VirtualCamera.Calibration.outputTarget"> for every
		/// camera in the environment.
		/// </summary>
		/// <param name="outputTarget">The camera output target.</param>
		public void SetOutputTarget(VirtualOutputTarget outputTarget) => SetOutputTarget(outputTarget, true);

		internal void SetOutputTarget(VirtualOutputTarget outputTarget, bool keepVirtualDisplays)
		{
			float viewportSize = 1f / m_Cameras.Length; // Only for split viewports.

			for (int i = 0; i < m_Cameras.Length; ++i)
			{
				var calibration = m_Cameras[i].GetCalibration();
				calibration.viewportSize = viewportSize;
				calibration.outputTarget = outputTarget;
				calibration.virtualDisplay = keepVirtualDisplays ? calibration.virtualDisplay : i;

				m_Cameras[i].ApplyCalibration(calibration);
			}

			m_OutputTarget = outputTarget;
		}

		/// <summary>
		/// Returns the <see cref="VirtualScreen"> component instance of
		/// the given <see cref="VirtualScreen.Kind">. 
		/// </summary>
		/// <param name="kind">The screen kind.</param>
		/// <returns>The screen component.</returns>
		public VirtualScreen GetScreen(VirtualScreen.Kind kind) => m_Screens[(int)kind];
		
		/// <summary>
		/// Returns if a given point is inside the <see cref="localBounds">.
		/// </summary>
		/// <param name="point">The position of the point.</param>
		/// <returns>If inside the environment <c>true</c> otherwise <c>false</c>.</returns>
		public bool Contains(Vector3 point) => localBounds.Contains(transform.InverseTransformPoint(point));

		/// <summary>
		/// Returns the default dimensions of the HTW CAVE.
		/// </summary>
		/// <returns>The dimensions in meters.</returns>
		public static Vector3 GetDefaultDimensions() => new Vector3(3f, 2.45f, 3f);
	}
}
