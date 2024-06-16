using System.Runtime.CompilerServices;
using UnityEngine;
using Windows.Kinect;

namespace HTW.CAVE.Kinect.Utils
{
	/// <summary>
	/// Converts Kinect 2.0 data to Unity.
	/// </summary>
	public static class KinectExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Vector4 ToUnityVector4(this Windows.Kinect.Vector4 vector) =>
			new(vector.X, vector.Y, vector.Z, vector.W);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Quaternion ToUnityQuaternion(this Windows.Kinect.Vector4 vector) =>
			new(vector.X, vector.Y, vector.Z, vector.W);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsZero(this Quaternion quaternion)
		{
			var sqrMagnitude = quaternion.x * quaternion.x
			                 + quaternion.y * quaternion.y
							 + quaternion.z * quaternion.z
							 + quaternion.w * quaternion.w;
		
			return Mathf.Approximately(sqrMagnitude, 0f);
		}	
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Vector2 GetLeanDirection(this Body body)
		{
			var lean = body.Lean; // Store it to avoid one additional native call.
			return new UnityEngine.Vector2((float)lean.X, (float)lean.Y);
		}

		public static string MakeHumanReadable(this JointType jointType)
		{
			string name = jointType.ToString();
			string result = "";
			int lastIndex = 0;

			for (int i = 1; i < name.Length; ++i)
			{
				if (char.IsUpper(name[i]))
				{
					result += name.Substring(lastIndex, i - lastIndex) + " ";
					lastIndex = i;
				}
			}

			if (lastIndex < name.Length - 1)
				result += name.Substring(lastIndex, name.Length - lastIndex);

			return result;
		}
	}
}
