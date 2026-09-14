using NightCourier.World;
using UnityEngine;

namespace NightCourier.Prototype.Vehicles.Sport
{
    internal static class SportLightingBuilder
    {
        public static void Build(Transform root, CityPalette palette, VehicleLampRig rig)
        {
            const float frontZ = SportBodyBuilder.FrontZ + .015f, rearZ = SportBodyBuilder.RearZ - .015f;
            foreach(int side in new[]{-1,1})
            {
                var indicators=side<0?rig.Left:rig.Right;
                var glows=side<0?rig.LeftGlow:rig.RightGlow;
                // Thin horizontal twin blades, housed in the nose rather than floating above it.
                foreach(float y in new[]{.28f,.225f})
                    rig.Head.Add(VehicleLampParts.Headlamp(root,"Sport matrix LED blade",
                        new Vector3(side*.72f,y,frontZ),new Vector3(.55f,.027f,.025f),palette));
                rig.Running.Add(VehicleLampParts.Running(root,"Sport red horizon tail",
                    new Vector3(side*.49f,.33f,rearZ),new Vector3(.93f,.025f,.025f),palette));
                rig.Brake.Add(VehicleLampParts.Lamp(root,"Sport brake strip",new Vector3(side*.49f,.29f,rearZ),
                    new Vector3(.93f,.04f,.028f),-1,palette.Red,palette));
                indicators.Add(VehicleLampParts.Lamp(root,"Sport front indicator",new Vector3(side*.74f,.175f,frontZ),
                    new Vector3(.46f,.025f,.03f),1,palette.Warm,palette));
                indicators.Add(VehicleLampParts.Lamp(root,"Sport rear indicator",new Vector3(side*.77f,.225f,rearZ),
                    new Vector3(.30f,.025f,.03f),-1,palette.Warm,palette));
                rig.Reverse.Add(VehicleLampParts.Lamp(root,"Sport reverse",new Vector3(side*.25f,.14f,rearZ),
                    new Vector3(.15f,.027f,.03f),-1,palette.Headlight,palette));
                glows.Add(VehicleLampParts.Point(root,"Sport indicator glow",new Vector3(side*.74f,.2f,frontZ+.1f),
                    new Color(1,.42f,.02f),2,3));
            }
            rig.BrakeGlow.Add(VehicleLampParts.Point(root,"Sport brake glow",new Vector3(0,.3f,rearZ-.15f),Color.red,4,5));
            rig.ReverseGlow.Add(VehicleLampParts.Point(root,"Sport reverse glow",new Vector3(0,.18f,rearZ-.1f),
                new Color(.7f,.9f,1),2,4));
            rig.HeadGlow.Add(VehicleLampParts.Beam(root,"Sport Headlight beam",new Vector3(0,.25f,frontZ+.03f),
                new Color(.78f,.9f,1),7.5f,52,54,30,4));
        }
    }
}
