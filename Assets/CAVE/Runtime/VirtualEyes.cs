using System;
using UnityEngine;

namespace HTW.CAVE
{
    /// <summary>
    /// Responsible for transforming stereoscopic eyes from local to world space.
    /// </summary>
    [AddComponentMenu("HTW/CAVE/Virtual Eyes")]
    public sealed class VirtualEyes : MonoBehaviour
    {

        /// <summary>
        /// Workaround for the kinect not correctly detecting the players rotation due to the 3D-glasses covering the users eyes.
        /// The patch will flip the virtual eye positions if an abnormal rotation is detected which *hopefully* prevents a breakdown of the stereoscopic view in most cases.
        /// If set to <c>true<c/>, rotational patch is enabled and users can only look in positive z directions.
        /// </summary>
        [Tooltip("Workaround for wrong kinect rotations.")]
        public bool forceForwardZ = true;

        /// <summary>
        /// Defines the eye of a stereo or mono output target.
        /// </summary>
        public enum StereoTarget
        {
            Mono,
            Left,
            Right
        }

        /// <summary>
        /// Returns the eye position based on the <see cref="StereoTarget"/>.
        /// </summary>
        /// <param name="stereoTarget">The target eye (stereo) or the eye anchor (mono).</param>
        /// <param name="seperation">The distance between the eyes (ignored when using <see cref="StereoTarget.Mono"/>).</param>
        /// <returns>The eye position in world space.</returns>
        public Vector3 GetPosition(StereoTarget stereoTarget, float seperation = 0f)
        {
            float seperationMultiplier = seperation * 0.5f;

            // This is a "patch" for the kinect detecting abnormal rotations when having the 3D glasses on.
            if (forceForwardZ)
            {
                var euler = transform.rotation.eulerAngles;
                var angleYAbs = Math.Abs(euler.y);
                seperationMultiplier *= (angleYAbs < 90 || angleYAbs > 270) ? 1 : -1;
            }

            switch (stereoTarget)
            {
                case StereoTarget.Mono:
                    return transform.position;
                case StereoTarget.Left:
                    return transform.TransformPoint(Vector3.right * -seperationMultiplier);
                case StereoTarget.Right:
                    return transform.TransformPoint(Vector3.right * seperationMultiplier);
            }

            return Vector3.zero;
        }
    }
}
