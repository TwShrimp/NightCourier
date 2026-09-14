using UnityEngine;

namespace NightCourier.Vehicle
{
    /// <summary>Two physical torque ratios with a short torque handover and downshift hysteresis.</summary>
    public sealed class TwoSpeedTransmission
    {
        public const float UpshiftKph = 110;
        public const float DownshiftKph = 75;
        public const float ShiftDuration = .28f;
        public int Gear { get; private set; } = 1;
        public float ShiftRemaining { get; private set; }
        public bool IsShifting => ShiftRemaining > 0;
        public float Ratio => Gear == 1 ? 14.8f : 8.2f;
        public float TorqueFactor => IsShifting
            ? 1 - .62f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(ShiftRemaining / ShiftDuration)) : 1;

        public void Reset() { Gear = 1; ShiftRemaining = 0; }

        public void Step(float forwardKph, float dt)
        {
            ShiftRemaining = Mathf.Max(0, ShiftRemaining - Mathf.Max(0, dt));
            if (forwardKph < 1) { Reset(); return; }
            if (IsShifting) return;
            if ((Gear == 1 && forwardKph >= UpshiftKph) || (Gear == 2 && forwardKph < DownshiftKph))
            {
                Gear = Gear == 1 ? 2 : 1;
                ShiftRemaining = ShiftDuration;
            }
        }

        public float MotorRpm(float forwardSpeed, float radius) =>
            Mathf.Abs(forwardSpeed) / (2 * Mathf.PI * radius) * 60 * Ratio;
    }
}
