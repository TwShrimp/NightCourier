using UnityEngine;

namespace NightCourier.Prototype.Vehicles.Sport
{
    /// <summary>Original, periodic EV motor voice: warm rotor body and restrained inverter harmonics.</summary>
    public static class SportMotorSound
    {
        public static AudioClip CreateLoop()
        {
            const int rate = 48000;
            var samples = new float[rate * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)rate;
                float phase = 2 * Mathf.PI * 70 * t;
                float rotor = .43f * Mathf.Sin(phase) + .23f * Mathf.Sin(2 * phase);
                float turbine = .14f * Mathf.Sin(4 * phase + .18f * Mathf.Sin(2 * Mathf.PI * 3 * t));
                float inverter = .065f * Mathf.Sin(8 * phase) + .025f * Mathf.Sin(12 * phase);
                float lowBody = .10f * Mathf.Sin(phase * .5f);
                samples[i] = (rotor + turbine + inverter + lowBody) * .58f;
            }
            var clip = AudioClip.Create("APEX E2 • twin-stage electric motor", samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
