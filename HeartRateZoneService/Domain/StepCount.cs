namespace HeartRateZoneService.Domain
{
    public class StepCount
    {

        public int Value { get; set; }
        public DateTime DateTime { get; set; }


        public StepCount(int value, DateTime dateTime)
        {
            Value = value;
            DateTime = dateTime;
        }


    }
}
