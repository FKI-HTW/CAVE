using System;
using System.Collections.Generic;

namespace Windows.Kinect
{
	public sealed partial class BodyFrame
	{
		public void GetAndRefreshBodyData(Body[] bodies)
		{
			if(_pNative == IntPtr.Zero)
				throw new ObjectDisposedException("BodyFrame");

			var pBodies = new IntPtr[bodies.Length];

			for(int i = 0; i < bodies.Length; ++i)
			{
				if(bodies[i] == null)
					bodies[i] = new Body();
				
				pBodies[i] = bodies[i].GetIntPtr();
			}

			Windows_Kinect_BodyFrame_GetAndRefreshBodyData(_pNative, pBodies, bodies.Length);
			Helper.ExceptionHelper.CheckLastError();

			for(int i = 0; i < bodies.Length; ++i)
				bodies[i].SetIntPtr(pBodies[i]);
		}
	}
}
