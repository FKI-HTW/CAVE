using System;
using System.Diagnostics;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using HTW.CAVE.Kinect.Utils;

namespace HTW.CAVE.Kinect
{
	public enum FaceFrameFeatureType
	{
		Required,
		Full
	}

	/// <summary>
	/// Provides the required functions to automatically retrieve data
	/// from the <see cref="KinectSensor"/>.
	/// </summary>
	[AddComponentMenu("HTW/CAVE/Kinect/Kinect Manager")]
	public sealed class KinectManager : MonoBehaviour
	{
		public event Action onSensorOpen;

		public event Action onSensorClose;

		/// <summary>
		/// Gets the connected <see cref="KinectSensor"/>.
		/// Can be <c>null</c> if no <see cref="KinectSensor"/> is found.
		/// </summary>
		public KinectSensor sensor => m_Sensor;

		/// <summary>
		/// The floor plane vector converted to <see cref="UnityEngine.Vector4"/>.
		/// </summary>
		public UnityEngine.Vector4 floorClipPlane => m_FloorClipPlane.ToUnityVector4();

		/// <summary>
		/// The maximum number of <see cref="Body"/> instances the system can track.
		/// </summary>
		public int trackingCapacity => m_Bodies == null ? 0 : m_Bodies.Length;

		/// <summary>
		/// Defines the feature set of the <see cref="FaceFrameSource"/>.
		/// </summary>
		public FaceFrameFeatureType faceFrameFeatureType;

		/// <summary>
		/// Calculates the tilt of the <see cref="KinectSensor"/> based on
		/// the <see cref="floorClipPlane"/>.
		/// </summary>
		public float tilt => Mathf.Atan(-(float)m_FloorClipPlane.Z / (float)m_FloorClipPlane.Y) * (180.0f / Mathf.PI);

		private KinectSensor m_Sensor;

		private Windows.Kinect.Vector4 m_FloorClipPlane;

		private MultiSourceFrameReader m_MultiSourceFrameReader;

		private Body[] m_Bodies;

		private TimeSpan m_RelativeTime;

		private FaceFrameSource[] m_FaceFrameSources;

		private FaceFrameReader[] m_FaceFrameReaders;

		private FaceFrameResult[] m_FaceFrameResults;

		private Stopwatch m_Stopwatch;

		private int m_BodyCount;

		private long m_Frame;

		public void Start()
		{
			m_Stopwatch = new Stopwatch();
			
			try
			{
				m_Sensor = KinectSensor.GetDefault();
			} catch {
#if UNITY_EDITOR
				UnityEngine.Debug.LogError("The Kinect v2 SDK was not installed properly.");
#endif
				m_Sensor = null;
			}

			enabled = m_Sensor != null;
			OnEnable();
		}

		public void OnEnable()
		{
			if (m_Sensor != null)
			{
				InitializeBodyReaders();
				InitializeFaceReaders();
				OpenSensor();
				onSensorOpen?.Invoke();
			}
		}

		public void OnDisable()
		{
			if (m_Sensor != null)
			{
				StopBodyReaders();
				StopFaceReaders();
				CloseSensor();
				onSensorClose?.Invoke();
			}
		}

		public long AcquireFrames(out Body[] bodies, out FaceFrameResult[] faceFrames, out int bodyCount)
		{
			if (m_Stopwatch.ElapsedMilliseconds > KinectHelper.frameTime)
			{
				AcquireBodyFrames();
				AcquireFaceFrames();
				
				m_Stopwatch.Restart();
			}
			
			bodies = m_Bodies;
			bodyCount = m_BodyCount;
			faceFrames = m_FaceFrameResults;

			return m_Frame;
		}

		public long ForceAcquireFrames(out Body[] bodies, out FaceFrameResult[] faceFrames, out int bodyCount)
		{
			AcquireBodyFrames();
			AcquireFaceFrames();
				
			m_Stopwatch.Restart();

			bodies = m_Bodies;
			bodyCount = m_BodyCount;
			faceFrames = m_FaceFrameResults;

			return m_Frame;
		}
		
		public bool IsSensorOpen() => m_Sensor.IsOpen;

		private void OpenSensor()
		{
			if (!IsSensorOpen())
				m_Sensor.Open();
			m_Stopwatch.Start();
		}

		private void CloseSensor()
		{
			if (IsSensorOpen())
				m_Sensor.Close();

			m_Stopwatch.Stop();
			m_Sensor = null;
		}

		private void InitializeBodyReaders()
		{
			// If more sources are needed add them with:
			// m_Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.BodyIndex | FrameSourceTypes.Depth);

			m_MultiSourceFrameReader = m_Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body);
			m_Bodies = new Body[m_Sensor.BodyFrameSource.BodyCount];
		}

		private void InitializeFaceReaders()
		{
			m_FaceFrameResults = new FaceFrameResult[m_Sensor.BodyFrameSource.BodyCount];
			m_FaceFrameSources = new FaceFrameSource[m_Sensor.BodyFrameSource.BodyCount];
			m_FaceFrameReaders = new FaceFrameReader[m_Sensor.BodyFrameSource.BodyCount];

			var faceFrameFeatures = faceFrameFeatureType == FaceFrameFeatureType.Required
				? RequiredFaceFrameFeatures()
				: FullFaceFrameFeatures();

			for (int i = 0; i < m_FaceFrameSources.Length; ++i)
			{
				m_FaceFrameSources[i] = FaceFrameSource.Create(m_Sensor, 0, faceFrameFeatures);
				m_FaceFrameReaders[i] = m_FaceFrameSources[i].OpenReader();
			}
		}

		private void AcquireBodyFrames()
		{
			MultiSourceFrame multiFrame = m_MultiSourceFrameReader.AcquireLatestFrame();

			if (multiFrame == null)
				return;

			using (BodyFrame bodyFrame = multiFrame.BodyFrameReference.AcquireFrame())
			{
				if (bodyFrame != null && bodyFrame.RelativeTime > m_RelativeTime)
				{
					bodyFrame.GetAndRefreshBodyData(m_Bodies);
					
					m_BodyCount = 0;
					
					// Count tracked bodies and move them to the
					// start of the array.
					for (int i = 0, j = m_Bodies.Length - 1; i < m_Bodies.Length && i < j; ++i)
					{
						if (m_Bodies[i] == null || !m_Bodies[i].GetIsTrackedFast())
						{
							var temp = m_Bodies[i];
							m_Bodies[i--] = m_Bodies[j];
							m_Bodies[j--] = temp;
							continue;
						} 
							
						++m_BodyCount;
					}

					m_RelativeTime = bodyFrame.RelativeTime;
					m_FloorClipPlane = bodyFrame.FloorClipPlane;
					++m_Frame;
				}
			}

			// In the documentation the MultiSourceFrame implements IDisposable
			// but this is not true for the provided scripts. Instead the finalizer
			// needs to be called to cleanup the resources.
			multiFrame = null; 
		}

		private void AcquireFaceFrames()
		{
			for (int i = 0; i < m_BodyCount; ++i)
			{
				m_FaceFrameSources[i].TrackingId = m_Bodies[i].GetTrackingIdFast();

				using (FaceFrame faceFrame = m_FaceFrameReaders[i].AcquireLatestFrame())
				{
					if(faceFrame == null)
						continue;

					m_FaceFrameResults[i] = faceFrame.FaceFrameResult;
				}
			}
		}

		private void StopBodyReaders()
		{
			if (m_MultiSourceFrameReader != null)
			{
				m_MultiSourceFrameReader.Dispose();
				m_MultiSourceFrameReader = null;
			}
		}

		private void StopFaceReaders()
		{
			for (int i = 0; i < m_FaceFrameSources.Length; ++i)
			{
				if (m_FaceFrameReaders[i] != null)
				{
					m_FaceFrameReaders[i].Dispose();
					m_FaceFrameReaders[i] = null;
				}

				if (m_FaceFrameSources[i] != null)
					m_FaceFrameSources[i] = null;
			}
		}

		private static FaceFrameFeatures RequiredFaceFrameFeatures() =>
			FaceFrameFeatures.BoundingBoxInColorSpace |
			FaceFrameFeatures.PointsInColorSpace |
			FaceFrameFeatures.BoundingBoxInInfraredSpace |
			FaceFrameFeatures.PointsInInfraredSpace |
			FaceFrameFeatures.RotationOrientation |
			FaceFrameFeatures.Glasses |
			FaceFrameFeatures.LookingAway;

		private static FaceFrameFeatures FullFaceFrameFeatures() =>
			FaceFrameFeatures.BoundingBoxInColorSpace |
			FaceFrameFeatures.PointsInColorSpace |
			FaceFrameFeatures.BoundingBoxInInfraredSpace |
			FaceFrameFeatures.PointsInInfraredSpace |
			FaceFrameFeatures.RotationOrientation |
			FaceFrameFeatures.FaceEngagement |
			FaceFrameFeatures.Glasses |
			FaceFrameFeatures.Happy |
			FaceFrameFeatures.LeftEyeClosed |
			FaceFrameFeatures.RightEyeClosed |
			FaceFrameFeatures.LookingAway |
			FaceFrameFeatures.MouthMoved |
			FaceFrameFeatures.MouthOpen;
	}
}
