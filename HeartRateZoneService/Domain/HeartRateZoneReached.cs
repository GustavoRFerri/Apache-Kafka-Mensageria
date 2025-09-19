namespace HeartRateZoneService.Domain
{
    public class HeartRateZoneReached
    {
        public Guid DeviceId { get; }
        public HeartRateZone Zone { get; }
        public DateTime DateTime { get; }
        public int HeartRate { get; }
        public int MaxHeartRate { get; }

        public HeartRateZoneReached(Guid deviceID, HeartRateZone zone, DateTime dateTime, int heartRate, int maxHeartRate)
        {
            DeviceId = deviceID;
            Zone = zone;
            DateTime = dateTime;
            HeartRate = heartRate;
            MaxHeartRate = maxHeartRate;
        }
    }
}
