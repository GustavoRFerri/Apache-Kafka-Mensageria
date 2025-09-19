namespace ClientGateway.Domain
{
    public class Biometrics
    {
        public Guid DeviceId { get; set; }
        public List<HeartRate> HeartRates { get; set; }
        public List<StepCount> StepCounts { get; set; }
        public int MaxHeartRate { get; set; }

        public Biometrics(Guid deviceId, List<HeartRate> heartRates, List<StepCount> stepCounts, int maxHeartRate)
        {
            DeviceId = deviceId;
            HeartRates = heartRates;
            StepCounts = stepCounts;
            MaxHeartRate = maxHeartRate;
        }
    }   

}
