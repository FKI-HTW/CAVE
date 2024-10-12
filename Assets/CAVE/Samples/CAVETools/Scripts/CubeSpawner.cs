using UnityEngine;

namespace HTW.CAVE.Samples.CAVETools
{
    public class CubeSpawner : MonoBehaviour
    {
        public Rigidbody cubePrefab;
        public int tickDelay;

        private int _delay = 0;

        private void FixedUpdate()
        {
            if (!CubeSpawnState.Enabled) return;

            if (_delay-- == 0)
            {
                SpawnCube();
                _delay = tickDelay;
            }
        }

        private void SpawnCube()
        {
            var cube = Instantiate(cubePrefab, gameObject.transform.position, gameObject.transform.rotation);
            cube.velocity = gameObject.transform.rotation * Vector3.forward;
        }
    }
}
