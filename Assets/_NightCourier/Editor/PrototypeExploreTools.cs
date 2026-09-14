using NightCourier.Vehicle;
using NightCourier.World;
using NightCourier.Prototype;
using UnityEditor;
using UnityEngine;

namespace NightCourier.EditorTools
{
    /// <summary>Editor-only shortcuts for checking the new road; R still returns to the city depot.</summary>
    public static class PrototypeExploreTools
    {
        [MenuItem("NightCourier/Explore/Bypass city entrance")]
        public static void BypassEntrance() => MoveNear(new Vector3(390, 0, 130));

        [MenuItem("NightCourier/Explore/Bypass Harbor exit")]
        public static void BypassHarborExit() => MoveNear(new Vector3(985, 0, 175));

        [MenuItem("NightCourier/Explore/Hillside drive")]
        public static void Hillside() => MoveNear(new Vector3(535, 0, 270));

        [MenuItem("NightCourier/Explore/River viaduct")]
        public static void Viaduct() => MoveNear(new Vector3(740, 0, 450));

        [MenuItem("NightCourier/Explore/Main city intersection")]
        public static void MainIntersection() => MoveCar(new Vector3(20, 1.1f, 20), Quaternion.identity);

        [MenuItem("NightCourier/Explore/Highway toll arch")]
        public static void HighwayToll() => MoveCar(new Vector3(470, 1.1f, -25), Quaternion.Euler(0, 90, 0));

        [MenuItem("NightCourier/Explore/Rest area entrance")]
        public static void RestArea() => MoveCar(new Vector3(680, 1.1f, -24), Quaternion.Euler(0, 90, 0));

        [MenuItem("NightCourier/Explore/Preview sport car %#9")]
        public static void PreviewSportCar()
        {
            var appearance = Object.FindAnyObjectByType<VehicleAppearance>();
            if (appearance == null) return;
            appearance.Select(CourierVehicle.Sport, appearance.SelectedColor, false);
            Object.FindAnyObjectByType<MainMenuController>()?.BeginShift(true);
            MoveCar(new Vector3(20, 1.1f, 20), Quaternion.Euler(0, 30, 0));
        }

        [MenuItem("NightCourier/Explore/Preview pickup")]
        public static void PreviewPickup()
        {
            var appearance = Object.FindAnyObjectByType<VehicleAppearance>();
            if (appearance == null) return;
            appearance.Select(CourierVehicle.Pickup, appearance.SelectedColor, false);
            Object.FindAnyObjectByType<MainMenuController>()?.BeginShift(true);
            MoveCar(new Vector3(20, 1.1f, 20), Quaternion.Euler(0, 30, 0));
        }

        [MenuItem("NightCourier/Explore/Preview van")]
        public static void PreviewVan()
        {
            var appearance = Object.FindAnyObjectByType<VehicleAppearance>();
            if (appearance == null) return;
            appearance.Select(CourierVehicle.Van, appearance.SelectedColor, false);
            Object.FindAnyObjectByType<MainMenuController>()?.BeginShift(true);
            MoveCar(new Vector3(20, 1.1f, 20), Quaternion.Euler(0, 30, 0));
        }

        [MenuItem("NightCourier/Explore/Hillside drive", true)]
        [MenuItem("NightCourier/Explore/Bypass city entrance", true)]
        [MenuItem("NightCourier/Explore/Bypass Harbor exit", true)]
        [MenuItem("NightCourier/Explore/River viaduct", true)]
        [MenuItem("NightCourier/Explore/Main city intersection", true)]
        [MenuItem("NightCourier/Explore/Highway toll arch", true)]
        [MenuItem("NightCourier/Explore/Rest area entrance", true)]
        [MenuItem("NightCourier/Explore/Preview sport car %#9", true)]
        [MenuItem("NightCourier/Explore/Preview pickup", true)]
        [MenuItem("NightCourier/Explore/Preview van", true)]
        public static bool CanExplore() => EditorApplication.isPlaying && Object.FindAnyObjectByType<ArcadeCarController>() != null;

        private static void MoveCar(Vector3 position, Quaternion rotation)
        {
            var car = Object.FindAnyObjectByType<ArcadeCarController>();
            if (car == null) return;
            var body = car.GetComponent<Rigidbody>();
            body.position = position; body.rotation = rotation;
            body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
        }

        private static void MoveNear(Vector3 location)
        {
            var car = Object.FindAnyObjectByType<ArcadeCarController>();
            if (car == null) return;
            var points = ScenicRoadPath.Points;
            int nearest = 1;
            float distance = float.MaxValue;
            for (int i = 1; i < points.Count - 1; i++)
            {
                float candidate = new Vector2(points[i].x - location.x, points[i].z - location.z).sqrMagnitude;
                if (candidate < distance) { nearest = i; distance = candidate; }
            }
            var body = car.GetComponent<Rigidbody>();
            body.position = points[nearest] + Vector3.up * 1.1f;
            body.rotation = Quaternion.LookRotation(points[nearest + 1] - points[nearest - 1], Vector3.up);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
        }
    }
}
