using UnityEngine;

namespace HTW.CAVE.Samples.CAVETools
{
    public class CubeSpawnState : MonoBehaviour
    {
        public static bool Enabled = false;

        private void OnEnable()
        {
            Enabled = true;
        }

        private void OnDisable()
        {
            Enabled = false;
        }
    }
}
