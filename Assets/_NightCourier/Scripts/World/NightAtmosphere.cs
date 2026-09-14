using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NightCourier.World
{
    public sealed class NightAtmosphere : MonoBehaviour
    {
        private VolumeProfile profile;
        private Material rainMaterial;
        private Transform rain;
        private Transform target;
        private ReflectionProbe reflections;

        public void Initialize(Camera camera, Transform car)
        {
            target = car;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.18f, 0.24f, 0.34f);
            RenderSettings.ambientEquatorColor = new Color(0.09f, 0.13f, 0.20f);
            RenderSettings.ambientGroundColor = new Color(0.05f, 0.07f, 0.1f);
            var ambient = new SphericalHarmonicsL2();
            ambient.AddAmbientLight(new Color(0.16f, 0.21f, 0.29f));
            RenderSettings.ambientProbe = ambient;
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.04f, 0.075f, 0.12f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0035f;
            camera.backgroundColor = RenderSettings.fogColor;
            camera.allowHDR = true;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            var volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.sharedProfile = profile;
            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(1.1f);
            bloom.intensity.Override(0.35f);
            bloom.scatter.Override(0.65f);
            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);
            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.65f);
            color.contrast.Override(12);
            color.saturation.Override(-8);
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.22f);
            vignette.smoothness.Override(0.5f);
            var moon = new GameObject("Blue moonlight", typeof(Light));
            moon.transform.SetParent(transform, false);
            moon.transform.rotation = Quaternion.Euler(48, -35, 0);
            var light = moon.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.58f, 0.73f, 1);
            light.intensity = 0.8f;
            light.shadows = LightShadows.Soft;
            var fillObject = new GameObject("Soft night fill", typeof(Light));
            fillObject.transform.SetParent(transform, false);
            fillObject.transform.rotation = Quaternion.Euler(28, 145, 0);
            var fill = fillObject.GetComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.42f, 0.58f, 0.8f);
            fill.intensity = 0.3f;
            fill.shadows = LightShadows.None;
            var probeObject = new GameObject("Wet street reflection probe", typeof(ReflectionProbe));
            probeObject.transform.SetParent(transform, false);
            probeObject.transform.position = new Vector3(-70, 6, 0);
            reflections = probeObject.GetComponent<ReflectionProbe>();
            reflections.mode = ReflectionProbeMode.Realtime;
            reflections.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            reflections.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
            reflections.resolution = 128;
            reflections.size = new Vector3(350, 100, 370);
            reflections.center = Vector3.zero;
            reflections.boxProjection = true;
            reflections.clearFlags = ReflectionProbeClearFlags.SolidColor;
            reflections.backgroundColor = RenderSettings.fogColor;
            reflections.cullingMask = Physics.DefaultRaycastLayers;
            reflections.RenderProbe();
            CreateRain();
        }

        private void CreateRain()
        {
            var item = new GameObject("Local rain", typeof(ParticleSystem));
            rain = item.transform;
            rain.SetParent(transform, false);
            rain.position = target.position + Vector3.up * 17;
            rain.rotation = Quaternion.Euler(90, 0, 0);
            var particles = item.GetComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.loop = true;
            main.startLifetime = 0.85f;
            main.startSpeed = 24;
            main.startSize = 0.008f;
            main.startColor = new Color(0.5f, 0.7f, 0.85f, 0.28f);
            main.maxParticles = 2500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission;
            emission.rateOverTime = 1600;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(34, 34, 1);
            rainMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rainMaterial.SetColor("_BaseColor", Color.white);
            rainMaterial.SetFloat("_Surface", 1);
            rainMaterial.SetFloat("_Blend", 0);
            rainMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            rainMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            rainMaterial.SetFloat("_ZWrite", 0);
            rainMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            rainMaterial.renderQueue = (int)RenderQueue.Transparent;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = rainMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 2;
            renderer.velocityScale = 0.014f;
            particles.Play();
        }

        private void LateUpdate()
        {
            if (rain != null && target != null) rain.position = target.position + Vector3.up * 17;
        }

        private void OnDestroy()
        {
            if (profile != null)
            {
                foreach (var component in profile.components) Destroy(component);
                Destroy(profile);
            }
            if (rainMaterial != null) Destroy(rainMaterial);
        }
    }
}
