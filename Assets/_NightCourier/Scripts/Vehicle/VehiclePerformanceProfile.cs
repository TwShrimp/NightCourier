using UnityEngine;

namespace NightCourier.Vehicle
{
    /// <summary>Immutable tuning values applied when the player changes vehicle.</summary>
    public readonly struct VehiclePerformanceProfile
    {
        public readonly string Name;
        public readonly float TargetTopSpeedKph;
        public readonly float Mass;
        public readonly Vector3 CenterOfMass;
        public readonly float PeakForce;
        public readonly float PeakPower;
        public readonly float DriveCutoffSpeed;
        public readonly float RollingResistance;
        public readonly float AerodynamicDrag;
        public readonly float LowSpeedSteer;
        public readonly float HighSpeedSteer;
        public readonly float LowSpeedSteerRate;
        public readonly float HighSpeedSteerRate;
        public readonly float ForwardGrip;
        public readonly float SideGrip;
        public readonly float Stability;
        public readonly float FrontAxleZ;
        public readonly float RearAxleZ;
        public readonly float TrackHalfWidth;
        public readonly float WheelRadius;
        public readonly float WheelAnchorY;
        public readonly float SuspensionDistance;
        public readonly bool TwoSpeed;

        public VehiclePerformanceProfile(string name, float targetTopSpeedKph, float mass, Vector3 centerOfMass,
            float peakForce, float peakPower, float driveCutoffSpeed, float rollingResistance, float aerodynamicDrag,
            float lowSpeedSteer, float highSpeedSteer, float lowSpeedSteerRate, float highSpeedSteerRate,
            float forwardGrip, float sideGrip, float stability,
            float frontAxleZ, float rearAxleZ, float trackHalfWidth, float wheelRadius,
            float wheelAnchorY, float suspensionDistance, bool twoSpeed = false)
        {
            Name = name;
            TargetTopSpeedKph = targetTopSpeedKph;
            Mass = mass;
            CenterOfMass = centerOfMass;
            PeakForce = peakForce;
            PeakPower = peakPower;
            DriveCutoffSpeed = driveCutoffSpeed;
            RollingResistance = rollingResistance;
            AerodynamicDrag = aerodynamicDrag;
            LowSpeedSteer = lowSpeedSteer;
            HighSpeedSteer = highSpeedSteer;
            LowSpeedSteerRate = lowSpeedSteerRate;
            HighSpeedSteerRate = highSpeedSteerRate;
            ForwardGrip = forwardGrip;
            SideGrip = sideGrip;
            Stability = stability;
            FrontAxleZ = frontAxleZ;
            RearAxleZ = rearAxleZ;
            TrackHalfWidth = trackHalfWidth;
            WheelRadius = wheelRadius;
            WheelAnchorY = wheelAnchorY;
            SuspensionDistance = suspensionDistance;
            TwoSpeed = twoSpeed;
        }
    }

    public static class VehiclePerformanceProfiles
    {
        public static readonly VehiclePerformanceProfile Pickup = new VehiclePerformanceProfile(
            "Electric pickup", 200, 1850, new Vector3(0, -.32f, .05f),
            14500, 300000, 62, 180, .78f, 26, 16.5f, 52, 27, 1.8f, 2.1f, 3.15f,
            1.57f, -1.55f, 1.01f, .48f, -.18f, .32f);

        public static readonly VehiclePerformanceProfile Sport = new VehiclePerformanceProfile(
            "Mid-engine electric sport", 320, 1640, new Vector3(0, -.28f, -.16f),
            19000, 650000, 110, 145, .52f, 25, 8.5f, 48, 17, 2.2f, 2.6f, 4.1f,
            1.8f, -1.65f, .96f, .39f, .02f, .18f, true);

        public static readonly VehiclePerformanceProfile Van = new VehiclePerformanceProfile(
            "Courier van", 160, 2380, new Vector3(0, -.22f, -.08f),
            12800, 235000, 52, 230, 1.02f, 29, 18, 48, 24, 1.95f, 2.2f, 3.45f,
            1.65f, -1.75f, 1.01f, .47f, -.18f, .3f);
    }
}
