using NightCourier.World;
using UnityEngine;

namespace NightCourier.Prototype.Vehicles.Van
{
    internal static class VanLightingBuilder
    {
        public static void Build(Transform root, CityPalette palette, VehicleLampRig rig)
        {
            const float frontZ = 2.79f, rearZ = -2.75f;
            foreach (int side in new[] { -1, 1 })
            {
                var indicators = side < 0 ? rig.Left : rig.Right;
                var glows = side < 0 ? rig.LeftGlow : rig.RightGlow;
                float x = side * .82f;
                rig.Running.Add(VehicleLampParts.Running(root, side < 0 ? "Van left vertical tail" : "Van right vertical tail",
                    new Vector3(x, .92f, rearZ), new Vector3(.1f, .58f, .03f), palette));
                rig.Brake.Add(VehicleLampParts.Lamp(root, side < 0 ? "Van left vertical brake" : "Van right vertical brake",
                    new Vector3(x, .92f, rearZ - .006f), new Vector3(.105f, .58f, .04f), -1, palette.Red, palette));
                indicators.Add(VehicleLampParts.Lamp(root, side < 0 ? "Van rear left indicator" : "Van rear right indicator",
                    new Vector3(x, .55f, rearZ), new Vector3(.19f, .09f, .04f), -1, palette.Warm, palette));
                rig.Reverse.Add(VehicleLampParts.Lamp(root, side < 0 ? "Van rear left reverse" : "Van rear right reverse",
                    new Vector3(x, .37f, rearZ), new Vector3(.19f, .09f, .04f), -1, palette.Headlight, palette));
                rig.Head.Add(VehicleLampParts.Headlamp(root, side < 0 ? "Van left projector" : "Van right projector",
                    new Vector3(side * .62f, .55f, frontZ), new Vector3(.48f, .18f, .06f), palette));
                rig.Head.Add(VehicleLampParts.Headlamp(root, side < 0 ? "Van left light brow" : "Van right light brow",
                    new Vector3(side * .62f, .76f, frontZ), new Vector3(.62f, .045f, .04f), palette));
                indicators.Add(VehicleLampParts.Lamp(root, side < 0 ? "Van front left indicator" : "Van front right indicator",
                    new Vector3(side * .82f, .36f, frontZ), new Vector3(.22f, .075f, .04f), 1, palette.Warm, palette));
                glows.Add(VehicleLampParts.Point(root, side < 0 ? "Van left indicator glow" : "Van right indicator glow",
                    new Vector3(side * .82f, .36f, frontZ + .08f), new Color(1, .42f, .02f), 2.5f, 4));
                rig.ReverseGlow.Add(VehicleLampParts.Point(root, "Van reverse glow", new Vector3(x, .37f, rearZ - .1f),
                    new Color(.75f, .9f, 1), 3, 5));
            }
            rig.BrakeGlow.Add(VehicleLampParts.Point(root, "Van brake glow", new Vector3(0, .75f, rearZ - .15f), Color.red, 5, 6));
            rig.HeadGlow.Add(VehicleLampParts.Beam(root, "Van Headlight beam", new Vector3(0, .58f, frontZ + .02f),
                new Color(.78f, .92f, 1), 5, 45, 58, 32, 6));
        }
    }
}
