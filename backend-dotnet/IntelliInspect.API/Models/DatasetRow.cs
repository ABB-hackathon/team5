namespace IntelliInspect.API.Models
{
    public class DatasetRow
    {
        public DateTime SyntheticTimestamp { get; set; }
        public double Sensor_A { get; set; }
        public double Sensor_B { get; set; }
        public double Sensor_C { get; set; }
        public double Temperature { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
        public int Response { get; set; }
    }
}
