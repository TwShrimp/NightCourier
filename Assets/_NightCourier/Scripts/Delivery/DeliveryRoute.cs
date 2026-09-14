using NightCourier.Vehicle;
using UnityEngine;

namespace NightCourier.Delivery
{
    public enum DeliveryPhase
    {
        AwaitingPickup,
        Delivering,
        Complete
    }

    /// <summary>Runs the garage-to-depot pickup and an eight-stop route across both cities.</summary>
    public sealed class DeliveryRoute : MonoBehaviour
    {
        private static readonly Vector3 Pickup = new Vector3(112, 0.08f, -230);
        private readonly Vector3[] stops =
        {
            new Vector3(20, .08f, -150), new Vector3(-240, .08f, -110),
            new Vector3(-60, .08f, 90), new Vector3(-330, .08f, 210),
            new Vector3(740, 20.08f, 450), new Vector3(735, .08f, 4),
            new Vector3(1200, .08f, -110), new Vector3(1470, .08f, 210)
        };
        private readonly string[] names =
        {
            "South Quarter residences", "West market arcade", "Central neighborhood park", "North civic district",
            "River viaduct service stop", "Expressway rest area", "Harbor west market", "Harbor waterfront"
        };

        private ArcadeCarController car;
        private CargoBedVisual cargo;
        private Transform marker;
        public int Completed { get; private set; }
        public int Total => stops.Length;
        public int Remaining => Phase == DeliveryPhase.AwaitingPickup ? 0 : Total - Completed;
        public float StoppedTime { get; private set; }
        public DeliveryPhase Phase { get; private set; } = DeliveryPhase.AwaitingPickup;
        public bool HasCargo => Phase == DeliveryPhase.Delivering;
        public bool IsComplete => Phase == DeliveryPhase.Complete;
        public float RequiredStopTime => Phase == DeliveryPhase.AwaitingPickup ? 2f : 1.5f;
        public Vector3 PickupPoint => Pickup;
        public Vector3 Destination => Phase == DeliveryPhase.AwaitingPickup ? Pickup : stops[Mathf.Min(Completed, Total - 1)];
        public string DestinationName => Phase switch
        {
            DeliveryPhase.AwaitingPickup => "Central Logistics / collect 8 parcels",
            DeliveryPhase.Complete => "All parcels delivered",
            _ => names[Completed]
        };
        public string ProgressText => Phase switch
        {
            DeliveryPhase.AwaitingPickup => "Drive to the depot / pickup bed empty",
            DeliveryPhase.Complete => "SHIFT COMPLETE   —   THANK YOU",
            _ => $"{Completed} delivered   /   {Remaining} parcels remaining"
        };
        public string StopAction => Phase == DeliveryPhase.AwaitingPickup ? "LOADING PARCELS…" : "HANDING OVER PARCEL…";

        public void Initialize(ArcadeCarController vehicle, Transform destination)
        {
            car = vehicle;
            cargo = car.GetComponent<CargoBedVisual>();
            marker = destination;
            marker.position = Pickup;
            cargo?.SetParcelCount(0);
        }

        private void Update()
        {
            if (car == null || IsComplete) return;
            Vector3 delta = car.transform.position - marker.position;
            StoppedTime = Mathf.Abs(delta.x) < 4f && Mathf.Abs(delta.z) < 4f && car.SpeedKph < 3f
                ? StoppedTime + Time.deltaTime : 0;
            if (StoppedTime < RequiredStopTime) return;

            StoppedTime = 0;
            if (Phase == DeliveryPhase.AwaitingPickup)
            {
                Phase = DeliveryPhase.Delivering;
                cargo?.SetParcelCount(Total);
                marker.position = stops[0];
                return;
            }

            Completed++;
            cargo?.SetParcelCount(Total - Completed);
            if (Completed < Total)
                marker.position = stops[Completed];
            else
            {
                Phase = DeliveryPhase.Complete;
                marker.gameObject.SetActive(false);
            }
        }
    }
}
