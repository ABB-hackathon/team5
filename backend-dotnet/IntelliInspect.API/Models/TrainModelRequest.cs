namespace IntelliInspect.API.Models
{
    public class TrainModelRequest
    {
        public DateTime TrainStart { get; set; }
        public DateTime TrainEnd { get; set; }
        public DateTime TestStart { get; set; }
        public DateTime TestEnd { get; set; }
    }
}
