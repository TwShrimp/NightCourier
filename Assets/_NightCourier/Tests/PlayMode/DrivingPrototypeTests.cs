using System.Collections;
using NightCourier.CameraSystem;
using NightCourier.Delivery;
using NightCourier.Vehicle;
using NightCourier.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NightCourier.Tests
{
    public sealed class DrivingPrototypeTests
    {
        private Keyboard keyboard;
        private Mouse mouse;
        private Keyboard physicalKeyboard;
        private Mouse physicalMouse;

        [UnityTest]
        public IEnumerator SportRunsTwoGearsWithIsolatedAudioAndBody()
        {
            yield return LoadPrototype();
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            var appearance = car.GetComponent<NightCourier.Prototype.VehicleAppearance>();
            appearance.Select(NightCourier.Prototype.CourierVehicle.Sport, 0, false);
            yield return new WaitForSeconds(.8f);
            Transform shell = car.transform.Find("Two-seat sport shell");
            foreach(var filter in shell.GetComponentsInChildren<MeshFilter>())
                foreach(var normal in filter.sharedMesh.normals)
                    Assert.That(normal.sqrMagnitude, Is.InRange(.99f,1.01f), filter.name);
            CaptureSport(car, "front", new Vector3(6,2.5f,7));
            CaptureSport(car, "rear", new Vector3(-6,2.8f,-7));

            CreateTestKeyboard();
            var runway = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runway.name = "Sport test runway";
            runway.transform.position = new Vector3(0,-.3f,6000);
            runway.transform.localScale = new Vector3(100,.6f,12000);
            var body = car.GetComponent<Rigidbody>();
            body.position = new Vector3(0,1,1000);
            body.rotation = Quaternion.identity;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            // Isolate drivetrain endurance from the deliberately short gameplay battery range.
            car.GetComponent<VehicleBattery>().enabled = false;
            Physics.SyncTransforms();
            yield return Hold(Key.Space,.8f);
            var audio = car.GetComponent<ElectricMotorAudio>();
            Assert.That(car.GetComponent<AudioSource>().clip.name, Does.Contain("APEX E2"));
            float until = Time.time + 45;
            int previousGear=1, shifts=0;
            float beforePitch=0, afterPitch=0, shiftAt=-1;
            Time.timeScale=3;
            while(Time.time<until)
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));
                if(car.CurrentGear==1) beforePitch=audio.CurrentPitch;
                if(car.CurrentGear!=previousGear) { shifts++; shiftAt=Time.time; }
                if(shiftAt>0 && afterPitch==0 && Time.time>shiftAt+.2f) afterPitch=audio.CurrentPitch;
                previousGear=car.CurrentGear;
                yield return null;
            }
            Debug.Log($"[Sport physics] {car.SpeedKph:F2} km/h; shifts {shifts}; pitch {beforePitch:F2} -> {afterPitch:F2}");
            Assert.That(shifts,Is.EqualTo(1));
            Assert.That(car.SpeedKph,Is.InRange(316,321));
            Assert.That(afterPitch,Is.LessThan(beforePitch*.85f));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            body.linearVelocity=Vector3.zero;
            appearance.Select(NightCourier.Prototype.CourierVehicle.Pickup,0,false);
            yield return null;
            Assert.That(car.HasTwoSpeedTransmission,Is.False);
            Assert.That(car.CurrentGear,Is.EqualTo(1));
            Assert.That(car.TargetTopSpeedKph,Is.EqualTo(200));
            Assert.That(car.GetComponent<AudioSource>().clip.name,Does.Not.Contain("APEX E2"));
            appearance.Select(NightCourier.Prototype.CourierVehicle.Van,0,false);
            yield return null;
            Assert.That(car.HasTwoSpeedTransmission,Is.False);
            Assert.That(car.TargetTopSpeedKph,Is.EqualTo(160));
            Assert.That(shell.gameObject.activeSelf,Is.False);
            Assert.That(car.GetComponent<AudioSource>().clip.name,Does.Not.Contain("APEX E2"));
        }

        private static void CaptureSport(ArcadeCarController car,string view,Vector3 offset)
        {
            var go=new GameObject("Sport review camera",typeof(Camera));
            var camera=go.GetComponent<Camera>();
            camera.CopyFrom(Camera.main);
            camera.enabled=false;
            camera.fieldOfView=36;
            camera.transform.position=car.transform.TransformPoint(offset);
            camera.transform.LookAt(car.transform.TransformPoint(new Vector3(0,.25f,0)));
            var rt=new RenderTexture(1440,900,24);
            var pixels=new Texture2D(1440,900,TextureFormat.RGB24,false);
            RenderTexture previous=RenderTexture.active;
            try
            {
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
                pixels.ReadPixels(new Rect(0,0,1440,900),0,0);pixels.Apply();
                System.IO.Directory.CreateDirectory("Logs/SportReview");
                System.IO.File.WriteAllBytes("Logs/SportReview/"+view+".png",pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=previous;camera.targetTexture=null;
                rt.Release();Object.Destroy(rt);Object.Destroy(pixels);Object.Destroy(go);
            }
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
            AudioListener.volume = 1;
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            if (physicalKeyboard != null && physicalKeyboard.added) InputSystem.EnableDevice(physicalKeyboard);
            if (physicalMouse != null && physicalMouse.added) InputSystem.EnableDevice(physicalMouse);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MainMenuPausesTheShiftUntilPlayerStarts()
        {
            yield return LoadPrototype(false);
            var menu = Object.FindFirstObjectByType<NightCourier.Prototype.MainMenuController>();
            var hud = Object.FindFirstObjectByType<DeliveryHud>();
            Assert.That(menu, Is.Not.Null);
            Assert.That(menu.IsOpen, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(AudioListener.pause, Is.True);
            Assert.That(hud.enabled, Is.False);
            Assert.That(menu.GraphicsQuality, Is.EqualTo("MEDIUM"));
            var appearance = Object.FindFirstObjectByType<NightCourier.Prototype.VehicleAppearance>();
            Assert.That(appearance, Is.Not.Null);
            menu.OpenVehicleSelection();
            Assert.That(menu.IsVehicleSelectionOpen, Is.True);
            menu.StageVehicle(NightCourier.Prototype.CourierVehicle.Sport);
            menu.StageColor(2);
            menu.ConfirmVehicleSelection();
            Assert.That(appearance.SelectedVehicle, Is.EqualTo(NightCourier.Prototype.CourierVehicle.Sport));
            Assert.That(appearance.SelectedColor, Is.EqualTo(2));
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            Transform pickupShell = car.transform.Find("Pickup shell");
            Transform sportShell = car.transform.Find("Two-seat sport shell");
            Assert.That(sportShell, Is.Not.Null);
            Assert.That(sportShell.gameObject.activeSelf, Is.True);
            Assert.That(pickupShell.gameObject.activeSelf, Is.False);
            Assert.That(sportShell.Find("Sport Headlight beam").gameObject.activeInHierarchy, Is.True,
                "The sport headlight must belong to and activate with only the sport shell.");
            Assert.That(pickupShell.Find("Headlight beam").gameObject.activeInHierarchy, Is.False,
                "Pickup lamps must not leak through the selected sport body.");
            Assert.That(car.TargetTopSpeedKph, Is.EqualTo(320));
            Assert.That(car.PerformanceName, Is.EqualTo("Mid-engine electric sport"));
            Assert.That(car.GetComponent<Rigidbody>().mass, Is.EqualTo(1640).Within(.1f));
            WheelCollider[] sportWheels = car.GetComponentsInChildren<WheelCollider>(true);
            Assert.That(sportWheels[0].transform.localPosition.z, Is.EqualTo(1.8f).Within(.01f));
            Assert.That(sportWheels[2].transform.localPosition.z, Is.EqualTo(-1.65f).Within(.01f));
            Assert.That(sportWheels[0].radius, Is.EqualTo(.39f).Within(.01f));
            Assert.That(ElectricDriveModel.DriveForce(83.33f, VehiclePerformanceProfiles.Sport),
                Is.GreaterThan(ElectricDriveModel.Resistance(83.33f, VehiclePerformanceProfiles.Sport)),
                "The sport profile must still pull at 300 km/h before drag gradually caps it.");
            appearance.Select(NightCourier.Prototype.CourierVehicle.Pickup, 0);
            Assert.That(car.TargetTopSpeedKph, Is.EqualTo(200));
            Assert.That(sportWheels[0].transform.localPosition.z, Is.EqualTo(1.57f).Within(.01f));
            Assert.That(sportShell.Find("Sport Headlight beam").gameObject.activeInHierarchy, Is.False);
            Assert.That(pickupShell.Find("Headlight beam").gameObject.activeInHierarchy, Is.True);
            menu.SetLanguage(NightCourier.Prototype.MainMenuController.MenuLanguage.Korean);
            Assert.That(menu.Language, Is.EqualTo(NightCourier.Prototype.MainMenuController.MenuLanguage.Korean));
            menu.SetLanguage(NightCourier.Prototype.MainMenuController.MenuLanguage.English);
            menu.BeginShift();
            yield return null;
            Assert.That(menu.IsOpen, Is.True);
            Assert.That(menu.IsTransitioning, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(AudioListener.pause, Is.False);
            Assert.That(AudioListener.volume, Is.GreaterThanOrEqualTo(0).And.LessThan(1));
            Assert.That(hud.enabled, Is.False);
            yield return new WaitForSecondsRealtime(1.55f);
            Assert.That(menu.IsOpen, Is.False);
            Assert.That(menu.IsTransitioning, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(AudioListener.volume, Is.EqualTo(1).Within(.01f));
            Assert.That(hud.enabled, Is.True);
            menu.BeginPause();
            yield return null;
            Assert.That(menu.IsPaused, Is.True);
            Assert.That(menu.IsTransitioning, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            yield return new WaitForSecondsRealtime(.48f);
            Assert.That(menu.PauseProgress, Is.GreaterThan(.95f));
            Assert.That(AudioListener.volume, Is.LessThan(.5f));
            menu.BeginResume();
            yield return new WaitForSecondsRealtime(.48f);
            Assert.That(menu.IsPaused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(AudioListener.volume, Is.EqualTo(1).Within(.01f));
        }

        [UnityTest]
        public IEnumerator SettledCarAcceptsKeyboardAcceleratesSteersBrakesAndResets()
        {
            yield return LoadPrototype();
            yield return new WaitForSeconds(1);
            CreateTestKeyboard();
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            var input = car.GetComponent<CarInput>();
            Assert.That(car.IsGrounded, Is.True, "A chassis resting on asphalt must remain grounded.");
            Vector3 start = car.transform.position;
            Vector3 startForward = car.transform.forward;
            yield return Hold(Key.W, 1);
            Assert.That(input.Throttle, Is.EqualTo(1), "W must reach the vehicle input component.");
            Assert.That(Vector3.Dot(car.transform.position - start, startForward), Is.GreaterThan(2), "W must move the settled car out of the garage.");
            Assert.That(car.SpeedKph, Is.GreaterThan(10));
            Quaternion straight = car.transform.rotation;
            yield return Hold(Key.D, 0.5f);
            Assert.That(Quaternion.Angle(straight, car.transform.rotation), Is.GreaterThan(3), "D must steer a moving car.");
            yield return Hold(Key.Space, 1);
            Assert.That(car.SpeedKph, Is.LessThan(1), "Space must bring the car to rest.");
            yield return Hold(Key.S, 0.6f);
            Assert.That(Vector3.Dot(car.GetComponent<Rigidbody>().linearVelocity, car.transform.forward), Is.LessThan(-1));
            yield return Hold(Key.R, 0.15f);
            yield return Hold(Key.Space, 0.7f); // Allow the recovered chassis to settle.
            Assert.That(Vector3.Distance(car.transform.position, start), Is.LessThan(0.7f), "R must recover the car.");
            Assert.That(car.SpeedKph, Is.LessThan(1));
        }

        [UnityTest]
        public IEnumerator DepotLoadsEightParcelsAndEachDeliveryRemovesOne()
        {
            yield return LoadPrototype();
            yield return new WaitForSeconds(1);
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            var route = Object.FindFirstObjectByType<DeliveryRoute>();
            var cargo = car.GetComponent<CargoBedVisual>();
            var body = car.GetComponent<Rigidbody>();
            Assert.That(route.Phase, Is.EqualTo(DeliveryPhase.AwaitingPickup));
            Assert.That(route.Total, Is.EqualTo(8));
            Assert.That(cargo.VisibleCount, Is.Zero, "The pickup bed must be empty at the garage.");
            body.constraints = RigidbodyConstraints.FreezeAll;
            body.position = route.PickupPoint + Vector3.up * 0.95f;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
            Time.timeScale = 4;
            yield return new WaitForSeconds(2.1f);
            Assert.That(route.HasCargo, Is.True);
            Assert.That(cargo.VisibleCount, Is.EqualTo(8));
            for (int i = 0; i < route.Total; i++)
            {
                body.position = route.Destination + Vector3.up * 0.95f;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                Physics.SyncTransforms();
                yield return new WaitForSeconds(0.5f);
                Assert.That(route.Completed, Is.EqualTo(i), "A brief stop must not complete a delivery.");
                yield return new WaitForSeconds(1.4f);
                Assert.That(route.Completed, Is.EqualTo(i + 1));
                Assert.That(cargo.VisibleCount, Is.EqualTo(route.Total - i - 1));
            }
            yield return new WaitForSeconds(1.7f);
            Assert.That(route.IsComplete, Is.True);
            Assert.That(route.Completed, Is.EqualTo(8), "Completed deliveries must not count twice.");
        }

        [UnityTest]
        public IEnumerator PickupClimbsBeveledPavementWithoutBouncingBack()
        {
            yield return LoadPrototype();
            yield return new WaitForSeconds(1);
            CreateTestKeyboard();
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            var body = car.GetComponent<Rigidbody>();
            body.position = new Vector3(-59, 1.05f, 70);
            body.rotation = Quaternion.Euler(0, 90, 0);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
            yield return Hold(Key.Space, 0.7f);
            float roadHeight = body.position.y;
            yield return Hold(Key.W, 1.5f);
            Assert.That(body.position.x, Is.GreaterThan(-53), "Pickup must climb onto the park pavement.");
            Assert.That(body.position.y, Is.GreaterThan(roadHeight + 0.12f));
            Assert.That(Vector3.Dot(car.transform.up, Vector3.up), Is.GreaterThan(0.8f));
            Assert.That(body.linearVelocity.x, Is.GreaterThan(0), "Curb must not bounce the vehicle backward.");
        }

        [UnityTest]
        public IEnumerator ElectricPickupApproachesTwoHundredGradually()
        {
            yield return LoadPrototype();
            yield return new WaitForSeconds(1);
            CreateTestKeyboard();
            var runway = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runway.name = "Test-only long straight";
            runway.transform.position = new Vector3(0, -0.3f, 4000);
            runway.transform.localScale = new Vector3(100, 0.6f, 8000);
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            var body = car.GetComponent<Rigidbody>();
            body.position = new Vector3(0, 1.05f, 1000);
            body.rotation = Quaternion.identity;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
            yield return Hold(Key.Space, 0.7f);
            Time.timeScale = 4;
            yield return Hold(Key.W, 3);
            float earlySpeed = car.SpeedKph;
            yield return Hold(Key.W, 37);
            float lateSpeed = car.SpeedKph;
            yield return Hold(Key.W, 3);
            Assert.That(earlySpeed, Is.GreaterThan(45));
            Assert.That(car.SpeedKph, Is.InRange(185f, 210f));
            Assert.That(car.SpeedKph - lateSpeed, Is.LessThan(earlySpeed * 0.15f), "High speed acceleration must taper.");
            Debug.Log($"[Pickup speed] 3s: {earlySpeed:0.0} km/h; 40s: {lateSpeed:0.0}; 43s: {car.SpeedKph:0.0}");
        }

        [UnityTest]
        public IEnumerator ParkedPickupRestartsAndSteersOnPavement()
        {
            yield return LoadPrototype();
            yield return new WaitForSeconds(1);
            CreateTestKeyboard();
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            var body = car.GetComponent<Rigidbody>();
            body.position = new Vector3(-20, 1.35f, 70);
            body.rotation = Quaternion.identity;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
            yield return Hold(Key.Space, 1.2f);
            Assert.That(car.IsGrounded, Is.True);
            Assert.That(car.SpeedKph, Is.LessThan(1));
            Vector3 start = body.position;
            yield return Hold(Key.W, 1);
            Debug.Log($"[Pavement restart] Distance {Vector3.Distance(start, body.position):0.00}m, speed {car.SpeedKph:0.0}km/h, grounded {car.IsGrounded}");
            Assert.That(body.position.z - start.z, Is.GreaterThan(2), "A stopped pickup must regain traction on the pavement.");
            Quaternion rotation = body.rotation;
            yield return Hold(Key.D, 0.5f);
            Assert.That(Quaternion.Angle(rotation, body.rotation), Is.GreaterThan(2), "Pavement steering must remain effective.");
        }

        [UnityTest]
        public IEnumerator PickupRecoversGripAfterModerateSpeedTurn()
        {
            yield return LoadPrototype();
            yield return new WaitForSeconds(1);
            CreateTestKeyboard();
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.transform.position = new Vector3(0, -0.3f, 1200);
            pad.transform.localScale = new Vector3(300, 0.6f, 500);
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            var body = car.GetComponent<Rigidbody>();
            body.position = new Vector3(0, 1.1f, 1000);
            body.rotation = Quaternion.identity;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
            yield return Hold(Key.Space, 1);
            yield return Hold(Key.W, 2.5f);
            float entrySpeed = car.SpeedKph;
            yield return Hold(Key.D, 0.25f);
            yield return Hold(Key.A, 0.45f);
            yield return Hold(Key.W, 1);
            Vector3 local = car.transform.InverseTransformDirection(body.linearVelocity);
            float slip = Mathf.Abs(Mathf.Atan2(local.x, Mathf.Abs(local.z)) * Mathf.Rad2Deg);
            Debug.Log($"[Grip recovery] Entry {entrySpeed:0.0}km/h, residual slip {slip:0.0}deg");
            Assert.That(entrySpeed, Is.GreaterThan(50));
            Assert.That(slip, Is.LessThan(10), "Pickup must recover lateral grip after reversing a moderate-speed steering input.");
            Assert.That(Vector3.Dot(car.transform.up, Vector3.up), Is.GreaterThan(0.85f));
        }

        [UnityTest]
        public IEnumerator CKeyCyclesOrbitCockpitHoodAndFrontWheelViews()
        {
            yield return LoadPrototype();
            yield return new WaitForSeconds(1);
            CreateTestKeyboard();
            var drivingCamera = Object.FindFirstObjectByType<ChaseCamera>();
            var cockpit = Object.FindFirstObjectByType<CockpitDisplay>();
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            var lens = drivingCamera.GetComponent<Camera>();
            Assert.That(drivingCamera.CurrentView, Is.EqualTo(CameraViewMode.OrbitChase));
            Assert.That(cockpit.IsVisible, Is.False);

            physicalMouse = Mouse.current;
            if (physicalMouse != null) InputSystem.DisableDevice(physicalMouse);
            mouse = InputSystem.AddDevice<Mouse>();
            mouse.MakeCurrent();
            InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left));
            InputSystem.QueueDeltaStateEvent(mouse.delta, new Vector2(50, 0));
            yield return null;
            Assert.That(Mathf.Abs(drivingCamera.LookYaw), Is.GreaterThan(1), "Every camera must accept mouse look.");

            car.GetComponent<Rigidbody>().linearVelocity = car.transform.forward * 40;
            yield return Tap(Key.C);
            Assert.That(drivingCamera.CurrentView, Is.EqualTo(CameraViewMode.Cockpit));
            Assert.That(cockpit.IsVisible, Is.True);
            Assert.That(drivingCamera.LookYaw, Is.Zero.Within(0.01f), "Changing views must reset the look direction.");
            Assert.That(drivingCamera.ViewTransition, Is.GreaterThan(0).And.LessThan(1), "C-key camera changes must animate rather than jump.");
            yield return new WaitForSecondsRealtime(.58f);
            yield return new WaitForEndOfFrame();
            Assert.That(lens.fieldOfView, Is.GreaterThan(65), "Cockpit acceleration must use a mild wide-angle FOV cue.");
            Vector3 cockpitMount = car.transform.InverseTransformPoint(lens.transform.position);
            Assert.That(Vector3.Distance(new Vector3(-0.43f, 1.2f, -0.05f), cockpitMount), Is.LessThan(0.04f),
                "The cockpit camera must remain rigidly mounted during acceleration.");
            yield return Tap(Key.C);
            yield return new WaitForSecondsRealtime(.58f);
            Assert.That(drivingCamera.CurrentView, Is.EqualTo(CameraViewMode.Hood));
            Assert.That(cockpit.IsVisible, Is.False);
            Assert.That(lens.fieldOfView, Is.EqualTo(62).Within(0.35f), "Hood view must settle back to the normal FOV without an acceleration effect.");
            yield return Tap(Key.C);
            yield return new WaitForSecondsRealtime(.58f);
            Assert.That(drivingCamera.CurrentView, Is.EqualTo(CameraViewMode.FrontLeftWheel));
            Assert.That(Vector3.Dot(lens.transform.forward, car.transform.forward), Is.GreaterThan(0.95f),
                "The wheel view must primarily show the road ahead.");
            yield return Tap(Key.C);
            yield return new WaitForSecondsRealtime(.58f);
            Assert.That(drivingCamera.CurrentView, Is.EqualTo(CameraViewMode.OrbitChase));
        }

        [UnityTest]
        public IEnumerator MKeyExpandsMapAndRouteUsesRoadGraph()
        {
            yield return LoadPrototype();
            CreateTestKeyboard();
            var hud = Object.FindFirstObjectByType<DeliveryHud>();
            var drivingCamera = Object.FindFirstObjectByType<ChaseCamera>();
            Assert.That(hud.IsMapExpanded, Is.False);
            Assert.That(hud.CurrentRoadRoute, Is.Not.Null);
            Assert.That(hud.CurrentRoadRoute.Count, Is.GreaterThan(2), "The next delivery must use a road route rather than one direct line.");
            var depotRoute = RoadRoutePlanner.FindRoute(new Vector3(112, 0, -230), new Vector3(20, 0, -150));
            for (int i = 0; i < depotRoute.Count - 1; i++)
            {
                Vector3 delta = depotRoute[i + 1] - depotRoute[i];
                Assert.That(Mathf.Min(Mathf.Abs(delta.x), Mathf.Abs(delta.z)), Is.LessThan(.1f),
                    "The logistics access route must join the city grid instead of cutting diagonally across a block.");
            }
            yield return Tap(Key.M);
            Assert.That(hud.IsMapExpanded, Is.True);
            Assert.That(hud.MapExpansion, Is.GreaterThan(0).And.LessThan(1), "The map must animate rather than jump to full size.");
            yield return new WaitForSecondsRealtime(.42f);
            Assert.That(hud.MapExpansion, Is.GreaterThan(.95f));
            Assert.That(drivingCamera.LookInputEnabled, Is.False, "Dragging the large map must not rotate the driving camera.");
            yield return Tap(Key.M);
            yield return new WaitForSecondsRealtime(.42f);
            Assert.That(hud.MapExpansion, Is.LessThan(.05f));
            Assert.That(drivingCamera.LookInputEnabled, Is.True);
        }

        [UnityTest]
        public IEnumerator PickupLightsRespondToBrakeReverseSignalsAndHazards()
        {
            yield return LoadPrototype();
            yield return new WaitForSeconds(1);
            CreateTestKeyboard();
            var lighting = Object.FindFirstObjectByType<VehicleLighting>();
            Object.FindFirstObjectByType<NightCourier.Prototype.VehicleAppearance>()
                .Select(NightCourier.Prototype.CourierVehicle.Pickup, 0, false);
            var brakeBar = GameObject.Find("Rear brake light bar");
            Assert.That(lighting, Is.Not.Null);
            Assert.That(brakeBar, Is.Not.Null);
            Assert.That(brakeBar.transform.localScale.x, Is.GreaterThan(1.8f), "The rear brake lamp must be a full-width light bar.");
            Assert.That(GameObject.Find("Angular hood"), Is.Not.Null);
            Assert.That(GameObject.Find("Angular front fascia"), Is.Not.Null);
            Assert.That(GameObject.Find("Front left wheel arch 4"), Is.Not.Null);
            Assert.That(GameObject.Find("Angular hood").GetComponent<Renderer>().sharedMaterial.GetFloat("_Smoothness"), Is.LessThan(0.3f));
            // Visual overhangs stay independent from the curb-friendly physics envelope.
            Assert.That(GameObject.Find("Front left indicator"), Is.Not.Null);
            Assert.That(GameObject.Find("Rear left indicator").transform.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(GameObject.Find("Left round headlight"), Is.Not.Null);
            var body = lighting.GetComponent<Rigidbody>();
            var motor = lighting.GetComponent<ElectricMotorAudio>();
            Assert.That(motor, Is.Not.Null);
            Assert.That(motor.GetComponent<AudioSource>().clip, Is.Not.Null);
            Assert.That(lighting.HeadlightsOn, Is.True);
            Assert.That(GameObject.Find("Rear running light bar").GetComponent<Renderer>().enabled, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return null;
            Assert.That(lighting.BrakeLightsOn, Is.True);
            Assert.That(brakeBar.GetComponent<Renderer>().enabled, Is.True);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            Assert.That(lighting.BrakeLightsOn, Is.False);

            body.linearVelocity = lighting.transform.forward * 10;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S));
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(lighting.BrakeLightsOn, Is.True, "S must brake while the pickup is still travelling forward.");
            Assert.That(lighting.ReverseLightsOn, Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;

            body.linearVelocity = -lighting.transform.forward * 3;
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(lighting.ReverseLightsOn, Is.True, "Reverse lamps must follow actual backward movement without requiring S to remain held.");
            Assert.That(GameObject.Find("Rear left reverse light").GetComponent<Renderer>().enabled, Is.True);
            float idlePitch = motor.CurrentPitch;
            body.linearVelocity = lighting.transform.forward * 35;
            yield return new WaitForSeconds(0.25f);
            Assert.That(motor.CurrentPitch, Is.GreaterThan(idlePitch + 0.2f), "The EV whine must rise in pitch with road speed.");
            yield return null;

            yield return Tap(Key.Comma);
            Assert.That(lighting.SignalMode, Is.EqualTo(TurnSignalMode.Left));
            Assert.That(lighting.LeftIndicatorOn, Is.True);
            Assert.That(lighting.RightIndicatorOn, Is.False);
            Assert.That(GameObject.Find("Front left indicator").GetComponent<Renderer>().enabled, Is.True);
            yield return new WaitForSecondsRealtime(0.55f);
            Assert.That(lighting.LeftIndicatorOn, Is.False, "Turn signals must visibly blink.");
            yield return Tap(Key.Comma);
            Assert.That(lighting.SignalMode, Is.EqualTo(TurnSignalMode.Off));

            yield return TapTogether(Key.Comma, Key.Period);
            Assert.That(lighting.SignalMode, Is.EqualTo(TurnSignalMode.Hazard));
            Assert.That(lighting.LeftIndicatorOn && lighting.RightIndicatorOn, Is.True);
            yield return TapTogether(Key.Comma, Key.Period);
            Assert.That(lighting.SignalMode, Is.EqualTo(TurnSignalMode.Off));

            yield return HoldTogether(Key.Comma, Key.Period, 2.15f);
            Assert.That(lighting.HeadlightsOn, Is.False);
            Assert.That(GameObject.Find("Rear running light bar").GetComponent<Renderer>().enabled, Is.False);
            Assert.That(GameObject.Find("Headlight beam").GetComponent<Light>().enabled, Is.False);
            yield return HoldTogether(Key.Comma, Key.Period, 2.15f);
            Assert.That(lighting.HeadlightsOn, Is.True);
        }

        [UnityTest]
        public IEnumerator BatteryStartsHalfFullDrainsAndChargesWhileParked()
        {
            yield return LoadPrototype();
            yield return new WaitForSeconds(1);
            CreateTestKeyboard();
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            var body = car.GetComponent<Rigidbody>();
            var battery = car.GetComponent<VehicleBattery>();
            Assert.That(battery.Charge01, Is.EqualTo(0.5f).Within(0.01f));
            float initial = battery.Charge01;
            yield return Hold(Key.W, 1.2f);
            Assert.That(battery.Charge01, Is.LessThan(initial));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            var charger = GameObject.Find("South Quarter EV hub").GetComponent<EVChargingStation>();
            body.position = charger.transform.position + Vector3.up * 0.95f;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
            float beforeCharge = battery.Charge01;
            yield return new WaitForSeconds(1);
            Debug.Log($"[Battery charge] active {battery.IsCharging}, charge {beforeCharge:0.000}->{battery.Charge01:0.000}, speed {car.SpeedKph:0.0}, distance {Vector2.Distance(new Vector2(car.transform.position.x, car.transform.position.z), new Vector2(charger.transform.position.x, charger.transform.position.z)):0.0}, stations {EVChargingStation.Stations.Count}");
            Assert.That(battery.IsCharging, Is.True);
            Assert.That(battery.Charge01, Is.GreaterThan(beforeCharge + 0.1f));
        }

        [UnityTest]
        public IEnumerator HighSpeedSteeringBuildsMoreGraduallyThanCitySpeedSteering()
        {
            yield return LoadPrototype();
            yield return new WaitForSeconds(1);
            CreateTestKeyboard();
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.transform.position = new Vector3(0, -0.3f, 1200);
            pad.transform.localScale = new Vector3(300, 0.6f, 500);
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            var body = car.GetComponent<Rigidbody>();
            body.position = new Vector3(0, 1.1f, 1000);
            body.rotation = Quaternion.identity;
            Physics.SyncTransforms();
            yield return Hold(Key.Space, 0.8f);

            body.linearVelocity = Vector3.forward * 12;
            yield return Hold(Key.D, 0.2f);
            float citySpeedAngle = Mathf.Abs(car.SteeringAngle);
            yield return Hold(Key.Space, 0.8f);
            body.rotation = Quaternion.identity;
            body.linearVelocity = Vector3.forward * 48;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
            yield return Hold(Key.D, 0.2f);
            float highSpeedAngle = Mathf.Abs(car.SteeringAngle);

            Debug.Log($"[Progressive steering] City {citySpeedAngle:0.0}deg; high speed {highSpeedAngle:0.0}deg");
            Assert.That(citySpeedAngle, Is.GreaterThan(highSpeedAngle + 1.5f));
            Assert.That(highSpeedAngle, Is.LessThan(8), "High speed keyboard steering must build progressively.");
        }

        [UnityTest]
        public IEnumerator ExpandedCityContainsLongBlocksAndFourSixEightLaneRoads()
        {
            yield return LoadPrototype();
            yield return null;
            Assert.That(CityBuilder.MaxX - CityBuilder.MinX, Is.GreaterThanOrEqualTo(700));
            Assert.That(CityBuilder.MaxZ - CityBuilder.MinZ, Is.GreaterThanOrEqualTo(600));
            CollectionAssert.Contains(CityBuilder.VerticalLanes, 4);
            CollectionAssert.Contains(CityBuilder.VerticalLanes, 6);
            CollectionAssert.Contains(CityBuilder.VerticalLanes, 8);
            Assert.That(Object.FindObjectsByType<CityGeometry>(FindObjectsSortMode.None).Length, Is.GreaterThanOrEqualTo(9));
            Assert.That(GameObject.Find("North-south 8-lane avenue"), Is.Not.Null);
            Assert.That(GameObject.Find("East-west 6-lane avenue"), Is.Not.Null);
            Assert.That(RoadMapTexture.WorldBounds.width, Is.GreaterThan(1700), "The complete playable world should be roughly twice as wide.");
            Assert.That(GameObject.Find("Six lane intercity expressway"), Is.Not.Null);
            Assert.That(GameObject.Find("WEST TOLL / HARBOR CITY sign"), Is.Not.Null);
            Assert.That(GameObject.Find("EAST TOLL / CITY CENTRE sign"), Is.Not.Null);
            Assert.That(GameObject.Find("Open toll arch beam"), Is.Not.Null);
            Assert.That(GameObject.Find("Toll booth"), Is.Null, "The expressway toll must leave every traffic lane open.");
            Assert.That(GameObject.Find("Harbor City district"), Is.Not.Null);
            Assert.That(GameObject.Find("Central logistics warehouse"), Is.Not.Null);
            Assert.That(GameObject.Find("Garage rear wall"), Is.Not.Null);
            Assert.That(GameObject.Find("City streetlight glow"), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<TrafficSignalController>(FindObjectsSortMode.None).Length, Is.GreaterThan(10));
            Assert.That(GameObject.Find("Red traffic light"), Is.Not.Null);
            Assert.That(GameObject.Find("West city boundary barrier"), Is.Not.Null);
            Assert.That(EVChargingStation.Stations.Count, Is.GreaterThanOrEqualTo(5));
            foreach (var collider in Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
                if (collider.name == "Noise barrier")
                    Assert.That(collider.bounds.Contains(new Vector3(735, 1, -17.8f)), Is.False,
                        "The north noise wall must leave the rest-area entrance open.");
        }

        [UnityTest]
        public IEnumerator ScenicRoadHasContinuousDriveableSurface()
        {
            yield return LoadPrototype();
            yield return null;
            Physics.SyncTransforms();
            var path = ScenicRoadPath.Points;
            float length = 0;
            float highest = 0;
            float grade = 0;
            for (int i = 0; i < path.Count; i++)
            {
                highest = Mathf.Max(highest, path[i].y);
                if (i > 0)
                {
                    Vector3 delta = path[i] - path[i - 1];
                    length += delta.magnitude;
                    grade = Mathf.Max(grade, Mathf.Abs(delta.y) / new Vector2(delta.x, delta.z).magnitude);
                }
                foreach (float lane in new[] { -3.5f, 0, 3.5f })
                {
                    Vector3 position = path[i] + ScenicRoadPath.Right(i) * lane;
                    bool found = Physics.Raycast(position + Vector3.up * 2, Vector3.down, out var hit, 2.3f,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                    Assert.That(found, Is.True, $"Missing road at point {i}, lane {lane}.");
                    Assert.That(Mathf.Abs(hit.point.y - position.y), Is.LessThan(0.15f), $"Road seam or obstruction at point {i}.");
                    Assert.That(hit.normal.y, Is.GreaterThan(0.94f));
                }
            }
            Debug.Log($"[Scenic route] Length {length:0}m, summit {highest:0.0}m, maximum grade {grade * 100:0.0}%");
            Assert.That(length, Is.GreaterThan(850));
            Assert.That(highest, Is.GreaterThan(18));
            Assert.That(grade, Is.LessThan(0.22f));
            Vector2 entrance = new Vector2(path[0].x, path[0].z);
            Vector2 exit = new Vector2(path[path.Count - 1].x, path[path.Count - 1].z);
            foreach (var signal in Object.FindObjectsByType<TrafficSignalController>(FindObjectsSortMode.None))
            {
                Vector2 position = new Vector2(signal.transform.position.x, signal.transform.position.z);
                Assert.That(Mathf.Min(Vector2.Distance(position, entrance), Vector2.Distance(position, exit)),
                    Is.GreaterThan(10), "The bypass portals must not carry city traffic signals.");
            }
            foreach (var collider in Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
            {
                Vector2 position = new Vector2(collider.transform.position.x, collider.transform.position.z);
                float portalDistance = Mathf.Min(Vector2.Distance(position, entrance), Vector2.Distance(position, exit));
                if (collider.name == "Safety rail")
                    Assert.That(portalDistance, Is.GreaterThan(38), "Safety rails must open well before each city connection.");
                else if (collider.name == "Hillside trunk")
                    Assert.That(portalDistance, Is.GreaterThan(55), "Trees must stay clear of each bypass connection.");
            }
        }

        [UnityTest]
        public IEnumerator PickupDrivesUphillAndRestartsOnViaduct()
        {
            yield return LoadPrototype();
            yield return new WaitForSeconds(1);
            CreateTestKeyboard();
            var car = Object.FindFirstObjectByType<ArcadeCarController>();
            var body = car.GetComponent<Rigidbody>();
            var path = ScenicRoadPath.Points;
            foreach (Vector3 reference in new[] { new Vector3(535, 0, 270), new Vector3(740, 0, 450) })
            {
                int nearest = 0;
                float distance = float.MaxValue;
                for (int i = 2; i < path.Count - 2; i++)
                {
                    float candidate = Vector2.Distance(new Vector2(path[i].x, path[i].z), new Vector2(reference.x, reference.z));
                    if (candidate < distance) { distance = candidate; nearest = i; }
                }
                Vector3 direction = (path[nearest + 1] - path[nearest - 1]).normalized;
                body.position = path[nearest] + Vector3.up * 1.2f;
                body.rotation = Quaternion.LookRotation(direction, Vector3.up);
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                Physics.SyncTransforms();
                yield return Hold(Key.Space, 1);
                Assert.That(car.IsGrounded, Is.True);
                Vector3 start = body.position;
                yield return Hold(Key.W, 1.1f);
                Assert.That(Vector3.Dot(body.position - start, direction), Is.GreaterThan(2));
                Assert.That(car.IsGrounded, Is.True, "Suspension must keep contact on slopes and the viaduct.");
                Assert.That(Vector3.Dot(car.transform.up, Vector3.up), Is.GreaterThan(0.8f));
            }
        }

        private static IEnumerator LoadPrototype(bool beginShift = true)
        {
#if UNITY_EDITOR
            AsyncOperation operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/_NightCourier/Scenes/DrivingPrototype.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            AsyncOperation operation = SceneManager.LoadSceneAsync("DrivingPrototype");
#endif
            yield return operation;
            yield return null;
            if (beginShift)
            {
                Object.FindFirstObjectByType<NightCourier.Prototype.VehicleAppearance>()?
                    .Select(NightCourier.Prototype.CourierVehicle.Pickup, 0, false);
                var menu = Object.FindFirstObjectByType<NightCourier.Prototype.MainMenuController>();
                menu?.BeginShift(true);
                yield return null;
            }
        }

        private void CreateTestKeyboard()
        {
            physicalKeyboard = Keyboard.current;
            if (physicalKeyboard != null) InputSystem.DisableDevice(physicalKeyboard);
            keyboard = InputSystem.AddDevice<Keyboard>();
            keyboard.MakeCurrent();
        }

        private IEnumerator Hold(Key key, float seconds)
        {
            float until = Time.time + seconds;
            while (Time.time < until)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
                yield return null;
            }
        }

        private IEnumerator Tap(Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
        }

        private IEnumerator TapTogether(Key first, Key second)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(first, second));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
        }

        private IEnumerator HoldTogether(Key first, Key second, float seconds)
        {
            float until = Time.unscaledTime + seconds;
            while (Time.unscaledTime < until)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(first, second));
                yield return null;
            }
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
        }
    }
}
