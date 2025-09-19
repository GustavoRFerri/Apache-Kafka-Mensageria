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
        private static double Zone50Threshold = 0.5;
        private static double Zone60Threshold = 0.6;
        private static double Zone70Threshold = 0.7;
        private static double Zone80Threshold = 0.8;
        private static double Zone90Threshold = 0.9;

        public static HeartRateZone GetHeartRateZone(this HeartRate heartRate, int maximumHeartRate)
        {
            var perc = (Double)heartRate.Value / (Double)maximumHeartRate;
            var zone = HeartRateZone.None;

            if (perc < Zone50Threshold)
            {
                zone= HeartRateZone.None;
            }
            else if (perc >= Zone50Threshold && perc < Zone60Threshold)
            {
                zone = HeartRateZone.Zone1;
            }
            else if (perc >= Zone60Threshold && perc < Zone70Threshold)
            {
                zone= HeartRateZone.Zone2;
            }
            else if (perc >= Zone70Threshold && perc < Zone80Threshold)
            {
                zone= HeartRateZone.Zone3;
            }
            else if (perc >= Zone80Threshold && perc < Zone90Threshold)
            {
                zone= HeartRateZone.Zone4;
            }
            else if (perc >= Zone90Threshold)
            {
                zone= HeartRateZone.Zone5;
            }

            return zone;
        }
    }


}