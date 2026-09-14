using System.Collections.Generic;
using NightCourier.Vehicle;
using UnityEngine;

namespace NightCourier.Prototype
{
    public enum CourierVehicle { Pickup, Sport, Van }

    /// <summary>Persists the garage selection and switches the generated vehicle shell.</summary>
    public sealed class VehicleAppearance : MonoBehaviour
    {
        private const string VehiclePreference = "NightCourier.Vehicle";
        private const string ColorPreference = "NightCourier.VehicleColor";
        private static readonly Color[] PaintColors =
        {
            new Color(.025f, .34f, .38f),
            new Color(.075f, .09f, .12f),
            new Color(.62f, .09f, .075f),
            new Color(.72f, .74f, .72f),
            new Color(.16f, .11f, .3f)
        };

        private readonly List<Renderer> paint = new List<Renderer>();
        private GameObject[] shells;
        private GameObject[][] wheelSkins;
        private Material paintMaterial;
        private ArcadeCarController controller;
        private BoxCollider chassis;
        private BoxCollider cabin;
        private GameObject visiblePickupCargo;

        public CourierVehicle SelectedVehicle { get; private set; }
        public int SelectedColor { get; private set; }
        public static IReadOnlyList<Color> AvailableColors => PaintColors;

        public void Configure(GameObject pickup, GameObject sport, GameObject van, Material sourcePaint,
            ArcadeCarController vehicleController, BoxCollider chassisCollider, BoxCollider cabinCollider,
            GameObject[][] vehicleWheelSkins, GameObject pickupCargo)
        {
            shells = new[] { pickup, sport, van };
            controller = vehicleController;
            chassis = chassisCollider;
            cabin = cabinCollider;
            wheelSkins = vehicleWheelSkins;
            visiblePickupCargo = pickupCargo;
            paintMaterial = new Material(sourcePaint) { name = "Selected courier vehicle matte paint" };
            foreach (GameObject shell in shells)
            foreach (Renderer renderer in shell.GetComponentsInChildren<Renderer>(true))
                if (renderer.sharedMaterial == sourcePaint)
                {
                    renderer.sharedMaterial = paintMaterial;
                    paint.Add(renderer);
                }
            Select((CourierVehicle)Mathf.Clamp(PlayerPrefs.GetInt(VehiclePreference, 0), 0, 2),
                Mathf.Clamp(PlayerPrefs.GetInt(ColorPreference, 0), 0, PaintColors.Length - 1), false);
        }

        public void Select(CourierVehicle vehicle, int colorIndex, bool save = true)
        {
            SelectedVehicle = vehicle;
            SelectedColor = Mathf.Clamp(colorIndex, 0, PaintColors.Length - 1);
            if (shells != null)
                for (int i = 0; i < shells.Length; i++) shells[i].SetActive(i == (int)vehicle);
            if (wheelSkins != null)
                for (int vehicleIndex = 0; vehicleIndex < wheelSkins.Length; vehicleIndex++)
                foreach (GameObject skin in wheelSkins[vehicleIndex])
                    skin.SetActive(vehicleIndex == (int)vehicle);
            if (visiblePickupCargo != null) visiblePickupCargo.SetActive(vehicle == CourierVehicle.Pickup);
            ApplyVehicleSetup(vehicle);
            if (paintMaterial != null) paintMaterial.SetColor("_BaseColor", PaintColors[SelectedColor]);
            if (!save) return;
            PlayerPrefs.SetInt(VehiclePreference, (int)SelectedVehicle);
            PlayerPrefs.SetInt(ColorPreference, SelectedColor);
            PlayerPrefs.Save();
        }

        private void ApplyVehicleSetup(CourierVehicle vehicle)
        {
            if (controller == null) return;
            VehiclePerformanceProfile profile;
            switch (vehicle)
            {
                case CourierVehicle.Sport:
                    profile = VehiclePerformanceProfiles.Sport;
                    SetCollider(chassis, new Vector3(0, .015f, 0), new Vector3(1.90f, .48f, 5.55f));
                    SetCollider(cabin, new Vector3(0, .68f, .12f), new Vector3(1.45f, .65f, 1.7f));
                    break;
                case CourierVehicle.Van:
                    profile = VehiclePerformanceProfiles.Van;
                    SetCollider(chassis, new Vector3(0, .18f, -.04f), new Vector3(2.04f, .68f, 5.25f));
                    SetCollider(cabin, new Vector3(0, 1.08f, -.48f), new Vector3(1.92f, 1.55f, 3.82f));
                    break;
                default:
                    profile = VehiclePerformanceProfiles.Pickup;
                    SetCollider(chassis, new Vector3(0, .12f, 0), new Vector3(2.02f, .52f, 4.9f));
                    SetCollider(cabin, new Vector3(0, .88f, .55f), new Vector3(1.84f, 1.08f, 2.05f));
                    break;
            }
            controller.ApplyPerformanceProfile(profile);
            Physics.SyncTransforms();
        }

        private static void SetCollider(BoxCollider target, Vector3 center, Vector3 size)
        {
            if (target == null) return;
            target.center = center;
            target.size = size;
        }

        private void OnDestroy()
        {
            if (paintMaterial != null) Destroy(paintMaterial);
        }
    }
}
