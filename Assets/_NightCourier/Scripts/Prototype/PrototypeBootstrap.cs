using NightCourier.CameraSystem;
using NightCourier.Delivery;
using NightCourier.Prototype.Vehicles;
using NightCourier.Prototype.Vehicles.Pickup;
using NightCourier.Prototype.Vehicles.Sport;
using NightCourier.Prototype.Vehicles.Van;
using NightCourier.Vehicle;
using NightCourier.World;
using UnityEngine;
using UnityEngine.Rendering;

namespace NightCourier.Prototype
{
    /// <summary>Scene composition only; vehicle, city, atmosphere and route own their behaviour.</summary>
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        private void Start()
        {
            var mainMenu = gameObject.AddComponent<MainMenuController>();
            var palette = gameObject.AddComponent<CityPalette>();
            palette.Initialize();
            gameObject.AddComponent<CityBuilder>().Build(palette);
            gameObject.AddComponent<ScenicRoadBuilder>().Build(palette);
            gameObject.AddComponent<WorldExpansionBuilder>().Build(palette);
            var car = CreateCar(palette);
            var cameraObject = new GameObject("Chase camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform, false);
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.nearClipPlane = 0.04f;
            camera.farClipPlane = 520;
            camera.fieldOfView = 62;
            var cameraController = cameraObject.AddComponent<ChaseCamera>();
            cameraController.Follow(car, car.GetComponent<CockpitDisplay>());
            gameObject.AddComponent<NightAtmosphere>().Initialize(camera, car.transform);
            var marker = new GameObject("Delivery bay");
            marker.transform.SetParent(transform, false);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(marker.transform, "Bay edge", new Vector3(side * 3.5f, 0, 0), new Vector3(0.12f, 0.04f, 7), palette.Cool);
                Part(marker.transform, "Bay edge", new Vector3(0, 0, side * 3.5f), new Vector3(7, 0.04f, 0.12f), palette.Cool);
            }
            Part(marker.transform, "Destination beacon", new Vector3(0, 4, 0), new Vector3(0.65f, 0.65f, 0.65f), palette.Cool);
            var route = gameObject.AddComponent<DeliveryRoute>();
            route.Initialize(car, marker.transform);
            var hud = gameObject.AddComponent<DeliveryHud>();
            hud.Initialize(route, car, cameraController);
            mainMenu.Initialize(hud, car.GetComponent<VehicleAppearance>());
            Physics.SyncTransforms();
        }

        private ArcadeCarController CreateCar(CityPalette palette)
        {
            var car = new GameObject("Courier electric pickup", typeof(Rigidbody), typeof(BoxCollider));
            car.transform.SetParent(transform, false);
            car.transform.SetPositionAndRotation(new Vector3(51, 1.36f, -230), Quaternion.Euler(0, -90, 0));
            var chassis = car.GetComponent<BoxCollider>();
            chassis.center = new Vector3(0, 0.12f, 0);
            // Keep the proven curb-friendly contact envelope while the visual
            // overhangs provide the longer grand-tourer proportion.
            chassis.size = new Vector3(2.02f, 0.52f, 4.9f);
            var cabinCollider = car.AddComponent<BoxCollider>();
            cabinCollider.center = new Vector3(0, 0.88f, 0.55f);
            cabinCollider.size = new Vector3(1.84f, 1.08f, 2.05f);
            var pickupShell = new GameObject("Pickup shell");
            pickupShell.transform.SetParent(car.transform, false);
            Transform shell = pickupShell.transform;
            Part(shell, "Angular lower body", new Vector3(0, 0.08f, 0),
                new Vector3(2.05f, 0.58f, 5.35f), palette.CarPaint);
            Part(shell, "Lower body shoulder", new Vector3(0, 0.25f, 0),
                new Vector3(2.08f, 0.32f, 4.75f), palette.CarPaint);
            Part(shell, "Angular hood", new Vector3(0, 0.39f, 2.02f),
                new Vector3(2.02f, 0.34f, 1.54f), palette.CarPaint);
            Part(shell, "Angular front fascia", new Vector3(0, 0.27f, 2.82f),
                new Vector3(2.06f, 0.4f, 0.16f), palette.CarPaint);
            Part(shell, "Curved cabin glass", new Vector3(0, 0.96f, 0.58f),
                new Vector3(1.84f, 0.94f, 2.02f), palette.Glass);
            Part(shell, "Floating cabin roof", new Vector3(0, 1.48f, 0.5f),
                new Vector3(1.92f, 0.12f, 2.08f), palette.CarPaint);
            Part(shell, "Bed floor", new Vector3(0, 0.3f, -1.55f), new Vector3(1.82f, 0.16f, 2.35f), palette.Metal);
            Part(shell, "Tailgate", new Vector3(0, 0.58f, -2.72f), new Vector3(2.02f, 0.72f, 0.14f), palette.CarPaint);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(shell, "Bed rail", new Vector3(side * 0.97f, 0.58f, -1.5f), new Vector3(0.14f, 0.7f, 2.42f), palette.CarPaint);
                Part(shell, "Bed cap", new Vector3(side * 0.97f, 0.95f, -1.5f), new Vector3(0.18f, 0.08f, 2.42f), palette.Metal);
                for (int end = -1; end <= 1; end += 2)
                    Part(shell, "Cabin pillar", new Vector3(side * 0.93f, 0.95f, 0.55f + end * 0.98f), new Vector3(0.1f, 1, 0.12f), palette.CarPaint);
                Part(shell, "Mirror", new Vector3(side * 1.12f, 0.86f, 1.28f), new Vector3(0.28f, 0.22f, 0.4f), palette.Metal, PrimitiveType.Sphere);
                Part(shell, "Door lower skin", new Vector3(side * 0.945f, 0.56f, 0.57f),
                    new Vector3(0.08f, 0.58f, 1.78f), palette.CarPaint);
                Part(shell, "Window belt trim", new Vector3(side * 0.97f, 0.9f, 0.57f),
                    new Vector3(0.055f, 0.055f, 1.88f), palette.Metal);
                Part(shell, "Front fender shoulder", new Vector3(side * 0.97f, 0.4f, 1.92f),
                    new Vector3(0.11f, 0.26f, 0.88f), palette.CarPaint);
                Part(shell, "Rear fender shoulder", new Vector3(side * 0.97f, 0.4f, -1.55f),
                    new Vector3(0.11f, 0.26f, 1.08f), palette.CarPaint);
                Part(shell, "Side step", new Vector3(side * 1.05f, -0.04f, 0.42f), new Vector3(0.2f, 0.1f, 2.05f), palette.Metal);
                CreateSmoothWheelArch(shell, palette, side, 1.57f, "Front");
                CreateSmoothWheelArch(shell, palette, side, -1.55f, "Rear");
            }
            Part(shell, "Windshield upper frame", new Vector3(0, 1.37f, 1.58f),
                new Vector3(1.88f, 0.08f, 0.09f), palette.CarPaint);
            Part(shell, "Windshield lower frame", new Vector3(0, 0.64f, 1.58f),
                new Vector3(1.88f, 0.09f, 0.09f), palette.CarPaint);
            Part(shell, "Front lower intake", new Vector3(0, 0.22f, 2.79f), new Vector3(0.95f, 0.2f, 0.05f), palette.Metal);
            Part(shell, "Rear bumper", new Vector3(0, -0.01f, -2.81f), new Vector3(2.08f, 0.2f, 0.15f), palette.Curb);
            Part(shell, "License plate", new Vector3(0, 0.32f, -2.8f), new Vector3(0.52f, 0.17f, 0.04f), palette.White);
            GameObject sportShell = CreateSportShell(car.transform, palette);
            GameObject vanShell = CreateVanShell(car.transform, palette);
            GameObject cargoRoot = CreateCargoBed(car.transform, palette);
            var wheels = new WheelCollider[4];
            var visuals = new Transform[4];
            var wheelSkins = new[] { new GameObject[4], new GameObject[4], new GameObject[4] };
            for (int i = 0; i < 4; i++)
            {
                int side = i % 2 == 0 ? -1 : 1;
                var wheel = new GameObject(i < 2 ? "Front suspension" : "Rear suspension");
                wheel.transform.SetParent(car.transform, false);
                wheel.transform.localPosition = new Vector3(side * 1.01f, -0.18f, i < 2 ? 1.57f : -1.55f);
                wheels[i] = wheel.AddComponent<WheelCollider>();
                var visual = new GameObject(i < 2 ? "Front wheel visual" : "Rear wheel visual");
                visual.transform.SetParent(car.transform, false);
                visuals[i] = visual.transform;
                wheelSkins[0][i] = CreateWheelSkin(visual.transform, palette, side, "Pickup", .96f, .18f, 4);
                wheelSkins[1][i] = CreateWheelSkin(visual.transform, palette, side, "Sport", .78f, .17f, 5);
                wheelSkins[2][i] = CreateWheelSkin(visual.transform, palette, side, "Van", .94f, .2f, 6);
            }
            var carInput = car.AddComponent<CarInput>();
            car.AddComponent<VehicleBattery>();
            var controller = car.AddComponent<ArcadeCarController>();
            controller.ConfigureWheels(wheels, visuals);
            CreateVehicleLighting(car.transform, palette, controller, pickupShell, sportShell, vanShell);
            var appearance = car.AddComponent<VehicleAppearance>();
            appearance.Configure(pickupShell, sportShell, vanShell, palette.CarPaint, controller,
                chassis, cabinCollider, wheelSkins, cargoRoot);
            CreateCockpit(car.transform, palette, controller);
            var motorAudio = car.gameObject.AddComponent<ElectricMotorAudio>();
            motorAudio.Configure(controller, carInput);
            return controller;
        }

        private GameObject CreateSportShell(Transform car, CityPalette palette)
            => SportBodyBuilder.Build(car, palette);

        private GameObject CreateVanShell(Transform car, CityPalette palette)
        {
            var root = new GameObject("Courier van shell");
            root.transform.SetParent(car, false);
            Part(root.transform, "Van lower body", new Vector3(0, .1f, -.05f),
                new Vector3(2.1f, .62f, 5.35f), palette.CarPaint);
            Part(root.transform, "Van cargo body", new Vector3(0, 1.08f, -.65f),
                new Vector3(2.02f, 1.52f, 3.7f), palette.CarPaint);
            Part(root.transform, "Van windscreen", new Vector3(0, 1.08f, 1.34f),
                new Vector3(1.88f, 1.25f, .12f), palette.Glass);
            Part(root.transform, "Van short nose", new Vector3(0, .52f, 2.18f),
                new Vector3(2.04f, .5f, 1.18f), palette.CarPaint);
            Part(root.transform, "Van roof", new Vector3(0, 1.89f, -.57f),
                new Vector3(2.1f, .13f, 3.92f), palette.Metal);
            Part(root.transform, "Van rear doors", new Vector3(0, 1.05f, -2.69f),
                new Vector3(1.92f, 1.45f, .08f), palette.CarPaint);
            Part(root.transform, "Van rear door seam", new Vector3(0, 1.05f, -2.75f),
                new Vector3(.035f, 1.24f, .03f), palette.Metal);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(root.transform, "Van side window", new Vector3(side * 1.02f, 1.25f, 1.05f),
                    new Vector3(.055f, .7f, .95f), palette.Glass);
                Part(root.transform, "Van rub rail", new Vector3(side * 1.055f, .44f, -.4f),
                    new Vector3(.08f, .13f, 4.15f), palette.Metal);
                CreateSmoothWheelArch(root.transform, palette, side, 1.65f, "Van front");
                CreateSmoothWheelArch(root.transform, palette, side, -1.75f, "Van rear");
            }
            return root;
        }

        private GameObject CreateCargoBed(Transform car, CityPalette palette)
        {
            var cargoRoot = new GameObject("Visible pickup cargo");
            cargoRoot.transform.SetParent(car, false);
            var parcels = new GameObject[8];
            Material[] colors = { palette.Yellow, palette.Facades[0], palette.Cool, palette.Facades[2], palette.Pink, palette.Curb, palette.Warm, palette.Facades[1] };
            for (int parcel = 0; parcel < parcels.Length; parcel++)
            {
                int row = parcel / 2;
                float width = parcel % 3 == 0 ? 0.66f : 0.54f;
                float height = parcel % 4 == 0 ? 0.52f : 0.38f;
                var root = new GameObject($"Delivery parcel {parcel + 1}");
                root.transform.SetParent(cargoRoot.transform, false);
                root.transform.localPosition = new Vector3(parcel % 2 == 0 ? -0.4f : 0.4f,
                    0.55f + height * 0.5f, -0.64f - row * 0.55f);
                Part(root.transform, "Parcel body", Vector3.zero, new Vector3(width, height, 0.48f), colors[parcel]);
                Part(root.transform, "Parcel strap", new Vector3(0, height * 0.51f, 0), new Vector3(0.09f, 0.018f, 0.5f), palette.White);
                if (parcel % 2 == 1)
                    Part(root.transform, "Parcel label", new Vector3(0, 0, 0.245f), new Vector3(width * 0.48f, height * 0.42f, 0.018f), palette.White);
                parcels[parcel] = root;
            }
            var cargo = car.gameObject.AddComponent<CargoBedVisual>();
            cargo.Configure(parcels);
            return cargoRoot;
        }

        private void CreateWheelArch(Transform car, CityPalette palette, int side, float wheelZ, string axle)
        {
            const int segments = 7;
            for (int segment = 0; segment < segments; segment++)
            {
                float angle = Mathf.Lerp(18, 162, segment / (segments - 1f));
                float radians = angle * Mathf.Deg2Rad;
                var arch = Part(car, $"{axle} {(side < 0 ? "left" : "right")} wheel arch {segment + 1}",
                    new Vector3(side * 1.035f, -0.16f + Mathf.Sin(radians) * 0.62f,
                        wheelZ + Mathf.Cos(radians) * 0.62f),
                    new Vector3(0.16f, 0.16f, 0.42f), palette.CarPaint);
                arch.transform.localRotation = Quaternion.Euler(angle - 90, 0, 0);
            }
        }

        private void CreateSmoothWheelArch(Transform parent, CityPalette palette, int side, float wheelZ, string axle)
        {
            const int segments = 18;
            var vertices = new Vector3[(segments + 1) * 2];
            var triangles = new int[segments * 12];
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(16, 164, i / (float)segments) * Mathf.Deg2Rad;
                vertices[i * 2] = new Vector3(side * 1.035f, -.16f + Mathf.Sin(angle) * .5f,
                    wheelZ + Mathf.Cos(angle) * .5f);
                vertices[i * 2 + 1] = new Vector3(side * 1.055f, -.16f + Mathf.Sin(angle) * .64f,
                    wheelZ + Mathf.Cos(angle) * .64f);
                if (i == segments) continue;
                int v = i * 2;
                int t = i * 12;
                int[] face = side > 0
                    ? new[] { v, v + 1, v + 3, v, v + 3, v + 2 }
                    : new[] { v, v + 3, v + 1, v, v + 2, v + 3 };
                for (int n = 0; n < 6; n++) triangles[t + n] = face[n];
                for (int n = 0; n < 6; n += 3)
                {
                    triangles[t + 6 + n] = face[n];
                    triangles[t + 7 + n] = face[n + 2];
                    triangles[t + 8 + n] = face[n + 1];
                }
            }
            var mesh = new Mesh { name = axle + " smooth wheel arch" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            VehicleMeshNormals.SeparateFaces(mesh);
            mesh.RecalculateBounds();
            var item = new GameObject($"{axle} {(side < 0 ? "left" : "right")} smooth wheel arch",
                typeof(MeshFilter), typeof(MeshRenderer));
            item.transform.SetParent(parent, false);
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            item.GetComponent<MeshRenderer>().sharedMaterial = palette.CarPaint;
        }

        private GameObject CreateWheelSkin(Transform parent, CityPalette palette, int side, string style,
            float diameter, float width, int spokes)
        {
            var root = new GameObject(style + " wheel skin");
            root.transform.SetParent(parent, false);
            var tire = Part(root.transform, style + " tire", Vector3.zero,
                new Vector3(diameter, width, diameter), palette.Metal, PrimitiveType.Cylinder);
            tire.transform.localRotation = Quaternion.Euler(0, 0, 90);
            var rim = Part(root.transform, style + " rim", new Vector3(side * (width + .015f), 0, 0),
                new Vector3(diameter * .64f, .018f, diameter * .64f), palette.Curb, PrimitiveType.Cylinder);
            rim.transform.localRotation = Quaternion.Euler(0, 0, 90);
            for (int spoke = 0; spoke < spokes; spoke++)
            {
                float angle = spoke * 180f / spokes;
                var blade = Part(root.transform, style + " rim spoke", new Vector3(side * (width + .035f), 0, 0),
                    new Vector3(.025f, diameter * .48f, style == "Sport" ? .055f : .085f), palette.Metal);
                blade.transform.localRotation = Quaternion.Euler(angle, 0, 0);
            }
            return root;
        }

        private void CreateVehicleLighting(Transform car, CityPalette palette, ArcadeCarController controller,
            GameObject pickupShell, GameObject sportShell, GameObject vanShell)
        {
            var lamps = new VehicleLampRig();
            PickupLightingBuilder.Build(pickupShell.transform, palette, lamps);
            SportLightingBuilder.Build(sportShell.transform, palette, lamps);
            VanLightingBuilder.Build(vanShell.transform, palette, lamps);
            lamps.Attach(car.gameObject, controller);
        }

        private void CreateCockpit(Transform car, CityPalette palette, ArcadeCarController controller)
        {
            var root = new GameObject("Cockpit instruments");
            root.transform.SetParent(car, false);
            Part(root.transform, "Dashboard", new Vector3(0, 0.69f, 1.08f), new Vector3(1.76f, 0.3f, 0.58f), palette.Metal);
            var steering = new GameObject("Steering wheel");
            steering.transform.SetParent(root.transform, false);
            steering.transform.localPosition = new Vector3(-0.48f, 0.87f, 0.73f);
            steering.transform.localScale = Vector3.one * 0.4f;
            for (int segment = 0; segment < 16; segment++)
            {
                float angle = segment * 22.5f;
                float radians = angle * Mathf.Deg2Rad;
                var rim = Part(steering.transform, "Steering rim",
                    new Vector3(Mathf.Sin(radians) * 0.3f, Mathf.Cos(radians) * 0.3f, 0),
                    new Vector3(0.06f, 0.12f, 0.06f), palette.Curb, PrimitiveType.Capsule);
                // Capsules follow the circumference instead of radiating like a fan.
                rim.transform.localRotation = Quaternion.Euler(0, 0, 90 - angle);
            }
            var hub = Part(steering.transform, "Steering hub", Vector3.zero,
                new Vector3(0.2f, 0.06f, 0.2f), palette.Metal, PrimitiveType.Cylinder);
            hub.transform.localRotation = Quaternion.Euler(90, 0, 0);
            for (int spoke = -1; spoke <= 1; spoke += 2)
            {
                var spokePart = Part(steering.transform, "Steering spoke", new Vector3(spoke * 0.12f, -0.08f, 0),
                    new Vector3(0.25f, 0.055f, 0.055f), palette.Metal);
                spokePart.transform.localRotation = Quaternion.Euler(0, 0, spoke * 22);
            }

            var display = car.gameObject.AddComponent<CockpitDisplay>();
            display.Configure(controller, root, steering.transform);
        }

        private GameObject Part(Transform parent, string label, Vector3 position, Vector3 scale,
            Material material, PrimitiveType shape = PrimitiveType.Cube)
        {
            var item = GameObject.CreatePrimitive(shape);
            item.name = label;
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = material;
            var collider = item.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            return item;
        }
    }
}
