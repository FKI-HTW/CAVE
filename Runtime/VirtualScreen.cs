using UnityEngine;

namespace HTW.CAVE
{
	public class VirtualScreen : MonoBehaviour
	{
		/// <summary>
		/// Defines the kind of the screen orientation inside
		/// an cubic environment.
		/// </summary>
		public enum Kind
		{
			Front,
			Back,
			Left,
			Right,
			Top,
			Bottom
		}
	
		/// <summary>
		/// The screen orientation kind.
		/// </summary>
		public Kind kind;
	
		/// <summary>
		/// Defines the width of the screen.
		/// </summary>
		public float width;
		
		/// <summary>
		/// Defines the height of the screen.
		/// </summary>
		public float height;

		/// <summary>
		/// The bottom left corner of the screen.
		/// </summary>
		public Vector3 bottomLeft => transform.TransformPoint(new Vector3(-width * 0.5f, -height * 0.5f, 0f));

		/// <summary>
		/// The bottom right corner of the screen.
		/// </summary>
		public Vector3 bottomRight => transform.TransformPoint(new Vector3(width * 0.5f, -height * 0.5f, 0f));

		/// <summary>
		/// The top left corner of the screen.
		/// </summary>
		public Vector3 topLeft => transform.TransformPoint(new Vector3(-width * 0.5f, height * 0.5f, 0f));

		/// <summary>
		/// The top right corner of the screen.
		/// </summary>
		public Vector3 topRight => transform.TransformPoint(new Vector3(width * 0.5f, height * 0.5f, 0f));

		/// <summary>
		/// Constructs and returns the plane using the screen corners.
		/// </summary>
		/// <returns>The screen plane.</returns>
		public Plane GetPlane() => new Plane(topLeft, topRight, bottomRight);
		
		internal static Vector3[] positions = {
			new Vector3(   0f, 0.5f,  0.5f), // Front
			new Vector3(   0f, 0.5f, -0.5f), // Back
			new Vector3(-0.5f, 0.5f,    0f), // Left
			new Vector3( 0.5f, 0.5f,    0f), // Right
			new Vector3(   0f,   1f,    0f), // Top
			new Vector3(   0f,   0f,    0f)  // Bottom
		};
		
		public static Vector3 GetLocalPosition(Kind kind, Vector3 dimensions) 
		{
			var position = positions[(int)kind];
			return new Vector3(
				position.x * dimensions.x,
				position.y * dimensions.y,
				position.z * dimensions.z);
		}

		internal static Quaternion[] rotations = {
			Quaternion.identity,              // Front
			Quaternion.Euler(  0f, 180f, 0f), // Back
			Quaternion.Euler(  0f, -90f, 0f), // Left
			Quaternion.Euler(  0f,  90f, 0f), // Right
			Quaternion.Euler(-90f,   0f, 0f), // Top
			Quaternion.Euler( 90f,   0f, 0f)  // Bottom
		};

		public static Quaternion GetLocalRotation(Kind kind) => rotations[(int)kind];
	}
}
