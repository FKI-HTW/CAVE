using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HTW.CAVE.Samples.CAVETools
{
    public class DistanceCubeMover : MonoBehaviour
    {
        public int ShowTime = 2;
        public int DelayTime = 3;
        public float MinDistance = 1;
        public float MaxDistance = 8;

        public GameObject cube;
        public GameObject text;
        public AudioSource audioSource;
        public List<Material> cubeMaterials;
        public List<Renderer> cubeRenderers;

        private Vector3 _defaultCubePosition;
        private double _lastUpdateTime = 0;

        private double _lastValue = 0;
        bool _delayPhase;

        void Start()
        {
            _defaultCubePosition = cube.gameObject.transform.position;
        }

        public void FixedUpdate()
        {
            if (_delayPhase)
            {
                if (Time.time - _lastUpdateTime >= DelayTime)
                {
                    _lastUpdateTime = Time.time;
                    _delayPhase = false;
                    UpdateCubePosition();
                }
                return;
            }

            if (Time.time - _lastUpdateTime >= ShowTime)
            {
                _lastUpdateTime = Time.time;
                cube.SetActive(false);
                _delayPhase = true;
            }
        }

        private void OnDisable()
        {
            ResetCubePosition();
        }

        private void UpdateCubePosition()
        {
            ResetCubePosition();
            double dist;
            // don't use the same distance twice in a row
            do
            {
                dist = GetRandomDistance();
            } while (dist == _lastValue);

            cube.transform.position = _defaultCubePosition + new Vector3(0, 0, (float)dist);
            text.GetComponent<TextMesh>().text = dist.ToString();
            float rdmScale = (float)(Random.Range(0.5f, 1.5f) * dist); // Scale with the distance + a small random to eleminate size based estimations
            // cube.transform.localScale = new Vector3(rdmScale, rdmScale, rdmScale);
            AssignRandomMaterial();
            audioSource.Play();
            _lastValue = dist;
        }

        private void ResetCubePosition()
        {
            cube.SetActive(true);
            cube.transform.localScale = Vector3.one;
            cube.transform.position = _defaultCubePosition;
        }

        private double GetRandomDistance()
        {
            // Exponential distances from MinDistance to MaxDistance
            var maxExp = (int)Math.Log(MaxDistance, 2) + 2; // Reverse of the function below to compute the max exp that is still inside given MaxDistance
            var exp = Random.Range(1, maxExp + 1); // Add one because Range max is exclusive
            var rdm = Math.Pow(2, exp) * MinDistance / 2; // Basic exponential function
            return rdm;
        }

        private void AssignRandomMaterial()
        {
            int index = Random.Range(0, cubeMaterials.Count);
            foreach (var renderer in cubeRenderers)
                renderer.material = cubeMaterials[index];
        }
    }
}
