using System.Collections.Generic;
using UnityEngine;

namespace NightCourier.World
{
    /// <summary>Finds a shortest route over the generated city, expressway and scenic-road graph.</summary>
    public static class RoadRoutePlanner
    {
        public static readonly Vector3[] LogisticsAccessPoints =
        {
            new Vector3(20, 0, -230), new Vector3(50, 0, -230), new Vector3(80, 0, -230),
            new Vector3(110, 0, -230), new Vector3(140, 0, -230), new Vector3(170, 0, -230), new Vector3(200, 0, -230)
        };
        public static readonly Vector3[] RestAreaAccessPoints =
        {
            new Vector3(700, 0, -30), new Vector3(700, 0, -12), new Vector3(735, 0, -12), new Vector3(735, 0, 4)
        };

        private struct Edge
        {
            public int To;
            public float Cost;
        }

        private static readonly List<Vector3> nodes = new List<Vector3>();
        private static readonly List<List<Edge>> edges = new List<List<Edge>>();
        private static readonly Dictionary<Vector2Int, int> lookup = new Dictionary<Vector2Int, int>();
        private static bool ready;

        public static List<Vector3> FindRoute(Vector3 from, Vector3 destination)
        {
            EnsureGraph();
            int start = Nearest(from);
            int goal = Nearest(destination);
            int count = nodes.Count;
            var distance = new float[count];
            var previous = new int[count];
            var visited = new bool[count];
            for (int i = 0; i < count; i++) { distance[i] = float.PositiveInfinity; previous[i] = -1; }
            distance[start] = 0;

            for (int step = 0; step < count; step++)
            {
                int current = -1;
                float best = float.PositiveInfinity;
                for (int i = 0; i < count; i++)
                    if (!visited[i] && distance[i] < best) { current = i; best = distance[i]; }
                if (current < 0 || current == goal) break;
                visited[current] = true;
                foreach (Edge edge in edges[current])
                {
                    float candidate = distance[current] + edge.Cost;
                    if (candidate >= distance[edge.To]) continue;
                    distance[edge.To] = candidate;
                    previous[edge.To] = current;
                }
            }

            var route = new List<Vector3> { destination };
            for (int node = goal; node >= 0; node = previous[node])
            {
                route.Add(nodes[node]);
                if (node == start) break;
            }
            route.Reverse();
            route.Insert(0, from);
            return route;
        }

        private static void EnsureGraph()
        {
            if (ready) return;
            AddGrid(CityBuilder.VerticalRoads, CityBuilder.HorizontalRoads);
            AddGrid(WorldExpansionBuilder.SecondCityVerticalRoads, WorldExpansionBuilder.SecondCityHorizontalRoads);
            AddPolyline(WorldExpansionBuilder.HighwayPoints, 1);
            AddPolyline(ScenicRoadPath.Points, 5);
            AddPolyline(LogisticsAccessPoints, 1);
            ConnectLogisticsAccessToCity();
            AddPolyline(RestAreaAccessPoints, 1);
            ready = true;
        }

        private static void ConnectLogisticsAccessToCity()
        {
            foreach (Vector3 access in LogisticsAccessPoints)
            {
                bool liesOnCityRoad = false;
                foreach (float roadX in CityBuilder.VerticalRoads)
                    if (Mathf.Approximately(access.x, roadX)) { liesOnCityRoad = true; break; }
                if (!liesOnCityRoad) continue;
                Connect(AddNode(access), AddNode(new Vector3(access.x, 0, -270)));
                Connect(AddNode(access), AddNode(new Vector3(access.x, 0, -190)));
            }
        }

        private static void AddGrid(float[] vertical, float[] horizontal)
        {
            for (int x = 0; x < vertical.Length; x++)
            for (int z = 0; z < horizontal.Length; z++)
            {
                int here = AddNode(new Vector3(vertical[x], 0, horizontal[z]));
                if (x > 0) Connect(here, AddNode(new Vector3(vertical[x - 1], 0, horizontal[z])));
                if (z > 0) Connect(here, AddNode(new Vector3(vertical[x], 0, horizontal[z - 1])));
            }
        }

        private static void AddPolyline(IReadOnlyList<Vector3> points, int stride)
        {
            int previous = -1;
            for (int i = 0; i < points.Count; i += stride)
            {
                int current = AddNode(points[i]);
                if (previous >= 0) Connect(previous, current);
                previous = current;
            }
            int last = AddNode(points[points.Count - 1]);
            if (previous != last) Connect(previous, last);
        }

        private static int AddNode(Vector3 point)
        {
            var key = new Vector2Int(Mathf.RoundToInt(point.x * 10), Mathf.RoundToInt(point.z * 10));
            if (lookup.TryGetValue(key, out int existing)) return existing;
            int index = nodes.Count;
            lookup.Add(key, index);
            nodes.Add(point);
            edges.Add(new List<Edge>());
            return index;
        }

        private static void Connect(int a, int b)
        {
            float cost = Vector3.Distance(nodes[a], nodes[b]);
            edges[a].Add(new Edge { To = b, Cost = cost });
            edges[b].Add(new Edge { To = a, Cost = cost });
        }

        private static int Nearest(Vector3 point)
        {
            int nearest = 0;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < nodes.Count; i++)
            {
                float candidate = (new Vector2(point.x - nodes[i].x, point.z - nodes[i].z)).sqrMagnitude;
                if (candidate < distance) { distance = candidate; nearest = i; }
            }
            return nearest;
        }
    }
}
