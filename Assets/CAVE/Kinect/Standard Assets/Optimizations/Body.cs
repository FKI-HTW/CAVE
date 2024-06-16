using System;
using System.Collections.Generic;

namespace Windows.Kinect
{
	/// <summary>
	/// Optimized methods for the Body properties.
	/// The methods are build around reusing buffers and caching
	/// instead of costly allocations and native calls.
	/// </summary>
	public sealed partial class Body
	{
		private IntPtr _pNativeCache;

		private ulong _trackingIdCache;

		private bool _isTrackedCache;

		public ulong GetTrackingIdFast()
		{
			if(_pNativeCache != _pNative)
			{
				if(_pNative == IntPtr.Zero)
					throw new ObjectDisposedException("Body");

				_trackingIdCache = TrackingId;
				_isTrackedCache = IsTracked;
			}

			return _trackingIdCache;
		}

		public bool GetIsTrackedFast()
		{
			if(_pNativeCache != _pNative)
			{
				if(_pNative == IntPtr.Zero)
					throw new ObjectDisposedException("Body");

				_trackingIdCache = TrackingId;
				_isTrackedCache = IsTracked;
			}

			return _isTrackedCache;
		}

		public void RefreshJointsFast(Dictionary<JointType, Joint> joints)
		{
			if(_pNative == IntPtr.Zero)
				throw new ObjectDisposedException("Body");
			
			int length = Windows_Kinect_Body_get_Joints_Length(_pNative);
			var keys = new Windows.Kinect.JointType[length];
			var values = new Windows.Kinect.Joint[length];

			length = Windows_Kinect_Body_get_Joints(_pNative, keys, values, length);

			joints.Clear();

			for(int i = 0; i < length; i++)
				joints.Add(keys[i], values[i]);
		}

		public void RefreshJointOrientationsFast(Dictionary<JointType, JointOrientation> jointOrientations)
		{
			if(_pNative == IntPtr.Zero)
				throw new ObjectDisposedException("Body");
			
			int length = Windows_Kinect_Body_get_JointOrientations_Length(_pNative);
			var keys = new Windows.Kinect.JointType[length];
			var values = new Windows.Kinect.JointOrientation[length];

			length = Windows_Kinect_Body_get_JointOrientations(_pNative, keys, values, length);

			jointOrientations.Clear();

			for(int i = 0; i < length; i++)
				jointOrientations.Add(keys[i], values[i]);
		}
	}
}
