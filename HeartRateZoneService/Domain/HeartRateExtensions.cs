namespace HeartRateZoneService.Domain
{
    public enum HeartRateZone
    {
        None,
        Zone1,
        Zone2,
        Zone3,
        Zone4,
        Zone5
    }

    public static class HeartRateExtensions
    {

        public static HeartRateZone GetHeartRateZone(this HeartRate heartRate, int maximumHeartRate)
        {
            var perc = (Double)heartRate.Value / (Double)maximumHeartRate;
            var zone = HeartRateZone.None;

            if (perc < 0.5)
            {
                zone= HeartRateZone.None;
            }
            else if (perc >= 0.5 && perc < 0.6)
            {
                zone = HeartRateZone.Zone1;
            }
            else if (perc >= 0.6 && perc <0.7)
            {
                zone= HeartRateZone.Zone2;
            }
            else if (perc >= 0.7 && perc <0.8)
            {
                zone= HeartRateZone.Zone3;
            }
            else if (perc >= 0.8 && perc < 0.9)
            {
                zone= HeartRateZone.Zone4;
            }
            else if (perc >= 0.9)
            {
                zone= HeartRateZone.Zone5;
            }

            return zone;
        }
    }


}