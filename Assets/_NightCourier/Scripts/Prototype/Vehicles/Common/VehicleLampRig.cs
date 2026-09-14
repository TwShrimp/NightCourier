using System.Collections.Generic;
using NightCourier.Vehicle;
using NightCourier.World;
using UnityEngine;

namespace NightCourier.Prototype.Vehicles
{
    /// <summary>Collects functional lamps built by each vehicle-specific lighting package.</summary>
    internal sealed class VehicleLampRig
    {
        public readonly List<Renderer> Brake = new List<Renderer>();
        public readonly List<Renderer> Reverse = new List<Renderer>();
        public readonly List<Renderer> Left = new List<Renderer>();
        public readonly List<Renderer> Right = new List<Renderer>();
        public readonly List<Renderer> Head = new List<Renderer>();
        public readonly List<Renderer> Running = new List<Renderer>();
        public readonly List<Light> BrakeGlow = new List<Light>();
        public readonly List<Light> ReverseGlow = new List<Light>();
        public readonly List<Light> LeftGlow = new List<Light>();
        public readonly List<Light> RightGlow = new List<Light>();
        public readonly List<Light> HeadGlow = new List<Light>();

        public void Attach(GameObject car, ArcadeCarController controller)
        {
            var lighting = car.AddComponent<VehicleLighting>();
            lighting.Configure(controller, Brake.ToArray(), Reverse.ToArray(), Left.ToArray(), Right.ToArray(),
                BrakeGlow.ToArray(), ReverseGlow.ToArray(), LeftGlow.ToArray(), RightGlow.ToArray(),
                Head.ToArray(), Running.ToArray(), HeadGlow.ToArray());
        }
    }

    internal static class VehicleLampParts
    {
        public static Renderer Running(Transform parent, string name, Vector3 position, Vector3 scale, CityPalette palette,
            Quaternion? rotation = null)
        {
            return Part(parent, name, position, scale, palette.TailRunning, rotation).GetComponent<Renderer>();
        }

        public static Renderer Lamp(Transform parent, string name, Vector3 position, Vector3 scale,
            float faceDirection, Material material, CityPalette palette, Quaternion? rotation = null)
        {
            Quaternion facing = rotation ?? Quaternion.identity;
            Part(parent, name + " housing", position, Vector3.Scale(scale, new Vector3(1.16f, 1.16f, 1.2f)),
                palette.DarkWindow, facing);
            GameObject face = Part(parent, name, position + Vector3.forward * (faceDirection * .025f), scale, material, facing);
            Renderer renderer = face.GetComponent<Renderer>();
            renderer.enabled = false;
            return renderer;
        }

        public static Renderer Headlamp(Transform parent, string name, Vector3 position, Vector3 scale,
            CityPalette palette, PrimitiveType shape = PrimitiveType.Cube, Quaternion? rotation = null)
        {
            return Part(parent, name, position, scale, palette.Headlight, rotation, shape).GetComponent<Renderer>();
        }

        public static Light Point(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
        {
            var item = new GameObject(name, typeof(Light));
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            Light light = item.GetComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.enabled = false;
            return light;
        }

        public static Light Beam(Transform parent, string name, Vector3 position, Color color,
            float intensity, float range, float outerAngle, float innerAngle, float pitch)
        {
            var item = new GameObject(name, typeof(Light));
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            Light light = item.GetComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = outerAngle;
            light.innerSpotAngle = innerAngle;
            return light;
        }

        private static GameObject Part(Transform parent, string name, Vector3 position, Vector3 scale,
            Material material, Quaternion? rotation = null, PrimitiveType shape = PrimitiveType.Cube)
        {
            GameObject item = GameObject.CreatePrimitive(shape);
            item.name = name;
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localRotation = rotation ?? Quaternion.identity;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = item.GetComponent<Collider>();
            collider.enabled = false;
            Object.Destroy(collider);
            return item;
        }
    }
}
