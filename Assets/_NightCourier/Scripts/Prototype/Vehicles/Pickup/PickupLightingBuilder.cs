using NightCourier.World;
using UnityEngine;

namespace NightCourier.Prototype.Vehicles.Pickup
{
    internal static class PickupLightingBuilder
    {
        public static void Build(Transform root, CityPalette palette, VehicleLampRig rig)
        {
            const float frontZ = 2.91f, rearZ = -2.82f;
            rig.Running.Add(VehicleLampParts.Running(root, "Rear running light bar", new Vector3(0, .78f, rearZ),
                new Vector3(1.88f, .075f, .025f), palette));
            rig.Brake.Add(VehicleLampParts.Lamp(root, "Rear brake light bar", new Vector3(0, .78f, rearZ - .006f),
                new Vector3(1.88f, .075f, .035f), -1, palette.Red, palette));

            foreach (int side in new[] { -1, 1 })
            {
                var indicators = side < 0 ? rig.Left : rig.Right;
                var glows = side < 0 ? rig.LeftGlow : rig.RightGlow;
                float x = side * .66f;
                indicators.Add(VehicleLampParts.Lamp(root, side < 0 ? "Rear left indicator" : "Rear right indicator",
                    new Vector3(x, .59f, rearZ), new Vector3(.42f, .075f, .035f), -1, palette.Warm, palette));
                rig.Reverse.Add(VehicleLampParts.Lamp(root, side < 0 ? "Rear left reverse light" : "Rear right reverse light",
                    new Vector3(x, .43f, rearZ), new Vector3(.42f, .075f, .035f), -1, palette.Headlight, palette));
                indicators.Add(VehicleLampParts.Lamp(root, side < 0 ? "Front left indicator" : "Front right indicator",
                    new Vector3(side * .67f, .33f, frontZ), new Vector3(.4f, .05f, .035f), 1, palette.Warm, palette));
                glows.Add(VehicleLampParts.Point(root, side < 0 ? "Left indicator glow" : "Right indicator glow",
                    new Vector3(side * .67f, .49f, frontZ + .08f), new Color(1, .42f, .02f), 2.5f, 4));
                rig.ReverseGlow.Add(VehicleLampParts.Point(root, "Reverse glow", new Vector3(x, .43f, rearZ - .1f),
                    new Color(.75f, .9f, 1), 3, 5));
            }
            rig.Head.Add(VehicleLampParts.Headlamp(root, "Front daytime light bar", new Vector3(0, .43f, frontZ),
                new Vector3(1.9f, .055f, .035f), palette));
            rig.BrakeGlow.Add(VehicleLampParts.Point(root, "Brake glow", new Vector3(0, .78f, rearZ - .16f), Color.red, 5, 6));
            rig.HeadGlow.Add(VehicleLampParts.Beam(root, "Headlight beam", new Vector3(0, .38f, frontZ + .02f),
                Color.white, 5, 42, 58, 32, 6));
        }
    }
}
