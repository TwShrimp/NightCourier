using System.Collections.Generic;
using NightCourier.CameraSystem;
using NightCourier.Vehicle;
using NightCourier.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NightCourier.Delivery
{
    public sealed class DeliveryHud : MonoBehaviour
    {
        private static readonly Color Ink = new Color(0.025f, 0.045f, 0.065f, 0.94f);
        private static readonly Color Cyan = new Color(0.3f, 0.9f, 0.95f);
        private static readonly Color RouteColor = new Color(0.2f, 1f, 0.76f);
        private static readonly Vector3[] LabelPositions =
        {
            new Vector3(0, 0, 255), new Vector3(1300, 0, 255), new Vector3(685, 0, -30),
            new Vector3(740, 0, 450), new Vector3(155, 0, -230), new Vector3(735, 0, 4)
        };
        private static readonly string[] LabelNames =
        {
            "MAIN CITY", "HARBOR CITY", "INTERCITY EXPRESSWAY", "RIVER VIADUCT", "LOGISTICS", "REST AREA"
        };

        private DeliveryRoute route;
        private ArcadeCarController car;
        private CarInput input;
        private VehicleBattery battery;
        private ChaseCamera drivingCamera;
        private GUIStyle label;
        private Texture2D roadMap;
        private Texture2D routeMap;
        private List<Vector3> currentRoute;
        private Vector3 lastRouteOrigin = new Vector3(float.MaxValue, 0, 0);
        private Vector3 lastRouteDestination = new Vector3(float.MaxValue, 0, 0);
        private Vector2 expandedCenter;
        private bool mapExpanded;
        private float mapExpansion;

        public bool IsMapExpanded => mapExpanded;
        public float MapExpansion => mapExpansion;
        public IReadOnlyList<Vector3> CurrentRoadRoute => currentRoute;

        public void Initialize(DeliveryRoute deliveries, ArcadeCarController vehicle, ChaseCamera cameraController)
        {
            route = deliveries;
            car = vehicle;
            input = car.GetComponent<CarInput>();
            battery = car.GetComponent<VehicleBattery>();
            drivingCamera = cameraController;
            roadMap = RoadMapTexture.Create();
            routeMap = RoadMapTexture.CreateRouteOverlay();
            expandedCenter = new Vector2(car.transform.position.x, car.transform.position.z);
            RefreshRoute(true);
        }

        private void Update()
        {
            if (car == null || route == null) return;
            bool toggle = false;
            foreach (var device in InputSystem.devices)
                if (device is Keyboard keyboard && keyboard.enabled)
                    toggle |= keyboard.mKey.wasPressedThisFrame;
            if (toggle)
            {
                mapExpanded = !mapExpanded;
                if (mapExpanded) expandedCenter = new Vector2(car.transform.position.x, car.transform.position.z);
            }
            mapExpansion = Mathf.MoveTowards(mapExpansion, mapExpanded ? 1 : 0, Time.unscaledDeltaTime / .38f);
            if (drivingCamera != null) drivingCamera.LookInputEnabled = mapExpansion < .02f;
            RefreshRoute(false);
        }

        private void RefreshRoute(bool force)
        {
            if (route.IsComplete)
            {
                currentRoute = null;
                RoadMapTexture.UpdateRoute(routeMap, null);
                return;
            }
            Vector3 origin = car.transform.position;
            Vector3 destination = route.Destination;
            if (!force && (origin - lastRouteOrigin).sqrMagnitude < 18 * 18 &&
                (destination - lastRouteDestination).sqrMagnitude < 1) return;
            lastRouteOrigin = origin;
            lastRouteDestination = destination;
            currentRoute = RoadRoutePlanner.FindRoute(origin, destination);
            RoadMapTexture.UpdateRoute(routeMap, currentRoute);
        }

        private void OnGUI()
        {
            if (car == null || route == null) return;
            if (label == null) label = new GUIStyle(GUI.skin.label);
            Matrix4x4 savedMatrix = GUI.matrix;
            Color savedColor = GUI.color;
            float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            Panel(new Rect(28, 28, 360, 162), Ink);
            Panel(new Rect(28, 28, 4, 162), Cyan);
            Text(new Rect(48, 39, 320, 30), "N I G H T   C O U R I E R", 21, Color.white, true);
            Text(new Rect(48, 72, 310, 22), "00:47   /   RAIN   /   " + ScenicRoadPath.District(car.transform.position), 12, Cyan);
            Text(new Rect(48, 104, 320, 27), route.DestinationName, 19, Color.white);
            float distance = Vector3.Distance(car.transform.position, route.Destination);
            Text(new Rect(48, 140, 330, 24), route.IsComplete ? route.ProgressText : $"{distance:0} m   /   {route.ProgressText}", 13, Cyan);
            if (route.StoppedTime > 0)
            {
                Panel(new Rect(width / 2 - 160, height * 0.65f, 320, 54), Ink);
                Text(new Rect(width / 2 - 140, height * 0.65f + 5, 280, 24), route.StopAction, 16, Cyan);
                Panel(new Rect(width / 2 - 140, height * 0.65f + 36, 280 * route.StoppedTime / route.RequiredStopTime, 4), Cyan);
            }
            if (drivingCamera.CurrentView != CameraViewMode.Cockpit)
            {
                Panel(new Rect(28, height - 136, 160, 108), Ink);
                Text(new Rect(46, height - 131, 100, 58), $"{car.SpeedKph:00}", 48, Color.white, true);
                Text(new Rect(48, height - 72, 128, 24), "KM/H  /  " + (car.IsBraking ? "BRAKE" : car.DriveLabel), 13, Cyan);
            }
            Text(new Rect(205, height - 67, 1030, 24), "WASD Drive   SPACE Brake   R Recover   C Camera   M Map   , / . Signals   Hold both 2s Lights", 14, Color.white);
            Text(new Rect(205, height - 43, 850, 22), $"{drivingCamera.CurrentViewName}   •   Hold left mouse to look   •   Input {input.Throttle:+0;-0;0} / {input.Steering:+0;-0;0}   •   {(car.IsGrounded ? "ROAD CONTACT" : "AIRBORNE")}", 12, Cyan);
            DrawBattery(new Rect(width - 232, height - 296, 204, 36));

            Rect smallPanel = new Rect(width - 232, height - 252, 204, 224);
            float largeSize = Mathf.Min(720, height - 80, width - 80);
            Rect largePanel = new Rect((width - largeSize) * .5f, (height - largeSize) * .5f, largeSize, largeSize);
            float blend = Mathf.SmoothStep(0, 1, mapExpansion);
            if (blend > .001f) Panel(new Rect(0, 0, width, height), new Color(0, 0, 0, .58f * blend));
            DrawMap(Lerp(smallPanel, largePanel, blend), smallPanel, largePanel, blend);

            GUI.matrix = savedMatrix;
            GUI.color = savedColor;
        }

        private void DrawBattery(Rect rect)
        {
            Panel(rect, Ink);
            float charge = battery == null ? 0 : battery.Charge01;
            Color gauge = charge < 0.18f ? new Color(1, 0.2f, 0.15f) : battery != null && battery.IsCharging ? Color.green : Cyan;
            Text(new Rect(rect.x + 10, rect.y + 3, 150, 22), battery != null && battery.IsCharging ? "BATTERY / CHARGING" : $"BATTERY  {charge * 100:0}%", 11, Color.white, true);
            Panel(new Rect(rect.x + 10, rect.y + 25, 184, 4), new Color(0.08f, 0.12f, 0.15f));
            Panel(new Rect(rect.x + 10, rect.y + 25, 184 * charge, 4), gauge);
        }

        private void DrawMap(Rect panelRect, Rect smallPanel, Rect largePanel, float blend)
        {
            Panel(panelRect, Ink);
            string heading = blend > .55f ? "DISTRICT MAP     N ↑     M  CLOSE     DRAG TO MOVE" : "DISTRICT MAP      N ↑";
            Text(new Rect(panelRect.x + 12, panelRect.y + 6, panelRect.width - 24, 30), heading, Mathf.RoundToInt(Mathf.Lerp(12, 15, blend)), Cyan, true);
            Rect smallArea = new Rect(smallPanel.x + 12, smallPanel.y + 32, 180, 180);
            Rect largeArea = new Rect(largePanel.x + 20, largePanel.y + 44, largePanel.width - 40, largePanel.height - 64);
            Rect area = Lerp(smallArea, largeArea, blend);
            float smallRadius = car.transform.position.x > 100 ? 150 : 108;
            float radius = Mathf.Lerp(smallRadius, 360, blend);
            Vector2 carCenter = new Vector2(car.transform.position.x, car.transform.position.z);
            Vector2 center = Vector2.Lerp(carCenter, expandedCenter, blend);
            HandleMapDrag(area, radius, blend);
            if (blend > .98f) center = expandedCenter;

            Rect bounds = RoadMapTexture.WorldBounds;
            var uv = new Rect((center.x - radius - bounds.xMin) / bounds.width,
                (center.y - radius - bounds.yMin) / bounds.height, radius * 2 / bounds.width, radius * 2 / bounds.height);
            GUI.BeginGroup(area);
            Rect local = new Rect(0, 0, area.width, area.height);
            GUI.DrawTextureWithTexCoords(local, roadMap, uv);
            if (!route.IsComplete) GUI.DrawTextureWithTexCoords(local, routeMap, uv, true);
            DrawMapLabels(local, center, radius, blend);
            DrawChargingStations(local, center, radius, blend);
            if (!route.IsComplete)
            {
                Vector2 stop = Map(route.Destination, local, center, radius);
                if (local.Contains(stop))
                {
                    float pulse = 7 + Mathf.Sin(Time.unscaledTime * 4) * 2;
                    Panel(new Rect(stop.x - pulse * .5f, stop.y - pulse * .5f, pulse, pulse), RouteColor);
                }
            }
            Vector2 player = Map(car.transform.position, local, center, radius);
            Panel(new Rect(player.x - 5, player.y - 5, 10, 10), Color.white);
            Vector2 direction = new Vector2(car.transform.forward.x, -car.transform.forward.z).normalized;
            for (int step = 1; step <= 4; step++)
            {
                Vector2 point = player + direction * (step * 3f);
                Panel(new Rect(point.x - 1.5f, point.y - 1.5f, 3, 3), Color.yellow);
            }
            GUI.EndGroup();
        }

        private void HandleMapDrag(Rect area, float radius, float blend)
        {
            Event inputEvent = Event.current;
            if (blend < .98f || inputEvent.type != EventType.MouseDrag || inputEvent.button != 0 || !area.Contains(inputEvent.mousePosition)) return;
            expandedCenter.x -= inputEvent.delta.x / area.width * radius * 2;
            expandedCenter.y += inputEvent.delta.y / area.height * radius * 2;
            Rect bounds = RoadMapTexture.WorldBounds;
            expandedCenter.x = Mathf.Clamp(expandedCenter.x, bounds.xMin + radius, bounds.xMax - radius);
            expandedCenter.y = Mathf.Clamp(expandedCenter.y, bounds.yMin + radius, bounds.yMax - radius);
            inputEvent.Use();
        }

        private void DrawMapLabels(Rect area, Vector2 center, float radius, float blend)
        {
            int fontSize = Mathf.RoundToInt(Mathf.Lerp(8, 13, blend));
            for (int i = 0; i < LabelPositions.Length; i++)
            {
                Vector2 point = Map(LabelPositions[i], area, center, radius);
                if (!area.Contains(point)) continue;
                Text(new Rect(point.x - 78, point.y - 18, 156, 20), LabelNames[i], fontSize, new Color(.8f, .94f, .96f, .9f), true);
            }
        }

        private void DrawChargingStations(Rect area, Vector2 center, float radius, float blend)
        {
            float size = Mathf.Lerp(8, 15, blend);
            foreach (EVChargingStation station in EVChargingStation.Stations)
            {
                Vector2 point = Map(station.transform.position, area, center, radius);
                if (!area.Contains(point)) continue;
                Panel(new Rect(point.x - size * .5f, point.y - size * .5f, size * .72f, size), Cyan);
                Panel(new Rect(point.x - size * .28f, point.y - size * .28f, size * .3f, size * .24f), Ink);
                Panel(new Rect(point.x + size * .2f, point.y - size * .25f, size * .25f, size * .12f), Cyan);
                if (blend > .75f) Text(new Rect(point.x + 11, point.y - 8, 70, 18), "EV", 11, Cyan, true);
            }
        }

        private static Rect Lerp(Rect a, Rect b, float t) => new Rect(
            Mathf.Lerp(a.x, b.x, t), Mathf.Lerp(a.y, b.y, t),
            Mathf.Lerp(a.width, b.width, t), Mathf.Lerp(a.height, b.height, t));

        private static Vector2 Map(Vector3 position, Rect area, Vector2 center, float radius) => new Vector2(
            area.x + ((position.x - center.x) / (radius * 2) + 0.5f) * area.width,
            area.y + (0.5f - (position.z - center.y) / (radius * 2)) * area.height);

        private void OnDisable()
        {
            if (drivingCamera != null) drivingCamera.LookInputEnabled = true;
        }

        private void OnDestroy()
        {
            if (roadMap != null) Destroy(roadMap);
            if (routeMap != null) Destroy(routeMap);
        }

        private void Text(Rect rect, string text, int size, Color color, bool bold = false)
        {
            label.fontSize = size;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            label.normal.textColor = color;
            GUI.Label(rect, text, label);
        }

        private static void Panel(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
