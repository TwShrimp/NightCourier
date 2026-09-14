using UnityEngine;

namespace NightCourier.Vehicle
{
    /// <summary>Creates a lightweight looping EV tone and shapes it from speed and accelerator load.</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class ElectricMotorAudio : MonoBehaviour
    {
        private ArcadeCarController controller;
        private CarInput input;
        private AudioSource source;
        private AudioClip motorLoop;
        private AudioClip sportLoop;
        private bool sportVoice;

        public float CurrentPitch => source == null ? 0 : source.pitch;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            motorLoop = CreateMotorLoop();
            sportLoop = NightCourier.Prototype.Vehicles.Sport.SportMotorSound.CreateLoop();
            source.clip = motorLoop;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0.32f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 2;
            source.maxDistance = 55;
            source.dopplerLevel = 0.25f;
            source.volume = 0.09f;
            source.pitch = 0.72f;
            source.Play();
        }

        public void Configure(ArcadeCarController vehicle, CarInput carInput)
        {
            controller = vehicle;
            input = carInput;
        }

        private void Update()
        {
            if (controller == null || source == null) return;
            bool sport = controller.HasTwoSpeedTransmission;
            if (sport != sportVoice)
            {
                sportVoice = sport;
                source.Stop();
                source.clip = sport ? sportLoop : motorLoop;
                source.pitch = .72f;
                source.volume = .03f;
                source.Play();
            }
            float speed = Mathf.InverseLerp(0, 190, controller.SpeedKph);
            float load = input == null ? 0 : Mathf.Abs(input.Throttle);
            float targetPitch = Mathf.Lerp(0.72f, 2.18f, Mathf.Pow(speed, 0.68f));
            float targetVolume = Mathf.Lerp(0.09f, 0.52f, Mathf.Clamp01(speed * 0.68f + load * 0.72f));
            if (sport)
            {
                float rpm = Mathf.Clamp01(controller.MotorRpm / 18000f);
                targetPitch = .62f + 2.15f * rpm;
                targetVolume = Mathf.Lerp(.035f, .48f, Mathf.Clamp01(rpm * .38f + load * .62f));
                targetVolume *= Mathf.Lerp(.55f, 1, controller.ShiftTorqueFactor);
            }
            source.pitch = Mathf.Lerp(source.pitch, targetPitch, 1 - Mathf.Exp(-(sport ? 12f : 4.5f) * Time.deltaTime));
            source.volume = Mathf.Lerp(source.volume, targetVolume, 1 - Mathf.Exp(-5.5f * Time.deltaTime));
        }

        private static AudioClip CreateMotorLoop()
        {
            const int sampleRate = 48000;
            var samples = new float[sampleRate];
            for (int i = 0; i < samples.Length; i++)
            {
                float phase = i / (float)sampleRate;
                float fundamental = Mathf.Sin(phase * Mathf.PI * 2 * 48) * 0.48f;
                float motorBody = Mathf.Sin(phase * Mathf.PI * 2 * 96) * 0.25f;
                float electricWhine = Mathf.Sin(phase * Mathf.PI * 2 * 210) * 0.13f;
                float inverterTone = Mathf.Sin(phase * Mathf.PI * 2 * 520) * 0.035f;
                float pulse = Mathf.Sin(phase * Mathf.PI * 2 * 72 + Mathf.Sin(phase * Mathf.PI * 2 * 5) * 0.2f) * 0.12f;
                samples[i] = (fundamental + motorBody + electricWhine + inverterTone + pulse) * 0.68f;
            }
            var clip = AudioClip.Create("Procedural electric motor loop", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (motorLoop != null) Destroy(motorLoop);
            if (sportLoop != null) Destroy(sportLoop);
        }
    }
}
