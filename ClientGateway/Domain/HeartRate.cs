namespace ClientGateway.Domain
{
    public class HeartRate
    {
        public int Value { get; set; }
        public DateTime DateTime { get; set; }


        public HeartRate(int value, DateTime dateTime  ) 
        { 
            Value = value;
            DateTime = dateTime;
        }

    }
}
