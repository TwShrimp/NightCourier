using UnityEngine;

namespace NightCourier.Prototype
{
    /// <summary>Renders three lightweight generated vehicle models into the garage menu.</summary>
    public sealed class VehiclePreviewRenderer : MonoBehaviour
    {
        private const int PreviewLayer = 31;
        private GameObject root;
        private GameObject[] shells;
        private Camera previewCamera;
        private RenderTexture texture;
        private Material paint, glass, metal, previewLamp;

        public RenderTexture Texture => texture;

        private void Awake()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            paint = CreateMaterial(shader, "Preview matte paint", new Color(.025f, .34f, .38f), .18f);
            glass = CreateMaterial(shader, "Preview glass", new Color(.02f, .12f, .17f), .95f);
            metal = CreateMaterial(shader, "Preview metal", new Color(.035f, .045f, .06f), .62f);
            previewLamp = CreateMaterial(shader, "Preview LED", new Color(.78f, .92f, 1), .5f);
            previewLamp.EnableKeyword("_EMISSION");
            previewLamp.SetColor("_EmissionColor", new Color(.78f, .92f, 1) * 4);
            root = new GameObject("Vehicle selection 3D preview");
            root.transform.position = new Vector3(0, -1000, 0);
            shells = new[] { Pickup(), Sport(), Van() };
            foreach (GameObject shell in shells) shell.transform.SetParent(root.transform, false);

            var cameraObject = new GameObject("Vehicle preview camera", typeof(Camera));
            cameraObject.transform.position = root.transform.position + new Vector3(6.3f, 3.1f, 6.8f);
            cameraObject.transform.rotation = Quaternion.LookRotation(root.transform.position + Vector3.up * .7f - cameraObject.transform.position);
            previewCamera = cameraObject.GetComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(.025f, .04f, .055f, 1);
            previewCamera.cullingMask = 1 << PreviewLayer;
            previewCamera.fieldOfView = 35;
            texture = new RenderTexture(768, 512, 16) { name = "Vehicle selection live preview", antiAliasing = 2 };
            previewCamera.targetTexture = texture;

            var lightObject = new GameObject("Vehicle preview softbox", typeof(Light));
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.localRotation = Quaternion.Euler(48, -32, 0);
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 2.2f;
            light.color = new Color(.72f, .92f, 1);
            light.cullingMask = 1 << PreviewLayer;
            SetLayer(root, PreviewLayer);
        }

        private void Update()
        {
            if (previewCamera != null && previewCamera.enabled)
                root.transform.Rotate(0, 18 * Time.unscaledDeltaTime, 0, Space.World);
        }

        public void Show(CourierVehicle vehicle, int colorIndex)
        {
            if (shells != null)
                for (int i = 0; i < shells.Length; i++) shells[i].SetActive(i == (int)vehicle);
            if (paint != null)
                paint.SetColor("_BaseColor", VehicleAppearance.AvailableColors[Mathf.Clamp(colorIndex, 0, VehicleAppearance.AvailableColors.Count - 1)]);
            SetRendering(true);
        }

        public void SetRendering(bool enabled)
        {
            if (previewCamera != null) previewCamera.enabled = enabled;
        }

        private GameObject Pickup()
        {
            var shell = new GameObject("Preview electric pickup");
            Part(shell.transform, new Vector3(0, .25f, 0), new Vector3(2, .55f, 5.1f), paint);
            Part(shell.transform, new Vector3(0, .8f, .65f), new Vector3(1.8f, .9f, 1.9f), glass);
            Part(shell.transform, new Vector3(0, .58f, -1.7f), new Vector3(1.9f, .65f, 1.7f), paint);
            Wheels(shell.transform);
            return shell;
        }

        private GameObject Sport()
        {
            var shell = Vehicles.Sport.SportBodyBuilder.Build(root.transform, paint, glass, metal, metal);
            foreach (int side in new[] {-1, 1})
            foreach (float z in new[] {1.8f, -1.65f})
            {
                GameObject tire = Part(shell.transform, new Vector3(side * .96f, -.065f, z),
                    new Vector3(.78f, .17f, .78f), metal, PrimitiveType.Cylinder);
                tire.transform.localRotation = Quaternion.Euler(0, 0, 90);
                GameObject rim = Part(shell.transform, new Vector3(side * 1.145f, -.065f, z),
                    new Vector3(.56f, .015f, .56f), glass, PrimitiveType.Cylinder);
                rim.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            foreach (int side in new[] {-1, 1})
            foreach (float y in new[] {.28f, .225f})
                Part(shell.transform, new Vector3(side * .72f, y, 2.855f), new Vector3(.55f, .027f, .025f), previewLamp);
            return shell;
        }

        private GameObject Van()
        {
            var shell = new GameObject("Preview courier van");
            Part(shell.transform, new Vector3(0, .2f, 0), new Vector3(2.05f, .55f, 5.2f), paint);
            Part(shell.transform, new Vector3(0, 1.05f, -.55f), new Vector3(2, 1.55f, 3.75f), paint);
            Part(shell.transform, new Vector3(0, 1.08f, 1.37f), new Vector3(1.86f, 1.15f, .12f), glass);
            Part(shell.transform, new Vector3(0, .55f, 2.15f), new Vector3(2.02f, .5f, 1.1f), paint);
            Wheels(shell.transform);
            return shell;
        }

        private void Wheels(Transform parent, float frontZ = 1.57f, float rearZ = -1.55f, float diameter = .92f)
        {
            foreach (int side in new[] { -1, 1 })
            foreach (float z in new[] { rearZ, frontZ })
            {
                GameObject wheel = Part(parent, new Vector3(side * 1.04f, -.14f, z), new Vector3(diameter, .2f, diameter), metal, PrimitiveType.Cylinder);
                wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
        }

        private GameObject Part(Transform parent, Vector3 position, Vector3 scale, Material material, PrimitiveType type = PrimitiveType.Cube)
        {
            GameObject item = GameObject.CreatePrimitive(type);
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(item.GetComponent<Collider>());
            return item;
        }

        private static Material CreateMaterial(Shader shader, string label, Color color, float smoothness)
        {
            var material = new Material(shader) { name = label };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            return material;
        }

        private static void SetLayer(GameObject item, int layer)
        {
            item.layer = layer;
            foreach (Transform child in item.transform) SetLayer(child.gameObject, layer);
        }

        private void OnDestroy()
        {
            if (texture != null) { texture.Release(); Destroy(texture); }
            if (previewCamera != null) Destroy(previewCamera.gameObject);
            if (root != null) Destroy(root);
            foreach (Material material in new[] { paint, glass, metal, previewLamp })
                if (material != null) Destroy(material);
        }
    }
}
