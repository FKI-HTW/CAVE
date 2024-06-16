using System;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;

namespace HTW.CAVE.Kinect
{
	/// <summary>
	/// The construction type defines how a newly
	/// tracked actor will be created and which components are added.
	/// </summary>
	public enum KinectActorConstructionType
	{
		Basic,
		Full,
		Prefab
	}

	/// <summary>
	/// This component can be attached to the <see cref="KinectManager"/>
	/// and is responsible for creating <see cref="KinectActor"/> instances
	/// for each tracked person. The <see cref="KinectActor"/> is
	/// constructed according to the <see cref="constructionType"/>.
	/// </summary>
	[AddComponentMenu("HTW/CAVE/Kinect/Kinect Tracker")]
	[RequireComponent(typeof(KinectManager))]
	public sealed class KinectTracker : MonoBehaviour
	{
		public event Action<KinectActor> onCreateActor;

		public event Action<KinectActor> onDestroyActor;

		/// <summary>
		/// List of currently tracked actors.
		/// </summary>
		public IReadOnlyList<KinectActor> actors => m_Actors;
		
		/// <summary>
		/// Defines how a <see cref="KinectActor"/> is created when a
		/// new person is tracked.
		/// </summary>
		public KinectActorConstructionType constructionType;

		/// <summary>
		/// The custom prefab that will be cloned when the <see cref="constructionType"/> 
		/// is set to <see cref="KinectActorConstructionType.Prefab"/>.
		/// </summary>
		public KinectActor prefab;

		private KinectManager m_Manager;
		
		private List<KinectActor> m_Actors;
		
		private long m_Frame;
		
		public void Awake()
		{
			m_Manager = GetComponent<KinectManager>();
			m_Manager.onSensorOpen += () => enabled = true;
			m_Manager.onSensorClose += () => enabled = false;
			m_Actors = new List<KinectActor>(m_Manager.trackingCapacity);
			
			enabled = false;
		}
		
		public void Update()
		{
			var frame = m_Manager.AcquireFrames(out Body[] bodies, out FaceFrameResult[] faces, out int bodyCount);

			// Only update the tracking data if the
			// acquired frame is new.
			if (frame > m_Frame)
			{
				TrackActors(bodies, faces, bodyCount, frame);
				m_Frame = frame;
			}
		}
		
		public void OnDisable()
		{			
			foreach (var actor in m_Actors)
			{
				onDestroyActor?.Invoke(actor);
				Destroy(actor.gameObject);
			}
			
			m_Actors.Clear();
		}

		public void Reset()
		{
			constructionType = KinectActorConstructionType.Full;
			prefab = null;
		}
		
		private void TrackActors(Body[] bodies, FaceFrameResult[] faces, int bodyCount, long frame)
		{
			// Normally this would be cleaner with a double buffer
			// (which is simple with Span<> unfortunately Unity is stuck with .NET 4.x)
			// but due to the fact that only 8 bodies can be tracked
			// two passes is just as fast.
			
			var lastActorCount = m_Actors.Count;
									
			for (int i = 0; i < bodyCount; ++i)
			{
				var trackingId = bodies[i].GetTrackingIdFast();
				var actor = FindActorByTrackingId(trackingId);
				
				if (actor == null)
				{
					actor = ConstructActor("Kinect Actor #" + trackingId);
					m_Actors.Add(actor);
					actor.OnUpdateTrackingData(m_Manager, bodies[i], faces[i], frame);
					onCreateActor?.Invoke(actor);
					continue;
				}
				
				actor.OnUpdateTrackingData(m_Manager, bodies[i], faces[i], frame);
			}
			
			for (int i = lastActorCount - 1; i >= 0; --i)
			{
				var actor = m_Actors[i];
			
				// Check if the actor is too old (not updated in this frame).
				if (actor.updatedAtFrame < frame)
				{
					m_Actors.RemoveAt(i);
					onDestroyActor?.Invoke(actor);
					Destroy(actor.gameObject);
				}
			}
		}
		
		private KinectActor ConstructActor(string name)
		{
			KinectActor actor = null;

			if (constructionType == KinectActorConstructionType.Prefab)
			{
				actor = Instantiate<KinectActor>(prefab, transform);
				actor.name = name;
			} else {
				actor = new GameObject(name).AddComponent<KinectActor>();
				actor.transform.parent = transform;
				
				if (constructionType == KinectActorConstructionType.Full)
				{
					var skeleton = actor.gameObject.AddComponent<KinectSkeleton>();
					skeleton.CreateJointTree(JointType.SpineBase);
				}
			}
			
			return actor;
		}
		
		private KinectActor FindActorByTrackingId(ulong trackingId)
		{
			for (int i = 0; i < m_Actors.Count; ++i)
				if (trackingId == m_Actors[i].trackingId)
					return m_Actors[i];
			return null;
		}
	}
}
