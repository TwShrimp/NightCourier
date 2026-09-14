using System.Collections.Generic;
using UnityEngine;

namespace NightCourier.World
{
    /// <summary>Rasterize road geometry once; the HUD samples this map without rotating GUI clip rectangles.</summary>
    public static class RoadMapTexture
    {
        public static readonly Rect WorldBounds = new Rect(-400, -340, 2040, 850);
        private const int Width = 1536;
        private const int Height = 512;

        public static Texture2D Create()
        {
            var pixels = new Color32[Width * Height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(13, 25, 34, 255);
            var city = new Color32(63, 89, 105, 255);
            for (int i = 0; i < CityBuilder.VerticalRoads.Length; i++)
            {
                int radius = CityBuilder.VerticalLanes[i] + 2;
                Stroke(pixels, new Vector3(CityBuilder.VerticalRoads[i], 0, CityBuilder.MinZ),
                    new Vector3(CityBuilder.VerticalRoads[i], 0, CityBuilder.MaxZ), city, radius);
            }
            for (int i = 0; i < CityBuilder.HorizontalRoads.Length; i++)
            {
                int radius = CityBuilder.HorizontalLanes[i] + 2;
                Stroke(pixels, new Vector3(CityBuilder.MinX, 0, CityBuilder.HorizontalRoads[i]),
                    new Vector3(CityBuilder.MaxX, 0, CityBuilder.HorizontalRoads[i]), city, radius);
            }
            var path = ScenicRoadPath.Points;
            for (int i = 0; i < path.Count - 1; i++)
                Stroke(pixels, path[i], path[i + 1], ScenicRoadPath.IsViaduct(path[i])
                    ? new Color32(75, 225, 235, 255) : new Color32(240, 180, 80, 255), 3);
            for (int i = 0; i < WorldExpansionBuilder.HighwayPoints.Length - 1; i++)
                Stroke(pixels, WorldExpansionBuilder.HighwayPoints[i], WorldExpansionBuilder.HighwayPoints[i + 1],
                    new Color32(120, 205, 225, 255), 5);
            for (int i = 0; i < WorldExpansionBuilder.SecondCityVerticalRoads.Length; i++)
                Stroke(pixels, new Vector3(WorldExpansionBuilder.SecondCityVerticalRoads[i], 0, WorldExpansionBuilder.SecondCityMinZ),
                    new Vector3(WorldExpansionBuilder.SecondCityVerticalRoads[i], 0, WorldExpansionBuilder.SecondCityMaxZ), city,
                    WorldExpansionBuilder.SecondCityVerticalLanes[i] + 2);
            for (int i = 0; i < WorldExpansionBuilder.SecondCityHorizontalRoads.Length; i++)
                Stroke(pixels, new Vector3(WorldExpansionBuilder.SecondCityMinX, 0, WorldExpansionBuilder.SecondCityHorizontalRoads[i]),
                    new Vector3(WorldExpansionBuilder.SecondCityMaxX, 0, WorldExpansionBuilder.SecondCityHorizontalRoads[i]), city,
                    WorldExpansionBuilder.SecondCityHorizontalLanes[i] + 2);
            for (int i = 0; i < RoadRoutePlanner.LogisticsAccessPoints.Length - 1; i++)
                Stroke(pixels, RoadRoutePlanner.LogisticsAccessPoints[i], RoadRoutePlanner.LogisticsAccessPoints[i + 1], city, 4);
            for (int i = 0; i < RoadRoutePlanner.RestAreaAccessPoints.Length - 1; i++)
                Stroke(pixels, RoadRoutePlanner.RestAreaAccessPoints[i], RoadRoutePlanner.RestAreaAccessPoints[i + 1], city, 4);
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                name = "Generated district and bypass map", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        public static Texture2D CreateRouteOverlay()
        {
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                name = "Current delivery road route", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp
            };
            UpdateRoute(texture, null);
            return texture;
        }

        public static void UpdateRoute(Texture2D texture, IReadOnlyList<Vector3> route)
        {
            var pixels = new Color32[Width * Height];
            if (route != null)
            {
                for (int i = 0; i < route.Count - 1; i++)
                    Stroke(pixels, route[i], route[i + 1], new Color32(5, 20, 28, 220), 5);
                for (int i = 0; i < route.Count - 1; i++)
                    Stroke(pixels, route[i], route[i + 1], new Color32(80, 245, 225, 255), 2);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
        }

        private static void Stroke(Color32[] pixels, Vector3 a, Vector3 b, Color32 color, int radius)
        {
            Vector2 from = Pixel(a);
            Vector2 to = Pixel(b);
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, to)));
            for (int i = 0; i <= steps; i++)
            {
                Vector2 point = Vector2.Lerp(from, to, i / (float)steps);
                int x = Mathf.RoundToInt(point.x);
                int y = Mathf.RoundToInt(point.y);
                for (int dy = -radius; dy <= radius; dy++)
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int px = x + dx; int py = y + dy;
                    if (px >= 0 && px < Width && py >= 0 && py < Height && dx * dx + dy * dy <= radius * radius)
                        pixels[py * Width + px] = color;
                }
            }
        }

        private static Vector2 Pixel(Vector3 point) => new Vector2(
            (point.x - WorldBounds.xMin) / WorldBounds.width * Width,
            (point.z - WorldBounds.yMin) / WorldBounds.height * Height);
    }
}
