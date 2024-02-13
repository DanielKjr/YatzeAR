using Emgu.CV;
using YatzeAR.YatzyLogik;

namespace YatzeAR.DTO
{
    public class ProcessedMarkers
    {
        public Mat DrawnFrame { get; set; } = new Mat();
        public List<User> Users { get; set; } = new List<User>();
    }
}