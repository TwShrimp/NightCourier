using UnityEngine;

namespace NightCourier.World
{
    /// <summary>Cycles a city intersection through protected north-south and east-west phases.</summary>
    public sealed class TrafficSignalController : MonoBehaviour
    {
        private Renderer[] northSouthRed, northSouthAmber, northSouthGreen;
        private Renderer[] eastWestRed, eastWestAmber, eastWestGreen;

        public void Configure(Renderer[] nsRed, Renderer[] nsAmber, Renderer[] nsGreen,
            Renderer[] ewRed, Renderer[] ewAmber, Renderer[] ewGreen)
        {
            northSouthRed = nsRed; northSouthAmber = nsAmber; northSouthGreen = nsGreen;
            eastWestRed = ewRed; eastWestAmber = ewAmber; eastWestGreen = ewGreen;
            UpdatePhase();
        }

        private void Update() => UpdatePhase();

        private void UpdatePhase()
        {
            if (northSouthRed == null) return;
            float phase = Mathf.Repeat(Time.time + transform.position.x * .01f + transform.position.z * .013f, 20f);
            bool nsGreen = phase < 7f;
            bool nsAmber = phase >= 7f && phase < 9f;
            bool allRed = phase >= 9f && phase < 10f || phase >= 19f;
            bool ewGreen = phase >= 10f && phase < 17f;
            bool ewAmber = phase >= 17f && phase < 19f;
            Set(northSouthRed, !nsGreen && !nsAmber || allRed);
            Set(northSouthAmber, nsAmber);
            Set(northSouthGreen, nsGreen);
            Set(eastWestRed, !ewGreen && !ewAmber || allRed);
            Set(eastWestAmber, ewAmber);
            Set(eastWestGreen, ewGreen);
        }

        private static void Set(Renderer[] renderers, bool enabled)
        {
            foreach (var renderer in renderers) renderer.enabled = enabled;
        }
    }
}
