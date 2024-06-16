using Windows.Kinect;
using HTW.CAVE.Kinect;
using HTW.CAVE.Kinect.Utils;
using UnityEngine;

namespace HTW.CAVE
{
	[AddComponentMenu("HTW/CAVE/Virtual Eye Tracking")]
	[RequireComponent(typeof(VirtualEnvironment))]
	public class VirtualEyeTracking : MonoBehaviour
	{
		public static OneEuroParams filterParams = new OneEuroParams(1f);
	
		public KinectTracker tracker
		{
			get => m_Tracker;
			set
			{
				StopTracking();
				m_Tracker = value;
				StartTracking();
			}
		}
		
		public KinectActor actor => m_Actor;
	
		[SerializeField]
		private KinectTracker m_Tracker;
	
		private VirtualEnvironment m_Environment;
		
		private KinectActor m_Actor;
		
		private OneEuroFilter3 m_PositionFilter;
		
		private OneEuroFilter4 m_RotationFilter;
		
		public void Awake()
		{
			m_Environment = GetComponent<VirtualEnvironment>();
		}
		
		public void OnEnable()
		{
			StartTracking();
		}
		
		public void Update()
		{
			if (m_Actor == null || !m_Environment.Contains(m_Actor.bounds.center))
				SetActor(FindActorInEnvironment());
		}
		
		public void OnDisable()
		{
			StopTracking();
		}
		
		private void StartTracking()
		{
			if (m_Tracker != null)
			{
				m_Tracker.onCreateActor += OnCreateActor;
				m_Tracker.onDestroyActor += OnDestroyActor;
			}
		}
		
		private void StopTracking()
		{
			if (m_Tracker != null)
			{
				m_Tracker.onCreateActor -= OnCreateActor;
				m_Tracker.onDestroyActor -= OnDestroyActor;
			}
		
			m_Actor = null;
		}
		
		private void OnCreateActor(KinectActor actor)
		{
			if (m_Actor == null && m_Environment.Contains(actor.bounds.center))
				SetActor(actor);
		}
		
		private void OnDestroyActor(KinectActor actor)
		{
			if (m_Actor == actor)
				SetActor(FindActorInEnvironment());
		}
		
		private void SetActor(KinectActor actor)
		{
			if (m_Actor != null)
				m_Actor.onTrackingDataUpdated -= OnTrackingUpdated;
			
			m_Actor = actor;
			
			if (m_Actor != null)
				m_Actor.onTrackingDataUpdated += OnTrackingUpdated;
		}
		
		private KinectActor FindActorInEnvironment()
		{
			KinectActor best = null;
			float createdAt = float.MaxValue; 
		
			foreach (var actor in m_Tracker.actors)
			{
				if (actor.createdAt < createdAt
				&& m_Environment.Contains(actor.bounds.center))
				{
					best = actor;
					createdAt = actor.createdAt;
				}
			}

			return best;
		}
		
		private void OnTrackingUpdated()
		{
			var head = m_Actor.GetJoint(JointType.Head);
			
			switch (head.trackingState)
			{
				case TrackingState.Tracked:
					var faceRotation = m_Actor.bodyFrame.faceRotation;
						
					SetFilteredTransform(m_Environment.eyes.transform,
						 head.position, faceRotation.IsZero() ? head.rotation : faceRotation);
					break;
				case TrackingState.Inferred:
					SetFilteredTransform(m_Environment.eyes.transform,
						 head.position, head.rotation);
					break;
				case TrackingState.NotTracked:
					var shoulder = m_Actor.GetJoint(JointType.SpineShoulder);
					
					SetFilteredTransform(m_Environment.eyes.transform,
						shoulder.position + Vector3.up * 0.3f, shoulder.rotation);
					break;
			}
		}
		
		private void SetFilteredTransform(Transform target, Vector3 position, Quaternion rotation)
		{
			target.position = m_PositionFilter.Filter(position, KinectHelper.frameTime, in filterParams);
			target.rotation = m_RotationFilter.Filter(rotation, KinectHelper.frameTime, in filterParams);
		}
	}
}
