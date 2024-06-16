using System;
using UnityEngine;

namespace HTW.CAVE
{
	/// <summary>
	/// Defines the output target of a <see cref="Camera"/>
	/// component. 
	/// </summary>
	public enum VirtualOutputTarget
	{
		Display,
		SplitHorizontal,
		SplitVertical
	}

	[AddComponentMenu("HTW/CAVE/Virtual Camera")]
	[RequireComponent(typeof(Camera))]
	public sealed class VirtualCamera : MonoBehaviour
	{
		/// <summary>
		/// A quad defined by four corners.
		/// </summary>
		[Serializable]
		public struct Quad
		{
			public Vector2 topLeft;
			
			public Vector2 topRight;
			
			public Vector2 bottomLeft;
			
			public Vector2 bottomRight;
			
			public static Quad Unitary() => new Quad
				{
					topLeft = new Vector2(-1f, 1f),
					topRight = new Vector2(1f, 1f),
					bottomLeft = new Vector2(-1f, -1f),
					bottomRight = new Vector2(1f, -1f)
				};
		}

		/// <summary>
		/// Contains calibration data and information of
		/// a single camera.
		/// </summary>
		[Serializable]
		public struct Calibration
		{
			[HideInInspector]
			public string name;

			[HideInInspector]
			public int virtualDisplay;

			[HideInInspector]
			public float viewportSize;

			[HideInInspector]
			public VirtualOutputTarget outputTarget;

			public bool projectionCorrection;

			public Quad projectionQuad;
		}

		/// <summary>
		/// The <see cref="Camera"/> component that renders
		/// the image.
		/// </summary>
		public Camera renderCamera => m_Camera;

		/// <summary>
		/// Defines the screen that the camera is facing
		/// and projects to.
		/// </summary>
		public VirtualScreen.Kind screenKind;
		
		/// <summary>
		/// Defines the eye of the stereo or mono rendering.
		/// </summary>
		public VirtualEyes.StereoTarget stereoTarget;

		[SerializeField]
		private Calibration m_Calibration;

		private Camera m_Camera;
		
		private Matrix4x4 m_BimberMatrix;
		
		public void Awake()
		{
			m_Camera = GetComponent<Camera>();
			RebuildBimberMatrix(m_Calibration.projectionQuad);
		}

		public void Reset()
		{
			screenKind = VirtualScreen.Kind.Front;
			stereoTarget = VirtualEyes.StereoTarget.Mono;

			m_Calibration.projectionCorrection = false;
			m_Calibration.projectionQuad = Quad.Unitary();
		}
		
		/// <summary>
		/// Updates the <see cref="Camera.projectionMatrix"/> of the <see cref="renderCamera"/>
		/// which creates a holographic projection effect.
		/// </summary>
		/// <param name="screen">The screen the camera is facing.</</param>
		/// <param name="eyes">The eye position in world space.</param>
		/// <param name="nearClipPlane">The near clip plane of the camera.</param>
		/// <param name="farClipPlane">The far clip plane of the camera.</param>
		public void UpdateCameraProjection(VirtualScreen screen, Vector3 eyes,
			float nearClipPlane, float farClipPlane)
		{
			// Use the world position of the specified eye anchor
			// to calculate the local position in the screen coordinate system.
			// Finally use the local position to compute the projection matrix
			// for the holographic projection.
			// Additionally correct the projection with a bimber matrix if
			// necessary.
			var local = screen.transform.InverseTransformPoint(eyes);
			var projection = VirtualProjection.ComputeHolographicProjectionMatrix(local,
				nearClipPlane, farClipPlane, screen.width, screen.height);
			var bimber = m_Calibration.projectionCorrection ? m_BimberMatrix : Matrix4x4.identity;
			
			m_Camera.nearClipPlane = nearClipPlane;
			m_Camera.farClipPlane = farClipPlane;
			m_Camera.projectionMatrix = bimber * projection;
		}
		
		/// <summary>
		/// Update the position and rotation based on the eye position and
		/// screen rotation.
		/// </summary>
		/// <param name="screen">The screen the camera is facing.</param>
		/// <param name="eyes">The eye position in world space.</param>
		public void UpdateCameraTransform(VirtualScreen screen, Vector3 eyes)
		{
			// Update the camera position to match the eye position
			// but KEEP the cameras forward oriented to the screen plane.
			transform.position = eyes;
			transform.rotation = screen.transform.rotation;
		}
		
		/// <summary>
		/// Returns the calibration containing the display and output target and
		/// projection correction.
		/// </summary>
		/// <returns>Current calibration data.</returns>
		public Calibration GetCalibration()
		{
			m_Calibration.name = gameObject.name; // Name can change at runtime.
			return m_Calibration;
		}

		/// <summary>
		/// Applies a given calibration to the camera. 
		/// Rebuilds the internal bimber matrix and updates the output target.
		/// </summary>
		/// <param name="calibration">Source calibration data.</param>
		public void ApplyCalibration(Calibration calibration)
		{
			if (m_Camera == null) // Applying the calibration can be done before Awake or in the editor.
				m_Camera = GetComponent<Camera>();

			m_Calibration = calibration;

			if (m_Calibration.projectionCorrection)
				RebuildBimberMatrix(m_Calibration.projectionQuad);

			if (m_Calibration.outputTarget == VirtualOutputTarget.Display)
			{
				SetAndActivateDisplay(m_Camera, m_Calibration.virtualDisplay);
				m_Calibration.viewportSize = 1f; // The display mode takes up the whole screen.
			} else {
				SetAndResizeViewport(m_Camera, 0, m_Calibration.virtualDisplay, m_Calibration.viewportSize,
					m_Calibration.outputTarget == VirtualOutputTarget.SplitHorizontal);
			}
		}

		private void RebuildBimberMatrix(Quad quad)
		{
			m_BimberMatrix = VirtualProjection.ComputeBimberMatrix(quad.bottomLeft,
				quad.topLeft, quad.topRight, quad.bottomRight);
		}

		/// <summary>
		/// Activates the <see cref="Display"/> at the given index and
		/// sets the <see cref="Camera.targetDisplay"/> afterwards.
		/// </summary>
		/// <param name="camera">The target camera.</param>
		/// <param name="display">Target display index.</param>
		public static void SetAndActivateDisplay(Camera camera, int display)
		{
			camera.rect = new Rect(0f, 0f, 1f, 1f);
			
#if !UNITY_EDITOR
			if(display >= Display.displays.Length)
				return;

			// Check if the display is inactive...
			// only required in a standalone application.
			if(!Display.displays[display].active)
				Display.displays[display].Activate();
#endif
			camera.targetDisplay = display;
		}

		/// <summary>
		/// Sets the <see cref="Camera.targetDisplay"/> and splits the <see cref="Camera.rect"/>
		/// viewport based on the given size, index and direction. 
		/// </summary>
		/// <param name="camera">The target camera.</param>
		/// <param name="display">Target display index.</param>
		/// <param name="index">Viewport index (order from left to right or top to bottom).</param>
		/// <param name="size">The size of the viewport.</param>
		/// <param name="horizontal">The direction of the viewport.</param>
		public static void SetAndResizeViewport(Camera camera, int display, int index, float size, bool horizontal = true)
		{
			camera.targetDisplay = display;

			if (horizontal)
				camera.rect = new Rect(index * size, 0f, size, 1f);
			else
				camera.rect = new Rect(0f, index * size, 1f, size);
		}
	}
}
