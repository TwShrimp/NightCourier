using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NightCourier.World
{
    /// <summary>Batches decorative boxes per material; only solid structures get colliders.</summary>
    public sealed class CityGeometry : MonoBehaviour
    {
        private readonly Dictionary<Material, List<CombineInstance>> batches = new Dictionary<Material, List<CombineInstance>>();
        private readonly List<Mesh> meshes = new List<Mesh>();
        private Mesh cube;

        public void Sidewalk(Vector3 center, Vector2 size, Material material)
        {
            // A 60 cm bevel lets raycast wheels climb the 28 cm pavement smoothly.
            float outerX = size.x * 0.5f;
            float outerZ = size.y * 0.5f;
            float innerX = Mathf.Max(0.1f, outerX - 0.6f);
            float innerZ = Mathf.Max(0.1f, outerZ - 0.6f);
            var mesh = new Mesh { name = "Beveled sidewalk surface" };
            mesh.vertices = new[]
            {
                new Vector3(-outerX, 0, -outerZ), new Vector3(-outerX, 0, outerZ),
                new Vector3(outerX, 0, outerZ), new Vector3(outerX, 0, -outerZ),
                new Vector3(-innerX, 0.28f, -innerZ), new Vector3(-innerX, 0.28f, innerZ),
                new Vector3(innerX, 0.28f, innerZ), new Vector3(innerX, 0.28f, -innerZ)
            };
            mesh.triangles = new[] { 4,5,6, 4,6,7, 0,1,5, 0,5,4, 1,2,6, 1,6,5, 2,3,7, 2,7,6, 3,0,4, 3,4,7 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshes.Add(mesh);
            var item = new GameObject("Raised sidewalk", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            item.transform.SetParent(transform, false);
            item.transform.localPosition = center;
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            item.GetComponent<MeshRenderer>().sharedMaterial = material;
            item.GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        public void Box(string label, Vector3 position, Vector3 size, Material material,
            bool solid = false, float yaw = 0f, Quaternion? orientation = null)
        {
            if (cube == null)
            {
                var source = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube = source.GetComponent<MeshFilter>().sharedMesh;
                source.SetActive(false);
                Destroy(source);
            }
            if (!batches.TryGetValue(material, out var instances))
            {
                instances = new List<CombineInstance>();
                batches.Add(material, instances);
            }
            Quaternion rotation = orientation ?? Quaternion.Euler(0, yaw, 0);
            instances.Add(new CombineInstance { mesh = cube, transform = Matrix4x4.TRS(position, rotation, size) });
            if (!solid) return;
            var item = new GameObject(label, typeof(BoxCollider));
            item.transform.SetParent(transform, false);
            item.transform.localPosition = position;
            item.transform.localRotation = rotation;
            item.GetComponent<BoxCollider>().size = size;
        }

        public void Bake()
        {
            foreach (var batch in batches)
            {
                var mesh = new Mesh { name = batch.Key.name + " geometry", indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(batch.Value.ToArray(), true, true);
                meshes.Add(mesh);
                var item = new GameObject(batch.Key.name, typeof(MeshFilter), typeof(MeshRenderer));
                item.transform.SetParent(transform, false);
                item.GetComponent<MeshFilter>().sharedMesh = mesh;
                item.GetComponent<MeshRenderer>().sharedMaterial = batch.Key;
            }
            batches.Clear();
        }

        private void OnDestroy()
        {
            foreach (var mesh in meshes) Destroy(mesh);
        }
    }
}
