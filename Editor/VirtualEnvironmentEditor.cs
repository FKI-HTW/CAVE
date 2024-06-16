using System;
using UnityEngine;
using UnityEditor;
using HTW.CAVE.Kinect;

namespace HTW.CAVE
{
	[CustomEditor(typeof(VirtualEnvironment))]
	public class VirtualEnvironmentEditor : Editor
	{
		[MenuItem("GameObject/Virtual Environment/Mono", false, 10)]
		public static void CreateVirtualEnvironmentMono(MenuCommand command) =>
			CreateVirtualEnvironment(command, false);
		
		[MenuItem("GameObject/Virtual Environment/Stereo", false, 10)]
		public static void CreateVirtualEnvironmentStereo(MenuCommand command) =>
			CreateVirtualEnvironment(command, true);

		private static void CreateVirtualEnvironment(MenuCommand command, bool stereo)
		{
			VirtualEnvironment environment = null;

			environment = VirtualUtility.CreateEnvironment(VirtualEnvironment.GetDefaultDimensions(), stereo,
				Vector3.forward * 0.5f * VirtualEnvironment.GetDefaultDimensions().z);
			
			GameObjectUtility.SetParentAndAlign(environment.gameObject, command.context as GameObject);
			Undo.RegisterCreatedObjectUndo(environment, "Create " + environment.name);
			Selection.activeObject = environment;
		}
	
		[DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy)]
		public static void DrawDimensionsGizmos(VirtualEnvironment environment, GizmoType type)
		{
			Gizmos.color = Color.yellow;
			
			var transform = environment.transform;
			var dimensions = environment.dimensions;
			
			var ba = transform.TransformPoint(new Vector3(dimensions.x * -0.5f, 0f, dimensions.z * 0.5f));
			var bb = transform.TransformPoint(new Vector3(dimensions.x * 0.5f, 0f, dimensions.z * 0.5f));
			var bc = transform.TransformPoint(new Vector3(dimensions.x * -0.5f, 0f, dimensions.z * -0.5f));
			var bd = transform.TransformPoint(new Vector3(dimensions.x * 0.5f, 0f, dimensions.z * -0.5f));
			var ta = transform.TransformPoint(new Vector3(dimensions.x * -0.5f, dimensions.y, dimensions.z * 0.5f));
			var tb = transform.TransformPoint(new Vector3(dimensions.x * 0.5f, dimensions.y, dimensions.z * 0.5f));
			var tc = transform.TransformPoint(new Vector3(dimensions.x * -0.5f, dimensions.y, dimensions.z * -0.5f));
			var td = transform.TransformPoint(new Vector3(dimensions.x * 0.5f, dimensions.y, dimensions.z * -0.5f));
			
			Gizmos.DrawRay(ba, Vector3.ClampMagnitude(bb - ba, 0.4f));
			Gizmos.DrawRay(ba, Vector3.ClampMagnitude(bc - ba, 0.4f));
			Gizmos.DrawRay(ba, Vector3.ClampMagnitude(ta - ba, 0.4f));
			
			Gizmos.DrawRay(bb, Vector3.ClampMagnitude(ba - bb, 0.4f));
			Gizmos.DrawRay(bb, Vector3.ClampMagnitude(bd - bb, 0.4f));
			Gizmos.DrawRay(bb, Vector3.ClampMagnitude(tb - bb, 0.4f));
			
			Gizmos.DrawRay(bc, Vector3.ClampMagnitude(bd - bc, 0.4f));
			Gizmos.DrawRay(bc, Vector3.ClampMagnitude(ba - bc, 0.4f));
			Gizmos.DrawRay(bc, Vector3.ClampMagnitude(tc - bc, 0.4f));
			
			Gizmos.DrawRay(bd, Vector3.ClampMagnitude(bc - bd, 0.4f));
			Gizmos.DrawRay(bd, Vector3.ClampMagnitude(bb - bd, 0.4f));
			Gizmos.DrawRay(bd, Vector3.ClampMagnitude(td - bd, 0.4f));
			
			Gizmos.DrawRay(ta, Vector3.ClampMagnitude(tb - ta, 0.4f));
			Gizmos.DrawRay(ta, Vector3.ClampMagnitude(tc - ta, 0.4f));
			Gizmos.DrawRay(ta, Vector3.ClampMagnitude(ba - ta, 0.4f));
			
			Gizmos.DrawRay(tb, Vector3.ClampMagnitude(ta - tb, 0.4f));
			Gizmos.DrawRay(tb, Vector3.ClampMagnitude(td - tb, 0.4f));
			Gizmos.DrawRay(tb, Vector3.ClampMagnitude(bb - tb, 0.4f));
			
			Gizmos.DrawRay(tc, Vector3.ClampMagnitude(td - tc, 0.4f));
			Gizmos.DrawRay(tc, Vector3.ClampMagnitude(ta - tc, 0.4f));
			Gizmos.DrawRay(tc, Vector3.ClampMagnitude(bc - tc, 0.4f));
			
			Gizmos.DrawRay(td, Vector3.ClampMagnitude(tc - td, 0.4f));
			Gizmos.DrawRay(td, Vector3.ClampMagnitude(tb - td, 0.4f));
			Gizmos.DrawRay(td, Vector3.ClampMagnitude(bd - td, 0.4f));
		}
	}
}