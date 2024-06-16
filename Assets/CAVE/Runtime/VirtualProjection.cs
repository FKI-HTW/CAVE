using System;
using UnityEngine;

namespace HTW.CAVE
{
	public static class VirtualProjection
	{
		public static Matrix4x4 ComputeBimberMatrix(Vector2 bl, Vector3 tl, Vector3 tr, Vector3 br)
		{
			int rows = 8, cols = rows + 2;
			
			float[][] m = new float[rows][];
			
			for (int i = 0; i < rows; ++i)
				m[i] = new float[cols];

			m[0][0] =  1f; m[0][1] =  1f; m[0][2] = -1f; m[0][6] = -bl.x; m[0][7] = -bl.x; // "m[0][8] = bl.x" goes to b, 3 ... 5 zero
			m[1][3] =  1f; m[1][4] =  1f; m[1][5] = -1f; m[1][6] = -bl.y; m[1][7] = -bl.y; // "m[1][8] = bl.y" goes to b, 0 ... 2 zero
			m[2][0] =  1f; m[2][1] = -1f; m[2][2] = -1f; m[2][6] = -tl.x; m[2][7] =  tl.x; // "m[2][8] = tl.x" goes to b, 3 ... 5 zero
			m[3][3] =  1f; m[3][4] = -1f; m[3][5] = -1f; m[3][6] = -tl.y; m[3][7] =  tl.y; // "m[3][8] = tl.y" goes to b, 0 ... 2 zero
			m[4][0] = -1f; m[4][1] = -1f; m[4][2] = -1f; m[4][6] =  tr.x; m[4][7] =  tr.x; // "m[4][8] = tr.x" goes to b, 3 ... 5 zero
			m[5][3] = -1f; m[5][4] = -1f; m[5][5] = -1f; m[5][6] =  tr.y; m[5][7] =  tr.y; // "m[5][8] = tr.y" goes to b, 0 ... 2 zero
			m[6][0] = -1f; m[6][1] =  1f; m[6][2] = -1f; m[6][6] =  br.x; m[6][7] = -br.x; // "m[6][8] = br.x" goes to b, 3 ... 5 zero
			m[7][3] = -1f; m[7][4] =  1f; m[7][5] = -1f; m[7][6] =  br.y; m[7][7] = -br.y; // "m[7][8] = br.y" goes to b, 0 ... 2 zero

			m[0][8] = -bl.x; m[1][8] = -bl.y; m[2][8] = -tl.x; m[3][8] = -tl.y; m[4][8] = -tr.x; m[5][8] = -tr.y; m[6][8] = -br.x; m[7][8] = -br.y;

			if(Solve(m))
			{
				Matrix4x4 mat = Matrix4x4.zero;

				mat[0, 0] = m[0][9]; mat[0, 1] = m[1][9]; mat[0, 3] = m[2][9];
				mat[1, 0] = m[3][9]; mat[1, 1] = m[4][9]; mat[1, 3] = m[5][9];
				mat[3, 0] = m[6][9]; mat[3, 1] = m[7][9]; mat[3, 3] = 1f;
				mat[2, 2] = 1f - Mathf.Abs(mat[3, 0]) - Mathf.Abs(mat[3, 1]);  // to avoid z overflow (see Bimber paper)
				
				return mat;
			}
			
			return Matrix4x4.identity;
		}
		
		internal static bool Solve(float[][] system)
		{
			// Solving m * x = c;  system must have two extra columns for vector c and for result, 
			//	| A1  B1...  N1 C1 0 |
			//  | A2  B2...  N2 C2 0 |
			//  |      ...         0 |
			//  | Am  Bm...  Nm Cm 0 |
			const float epsilon = 0.00001f;
			
			// Start solving.
			for (int r = 0; r < system.Length - 1; r++)
			{
				// Zero out all entries in column r after this row.
				// See if this row has a non-zero entry in column r.
				if (Math.Abs(system[r][r]) <= epsilon)
				{
					// Too close to zero. Try to swap with a later row.
					for (int r2 = r + 1; r2 < system.Length; r2++)
					{
						if (Math.Abs(system[r2][r]) > epsilon)
						{
							// This row will work. Swap them.
							for(int c = 0; c <= system.Length; c++)
							{
								(system[r][c], system[r2][c]) = (system[r2][c], system[r][c]);
							}
							
							break;
						}
					}
				}

				// If this row has a non-zero entry in column r, use it.
				if (Math.Abs(system[r][r]) > epsilon)
				{
					// Zero out this column in later rows.
					for (int r2 = r + 1; r2 < system.Length; r2++)
					{
						float factor = -system[r2][r] / system[r][r];
						for (int c = r; c <= system.Length; c++)
						{
							system[r2][c] = system[r2][c] + factor * system[r][c];
						}
					}
				}
			}

			// Is there a solution?
			if (Mathf.Approximately(system[system.Length - 1][system.Length - 1], 0f))
				return false;
			
			// Backsolve.
			for (int r = system.Length - 1; r >= 0; r--)
			{
				float tmp = system[r][system.Length];
				for (int r2 = r + 1; r2 < system.Length; r2++)
					tmp -= system[r][r2] * system[r2][system.Length + 1];
					
				system[r][system.Length + 1] = tmp / system[r][r];
			}

			return true;
		}
	
		public static Matrix4x4 ComputeHolographicProjectionMatrix(Vector3 local,
			float nearClipPlane, float farClipPlane, float width, float height)
		{
			float l = -local.x - 0.5f * width;
			float r = -local.x + 0.5f * width;
			float b = -local.y - 0.5f * height;
			float t = -local.y + 0.5f * height;
			
			// Fix for avoiding that all terms based on the near distance become zero.
			// Otherwise use the negative z position because the POV is in front of the screen.
			float near = Mathf.Approximately(local.z, 0f) ? 0.01f : -local.z;
			
			Matrix4x4 mat = Matrix4x4.zero;
			
			// See perspective off-center:
			// https://docs.unity3d.com/ScriptReference/Camera-projectionMatrix.html

			mat[0, 0] = (2f * near) / (r - l);
			mat[1, 1] = (2f * near) / (t - b);
			mat[0, 2] = (r + l)     / (r - l);
			mat[1, 2] = (t + b)     / (t - b);
			mat[2, 2] = -(farClipPlane + nearClipPlane * near) / (farClipPlane - nearClipPlane * near);
			mat[3, 2] = -1f;
			mat[2, 3] = -(2f * farClipPlane * nearClipPlane * near) / (farClipPlane - nearClipPlane * near);
			
			return mat;
		}

		[Obsolete("This is a direct port of the old projection and should not be used anymore.")]
		public static Matrix4x4 ComputeHolographicProjectionMatrix(Vector3 bl, Vector3 br, Vector3 tl, Vector3 tr, Vector3 eye, Transform screen, float farClipPlane)
		{
			var vr = (br - bl).normalized;             // Right axis.
			var vu = (tl - bl).normalized;             // Up axis.
			var vn = Vector3.Cross(vr, vu).normalized; // Normal vector (forward axis).

			var va = bl - eye;
			var vb = br - eye;
			var vc = tl - eye;
			var vd = tr - eye;
		
			var n = -screen.InverseTransformPoint(eye).z;
			var d = Vector3.Dot(va, vn);
			var nd = n / d;
			var l = Vector3.Dot( vr, va ) * nd;
			var r = Vector3.Dot( vr, vb ) * nd;
			var b = Vector3.Dot( vu, va ) * nd;
			var t = Vector3.Dot( vu, vc ) * nd;
		 
		 	Matrix4x4 mat = new Matrix4x4();
			mat[0, 0] = 2f * n  / (r - l);
			mat[1, 1] = 2f * n  / (t - b);
			mat[0, 2] = (r + l) / (r - l);
			mat[1, 2] = (t + b) / (t - b);
			mat[2, 2] = (farClipPlane + n) / (n - farClipPlane);
			mat[3, 2] = -1f;
			mat[2, 3] = 2f * farClipPlane * n / (n - farClipPlane);

			return mat;
		}
	}
}
