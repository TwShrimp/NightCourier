using System.Collections.Generic;
using UnityEngine;

namespace NightCourier.World
{
    /// <summary>Shared centreline for road construction, the minimap, and continuity tests.</summary>
    public static class ScenicRoadPath
    {
        public const float Width = 14;
        private static readonly Vector3[] Knots =
        {
            new Vector3(350, 0, 130), new Vector3(410, 0, 130), new Vector3(470, 2, 175),
            new Vector3(535, 8, 270), new Vector3(620, 16, 390), new Vector3(740, 20, 450),
            new Vector3(865, 18, 420), new Vector3(950, 10, 330), new Vector3(1000, 4, 220),
            new Vector3(1020, 0, 130)
        };
        private static readonly List<Vector3> points = Sample();
        public static IReadOnlyList<Vector3> Points => points;
        public static bool IsViaduct(Vector3 p) => p.z > 365;

        public static Vector3 Right(int index)
        {
            Vector3 direction = points[Mathf.Min(index + 1, points.Count - 1)] - points[Mathf.Max(index - 1, 0)];
            return Vector3.Cross(Vector3.up, direction).normalized;
        }

        public static string District(Vector3 position) => position.x >= WorldExpansionBuilder.SecondCityMinX ? "HARBOR CITY"
            : position.x >= CityBuilder.MaxX && Mathf.Abs(position.z + 30) < 60 ? "INTERCITY EXPRESSWAY"
            : position.x <= CityBuilder.MaxX ? "CITY DISTRICT"
            : IsViaduct(position) ? "RIVER VIADUCT" : "HILLSIDE BYPASS";

        private static List<Vector3> Sample()
        {
            var result = new List<Vector3>();
            for (int i = 0; i < Knots.Length - 1; i++)
            {
                Vector3 a = i == 0 ? 2 * Knots[0] - Knots[1] : Knots[i - 1];
                Vector3 b = Knots[i];
                Vector3 c = Knots[i + 1];
                Vector3 d = i + 2 < Knots.Length ? Knots[i + 2] : 2 * c - b;
                int steps = Mathf.CeilToInt(Vector3.Distance(b, c) / 2.5f);
                for (int step = 0; step < steps; step++)
                {
                    float t = step / (float)steps;
                    Vector3 point = 0.5f * ((2 * b) + (-a + c) * t + (2 * a - 5 * b + 4 * c - d) * t * t
                        + (-a + 3 * b - 3 * c + d) * t * t * t);
                    point.y = Mathf.Max(0, point.y);
                    result.Add(point);
                }
            }
            result.Add(Knots[Knots.Length - 1]);
            return result;
        }
    }
}
