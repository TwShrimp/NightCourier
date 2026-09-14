using System.Collections.Generic;
using UnityEngine;

namespace NightCourier.World
{
    /// <summary>Project-authored surfaces, shared by the city and vehicle.</summary>
    public sealed class CityPalette : MonoBehaviour
    {
        private readonly List<Material> owned = new List<Material>();
        private Texture2D roadTexture;
        public Material Asphalt, Pavement, Curb, White, Yellow, Metal, Glass, CarPaint;
        public Material Warm, Cool, Pink, Red, Green, TailRunning, Headlight, DarkWindow, Grass, Trunk;
        public Material[] Facades;
        public Material BypassAsphalt, Water;
        public Font SignFont { get; private set; }
        public Material SignMaterial { get; private set; }

        public void Initialize()
        {
            Asphalt = Make("Wet asphalt", new Color(0.10f, 0.13f, 0.16f), 0.82f);
            Pavement = Make("Sidewalk", new Color(0.28f, 0.31f, 0.34f), 0.35f);
            Curb = Make("Limestone trim", new Color(0.46f, 0.48f, 0.46f), 0.35f);
            White = Make("Ivory road paint", new Color(0.8f, 0.82f, 0.73f), 0.6f);
            Yellow = Make("Amber road paint", new Color(0.95f, 0.64f, 0.17f), 0.55f);
            Metal = Make("Graphite metal", new Color(0.055f, 0.065f, 0.08f), 0.55f);
            Glass = Make("Smoked glass", new Color(0.035f, 0.09f, 0.13f), 0.92f);
            CarPaint = Make("Matte courier petrol blue", new Color(0.025f, 0.34f, 0.38f), 0.16f);
            Warm = Make("Warm window light", new Color(1f, 0.63f, 0.24f), 0.4f, 1.3f);
            Cool = Make("Cyan neon", new Color(0.14f, 0.82f, 1f), 0.4f, 3f);
            Pink = Make("Rose neon", new Color(1f, 0.12f, 0.36f), 0.4f, 3f);
            Red = Make("Tail lamps", new Color(1f, 0.025f, 0.02f), 0.4f, 3f);
            Green = Make("Traffic green", new Color(0.08f, 0.9f, 0.34f), 0.35f, 3f);
            TailRunning = Make("Dim rear running lamps", new Color(0.38f, 0.008f, 0.006f), 0.3f, 0.65f);
            Headlight = Make("Cold white vehicle lamps", new Color(0.78f, 0.92f, 1f), 0.75f, 4f);
            DarkWindow = Make("Unlit windows", new Color(0.025f, 0.055f, 0.075f), 0.8f);
            Grass = Make("Park foliage", new Color(0.07f, 0.18f, 0.15f), 0.1f);
            Trunk = Make("Tree bark", new Color(0.13f, 0.10f, 0.085f));
            Facades = new[]
            {
                Make("Terracotta brick", new Color(0.32f, 0.16f, 0.13f)),
                Make("Blue concrete", new Color(0.16f, 0.24f, 0.30f)),
                Make("Sandstone", new Color(0.38f, 0.34f, 0.26f)),
                Make("Charcoal tower", new Color(0.10f, 0.14f, 0.19f), 0.55f),
                Make("Olive masonry", new Color(0.23f, 0.27f, 0.23f))
            };
            roadTexture = new Texture2D(128, 128, TextureFormat.RGBA32, true) { name = "Generated asphalt grain", wrapMode = TextureWrapMode.Repeat };
            var random = new System.Random(21);
            var pixels = new Color[128 * 128];
            for (int i = 0; i < pixels.Length; i++)
            {
                float value = 0.65f + (float)random.NextDouble() * 0.35f;
                pixels[i] = new Color(value, value, value, 1);
            }
            roadTexture.SetPixels(pixels);
            roadTexture.Apply();
            Asphalt.SetTexture("_BaseMap", roadTexture);
            Asphalt.SetTextureScale("_BaseMap", new Vector2(90, 90));
            BypassAsphalt = new Material(Asphalt) { name = "Bypass wet asphalt" };
            BypassAsphalt.SetTextureScale("_BaseMap", Vector2.one);
            owned.Add(BypassAsphalt);
            Water = Make("River water", new Color(0.035f, 0.15f, 0.22f), 0.92f, 0.15f);
            SignFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            SignFont.RequestCharactersInTexture("ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789", 48);
            SignMaterial = new Material(Resources.Load<Shader>("WorldSign")) { name = "Depth tested shop lettering" };
            SignMaterial.SetTexture("_MainTex", SignFont.material.mainTexture);
            owned.Add(SignMaterial);
            Font.textureRebuilt += RefreshFontTexture;
        }

        private void RefreshFontTexture(Font font)
        {
            if (font == SignFont && SignMaterial != null)
                SignMaterial.SetTexture("_MainTex", font.material.mainTexture);
        }

        private Material Make(string label, Color color, float smoothness = 0.25f, float emission = 0f)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = label, enableInstancing = true };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            if (emission > 0)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            owned.Add(material);
            return material;
        }

        private void OnDestroy()
        {
            Font.textureRebuilt -= RefreshFontTexture;
            foreach (var material in owned) Destroy(material);
            if (roadTexture != null) Destroy(roadTexture);
        }
    }
}
