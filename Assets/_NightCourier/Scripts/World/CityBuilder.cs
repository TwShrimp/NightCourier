using UnityEngine;

namespace NightCourier.World
{
    /// <summary>Builds the expanded main city as one coherent road and block plan.</summary>
    public sealed class CityBuilder : MonoBehaviour
    {
        public const float MinX = -360, MaxX = 360, MinZ = -300, MaxZ = 300;
        public static readonly float[] VerticalRoads = { -330, -240, -150, -60, 20, 110, 200, 290, 350 };
        public static readonly int[] VerticalLanes = { 4, 4, 6, 4, 8, 4, 6, 4, 4 };
        public static readonly float[] HorizontalRoads = { -270, -190, -110, -30, 50, 130, 210, 290 };
        public static readonly int[] HorizontalLanes = { 4, 4, 6, 8, 6, 4, 6, 4 };
        public static readonly Vector3[] PrimaryChargingSites =
        {
            new Vector3(65, .32f, -150), new Vector3(-195, .32f, 90)
        };

        private CityPalette palette;
        private CityGeometry geometry;

        public static float RoadWidth(int lanes) => lanes * 3.35f + 1.2f;

        public void Build(CityPalette colors)
        {
            palette = colors;
            var root = new GameObject("Expanded main city / 56 long blocks");
            root.transform.SetParent(transform, false);
            geometry = root.AddComponent<CityGeometry>();
            geometry.Box("Main city road foundation", new Vector3(0, -.3f, 0),
                new Vector3(MaxX - MinX, .6f, MaxZ - MinZ), palette.Asphalt, true);
            BuildRoads();
            BuildBlocks();
            BuildBoundaryBarriers();
            geometry.Bake();
        }

        private void BuildRoads()
        {
            for (int i = 0; i < VerticalRoads.Length; i++)
            {
                string name = $"North-south {VerticalLanes[i]}-lane avenue";
                var marker = new GameObject(name); marker.transform.SetParent(transform, false);
                marker.transform.position = new Vector3(VerticalRoads[i], 0, 0);
                MarkVerticalRoad(i);
            }
            for (int i = 0; i < HorizontalRoads.Length; i++)
            {
                string name = $"East-west {HorizontalLanes[i]}-lane avenue";
                var marker = new GameObject(name); marker.transform.SetParent(transform, false);
                marker.transform.position = new Vector3(0, 0, HorizontalRoads[i]);
                MarkHorizontalRoad(i);
            }
            for (int x = 0; x < VerticalRoads.Length; x++)
            for (int z = 0; z < HorizontalRoads.Length; z++) IntersectionDetails(x, z);
        }

        private void MarkVerticalRoad(int road)
        {
            float start = MinZ;
            for (int cross = 0; cross < HorizontalRoads.Length; cross++)
            {
                float end = HorizontalRoads[cross] - RoadWidth(HorizontalLanes[cross]) * .5f - 3.2f;
                RoadMarkings(new Vector3(VerticalRoads[road], .02f, (start + end) * .5f), end - start, true, VerticalLanes[road]);
                start = HorizontalRoads[cross] + RoadWidth(HorizontalLanes[cross]) * .5f + 3.2f;
            }
            RoadMarkings(new Vector3(VerticalRoads[road], .02f, (start + MaxZ) * .5f), MaxZ - start, true, VerticalLanes[road]);
        }

        private void MarkHorizontalRoad(int road)
        {
            float start = MinX;
            for (int cross = 0; cross < VerticalRoads.Length; cross++)
            {
                float end = VerticalRoads[cross] - RoadWidth(VerticalLanes[cross]) * .5f - 3.2f;
                RoadMarkings(new Vector3((start + end) * .5f, .02f, HorizontalRoads[road]), end - start, false, HorizontalLanes[road]);
                start = VerticalRoads[cross] + RoadWidth(VerticalLanes[cross]) * .5f + 3.2f;
            }
            RoadMarkings(new Vector3((start + MaxX) * .5f, .02f, HorizontalRoads[road]), MaxX - start, false, HorizontalLanes[road]);
        }

        private void RoadMarkings(Vector3 center, float length, bool vertical, int lanes)
        {
            if (length < 1) return;
            Vector3 line = vertical ? new Vector3(.22f, .025f, length) : new Vector3(length, .025f, .22f);
            geometry.Box("Road centre line", center, line, palette.Yellow);
            for (int lane = 1; lane < lanes / 2; lane++)
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 offset = vertical ? Vector3.right * side * lane * 3.35f : Vector3.forward * side * lane * 3.35f;
                int dashCount = Mathf.Max(1, Mathf.FloorToInt(length / 12));
                for (int d = 0; d < dashCount; d++)
                {
                    float along = -length * .5f + 5 + d * 12;
                    Vector3 p = center + offset + (vertical ? Vector3.forward : Vector3.right) * along;
                    Vector3 size = vertical ? new Vector3(.1f, .025f, 5.5f) : new Vector3(5.5f, .025f, .1f);
                    geometry.Box("Lane dash", p, size, palette.White);
                }
            }
        }

        private void IntersectionDetails(int x, int z)
        {
            float px = VerticalRoads[x], pz = HorizontalRoads[z];
            bool scenicPortal = Mathf.Abs(px - ScenicRoadPath.Points[0].x) < .1f
                && Mathf.Abs(pz - ScenicRoadPath.Points[0].z) < .1f;
            float verticalWidth = RoadWidth(VerticalLanes[x]);
            float horizontalWidth = RoadWidth(HorizontalLanes[z]);
            for (int side = -1; side <= 1; side += 2)
            for (int stripe = 0; stripe < 5; stripe++)
            {
                float offset = horizontalWidth * .5f + .7f + stripe * 1.05f;
                geometry.Box("Marked intersection crosswalk", new Vector3(px, .04f, pz + side * offset),
                    new Vector3(verticalWidth - .8f, .03f, .7f), palette.White);
                offset = verticalWidth * .5f + .7f + stripe * 1.05f;
                geometry.Box("Marked intersection crosswalk", new Vector3(px + side * offset, .04f, pz),
                    new Vector3(.7f, .03f, horizontalWidth - .8f), palette.White);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                geometry.Box("Intersection stop line", new Vector3(px, .034f, pz + side * (horizontalWidth * .5f + 5.25f)), new Vector3(verticalWidth - 1, .025f, .3f), palette.White);
                geometry.Box("Intersection stop line", new Vector3(px + side * (verticalWidth * .5f + 5.25f), .034f, pz), new Vector3(.3f, .025f, horizontalWidth - 1), palette.White);
            }
            if ((x + z) % 2 == 0 && !scenicPortal)
            {
                Vector3 corner = new Vector3(px + verticalWidth * .5f + 2.3f, 0, pz + horizontalWidth * .5f + 2.3f);
                StreetLamp(corner);
            }
            if (!scenicPortal && (VerticalLanes[x] >= 6 || HorizontalLanes[z] >= 6) && (x + z) % 2 == 0)
                TrafficSignals(px, pz, verticalWidth, horizontalWidth);
        }

        private void TrafficSignals(float x, float z, float verticalWidth, float horizontalWidth)
        {
            var root = new GameObject("Timed traffic signals", typeof(TrafficSignalController));
            root.transform.SetParent(transform, false); root.transform.position = new Vector3(x, 0, z);
            Vector3 nsA = new Vector3(x + verticalWidth * .5f + 2, 0, z - horizontalWidth * .5f - 2);
            Vector3 nsB = new Vector3(x - verticalWidth * .5f - 2, 0, z + horizontalWidth * .5f + 2);
            Vector3 ewA = new Vector3(x - verticalWidth * .5f - 2, 0, z - horizontalWidth * .5f - 2);
            Vector3 ewB = new Vector3(x + verticalWidth * .5f + 2, 0, z + horizontalWidth * .5f + 2);
            Renderer nsR1, nsA1, nsG1, nsR2, nsA2, nsG2, ewR1, ewA1, ewG1, ewR2, ewA2, ewG2;
            SignalHead(root.transform, nsA, Vector3.back, out nsR1, out nsA1, out nsG1);
            SignalHead(root.transform, nsB, Vector3.forward, out nsR2, out nsA2, out nsG2);
            SignalHead(root.transform, ewA, Vector3.left, out ewR1, out ewA1, out ewG1);
            SignalHead(root.transform, ewB, Vector3.right, out ewR2, out ewA2, out ewG2);
            root.GetComponent<TrafficSignalController>().Configure(
                new[] { nsR1, nsR2 }, new[] { nsA1, nsA2 }, new[] { nsG1, nsG2 },
                new[] { ewR1, ewR2 }, new[] { ewA1, ewA2 }, new[] { ewG1, ewG2 });
        }

        private void SignalHead(Transform root, Vector3 position, Vector3 facing, out Renderer red, out Renderer amber, out Renderer green)
        {
            Quaternion rotation = Quaternion.LookRotation(facing, Vector3.up);
            geometry.Box("Traffic signal pole", position + Vector3.up * 2.2f, new Vector3(.19f, 4.4f, .19f), palette.Curb);
            geometry.Box("Traffic signal black housing", position + Vector3.up * 5.15f, new Vector3(1.05f, 2.8f, .66f), palette.Metal, orientation: rotation);
            Vector3 face = facing * .37f;
            red = SignalLamp(root, "Red traffic light", position + face + Vector3.up * 6.0f, palette.Red);
            amber = SignalLamp(root, "Amber traffic light", position + face + Vector3.up * 5.15f, palette.Warm);
            green = SignalLamp(root, "Green traffic light", position + face + Vector3.up * 4.3f, palette.Green);
        }

        private static Renderer SignalLamp(Transform root, string label, Vector3 position, Material material)
        {
            var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere); lamp.name = label;
            lamp.transform.SetParent(root, true); lamp.transform.position = position; lamp.transform.localScale = Vector3.one * .56f;
            lamp.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(lamp.GetComponent<Collider>());
            return lamp.GetComponent<Renderer>();
        }

        private void StreetLamp(Vector3 position)
        {
            geometry.Box("City streetlight pole", position + Vector3.up * 3.4f, new Vector3(.14f, 6.8f, .14f), palette.Metal);
            geometry.Box("City streetlight arm", position + new Vector3(-.65f, 6.65f, 0), new Vector3(1.4f, .12f, .12f), palette.Metal);
            geometry.Box("City streetlight lamp", position + new Vector3(-1.25f, 6.55f, 0), new Vector3(.55f, .16f, .5f), palette.Warm);
            var item = new GameObject("City streetlight glow", typeof(Light)); item.transform.SetParent(transform, false);
            item.transform.position = position + new Vector3(-1.25f, 6.25f, 0);
            var light = item.GetComponent<Light>(); light.type = LightType.Point; light.color = new Color(1, .68f, .38f);
            light.intensity = 4.5f; light.range = 18;
        }

        private void BuildBoundaryBarriers()
        {
            geometry.Box("West city boundary barrier", new Vector3(MinX, .7f, 0), new Vector3(.45f, 1.4f, MaxZ - MinZ), palette.Curb, true);
            geometry.Box("North city boundary barrier", new Vector3(0, .7f, MaxZ), new Vector3(MaxX - MinX, 1.4f, .45f), palette.Curb, true);
            geometry.Box("South city boundary barrier", new Vector3(0, .7f, MinZ), new Vector3(MaxX - MinX, 1.4f, .45f), palette.Curb, true);
            BoundarySegment(-300, -49); BoundarySegment(-11, 109); BoundarySegment(151, 300);
        }

        private void BoundarySegment(float startZ, float endZ)
        {
            geometry.Box("East city boundary barrier", new Vector3(MaxX, .7f, (startZ + endZ) * .5f),
                new Vector3(.45f, 1.4f, endZ - startZ), palette.Curb, true);
        }

        private void BuildBlocks()
        {
            for (int x = 0; x < VerticalRoads.Length - 1; x++)
            for (int z = 0; z < HorizontalRoads.Length - 1; z++)
            {
                float left = VerticalRoads[x] + RoadWidth(VerticalLanes[x]) * .5f + 1;
                float right = VerticalRoads[x + 1] - RoadWidth(VerticalLanes[x + 1]) * .5f - 1;
                float bottom = HorizontalRoads[z] + RoadWidth(HorizontalLanes[z]) * .5f + 1;
                float top = HorizontalRoads[z + 1] - RoadWidth(HorizontalLanes[z + 1]) * .5f - 1;
                Vector3 center = new Vector3((left + right) * .5f, 0, (bottom + top) * .5f);
                Vector2 size = new Vector2(right - left, top - bottom);
                geometry.Sidewalk(center, size, palette.Pavement);

                if (IsReserved(x, z)) continue;
                if (x == 3 && z == 4) BuildPark(center, size);
                else BuildBlock(x, z, center, size);
            }
        }

        private static bool IsReserved(int x, int z) =>
            (x == 4 && z == 0) || (x == 5 && z == 0) ||
            (x == 4 && z == 1) || (x == 1 && z == 4);

        private void BuildBlock(int x, int z, Vector3 center, Vector2 size)
        {
            int style = (x * 5 + z * 3) % 8;
            switch (style)
            {
                case 0: TowerCluster(x, z, center, size); break;
                case 1: RowShops(x, z, center, size); break;
                case 2: CourtyardApartments(x, z, center, size); break;
                case 3: Industrial(x, z, center, size); break;
                case 4: Civic(x, z, center, size); break;
                case 5: OfficeCampus(x, z, center, size); break;
                case 6: ParkingStructure(x, z, center, size); break;
                default: MixedBlock(x, z, center, size); break;
            }
        }

        private void TowerCluster(int x, int z, Vector3 c, Vector2 s)
        {
            for (int i = -1; i <= 1; i++)
            {
                float h = 17 + ((x * 7 + z * 11 + i * i) % 6) * 4;
                Vector3 p = c + new Vector3(i * s.x * .26f, h * .5f + .28f, (i & 1) == 0 ? -s.y * .12f : s.y * .14f);
                Building($"Distinct tower {x}-{z}-{i + 1}", p, new Vector3(s.x * .22f, h, s.y * .46f), h, x + z + i);
            }
        }

        private void RowShops(int x, int z, Vector3 c, Vector2 s)
        {
            for (int i = 0; i < 5; i++)
            {
                float h = 6.5f + ((x + z + i) % 3) * 2.2f;
                Vector3 p = c + new Vector3(Mathf.Lerp(-s.x * .38f, s.x * .38f, i / 4f), h * .5f + .28f, 2);
                Building($"Storefront {x}-{z}-{i + 1}", p, new Vector3(s.x * .17f, h, s.y * .62f), h, i + x);
                geometry.Box("Shop awning", p + new Vector3(0, -.5f, -s.y * .32f), new Vector3(s.x * .15f, .18f, 1.8f), i % 2 == 0 ? palette.Pink : palette.Cool);
            }
        }

        private void CourtyardApartments(int x, int z, Vector3 c, Vector2 s)
        {
            float h = 12 + ((x + z) % 4) * 2.5f;
            Building($"Courtyard north apartments {x}-{z}", c + new Vector3(0, h * .5f + .28f, s.y * .33f), new Vector3(s.x * .82f, h, s.y * .18f), h, x);
            Building($"Courtyard south apartments {x}-{z}", c + new Vector3(0, h * .5f + .28f, -s.y * .33f), new Vector3(s.x * .82f, h, s.y * .18f), h, z);
            Building($"Courtyard east apartments {x}-{z}", c + new Vector3(s.x * .36f, h * .5f + .28f, 0), new Vector3(s.x * .12f, h, s.y * .48f), h, x + z);
        }

        private void Industrial(int x, int z, Vector3 c, Vector2 s)
        {
            float h = 7 + (x + z) % 3;
            Building($"Sawtooth warehouse {x}-{z}", c + Vector3.up * (h * .5f + .28f), new Vector3(s.x * .76f, h, s.y * .68f), h, z);
            for (int i = -2; i <= 2; i++)
                geometry.Box("Warehouse roof ridge", c + new Vector3(i * s.x * .14f, h + .8f, 0), new Vector3(s.x * .07f, 1.4f, s.y * .7f), palette.Metal, yaw: 18);
        }

        private void Civic(int x, int z, Vector3 c, Vector2 s)
        {
            string[] names = { "GENERAL HOSPITAL", "CENTRAL LIBRARY", "CITY SCHOOL", "FIRE STATION", "POLICE PRECINCT", "CITY HALL" };
            string label = names[(x + z) % names.Length];
            float h = 9 + (x + z) % 4 * 2;
            Building(label, c + Vector3.up * (h * .5f + .28f), new Vector3(s.x * .7f, h, s.y * .58f), h, x + z);
            SignFace(label, c + new Vector3(0, 3.8f, -s.y * .295f - .05f), Quaternion.identity, palette.Cool);
        }

        private void OfficeCampus(int x, int z, Vector3 c, Vector2 s)
        {
            float h1 = 13 + x % 4 * 3, h2 = 9 + z % 3 * 3;
            Building($"Glass office {x}-{z} A", c + new Vector3(-s.x * .2f, h1 * .5f + .28f, 0), new Vector3(s.x * .32f, h1, s.y * .68f), h1, x);
            Building($"Glass office {x}-{z} B", c + new Vector3(s.x * .22f, h2 * .5f + .28f, 0), new Vector3(s.x * .3f, h2, s.y * .52f), h2, z + 3);
        }

        private void ParkingStructure(int x, int z, Vector3 c, Vector2 s)
        {
            float h = 10;
            Building($"Multi-storey parking {x}-{z}", c + Vector3.up * (h * .5f + .28f), new Vector3(s.x * .76f, h, s.y * .67f), h, 3);
            for (int floor = 1; floor <= 3; floor++)
                geometry.Box("Parking deck stripe", c + new Vector3(0, floor * 2.5f, -s.y * .342f), new Vector3(s.x * .7f, .16f, .08f), palette.Cool);
        }

        private void MixedBlock(int x, int z, Vector3 c, Vector2 s)
        {
            for (int i = 0; i < 4; i++)
            {
                float h = 8 + ((x * 3 + z + i) % 5) * 3;
                Vector3 p = c + new Vector3((i % 2 == 0 ? -1 : 1) * s.x * .23f, h * .5f + .28f, (i < 2 ? -1 : 1) * s.y * .22f);
                Building($"Mixed-use building {x}-{z}-{i + 1}", p, new Vector3(s.x * .37f, h, s.y * .34f), h, x + i);
            }
        }

        private void Building(string label, Vector3 center, Vector3 size, float height, int materialIndex)
        {
            Material facade = palette.Facades[Mathf.Abs(materialIndex) % palette.Facades.Length];
            geometry.Box(label, center, size, facade, true);
            geometry.Box("Contrasting roof", center + Vector3.up * (height * .5f + .3f), new Vector3(size.x + .6f, .45f, size.z + .6f), palette.Metal);
            int floors = Mathf.Max(1, Mathf.FloorToInt(height / 3));
            int frontColumns = Mathf.Clamp(Mathf.FloorToInt(size.x / 3.6f), 2, 8);
            for (int floor = 0; floor < floors; floor++)
            for (int column = 0; column < frontColumns; column++)
            {
                float x = Mathf.Lerp(-size.x * .38f, size.x * .38f, frontColumns == 1 ? .5f : column / (float)(frontColumns - 1));
                Material window = (floor * 3 + column + materialIndex) % 4 == 0 ? palette.Warm : palette.DarkWindow;
                geometry.Box("Individual front window", center + new Vector3(x, -height * .5f + 1.75f + floor * 2.8f, -size.z * .502f),
                    new Vector3(Mathf.Min(1.45f, size.x / (frontColumns + 1)), 1.15f, .055f), window);
                geometry.Box("Individual rear window", center + new Vector3(-x, -height * .5f + 1.75f + floor * 2.8f, size.z * .502f),
                    new Vector3(Mathf.Min(1.45f, size.x / (frontColumns + 1)), 1.15f, .055f), window);
            }
            int sideColumns = Mathf.Clamp(Mathf.FloorToInt(size.z / 5), 1, 5);
            for (int floor = 0; floor < floors; floor++)
            for (int column = 0; column < sideColumns; column++)
            {
                float z = Mathf.Lerp(-size.z * .34f, size.z * .34f, sideColumns == 1 ? .5f : column / (float)(sideColumns - 1));
                Material window = (floor + column + materialIndex) % 5 == 0 ? palette.Cool : palette.DarkWindow;
                geometry.Box("Individual side window", center + new Vector3(size.x * .502f, -height * .5f + 1.75f + floor * 2.8f, z),
                    new Vector3(.055f, 1.15f, Mathf.Min(1.4f, size.z / (sideColumns + 1))), window);
                geometry.Box("Individual side window", center + new Vector3(-size.x * .502f, -height * .5f + 1.75f + floor * 2.8f, -z),
                    new Vector3(.055f, 1.15f, Mathf.Min(1.4f, size.z / (sideColumns + 1))), window);
            }
            geometry.Box("Building entrance", center + new Vector3(0, -height * .5f + 1.25f, -size.z * .505f),
                new Vector3(Mathf.Min(2.2f, size.x * .25f), 2.5f, .08f), palette.Glass);
            if (height > 10)
            {
                geometry.Box("Rooftop mechanical room", center + new Vector3(size.x * .18f, height * .5f + .85f, 0),
                    new Vector3(Mathf.Min(4, size.x * .22f), 1.2f, Mathf.Min(4, size.z * .22f)), palette.Metal);
                geometry.Box("Rooftop antenna", center + new Vector3(-size.x * .18f, height * .5f + 2.2f, 0), new Vector3(.12f, 3.8f, .12f), palette.Metal);
            }
        }

        private void BuildPark(Vector3 c, Vector2 s)
        {
            geometry.Box("Central neighborhood park", c + Vector3.up * .3f, new Vector3(s.x * .88f, .05f, s.y * .86f), palette.Grass);
            geometry.Box("Park crosswalk path", c + Vector3.up * .34f, new Vector3(s.x * .75f, .04f, 3), palette.Pavement);
            for (int x = -2; x <= 2; x++)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 p = c + new Vector3(x * s.x * .13f, 0, z * s.y * .29f);
                geometry.Box("Park tree trunk", p + Vector3.up * 2, new Vector3(.6f, 3.4f, .6f), palette.Trunk, true);
                geometry.Box("Park tree canopy", p + Vector3.up * 4.5f, new Vector3(4.5f, 4.5f, 4.5f), palette.Grass);
            }
        }

        private void DoubleSidedSign(string label, Vector3 position, Quaternion rotation, Material glow)
        {
            SignFace(label, position - rotation * Vector3.forward * .03f, rotation, glow);
            SignFace(label, position + rotation * Vector3.forward * .03f, rotation * Quaternion.Euler(0, 180, 0), glow);
        }

        private void SignFace(string label, Vector3 position, Quaternion rotation, Material glow)
        {
            var item = new GameObject(label + " readable sign", typeof(TextMesh));
            item.transform.SetParent(transform, false); item.transform.SetPositionAndRotation(position, rotation);
            var text = item.GetComponent<TextMesh>(); text.font = palette.SignFont; text.text = label;
            text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center;
            text.fontSize = 48; text.characterSize = Mathf.Clamp(4.8f / label.Length, .08f, .14f);
            var renderer = item.GetComponent<MeshRenderer>(); renderer.sharedMaterial = palette.SignMaterial;
            var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", glow.GetColor("_BaseColor") * 1.8f); renderer.SetPropertyBlock(block);
        }
    }
}
