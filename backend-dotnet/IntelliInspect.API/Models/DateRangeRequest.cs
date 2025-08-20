namespace IntelliInspect.API.Models
{
    public class DateRangeRequest
    {
        public DateTime TrainStart { get; set; }
        public DateTime TrainEnd { get; set; }
        public DateTime TestStart { get; set; }
        public DateTime TestEnd { get; set; }
        public DateTime SimStart { get; set; }
        public DateTime SimEnd { get; set; }
    }
}
