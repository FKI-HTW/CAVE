using Windows.Kinect;

namespace HTW.CAVE.Kinect.Utils
{
	/// <summary>
	/// Contains predefined <see cref="Windows.Kinect.JointType"/> arrays
	/// for different body parts.
	/// </summary>
	public static class PredefinedJointTypes
	{
		public static readonly JointType[] head = { JointType.Head, JointType.Neck };

		public static readonly JointType[] torso = { JointType.SpineShoulder, JointType.SpineMid, JointType.SpineBase };

		public static readonly JointType[] legLeft = { JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft };

		public static readonly JointType[] legRight = { JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight };

		public static readonly JointType[] armLeft = { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft };

		public static readonly JointType[] armRight = { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight };

		public static readonly JointType[] handLeft = { JointType.HandLeft, JointType.ThumbLeft, JointType.HandTipLeft };
	
		public static readonly JointType[] handRight = { JointType.HandRight, JointType.ThumbRight, JointType.HandTipRight };
	}
}
