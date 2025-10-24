#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Modules.Additional.DynamicBackground.Scripts
{
    [ExecuteInEditMode]
    public class DynamicParticleController : MonoBehaviour
    {
        public new ParticleSystem particleSystem;
        [Range(0, 1)] public float parameter = 0.5f;
        private float _previousParameter;

        private void OnEnable()
        {
            _previousParameter = parameter;
#if UNITY_EDITOR
            EditorApplication.update += EditorUpdate;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }

#if UNITY_EDITOR
        private void EditorUpdate()
        {
            if (Mathf.Approximately(parameter, _previousParameter)) return;
            UpdateParticleSystem(parameter);
            _previousParameter = parameter;
        }
#endif

        private void UpdateParticleSystem(float param)
        {
            var colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.Lerp(Color.white, Color.red, param), 
                    0.0f), new GradientColorKey(Color.Lerp(Color.clear, Color.black, param), 1.0f) },
                new[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            var emission = particleSystem.emission;
            emission.rateOverTime = Mathf.Lerp(10, 100, param);

            var noise = particleSystem.noise;
            noise.enabled = true;
            noise.strength = Mathf.Lerp(1, 5, param);
            noise.frequency = Mathf.Lerp(0.1f, 2.0f, param);
        }
    }
}