using UnityEngine;

namespace NightCourier.Delivery
{
    /// <summary>Keeps the visible pickup-bed parcels synchronized with route progress.</summary>
    public sealed class CargoBedVisual : MonoBehaviour
    {
        private GameObject[] parcels;
        public int VisibleCount { get; private set; }

        public void Configure(GameObject[] parcelObjects)
        {
            parcels = parcelObjects;
            SetParcelCount(0);
        }

        public void SetParcelCount(int count)
        {
            if (parcels == null) return;
            VisibleCount = Mathf.Clamp(count, 0, parcels.Length);
            for (int i = 0; i < parcels.Length; i++)
                parcels[i].SetActive(i < VisibleCount);
        }
    }
}
