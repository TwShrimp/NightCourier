using UnityEngine;

namespace NightCourier.Vehicle
{
    /// <summary>Minimal prototype instruments visible only from the cockpit camera.</summary>
    public sealed class CockpitDisplay : MonoBehaviour
    {
        private ArcadeCarController car;
        private GameObject root;
        private Transform steeringWheel;
        private GameObject instrumentHood;
        private GUIStyle speedStyle;
        private bool compactSportLayout;
        public bool IsVisible => root != null && root.activeSelf;

        public void Configure(ArcadeCarController vehicle, GameObject displayRoot, Transform wheel)
        {
            car = vehicle;
            root = displayRoot;
            steeringWheel = wheel;
            Transform hood = root.transform.Find("Instrument hood");
            instrumentHood = hood == null ? null : hood.gameObject;
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (root != null) root.SetActive(visible);
        }

        private void LateUpdate()
        {
            if (car == null || !IsVisible) return;
            bool sport = car.TargetTopSpeedKph > 250;
            if (sport != compactSportLayout)
            {
                compactSportLayout = sport;
                root.transform.localScale = sport ? Vector3.one * .62f : Vector3.one;
                root.transform.localPosition = sport ? new Vector3(0, .22f, .12f) : Vector3.zero;
                if (instrumentHood != null) instrumentHood.SetActive(!sport);
            }
            float wheelAngle = Mathf.Clamp(car.SteeringAngle / 26f, -1, 1) * -45f;
            steeringWheel.localRotation = Quaternion.Euler(0, 0, wheelAngle);
        }

        private void OnGUI()
        {
            if (car == null || !IsVisible) return;
            if (speedStyle == null)
            {
                speedStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = 22
                };
                speedStyle.normal.textColor = new Color(0.45f, 0.95f, 1f);
            }
            float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
            Matrix4x4 savedMatrix = GUI.matrix;
            Color savedColor = GUI.color;
            int savedDepth = GUI.depth;
            GUI.depth = -20;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float width = Screen.width / scale;
            float height = Screen.height / scale;
            Rect gauge = new Rect(width * 0.5f + 90, height - 150, 145, 82);
            GUI.color = new Color(0.015f, 0.025f, 0.035f, 0.96f);
            GUI.DrawTexture(gauge, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(gauge, $"{car.SpeedKph:000}\nKM/H  {car.DriveLabel}", speedStyle);
            GUI.matrix = savedMatrix;
            GUI.color = savedColor;
            GUI.depth = savedDepth;
        }
    }
}
