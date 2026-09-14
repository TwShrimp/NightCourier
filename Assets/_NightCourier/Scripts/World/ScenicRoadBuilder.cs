using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NightCourier.World
{
    /// <summary>Continuous curved road meshes with hills, an embankment and a supported river viaduct.</summary>
    public sealed class ScenicRoadBuilder : MonoBehaviour
    {
        private readonly List<Mesh> ownedMeshes = new List<Mesh>();
        private readonly Dictionary<Material, List<CombineInstance>> markings = new Dictionary<Material, List<CombineInstance>>();
        private CityPalette palette;
        private CityGeometry details;
        private IReadOnlyList<Vector3> points;

        public void Build(CityPalette colors)
        {
            palette = colors;
            points = ScenicRoadPath.Points;
            var detailObject = new GameObject("Bypass rails, piers and trees");
            detailObject.transform.SetParent(transform, false);
            details = detailObject.AddComponent<CityGeometry>();
            for (int start = 0; start < points.Count - 1; start += 48)
            {
                int end = Mathf.Min(start + 48, points.Count - 1);
                Strip($"Bypass pavement {start}", start, end, -7, 7, 0, palette.BypassAsphalt, true);
                Strip("Left shoulder", start, end, -6.65f, -6.5f, 0.025f, palette.White);
                Strip("Right shoulder", start, end, 6.5f, 6.65f, 0.025f, palette.White);
                // Each ramp and bridge shares exact edge vertices with the next mesh.
                for (int i = start; i < end; i++)
                {
                    Vector3 a = points[i];
                    Vector3 b = points[i + 1];
                    // Let both city streets flow cleanly into the bypass before the safety rails begin.
                    bool openPortal = i < 18 || i >= points.Count - 19;
                    for (int side = -1; side <= 1 && !openPortal; side += 2)
                    {
                        Vector3 left = a + ScenicRoadPath.Right(i) * (side * 7.35f);
                        Vector3 right = b + ScenicRoadPath.Right(i + 1) * (side * 7.35f);
                        Beam("Safety rail", left + Vector3.up * 0.85f, right + Vector3.up * 0.85f, 0.18f, 0.5f, palette.Curb, true);
                        if (i % 4 == 0)
                            details.Box("Rail post", left + Vector3.up * 0.45f, new Vector3(0.15f, 0.9f, 0.15f), palette.Metal);
                    }
                    if (i % 4 < 2) Strip("Dashed divider", i, i + 1, -0.08f, 0.08f, 0.03f, palette.Yellow);
                    if (ScenicRoadPath.IsViaduct(a))
                    {
                        Beam("Bridge deck fascia", a + ScenicRoadPath.Right(i) * 7 + Vector3.down * 0.42f,
                            b + ScenicRoadPath.Right(i + 1) * 7 + Vector3.down * 0.42f, 0.35f, 0.8f, palette.Curb);
                        Beam("Bridge deck fascia", a - ScenicRoadPath.Right(i) * 7 + Vector3.down * 0.42f,
                            b - ScenicRoadPath.Right(i + 1) * 7 + Vector3.down * 0.42f, 0.35f, 0.8f, palette.Curb);
                        if (i % 12 == 0)
                        {
                            float height = a.y + 2;
                            details.Box("Viaduct pier", new Vector3(a.x, height / 2 - 2, a.z), new Vector3(2.5f, height - 0.7f, 2.5f), palette.Curb, true);
                            Beam("Pier cap", a - ScenicRoadPath.Right(i) * 6 + Vector3.down,
                                a + ScenicRoadPath.Right(i) * 6 + Vector3.down, 2, 1, palette.Curb);
                        }
                    }
                    if (i % 22 == 0 && i > 10 && i < points.Count - 10) Lamp(a, ScenicRoadPath.Right(i));
                }
            }
            Terrain();
            // A river under the northern viaduct gives the elevation a clear purpose.
            details.Box("River", new Vector3(760, -1.7f, 425), new Vector3(300, 0.1f, 34), palette.Water);
            Sign(10, "HILLSIDE LOOP  >", palette.Cool);
            Sign(points.Count - 11, "<  RIVER VIADUCT", palette.Cool, true);
            Sign(points.Count / 2, "SLOW / RIVER VIADUCT", palette.Warm);
            details.Bake();
            foreach (var batch in markings)
            {
                var mesh = new Mesh { name = "Bypass road markings", indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(batch.Value.ToArray(), true, true);
                ownedMeshes.Add(mesh);
                var item = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer));
                item.transform.SetParent(transform, false);
                item.GetComponent<MeshFilter>().sharedMesh = mesh;
                item.GetComponent<MeshRenderer>().sharedMaterial = batch.Key;
            }
            markings.Clear();
        }

        private void Strip(string label, int start, int end, float left, float right, float elevation, Material material, bool solid = false)
        {
            int count = end - start + 1;
            var vertices = new Vector3[count * 2];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[(count - 1) * 6];
            float distance = 0;
            for (int i = 0; i < count; i++)
            {
                int index = start + i;
                if (i > 0) distance += Vector3.Distance(points[index], points[index - 1]);
                Vector3 lateral = ScenicRoadPath.Right(index);
                vertices[i * 2] = points[index] + lateral * left + Vector3.up * elevation;
                vertices[i * 2 + 1] = points[index] + lateral * right + Vector3.up * elevation;
                uv[i * 2] = new Vector2(0, distance / 6);
                uv[i * 2 + 1] = new Vector2((right - left) / 6, distance / 6);
                if (i == count - 1) continue;
                int v = i * 2;
                int t = i * 6;
                triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
            }
            Surface(label, vertices, triangles, uv, material, solid);
        }

        private void Terrain()
        {
            const int columns = 84;
            const int rows = 52;
            var vertices = new Vector3[(columns + 1) * (rows + 1)];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[columns * rows * 6];
            for (int z = 0; z <= rows; z++)
            for (int x = 0; x <= columns; x++)
            {
                float wx = 345 + x * 8;
                float wz = 80 + z * 8;
                float nearDistance = float.MaxValue;
                float roadHeight = 0;
                foreach (var point in points)
                {
                    float distance = Vector2.Distance(new Vector2(wx, wz), new Vector2(point.x, point.z));
                    if (distance < nearDistance) { nearDistance = distance; roadHeight = point.y; }
                }
                float hill = 11 * Mathf.Exp(-((wx - 660) * (wx - 660) + (wz - 330) * (wz - 330)) / 20000);
                float height = -2 + hill + Mathf.PerlinNoise(wx * 0.018f, wz * 0.018f) * 1.5f;
                if (wz > 405) height = -2.3f;
                else if (nearDistance < 32)
                    height = Mathf.Lerp(roadHeight - 0.8f, height, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(8, 32, nearDistance)));
                int index = z * (columns + 1) + x;
                vertices[index] = new Vector3(wx, height, wz);
                uv[index] = new Vector2(x, z) / 8;
                if (x < columns && z < rows)
                {
                    int t = (z * columns + x) * 6;
                    triangles[t] = index; triangles[t + 1] = index + columns + 1; triangles[t + 2] = index + 1;
                    triangles[t + 3] = index + 1; triangles[t + 4] = index + columns + 1; triangles[t + 5] = index + columns + 2;
                }
                Vector2 ground = new Vector2(wx, wz);
                bool clearPortal = Vector2.Distance(ground, new Vector2(points[0].x, points[0].z)) < 62
                    || Vector2.Distance(ground, new Vector2(points[points.Count - 1].x, points[points.Count - 1].z)) < 62;
                if (x % 3 == 0 && z % 3 == 0 && nearDistance > 24 && height > -1.5f && wz < 400 && !clearPortal)
                {
                    details.Box("Hillside trunk", new Vector3(wx, height + 1.6f, wz), new Vector3(0.6f, 3.2f, 0.6f), palette.Trunk, true);
                    details.Box("Hillside canopy", new Vector3(wx, height + 4, wz), new Vector3(4.5f, 4, 4.5f), palette.Grass, false, (x * 17) % 90);
                }
            }
            Surface("Hillside terrain", vertices, triangles, uv, palette.Grass, true);
        }

        private void Surface(string label, Vector3[] vertices, int[] triangles, Vector2[] uv, Material material, bool solid)
        {
            var mesh = new Mesh { name = label, indexFormat = IndexFormat.UInt32, vertices = vertices, triangles = triangles, uv = uv };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            ownedMeshes.Add(mesh);
            if (!solid)
            {
                if (!markings.TryGetValue(material, out var batch))
                {
                    batch = new List<CombineInstance>();
                    markings.Add(material, batch);
                }
                batch.Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.identity });
                return;
            }
            var item = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer));
            item.transform.SetParent(transform, false);
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            item.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (solid) item.AddComponent<MeshCollider>().sharedMesh = mesh;
        }

        private void Beam(string label, Vector3 start, Vector3 end, float width, float height, Material material, bool solid = false)
        {
            Vector3 direction = end - start;
            details.Box(label, (start + end) * 0.5f, new Vector3(width, height, direction.magnitude + 0.025f),
                material, solid, orientation: Quaternion.LookRotation(direction, Vector3.up));
        }

        private void Lamp(Vector3 point, Vector3 right)
        {
            Vector3 position = point + right * 8;
            details.Box("Bypass lamp post", position + Vector3.up * 3, new Vector3(0.14f, 6, 0.14f), palette.Metal);
            details.Box("Bypass lamp", position + Vector3.up * 6, new Vector3(0.7f, 0.15f, 0.7f), palette.Warm);
            var item = new GameObject("Bypass downlight", typeof(Light));
            item.transform.SetParent(transform, false);
            item.transform.position = position + Vector3.up * 5.8f;
            item.transform.rotation = Quaternion.Euler(90, 0, 0);
            var light = item.GetComponent<Light>();
            light.type = LightType.Spot; light.range = 22; light.intensity = 8;
            light.spotAngle = 115; light.color = new Color(1, 0.74f, 0.45f);
        }

        private void Sign(int index, string label, Material light, bool reverse = false)
        {
            Vector3 point = points[index];
            Vector3 tangent = points[Mathf.Min(index + 1, points.Count - 1)] - points[index - 1];
            if (reverse) tangent = -tangent;
            Quaternion rotation = Quaternion.LookRotation(new Vector3(tangent.x, 0, tangent.z));
            details.Box("Overhead sign", point + Vector3.up * 6.5f, new Vector3(10, 1.5f, 0.18f), palette.Metal, orientation: rotation);
            foreach (int side in new[] { -1, 1 })
                details.Box("Sign column", point + ScenicRoadPath.Right(index) * side * 8 + Vector3.up * 3.2f, new Vector3(0.2f, 6.4f, 0.2f), palette.Metal);
            var sign = new GameObject(label, typeof(TextMesh));
            sign.transform.SetParent(transform, false);
            sign.transform.SetPositionAndRotation(point + Vector3.up * 6.5f - rotation * Vector3.forward * 0.12f, rotation);
            var text = sign.GetComponent<TextMesh>();
            text.font = palette.SignFont; text.fontSize = 48; text.characterSize = 0.22f;
            text.anchor = TextAnchor.MiddleCenter; text.text = label;
            var renderer = sign.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = palette.SignMaterial;
            var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", light.GetColor("_BaseColor") * 2);
            renderer.SetPropertyBlock(block);
        }

        private void OnDestroy()
        {
            foreach (var mesh in ownedMeshes) Destroy(mesh);
        }
    }
}
