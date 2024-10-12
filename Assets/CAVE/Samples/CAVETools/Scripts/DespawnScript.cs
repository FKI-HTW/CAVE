using UnityEngine;

namespace HTW.CAVE.Samples.CAVETools
{
    public class DespawnScript : MonoBehaviour
    {

        public int maxTicksLive;

        private int ticksLived = 0;

        private void FixedUpdate()
        {
            if (ticksLived++ > maxTicksLive)
            {
                Destroy(gameObject);
            }
        }
    }
}
