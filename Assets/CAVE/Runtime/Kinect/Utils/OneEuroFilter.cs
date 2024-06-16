using System;
using UnityEngine;

namespace HTW.CAVE.Kinect.Utils
{
	public readonly struct OneEuroParams
	{
		public readonly float minCutoff;

		public readonly float beta;

		public readonly float derivateCutoff;

		public OneEuroParams(float minCutoff, float beta = 0f, float derivateCutoff = 1f)
		{
			this.minCutoff = minCutoff;
			this.beta = beta;
			this.derivateCutoff = derivateCutoff;
		}
	}

	public struct OneEuroFilter3
	{
		private OneEuroFilter m_X;

		private OneEuroFilter m_Y;

		private OneEuroFilter m_Z;

		public Vector3 Filter(Vector3 value, float rate, in OneEuroParams parameters) =>
			new Vector3(
				m_X.Filter(value.x, rate, in parameters),
				m_Y.Filter(value.y, rate, in parameters),
				m_Z.Filter(value.z, rate, in parameters));
	}

	public struct OneEuroFilter4
	{
		private OneEuroFilter m_X;

		private OneEuroFilter m_Y;

		private OneEuroFilter m_Z;

		private OneEuroFilter m_W;

		public Vector4 Filter(Vector4 value, float rate, in OneEuroParams parameters) =>
			new Vector4(
				m_X.Filter(value.x, rate, in parameters),
				m_Y.Filter(value.y, rate, in parameters),
				m_Z.Filter(value.z, rate, in parameters),
				m_W.Filter(value.w, rate, in parameters));

		public Quaternion Filter(Quaternion value, float rate, in OneEuroParams parameters) =>
			new Quaternion(
				m_X.Filter(value.x, rate, in parameters),
				m_Y.Filter(value.y, rate, in parameters),
				m_Z.Filter(value.z, rate, in parameters),
				m_W.Filter(value.w, rate, in parameters));
	}

	internal struct LowPassFilter
	{
		public float hatXPrev;

		public LowPassFilter(float hatXPrev)
		{
			this.hatXPrev = hatXPrev;
		}

		public float Filter(float x, float alpha) => hatXPrev = alpha * x + (1 - alpha) * hatXPrev;
	}

	public struct OneEuroFilter
	{
		private LowPassFilter m_XFilter;

		private LowPassFilter m_DxFilter;

		private bool m_Initialized;

		public float Filter(float x, float rate, in OneEuroParams parameters)
		{
			if (!m_Initialized)
			{
				m_DxFilter = new LowPassFilter(0f);
				m_XFilter = new LowPassFilter(x);
				return x;
			}

			m_Initialized = true;

			float dx = (x - m_XFilter.hatXPrev) * rate;
			float edx = m_DxFilter.Filter(dx, Alpha(rate, parameters.derivateCutoff));
			float cutoff = parameters.minCutoff + parameters.beta * Math.Abs(edx);

			return m_XFilter.Filter(x, Alpha(rate, cutoff));
		}

		public static float Freq(float lastTime, float currentTime) => 1f / (currentTime - lastTime);

		private static float Alpha(float rate, float cutoff)
		{
			float te = 1f / rate;
			float tau = 1f / (2f * Mathf.PI * cutoff);
			return 1f / (1f + tau / te);
		}
	}
}
