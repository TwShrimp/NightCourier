using NightCourier.Vehicle;
using UnityEngine;

namespace NightCourier.World
{
    /// <summary>Connects the main city, expressway, and Harbor City in one road plan.</summary>
    public sealed class WorldExpansionBuilder : MonoBehaviour
    {
        public const float HighwayStartX = 350, HighwayEndX = 1020;
        public const float SecondCityMinX = 1000, SecondCityMaxX = 1600;
        public const float SecondCityMinZ = -300, SecondCityMaxZ = 300;
        public static readonly float[] SecondCityVerticalRoads = { 1020, 1110, 1200, 1290, 1380, 1470, 1580 };
        public static readonly int[] SecondCityVerticalLanes = { 6, 4, 6, 8, 6, 4, 6 };
        public static readonly float[] SecondCityHorizontalRoads = { -270, -190, -110, -30, 50, 130, 210, 290 };
        public static readonly int[] SecondCityHorizontalLanes = { 4, 4, 6, 6, 6, 4, 6, 4 };
        public static readonly Vector3[] HighwayPoints =
        {
            new Vector3(350, 0, -30), new Vector3(420, 0, -30), new Vector3(520, 0, -30),
            new Vector3(700, 0, -30), new Vector3(880, 0, -30), new Vector3(1020, 0, -30)
        };

        private CityPalette palette;
        private CityGeometry geometry;

        public void Build(CityPalette colors)
        {
            palette = colors;
            BuildGarageAndDepot();
            BuildExpressway();
            BuildHarborCity();
            BuildChargingNetwork();
        }

        private void BuildGarageAndDepot()
        {
            var root = Root("Working courier garage and logistics block");
            geometry = root.AddComponent<CityGeometry>();
            Vector3 garage = new Vector3(67, 0, -230);
            geometry.Box("Garage concrete floor", garage + Vector3.up * .34f, new Vector3(54, .12f, 45), palette.Metal, true);
            geometry.Box("Garage rear wall", garage + new Vector3(27, 5.8f, 0), new Vector3(.6f, 11, 45), palette.Facades[3], true);
            geometry.Box("Garage north wall", garage + new Vector3(0, 5.8f, 22.2f), new Vector3(54, 11, .6f), palette.Facades[3], true);
            geometry.Box("Garage south wall", garage + new Vector3(0, 5.8f, -22.2f), new Vector3(54, 11, .6f), palette.Facades[3], true);
            geometry.Box("Garage roof", garage + new Vector3(0, 11.3f, 0), new Vector3(55, .7f, 46), palette.Metal, true);
            geometry.Box("Garage front north pier", garage + new Vector3(-27, 5.8f, 16.6f), new Vector3(.7f, 11, 11), palette.Facades[3], true);
            geometry.Box("Garage front south pier", garage + new Vector3(-27, 5.8f, -16.6f), new Vector3(.7f, 11, 11), palette.Facades[3], true);
            geometry.Box("Garage door lintel", garage + new Vector3(-27, 9.4f, 0), new Vector3(.7f, 3.8f, 22.5f), palette.Facades[3], true);
            geometry.Box("Garage cyan lintel", garage + new Vector3(-27.38f, 8.1f, 0), new Vector3(.08f, .22f, 21), palette.Cool);
            geometry.Box("Garage driveway apron", new Vector3(35, .33f, -230), new Vector3(11, .12f, 20), palette.Metal, true);
            for (int z = -1; z <= 1; z++)
            {
                geometry.Box("Garage ceiling light strip", garage + new Vector3(0, 10.85f, z * 12), new Vector3(30, .08f, .28f), palette.Cool);
                GarageLight(garage + new Vector3(-4, 9.9f, z * 12));
            }
            geometry.Box("Garage workbench", garage + new Vector3(22, 1.35f, 14), new Vector3(5, 2, 1.2f), palette.Facades[0], true);
            geometry.Box("Garage tool wall", garage + new Vector3(26.6f, 3.5f, 14), new Vector3(.12f, 3.2f, 8), palette.Yellow);
            geometry.Box("Garage floor guide left", garage + new Vector3(-5, .415f, -4), new Vector3(35, .025f, .14f), palette.Yellow);
            geometry.Box("Garage floor guide right", garage + new Vector3(-5, .415f, 4), new Vector3(35, .025f, .14f), palette.Yellow);
            SignFace("NIGHT COURIER GARAGE", new Vector3(39.55f, 9.1f, -230), Quaternion.Euler(0, -90, 0), palette.Cool);

            Vector3 depot = new Vector3(155, 0, -230);
            geometry.Box("Central logistics warehouse", depot + Vector3.up * 6.1f, new Vector3(56, 11.5f, 48), palette.Facades[1], true);
            geometry.Box("Warehouse stepped roof", depot + Vector3.up * 12.1f, new Vector3(59, .7f, 51), palette.Metal);
            geometry.Box("Loading dock", depot + new Vector3(-29, 1.05f, 0), new Vector3(4, 1.5f, 28), palette.Curb, true);
            geometry.Box("Loading canopy", depot + new Vector3(-31, 4.5f, 0), new Vector3(7, .45f, 31), palette.Metal);
            geometry.Box("Loading doors", depot + new Vector3(-28.05f, 3.3f, 0), new Vector3(.08f, 4.8f, 25), palette.DarkWindow);
            SignFace("CENTRAL LOGISTICS", depot + new Vector3(-28.36f, 8.5f, 0), Quaternion.Euler(0, -90, 0), palette.Warm);
            geometry.Bake();
        }

        private void BuildExpressway()
        {
            geometry = Root("Six lane intercity expressway").AddComponent<CityGeometry>();
            for (int segment = 0; segment < HighwayPoints.Length - 1; segment++)
            {
                Vector3 a = HighwayPoints[segment], b = HighwayPoints[segment + 1];
                Vector3 direction = (b - a).normalized, right = Vector3.Cross(Vector3.up, direction);
                Quaternion rotation = Quaternion.LookRotation(direction);
                float length = Vector3.Distance(a, b) + 1.5f;
                Vector3 center = (a + b) * .5f;
                geometry.Box("Expressway pavement", center + Vector3.down * .3f, new Vector3(23, .6f, length), palette.BypassAsphalt, true, orientation: rotation);
                geometry.Box("Median barrier", center + Vector3.up * .38f, new Vector3(.34f, .76f, length), palette.Curb, true, orientation: rotation);
                for (int side = -1; side <= 1; side += 2)
                {
                    geometry.Box("Reflective road edge", center + right * side * 10.7f + Vector3.up * .02f, new Vector3(.12f, .025f, length), palette.White, orientation: rotation);
                    bool restAreaEntrance = segment == 3 && side == -1;
                    if (segment > 0 && !restAreaEntrance)
                        geometry.Box("Noise barrier", center + right * side * 12.2f + Vector3.up * 1.55f, new Vector3(.28f, 3.1f, length), palette.DarkWindow, true, orientation: rotation);
                    for (int lane = 1; lane <= 2; lane++) Dashes(a, b, right * side * lane * 3.35f, rotation);
                }
            }
            // Keep a 700-780 m opening in the north wall for the rest-area slip road.
            geometry.Box("Noise barrier after rest area", new Vector3(830, 1.55f, -17.8f), new Vector3(100, 3.1f, .28f), palette.DarkWindow, true);
            Toll(new Vector3(505, 0, -30), "WEST TOLL / HARBOR CITY");
            Toll(new Vector3(955, 0, -30), "EAST TOLL / CITY CENTRE");
            RestArea(new Vector3(735, 0, 4));
            geometry.Bake();
        }

        private void Dashes(Vector3 a, Vector3 b, Vector3 offset, Quaternion rotation)
        {
            int count = Mathf.Max(1, Mathf.FloorToInt(Vector3.Distance(a, b) / 10));
            for (int i = 0; i < count; i++) geometry.Box("Expressway lane dash",
                Vector3.Lerp(a, b, (i + .5f) / count) + offset + Vector3.up * .025f,
                new Vector3(.1f, .025f, 5.5f), palette.White, orientation: rotation);
        }

        private void Toll(Vector3 center, string label)
        {
            var signMarker = new GameObject(label + " sign"); signMarker.transform.SetParent(transform, false); signMarker.transform.position = center;
            geometry.Box("Open toll arch left post", center + new Vector3(0, 3.5f, -13.3f), new Vector3(.8f, 7, .8f), palette.Facades[1], true);
            geometry.Box("Open toll arch right post", center + new Vector3(0, 3.5f, 13.3f), new Vector3(.8f, 7, .8f), palette.Facades[1], true);
            geometry.Box("Open toll arch beam", center + Vector3.up * 6.8f, new Vector3(.8f, 1.25f, 27.4f), palette.Facades[1], true);
            geometry.Box("Toll arch cyan edge", center + new Vector3(-.43f, 6.25f, 0), new Vector3(.05f, .12f, 25.5f), palette.Cool);
            for (int lane = -2; lane <= 2; lane++)
                geometry.Box("Toll lane status light", center + new Vector3(-.42f, 6.25f, lane * 3.7f), new Vector3(.05f, .38f, .7f), palette.Cool);
            DoubleSign(label, center + new Vector3(0, 6.82f, 0), Quaternion.Euler(0, 90, 0), palette.Cool);
        }

        private void RestArea(Vector3 c)
        {
            geometry.Box("Expressway rest area lot", c + Vector3.down * .12f, new Vector3(90, .3f, 34), palette.Asphalt, true);
            geometry.Box("Rest area access lane", new Vector3(c.x, -.1f, -12), new Vector3(65, .25f, 14), palette.Asphalt, true);
            geometry.Box("Rest area service building", c + new Vector3(21, 3, 10), new Vector3(30, 6, 10), palette.Facades[2], true);
            DoubleSign("ELECTRIC REST AREA", c + new Vector3(21, 5.8f, 4.9f), Quaternion.identity, palette.Cool);
        }

        private void BuildHarborCity()
        {
            geometry = Root("Harbor City district").AddComponent<CityGeometry>();
            geometry.Box("Harbor City road foundation", new Vector3(1300, -.3f, 0), new Vector3(600, .6f, 600), palette.Asphalt, true);
            DrawHarborRoads();
            for (int x = 0; x < SecondCityVerticalRoads.Length - 1; x++)
            for (int z = 0; z < SecondCityHorizontalRoads.Length - 1; z++) BuildHarborBlock(x, z);
            HarborBoundaries();
            geometry.Bake();
        }

        private void DrawHarborRoads()
        {
            for (int road = 0; road < SecondCityVerticalRoads.Length; road++)
            {
                float start = SecondCityMinZ;
                for (int cross = 0; cross < SecondCityHorizontalRoads.Length; cross++)
                {
                    float end = SecondCityHorizontalRoads[cross] - CityBuilder.RoadWidth(SecondCityHorizontalLanes[cross]) * .5f - 3.2f;
                    HarborMarkings(new Vector3(SecondCityVerticalRoads[road], .02f, (start + end) * .5f), end - start, true, SecondCityVerticalLanes[road]);
                    start = SecondCityHorizontalRoads[cross] + CityBuilder.RoadWidth(SecondCityHorizontalLanes[cross]) * .5f + 3.2f;
                }
                HarborMarkings(new Vector3(SecondCityVerticalRoads[road], .02f, (start + SecondCityMaxZ) * .5f), SecondCityMaxZ - start, true, SecondCityVerticalLanes[road]);
            }
            for (int road = 0; road < SecondCityHorizontalRoads.Length; road++)
            {
                float start = SecondCityMinX;
                for (int cross = 0; cross < SecondCityVerticalRoads.Length; cross++)
                {
                    float end = SecondCityVerticalRoads[cross] - CityBuilder.RoadWidth(SecondCityVerticalLanes[cross]) * .5f - 3.2f;
                    HarborMarkings(new Vector3((start + end) * .5f, .02f, SecondCityHorizontalRoads[road]), end - start, false, SecondCityHorizontalLanes[road]);
                    start = SecondCityVerticalRoads[cross] + CityBuilder.RoadWidth(SecondCityVerticalLanes[cross]) * .5f + 3.2f;
                }
                HarborMarkings(new Vector3((start + SecondCityMaxX) * .5f, .02f, SecondCityHorizontalRoads[road]), SecondCityMaxX - start, false, SecondCityHorizontalLanes[road]);
            }
            for (int x = 0; x < SecondCityVerticalRoads.Length; x++)
            for (int z = 0; z < SecondCityHorizontalRoads.Length; z++) HarborIntersection(x, z);
        }

        private void HarborMarkings(Vector3 center, float length, bool vertical, int lanes)
        {
            if (length < 1) return;
            geometry.Box("Harbor road centre line", center, vertical ? new Vector3(.22f, .025f, length) : new Vector3(length, .025f, .22f), palette.Yellow);
            for (int lane = 1; lane < lanes / 2; lane++)
            for (int side = -1; side <= 1; side += 2)
            {
                int count = Mathf.Max(1, Mathf.FloorToInt(length / 12));
                for (int dash = 0; dash < count; dash++)
                {
                    float along = -length * .5f + 5 + dash * 12;
                    Vector3 offset = vertical ? Vector3.right * side * lane * 3.35f : Vector3.forward * side * lane * 3.35f;
                    Vector3 p = center + offset + (vertical ? Vector3.forward : Vector3.right) * along;
                    geometry.Box("Harbor lane dash", p, vertical ? new Vector3(.1f, .025f, 5.5f) : new Vector3(5.5f, .025f, .1f), palette.White);
                }
            }
        }

        private void HarborIntersection(int x, int z)
        {
            float px = SecondCityVerticalRoads[x], pz = SecondCityHorizontalRoads[z];
            Vector3 scenicEnd = ScenicRoadPath.Points[ScenicRoadPath.Points.Count - 1];
            bool scenicPortal = Mathf.Abs(px - scenicEnd.x) < .1f && Mathf.Abs(pz - scenicEnd.z) < .1f;
            float vw = CityBuilder.RoadWidth(SecondCityVerticalLanes[x]);
            float hw = CityBuilder.RoadWidth(SecondCityHorizontalLanes[z]);
            for (int side = -1; side <= 1; side += 2)
            for (int stripe = 0; stripe < 5; stripe++)
            {
                float offset = hw * .5f + .7f + stripe * 1.05f;
                geometry.Box("Harbor crosswalk", new Vector3(px, .04f, pz + side * offset), new Vector3(vw - .8f, .03f, .7f), palette.White);
                offset = vw * .5f + .7f + stripe * 1.05f;
                geometry.Box("Harbor crosswalk", new Vector3(px + side * offset, .04f, pz), new Vector3(.7f, .03f, hw - .8f), palette.White);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                geometry.Box("Harbor stop line", new Vector3(px, .034f, pz + side * (hw * .5f + 5.25f)), new Vector3(vw - 1, .025f, .3f), palette.White);
                geometry.Box("Harbor stop line", new Vector3(px + side * (vw * .5f + 5.25f), .034f, pz), new Vector3(.3f, .025f, hw - 1), palette.White);
            }
            if ((x + z) % 3 == 0 && !scenicPortal) HarborLamp(new Vector3(px + vw * .5f + 2.3f, 0, pz + hw * .5f + 2.3f));
            if (!scenicPortal && (SecondCityVerticalLanes[x] >= 6 || SecondCityHorizontalLanes[z] >= 6) && (x + z) % 2 == 0)
                HarborSignals(px, pz, vw, hw);
        }

        private void HarborSignals(float x, float z, float vw, float hw)
        {
            var root = new GameObject("Harbor timed traffic signals", typeof(TrafficSignalController));
            root.transform.SetParent(transform, false); root.transform.position = new Vector3(x, 0, z);
            Vector3[] p =
            {
                new Vector3(x + vw * .5f + 2, 0, z - hw * .5f - 2), new Vector3(x - vw * .5f - 2, 0, z + hw * .5f + 2),
                new Vector3(x - vw * .5f - 2, 0, z - hw * .5f - 2), new Vector3(x + vw * .5f + 2, 0, z + hw * .5f + 2)
            };
            var red = new Renderer[4]; var amber = new Renderer[4]; var green = new Renderer[4];
            Vector3[] facing = { Vector3.back, Vector3.forward, Vector3.left, Vector3.right };
            for (int i = 0; i < 4; i++) HarborSignalHead(root.transform, p[i], facing[i], out red[i], out amber[i], out green[i]);
            root.GetComponent<TrafficSignalController>().Configure(
                new[] { red[0], red[1] }, new[] { amber[0], amber[1] }, new[] { green[0], green[1] },
                new[] { red[2], red[3] }, new[] { amber[2], amber[3] }, new[] { green[2], green[3] });
        }

        private void HarborSignalHead(Transform root, Vector3 p, Vector3 facing, out Renderer red, out Renderer amber, out Renderer green)
        {
            Quaternion rotation = Quaternion.LookRotation(facing, Vector3.up);
            geometry.Box("Harbor traffic signal pole", p + Vector3.up * 2.2f, new Vector3(.19f, 4.4f, .19f), palette.Curb);
            geometry.Box("Harbor traffic signal housing", p + Vector3.up * 5.15f, new Vector3(1.05f, 2.8f, .66f), palette.Metal, orientation: rotation);
            Vector3 face = facing * .37f;
            red = HarborSignalLamp(root, "Harbor red traffic light", p + face + Vector3.up * 6.0f, palette.Red);
            amber = HarborSignalLamp(root, "Harbor amber traffic light", p + face + Vector3.up * 5.15f, palette.Warm);
            green = HarborSignalLamp(root, "Harbor green traffic light", p + face + Vector3.up * 4.3f, palette.Green);
        }

        private static Renderer HarborSignalLamp(Transform root, string label, Vector3 position, Material material)
        {
            var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere); lamp.name = label; lamp.transform.SetParent(root, true);
            lamp.transform.position = position; lamp.transform.localScale = Vector3.one * .56f;
            lamp.GetComponent<Renderer>().sharedMaterial = material; Destroy(lamp.GetComponent<Collider>());
            return lamp.GetComponent<Renderer>();
        }

        private void HarborLamp(Vector3 p)
        {
            geometry.Box("Harbor streetlight pole", p + Vector3.up * 3.5f, new Vector3(.14f, 7, .14f), palette.Metal);
            geometry.Box("Harbor streetlight arm", p + new Vector3(-.7f, 6.8f, 0), new Vector3(1.5f, .12f, .12f), palette.Metal);
            geometry.Box("Harbor streetlight lamp", p + new Vector3(-1.35f, 6.65f, 0), new Vector3(.6f, .16f, .5f), palette.Cool);
            var item = new GameObject("Harbor streetlight glow", typeof(Light)); item.transform.SetParent(transform, false); item.transform.position = p + new Vector3(-1.35f, 6.3f, 0);
            var light = item.GetComponent<Light>(); light.type = LightType.Point; light.color = new Color(.45f, .8f, 1); light.intensity = 4; light.range = 18;
        }

        private void HarborBoundaries()
        {
            geometry.Box("Harbor north boundary barrier", new Vector3(1300, .7f, SecondCityMaxZ), new Vector3(600, 1.4f, .45f), palette.Curb, true);
            geometry.Box("Harbor south boundary barrier", new Vector3(1300, .7f, SecondCityMinZ), new Vector3(600, 1.4f, .45f), palette.Curb, true);
            geometry.Box("Harbor east boundary barrier", new Vector3(SecondCityMaxX, .7f, 0), new Vector3(.45f, 1.4f, 600), palette.Curb, true);
            HarborWestBarrier(-300, -49); HarborWestBarrier(-11, 109); HarborWestBarrier(151, 300);
        }

        private void HarborWestBarrier(float start, float end)
        {
            geometry.Box("Harbor west boundary barrier", new Vector3(SecondCityMinX, .7f, (start + end) * .5f), new Vector3(.45f, 1.4f, end - start), palette.Curb, true);
        }

        private void BuildHarborBlock(int x, int z)
        {
            float left = SecondCityVerticalRoads[x] + CityBuilder.RoadWidth(SecondCityVerticalLanes[x]) * .5f + 1;
            float right = SecondCityVerticalRoads[x + 1] - CityBuilder.RoadWidth(SecondCityVerticalLanes[x + 1]) * .5f - 1;
            float bottom = SecondCityHorizontalRoads[z] + CityBuilder.RoadWidth(SecondCityHorizontalLanes[z]) * .5f + 1;
            float top = SecondCityHorizontalRoads[z + 1] - CityBuilder.RoadWidth(SecondCityHorizontalLanes[z + 1]) * .5f - 1;
            Vector3 c = new Vector3((left + right) * .5f, 0, (bottom + top) * .5f);
            Vector2 s = new Vector2(right - left, top - bottom);
            geometry.Sidewalk(c, s, palette.Pavement);
            if ((x == 1 && z == 1) || (x == 5 && z == 5)) return;
            string[] labels = { "MARINA HOTEL", "FISH MARKET", "FERRY TERMINAL", "TECH CAMPUS", "COMMUNITY ARENA", "PUBLIC WORKS", "WATERFRONT APARTMENTS", "MUSEUM" };
            int count = (x * 3 + z) % 4 == 0 ? 3 : (x + z) % 3 == 0 ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                float h = 8 + ((x * 5 + z * 2 + i) % 7) * 3;
                float px = count == 1 ? 0 : Mathf.Lerp(-s.x * .27f, s.x * .27f, i / (float)(count - 1));
                Vector3 p = c + new Vector3(px, h * .5f + .28f, (i & 1) == 0 ? -s.y * .08f : s.y * .1f);
                Vector3 size = new Vector3(s.x * (.7f / count), h, s.y * .62f);
                geometry.Box(labels[(x + z + i) % labels.Length], p, size, palette.Facades[(x + i + 2) % palette.Facades.Length], true);
                geometry.Box("Harbor roof", p + Vector3.up * (h * .5f + .35f), new Vector3(size.x + .7f, .5f, size.z + .7f), palette.Metal);
                int columns = Mathf.Clamp(Mathf.FloorToInt(size.x / 3.6f), 2, 8);
                for (int floor = 0; floor < Mathf.Max(1, Mathf.FloorToInt(h / 3)); floor++)
                for (int column = 0; column < columns; column++)
                {
                    float wx = Mathf.Lerp(-size.x * .38f, size.x * .38f, column / (float)(columns - 1));
                    Material window = (floor + column + x + z) % 4 == 0 ? palette.Warm : palette.DarkWindow;
                    geometry.Box("Harbor individual window", p + new Vector3(wx, -h * .5f + 1.75f + floor * 2.8f, -size.z * .502f),
                        new Vector3(Mathf.Min(1.4f, size.x / (columns + 1)), 1.1f, .055f), window);
                    geometry.Box("Harbor individual rear window", p + new Vector3(-wx, -h * .5f + 1.75f + floor * 2.8f, size.z * .502f),
                        new Vector3(Mathf.Min(1.4f, size.x / (columns + 1)), 1.1f, .055f), window);
                }
                geometry.Box("Harbor entrance", p + new Vector3(0, -h * .5f + 1.25f, -size.z * .505f), new Vector3(2, 2.5f, .08f), palette.Glass);
                if (h > 12) geometry.Box("Harbor rooftop plant", p + new Vector3(size.x * .18f, h * .5f + .9f, 0), new Vector3(3.5f, 1.3f, 3.5f), palette.Metal);
            }
        }

        private void BuildChargingNetwork()
        {
            Charger("South Quarter EV hub", CityBuilder.PrimaryChargingSites[0], Quaternion.identity, new Vector2(48, 42));
            Charger("Garden district EV hub", CityBuilder.PrimaryChargingSites[1], Quaternion.Euler(0, 90, 0), new Vector2(48, 42));
            Charger("Expressway rapid chargers", new Vector3(712, .18f, 5), Quaternion.identity, new Vector2(34, 24));
            Charger("Harbor west EV hub", new Vector3(1155, .32f, -150), Quaternion.identity, new Vector2(48, 42));
            Charger("Harbor east EV hub", new Vector3(1525, .32f, 170), Quaternion.Euler(0, 90, 0), new Vector2(52, 42));
        }

        private void Charger(string label, Vector3 center, Quaternion rotation, Vector2 lot)
        {
            var root = new GameObject(label, typeof(EVChargingStation)); root.transform.SetParent(transform, false);
            root.transform.SetPositionAndRotation(center, rotation); root.GetComponent<EVChargingStation>().Configure(Mathf.Min(lot.x, lot.y) * .42f);
            var g = root.AddComponent<CityGeometry>();
            g.Box("Dedicated EV station asphalt lot", Vector3.zero, new Vector3(lot.x, .1f, lot.y), palette.Asphalt, true);
            g.Box("EV station canopy", new Vector3(0, 4, 2), new Vector3(lot.x * .72f, .35f, lot.y * .48f), palette.Metal);
            for (int side = -1; side <= 1; side += 2)
            for (int row = -1; row <= 1; row += 2)
            {
                Vector3 p = new Vector3(side * lot.x * .22f, 1.2f, 2 + row * lot.y * .16f);
                g.Box("EV charger", p, new Vector3(.8f, 2.4f, .65f), palette.Curb, true);
                g.Box("EV charger screen", p + new Vector3(0, .28f, -.34f), new Vector3(.5f, .55f, .04f), palette.Cool);
            }
            g.Box("EV pylon", new Vector3(-lot.x * .38f, 3.5f, -lot.y * .35f), new Vector3(1.2f, 7, 1.2f), palette.Cool);
            g.Bake();
            DoubleSign(label.ToUpperInvariant(), center + rotation * new Vector3(0, 5.5f, -lot.y * .24f), rotation, palette.Cool);
            var glow = new GameObject("EV station glow", typeof(Light)); glow.transform.SetParent(root.transform, false); glow.transform.localPosition = new Vector3(0, 3.7f, 1);
            var light = glow.GetComponent<Light>(); light.type = LightType.Point; light.color = new Color(.1f, .85f, 1); light.intensity = 7; light.range = 20;
        }

        private GameObject Root(string label) { var item = new GameObject(label); item.transform.SetParent(transform, false); return item; }

        private void GarageLight(Vector3 position)
        {
            var item = new GameObject("Garage interior light", typeof(Light)); item.transform.SetParent(transform, false); item.transform.position = position;
            var light = item.GetComponent<Light>(); light.type = LightType.Point; light.color = new Color(.5f, .85f, 1); light.intensity = 5; light.range = 18;
        }

        private void DoubleSign(string label, Vector3 position, Quaternion rotation, Material glow)
        {
            SignFace(label, position - rotation * Vector3.forward * .05f, rotation, glow);
            SignFace(label, position + rotation * Vector3.forward * .05f, rotation * Quaternion.Euler(0, 180, 0), glow);
        }

        private void SignFace(string label, Vector3 position, Quaternion rotation, Material glow)
        {
            var item = new GameObject(label + " sign face", typeof(TextMesh)); item.transform.SetParent(transform, false); item.transform.SetPositionAndRotation(position, rotation);
            var text = item.GetComponent<TextMesh>(); text.font = palette.SignFont; text.text = label; text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center;
            text.fontSize = 48; text.characterSize = Mathf.Clamp(5.5f / label.Length, .075f, .15f);
            var renderer = item.GetComponent<MeshRenderer>(); renderer.sharedMaterial = palette.SignMaterial;
            var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", glow.GetColor("_BaseColor") * 1.8f); renderer.SetPropertyBlock(block);
        }
    }
}
