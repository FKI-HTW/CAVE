using UnityEngine;

namespace HTW.CAVE.Samples.CAVETools
{
    public class ObjectToggleButton : MonoBehaviour
    {
        public GameObject toggleObject;
        public Material onMaterial;
        public Material offMaterial;
        public double toggleCooldown = 1.0;

        private double _lastInteractionTimestamp;

        private void Start()
        {
            ApplyMaterial();
        }

        void OnTriggerEnter(Collider other)
        {
            if (Time.time - _lastInteractionTimestamp < toggleCooldown)
            {
                return;
            }
            _lastInteractionTimestamp = Time.time;
            toggleObject.SetActive(!toggleObject.activeSelf);
            ApplyMaterial();
        }

        private void ApplyMaterial()
        {
            GetComponent<Renderer>().material = toggleObject.activeSelf ? onMaterial : offMaterial;
        }
    }
}
