using UnityEngine;

namespace NightCourier.Prototype.Vehicles
{
    internal static class VehicleMeshNormals
    {
        // Opposing faces must not share vertices: their normals would cancel.
        public static void SeparateFaces(Mesh mesh)
        {
            Vector3[] source = mesh.vertices;
            int[] indices = mesh.triangles;
            var vertices = new Vector3[indices.Length];
            var triangles = new int[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                vertices[i] = source[indices[i]];
                triangles[i] = i;
            }
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}
