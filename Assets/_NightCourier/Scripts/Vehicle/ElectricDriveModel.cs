using UnityEngine;

namespace NightCourier.Vehicle
{
    /// <summary>Wheel force in N, speed in m/s. Power-limited drive meets quadratic road drag near 200 km/h.</summary>
    public static class ElectricDriveModel
    {
        public static float DriveForce(float speed, VehiclePerformanceProfile profile, int gear = 2)
        {
            float v = Mathf.Abs(speed);
            if (profile.TwoSpeed)
            {
                float wheelForce = profile.PeakForce * (gear == 1 ? 1 : 8.2f / 14.8f);
                float limit = profile.TargetTopSpeedKph / 3.6f;
                // A speed governor blends to road-load torque, rather than clamping velocity.
                float drive = Mathf.Min(wheelForce, profile.PeakPower / Mathf.Max(v, 1));
                float governor = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(limit - 2, limit, v));
                return Mathf.Lerp(drive, Resistance(v, profile), governor)
                    * Mathf.Clamp01(1 - Mathf.Max(0, v - limit) / 1.5f);
            }
            return Mathf.Min(profile.PeakForce, profile.PeakPower / Mathf.Max(v, 1))
                * Mathf.Clamp01(1 - Mathf.Pow(v / profile.DriveCutoffSpeed, 6));
        }

        public static float Resistance(float speed, VehiclePerformanceProfile profile) =>
            profile.RollingResistance + profile.AerodynamicDrag * speed * speed;
        /// <summary>Maximum front-wheel angle falls progressively from parking speed to motorway speed.</summary>
        public static float SteeringLimit(float speed, VehiclePerformanceProfile profile)
        {
            float normalizedSpeed = Mathf.InverseLerp(4, profile.DriveCutoffSpeed * .9f, Mathf.Abs(speed));
            return Mathf.Lerp(profile.LowSpeedSteer, profile.HighSpeedSteer, Mathf.SmoothStep(0, 1, normalizedSpeed));
        }

        /// <summary>High speed steering builds more slowly, preventing abrupt keyboard-input yaw.</summary>
        public static float SteeringResponse(float speed, VehiclePerformanceProfile profile)
        {
            float normalizedSpeed = Mathf.InverseLerp(5, profile.DriveCutoffSpeed * .9f, Mathf.Abs(speed));
            return Mathf.Lerp(profile.LowSpeedSteerRate, profile.HighSpeedSteerRate, Mathf.SmoothStep(0, 1, normalizedSpeed));
        }
    }
}
